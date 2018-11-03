using System;
using System.Configuration;
using System.IO;
using QuickDeploy.Server;
using Topshelf;

namespace QuickDeploy.ServerService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var port = int.Parse(ConfigurationManager.AppSettings["port"]);
            var serverCertificateFilename = ConfigurationManager.AppSettings["serverCertificateFilename"];
            var serverCertificatePassword = ConfigurationManager.AppSettings["serverCertificatePassword"];
            var expectedClientCertificateFilename = ConfigurationManager.AppSettings["expectedClientCertificateFilename"];

            if (!File.Exists(serverCertificateFilename))
            {
                serverCertificateFilename = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, serverCertificateFilename);
            }

            if (!File.Exists(serverCertificateFilename))
            {
                throw new InvalidOperationException($"Server certificate file '{serverCertificateFilename}' not found.");
            }

            if (!File.Exists(expectedClientCertificateFilename))
            {
                expectedClientCertificateFilename = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, expectedClientCertificateFilename);
            }

            if (!File.Exists(expectedClientCertificateFilename))
            {
                throw new InvalidOperationException($"Expected client certificate file '{expectedClientCertificateFilename}' not found.");
            }

            HostFactory.Run(x =>
            {
                x.Service<QuickDeployTcpSslServer>(s =>
                {
                    s.ConstructUsing(name => new QuickDeployTcpSslServer(port, serverCertificateFilename, serverCertificatePassword, expectedClientCertificateFilename));
                    s.WhenStarted(tc => tc.Start());
                    s.WhenStopped(tc => tc.Stop());
                });

                x.RunAsNetworkService();

                x.SetDescription("QuickDeployService");
                x.SetDisplayName("QuickDeployService");
                x.SetServiceName("QuickDeployService");
            });
        }
    }
}
