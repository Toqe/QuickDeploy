﻿using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Principal;
using System.ServiceProcess;
using QuickDeploy.Common;
using QuickDeploy.Common.FileFinder;
using QuickDeploy.Common.Messages;
using QuickDeploy.Common.Process;
using SimpleImpersonation;

namespace QuickDeploy.Server
{
    public class Controller
    {
        private readonly StatusMessageSender statusMessageSender;

        public Controller(StatusMessageSender statusMessageSender)
        {
            this.statusMessageSender = statusMessageSender;
        }

        public object Handle(object request)
        {
            if (request is AuthorizedRequest authorizedRequest)
            {
                if (string.IsNullOrWhiteSpace(authorizedRequest.Credentials.Domain))
                {
                    authorizedRequest.Credentials.Domain = ".";
                }
            }

            try
            {
                if (request is AnalyzeDirectoryRequest analyzeDirectoryRequest)
                {
                    return this.AnalyzeDirectory(analyzeDirectoryRequest);
                }

                if (request is SyncDirectoryRequest syncDirectoryRequest)
                {
                    return this.SyncDirectory(syncDirectoryRequest);
                }

                if (request is ChangeServiceStatusRequest changeServiceStatusRequest)
                {
                    return this.ChangeServiceStatus(changeServiceStatusRequest);
                }

                if (request is ExecuteCommandRequest executeCommandRequest)
                {
                    return this.ExecuteCommand(executeCommandRequest);
                }

                if (request is ExtractZipRequest extractZipRequest)
                {
                    return this.ExtractZip(extractZipRequest);
                }
            }
            catch (Exception ex)
            {
                this.LogError(ex.ToString());
            }

            return null;
        }

        private AnalyzeDirectoryResponse AnalyzeDirectory(AnalyzeDirectoryRequest request)
        {
            var response = new AnalyzeDirectoryResponse();

            this.LogInfo($"Trying to logon as {request.Credentials.Username}");

            using (this.Impersonate(request.Credentials))
            {
                this.LogInfo($"Logged on, now searching files in {request.Directory}");
                var fileFinder = new FileFinder();
                var fileFindResult = fileFinder.Find(new DirectoryInfo(request.Directory));
                this.LogInfo(fileFindResult.Files.Count + " files, " + fileFindResult.Directories.Count + " directories");
                response.Success = true;
                response.Contents = fileFindResult;
            }

            return response;
        }

        private SyncDirectoryResponse SyncDirectory(SyncDirectoryRequest request)
        {
            var response = new SyncDirectoryResponse();

            this.LogInfo($"Trying to logon as {request.Credentials.Username}");

            using (this.Impersonate(request.Credentials))
            {
                this.LogInfo($"Logged on, now syncing files in {request.Directory}");

                foreach (var fileToDelete in request.FilesToDelete)
                {
                    var fullFilename = Path.Combine(request.Directory, fileToDelete);
                    new FileInfo(fullFilename).IsReadOnly = false;
                    File.Delete(fullFilename);
                }

                if (request.Archive != null)
                {
                    new Zipper().Unzip(request.Archive, request.Directory);
                }

                response.Success = true;
            }

            return response;
        }

        private ChangeServiceStatusResponse ChangeServiceStatus(ChangeServiceStatusRequest request)
        {
            this.LogInfo($"Trying to logon as {request.Credentials.Username}");

            WindowsServiceAccessGranter.GrantAccessToWindowStationAndDesktop(request.Credentials.Domain, request.Credentials.Username);

            using (this.Impersonate(request.Credentials, LogonType.Batch))
            {
                this.LogInfo($"Logged on, now changing service {request.ServiceName} status to {request.DesiredServiceStatus}");

                using (var sc = new ServiceController { ServiceName = request.ServiceName })
                {
                    if (sc.Status == ServiceControllerStatus.Stopped && request.DesiredServiceStatus == ServiceStatus.Start)
                    {
                        sc.Start();
                        sc.WaitForStatus(ServiceControllerStatus.Running);
                    }

                    if ((sc.Status == ServiceControllerStatus.Running || sc.Status == ServiceControllerStatus.Paused)
                        && request.DesiredServiceStatus == ServiceStatus.Stop)
                    {
                        sc.Stop();
                        sc.WaitForStatus(ServiceControllerStatus.Stopped);
                    }

                    return new ChangeServiceStatusResponse { Success = true };
                }
            }
        }

        private ExecuteCommandResponse ExecuteCommand(ExecuteCommandRequest request)
        {
            var result = this.RunAsDifferentUser(request.Command, request.Arguments, request.Credentials.Domain, request.Credentials.Username, request.Credentials.Password);

            return new ExecuteCommandResponse
            {
                ExitCode = result.ExitCode,
                Success = result.ExitCode == 0,
                ErrorMessage = result?.Exception?.ToString()
            };
        }

        private ProcessResult RunAsDifferentUser(string command, string arguments, string domain, string username, string password)
        {
            var securePassword = new System.Security.SecureString();

            for (int x = 0; x < password.Length; x++)
            {
                securePassword.AppendChar(password[x]);
            }
            if (string.IsNullOrWhiteSpace(domain))
            {
                domain = ".";
            }

            WindowsServiceAccessGranter.GrantAccessToWindowStationAndDesktop(domain, username);
            var process = new ProcessRunner(command, arguments, true, this.LogProcessLogEntry, domain, username, securePassword);
            var result = process.StartProcessAndWaitForIt();
            return result;
        }

        private ExtractZipResponse ExtractZip(ExtractZipRequest request)
        {
            var response = new ExtractZipResponse();

            this.LogInfo($"Trying to logon as {request.Credentials.Username}");

            using (this.Impersonate(request.Credentials))
            {
                this.LogInfo($"Logged on, now extracting '{request.ZipFileName}' to '{request.DestinationDirectory}'");

                ZipFile.ExtractToDirectory(request.ZipFileName, request.DestinationDirectory);

                response.Success = true;
            }

            return response;
        }

        private void LogProcessLogEntry(ProcessLogEntry logEntry)
        {
            var errorTypes = new[] { ProcessLogType.ErrorOutput, ProcessLogType.Exception };

            if (errorTypes.Contains(logEntry.Type))
            {
                this.LogError(logEntry.Text);
            }
            else
            {
                this.LogInfo(logEntry.Text);
            }
        }

        private Impersonation Impersonate(Credentials credentials, LogonType logonType = LogonType.Interactive)
        {
            this.LogInfo("Before impersonation: " + WindowsIdentity.GetCurrent().Name);

            var result = Impersonation.LogonUser(
                credentials.Domain,
                credentials.Username,
                credentials.Password,
                logonType);

            this.LogInfo("After impersonation: " + WindowsIdentity.GetCurrent().Name);

            return result;
        }

        private void LogInfo(string text)
        {
            Trace.TraceInformation(text);
            this.statusMessageSender.SendInfo(text);
        }

        private void LogError(string text)
        {
            Trace.TraceError(text);
            this.statusMessageSender.SendError(text);
        }
    }
}