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
