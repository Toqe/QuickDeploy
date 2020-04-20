using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace QuickDeploy.Common
{
    public class Zipper
    {
        public byte[] Zip(IEnumerable<string> filenames, int commonRootLength)
        {
            using (var memoryStream = new MemoryStream())
            {
                using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
                {
                    foreach (var filename in filenames)
                    {
                        var croppedFilename = filename.Substring(commonRootLength);

                        if (croppedFilename.StartsWith(Path.DirectorySeparatorChar + ""))
                        {
                            croppedFilename = croppedFilename.Substring(1);
                        }

                        var entry = archive.CreateEntryFromFile(filename, croppedFilename);
                    }
                }

                return memoryStream.ToArray();
            }
        }

        public void Unzip(byte[] archiveBytes, string targetDirectory)
        {
            using (var memoryStream = new MemoryStream(archiveBytes))
            {
                using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Read, true))
                {
                    foreach (var entry in archive.Entries)
                    {
                        var targetFilename = Path.Combine(targetDirectory, entry.FullName);
                        var targetFile = new FileInfo(targetFilename);

                        if (targetFile.Exists)
                        {
                            targetFile.IsReadOnly = false;
                            targetFile.Delete();
                        }

                        if (!targetFile.Directory.Exists)
                        {
                            targetFile.Directory.Create();
                        }

                        entry.ExtractToFile(targetFilename);
                    }
                }
            }
        }

        public byte[] Gzip(byte[] data)
        {
            byte[] compressesData;

            using (var outputStream = new MemoryStream())
            {
                using (var zip = new GZipStream(outputStream, CompressionMode.Compress))
                {
                    zip.Write(data, 0, data.Length);
                }

                compressesData = outputStream.ToArray();
            }

            return compressesData;
        }

        public byte[] UnGzip(byte[] compressedData)
        {
            byte[] decompressedData;

            using (var outputStream = new MemoryStream())
            {
                using (var inputStream = new MemoryStream(compressedData))
                {
                    using (var zip = new GZipStream(inputStream, CompressionMode.Decompress))
                    {
                        zip.CopyTo(outputStream);
                    }
                }

                decompressedData = outputStream.ToArray();
            }

            return decompressedData;
        }
    }
}
