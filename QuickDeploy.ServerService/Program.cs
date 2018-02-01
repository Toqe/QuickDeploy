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
            var certificateFilename = ConfigurationManager.AppSettings["certificateFilename"];

            if (!File.Exists(certificateFilename))
            {
                certificateFilename = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, certificateFilename);
            }

            if (!File.Exists(certificateFilename))
            {
                throw new InvalidOperationException($"Certificate file '{certificateFilename}' not found.");
            }
            
            HostFactory.Run(x =>
            {
                x.Service<QuickDeployTcpSslServer>(s =>
                {
                    s.ConstructUsing(name => new QuickDeployTcpSslServer(port, certificateFilename));
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
