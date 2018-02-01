using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using QuickDeploy.Common.Messages;

namespace QuickDeploy.Common.DirectorySyncer
{
    public class DirectorySyncer
    {
        private readonly IQuickDeployClient client;

        public DirectorySyncer(IQuickDeployClient client)
        {
            this.client = client;
        }

        public void Sync(string localDirectory, string remoteDirectory, Credentials credentials)
        {
            this.LogInfo($"Analyzing remote directory '{remoteDirectory}' at '{this.client.RemoteAddress}'");

            var analyzeDirectoryRequest = new AnalyzeDirectoryRequest
            {
                Directory = remoteDirectory,
                Credentials = credentials
            };

            var analyzeDirectoryResponse = this.client.AnalyzeDirectory(analyzeDirectoryRequest);

            if ((analyzeDirectoryResponse?.Success ?? false) == false)
            {
                this.LogError("failed: " + analyzeDirectoryResponse?.ErrorMessage);
                return;
            }

            this.LogInfo($"Analyzing local directory '{localDirectory}'");

            var basePath = localDirectory;
            var localFileFindResult = new FileFinder.FileFinder().Find(new DirectoryInfo(basePath));
            var localFiles = localFileFindResult.Files;
            this.LogInfo($"{localFiles.Count} local files");

            var remoteDict = analyzeDirectoryResponse.Contents.Files.ToDictionary(x => x.FileName);

            var filesToTransfer = new List<string>();

            foreach (var localFile in localFiles)
            {
                remoteDict.TryGetValue(localFile.FileName, out var remoteFile);

                if (remoteFile == null)
                {
                    filesToTransfer.Add(localFile.FileName);
                    continue;
                }

                var twoSecondsInTicks = 20 * 1000 * 1000;

                if (Math.Abs(remoteFile.ChangeTimestamp.Ticks - localFile.ChangeTimestamp.Ticks) > twoSecondsInTicks || remoteFile.Md5 != localFile.Md5)
                {
                    filesToTransfer.Add(localFile.FileName);
                }

                remoteDict.Remove(remoteFile.FileName);
            }

            var filesToDelete = remoteDict.Keys.ToList();

            this.LogInfo($"{filesToTransfer.Count} files to transfer");
            this.LogInfo($"{filesToDelete.Count} files to delete");

            byte[] archive = null;

            this.LogInfo($"Syncing files to remote directory '{remoteDirectory}' at '{this.client.RemoteAddress}'");

            if (filesToTransfer.Any())
            {
                var commonRootLength = basePath.Length + (basePath.EndsWith(Path.DirectorySeparatorChar + "") ? 0 : 1);
                archive = new Zipper().Zip(filesToTransfer.Select(x => Path.Combine(basePath, x)), commonRootLength);

                // for debugging purposes:
                // File.WriteAllBytes(@"c:\temp\" + Guid.NewGuid() + ".zip", archive);

                this.LogInfo($"{archive.Length / 1024} kB to transfer");
            }

            var syncDirectoryRequest = new SyncDirectoryRequest
            {
                Credentials = credentials,
                Directory = remoteDirectory,
                Archive = archive,
                FilesToDelete = filesToDelete
            };

            var syncDirectoryResponse = this.client.SyncDirectory(syncDirectoryRequest);

            if (syncDirectoryResponse?.Success != true)
            {
                throw new DirectorySyncException(syncDirectoryResponse?.ErrorMessage ?? "Unknown error: Error message in response not available.");
            }
        }

        private void LogInfo(string text)
        {
            Console.WriteLine(text);
        }

        private void LogError(string text)
        {
            Console.Error.WriteLine(text);
        }
    }
}
