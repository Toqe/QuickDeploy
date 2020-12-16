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
            var serviceName = ConfigurationManager.AppSettings["serviceName"]?.Trim();
            var runAsLocalSystemString = ConfigurationManager.AppSettings["runAsLocalSystem"]?.Trim().ToLowerInvariant();

            var runAsLocalSystem = runAsLocalSystemString == "true";

            if (string.IsNullOrWhiteSpace(serviceName))
            {
                serviceName = "QuickDeployService";
            }

            HostFactory.Run(x =>
            {
                x.Service<QuickDeployTcpSslServer>(s =>
                {
                    s.ConstructUsing(name => new QuickDeployTcpSslServer(port, serverCertificateFilename, serverCertificatePassword, expectedClientCertificateFilename));
                    s.WhenStarted(tc => tc.Start());
                    s.WhenStopped(tc => tc.Stop());
                });

                if (runAsLocalSystem)
                {
                    x.RunAsLocalSystem();
                }
                else
                {
                    x.RunAsNetworkService();
                }

                x.SetDescription(serviceName);
                x.SetDisplayName(serviceName);
                x.SetServiceName(serviceName);
            });
        }
    }
}
