using System;
using System.Diagnostics;
using System.Security;

namespace QuickDeploy.Common.Process
{
    public class ProcessRunner
    {
        private readonly ProcessResult result = new ProcessResult();

        private readonly string filename;

        private readonly string args;

        private readonly bool asAdmin;

        private readonly Action<ProcessLogEntry> logListener;

        private readonly string domain;

        private readonly string username;

        private readonly SecureString password;

        private bool started = false;

        public ProcessRunner(
            string filename, 
            string args, 
            bool asAdmin = true,
            Action<ProcessLogEntry> logListener = null, 
            string domain = null,
            string username = null,
            SecureString password = null)
        {
            this.filename = filename;
            this.args = args;
            this.asAdmin = asAdmin;
            this.logListener = logListener;
            this.domain = domain;
            this.username = username;
            this.password = password;
        }

        public ProcessResult StartProcessAndWaitForIt()
        {
            if (this.started)
            {
                throw new InvalidOperationException("Process has already been started before.");
            }

            this.started = true;

            var startInfo = new ProcessStartInfo
            {
                FileName = this.filename,
                Arguments = this.args,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            if (this.asAdmin)
            {
                startInfo.Verb = "runas";
            }

            if (this.domain != null && this.username != null && this.password != null)
            {
                startInfo.Domain = this.domain;
                startInfo.UserName = this.username;
                startInfo.Password = this.password;
            }

            try
            {
                var process = new System.Diagnostics.Process { StartInfo = startInfo };
                process.OutputDataReceived += this.CaptureOutput;
                process.ErrorDataReceived += this.CaptureError;
                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
                var log = this.result.Log(ProcessLogType.ProcessStarted, $"Filename: {this.filename}{Environment.NewLine}Arguments: {this.args}{Environment.NewLine}AsAdmin: {this.username}{Environment.NewLine}Domain\\Username: {this.domain}\\{this.username}");
                this.logListener?.Invoke(log);
                process.WaitForExit();

                this.result.ExitCode = process.ExitCode;
                log = this.result.Log(ProcessLogType.ProcessFinished, $"ExitCode: {process.ExitCode}");
                this.logListener?.Invoke(log);
            }
            catch (Exception ex)
            {
                this.result.Exception = ex;
                var log = this.result.Log(ProcessLogType.Exception, ex.ToString());
                this.logListener?.Invoke(log);
            }

            return this.result;
        }

        private void CaptureOutput(object sender, DataReceivedEventArgs e)
        {
            if (e?.Data != null)
            {
                var log = this.result.Log(ProcessLogType.Output, e.Data);
                this.logListener?.Invoke(log);
            }
        }

        private void CaptureError(object sender, DataReceivedEventArgs e)
        {
            if (e?.Data != null)
            {
                var log = this.result.Log(ProcessLogType.ErrorOutput, e.Data);
                this.logListener?.Invoke(log);
            }
        }
    }
}
