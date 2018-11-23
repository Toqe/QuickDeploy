using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickDeploy.Common.CertificateGeneration.Console
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var baseDirectoryName = "certificates_" + DateTime.Now.ToString("yyyyMMdd_HHmmss");
            var serverDirectoryName = Path.Combine(baseDirectoryName, "server");
            var clientDirectoryName = Path.Combine(baseDirectoryName, "client");
            System.Console.WriteLine($"Creating directory {serverDirectoryName}");
            Directory.CreateDirectory(serverDirectoryName);
            System.Console.WriteLine($"Creating directory {clientDirectoryName}");
            Directory.CreateDirectory(clientDirectoryName);

            var generator = new CertificateGenerator();

            {
                System.Console.WriteLine($"Generating server certificate");
                var serverCert = generator.GenerateCertificate("example.org");
                var serverCertExports = generator.SerializeToPfx(serverCert, "");

                System.Console.WriteLine($"Saving server certificate");
                File.WriteAllBytes(Path.Combine(serverDirectoryName, "server-private.pfx"), serverCertExports.Private);
                File.WriteAllBytes(Path.Combine(clientDirectoryName, "server-public.pfx"), serverCertExports.Public);
            }

            {
                System.Console.WriteLine($"Generating client certificate");
                var clientCert = generator.GenerateCertificate("example.org");
                var clientCertExports = generator.SerializeToPfx(clientCert, "");

                System.Console.WriteLine($"Saving client certificate");
                File.WriteAllBytes(Path.Combine(clientDirectoryName, "client-private.pfx"), clientCertExports.Private);
                File.WriteAllBytes(Path.Combine(serverDirectoryName, "client-public.pfx"), clientCertExports.Public);
            }

            System.Console.WriteLine("Done");
        }
    }
}
