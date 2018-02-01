using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;

namespace QuickDeploy.Common.FileFinder
{
    public class FileFinder
    {
        public FileFindResult Find(DirectoryInfo path, string basePath = "")
        {
            var result = new FileFindResult();
            this.Find(result, path, basePath);
            return result;
        }

        private void Find(FileFindResult result, DirectoryInfo path, string basePath = "")
        {
            foreach (var file in path.EnumerateFiles())
            {
                result.Files.Add(this.Build(file, basePath));
            }

            foreach (var folder in path.EnumerateDirectories())
            {
                result.Directories.Add(this.Build(folder, basePath));
                this.Find(result, folder, Path.Combine(basePath, folder.Name));
            }
        }

        private FileFindFile Build(FileInfo file, string basePath)
        {
            return new FileFindFile
            {
                FileName = Path.Combine(basePath, file.Name),
                ChangeTimestamp = file.LastWriteTimeUtc,
                Md5 = this.CalculateMd5(file)
            };
        }

        private FileFindDirectory Build(DirectoryInfo dir, string basePath)
        {
            return new FileFindDirectory
            {
                DirectoryName = Path.Combine(basePath, dir.Name)
            };
        }

        private string CalculateMd5(FileInfo file)
        {
            using (var md5 = MD5.Create())
            {
                using (var stream = File.OpenRead(file.FullName))
                {
                    var hash = md5.ComputeHash(stream);
                    return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
                }
            }
        }
    }
}
