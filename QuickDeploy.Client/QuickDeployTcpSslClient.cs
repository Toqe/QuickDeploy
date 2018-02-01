using System;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using QuickDeploy.Common;
using QuickDeploy.Common.Messages;

namespace QuickDeploy.Client
{
    public class QuickDeployTcpSslClient : IQuickDeployClient
    {
        private readonly string sslHostname = "example.org";

        private readonly StreamHelper streamHelper = new StreamHelper();

        private readonly X509Certificate2 expectedServerCertificate;

        private readonly string hostname;

        private readonly int port;

        public QuickDeployTcpSslClient(string hostname, int port, string certificateFilename)
        {
            this.hostname = hostname;
            this.port = port;

            this.expectedServerCertificate = new X509Certificate2(certificateFilename);
        }

        public string RemoteAddress => $"TCP {this.hostname}:{this.port}";

        public TResponse Call<TRequest, TResponse>(TRequest request) where TResponse : class
        {
            using (var client = new TcpClient())
            {
                client.Connect(this.hostname, this.port);

                using (var stream = new SslStream(client.GetStream(), false, this.VerifyServerCertificate))
                {
                    stream.AuthenticateAsClient(this.sslHostname);

                    this.streamHelper.Send(stream, request);

                    while (true)
                    {
                        var receivedMessage = this.streamHelper.Receive(stream);

                        if (receivedMessage is StatusMessage)
                        {
                            this.HandleStatusMessage(receivedMessage as StatusMessage);
                            continue;
                        }

                        if (receivedMessage is TResponse)
                        {
                            return receivedMessage as TResponse;
                        }

                        throw new InvalidOperationException($"Unknown message type: {receivedMessage?.GetType()?.ToString() ?? "null"}");
                    }
                }
            }
        }

        public AnalyzeDirectoryResponse AnalyzeDirectory(AnalyzeDirectoryRequest analyzeDirectoryRequest)
        {
            return this.Call<AnalyzeDirectoryRequest, AnalyzeDirectoryResponse>(analyzeDirectoryRequest);
        }

        public SyncDirectoryResponse SyncDirectory(SyncDirectoryRequest syncDirectoryRequest)
        {
            return this.Call<SyncDirectoryRequest, SyncDirectoryResponse>(syncDirectoryRequest);
        }

        public ChangeServiceStatusResponse ChangeServiceStatus(ChangeServiceStatusRequest changeServiceStatusRequest)
        {
            return this.Call<ChangeServiceStatusRequest, ChangeServiceStatusResponse>(changeServiceStatusRequest);
        }

        public ExecuteCommandResponse ExecuteCommand(ExecuteCommandRequest executeCommandRequest)
        {
            return this.Call<ExecuteCommandRequest, ExecuteCommandResponse>(executeCommandRequest);
        }

        public ExtractZipResponse ExtractZip(ExtractZipRequest extractZipRequest)
        {
            return this.Call<ExtractZipRequest, ExtractZipResponse>(extractZipRequest);
        }

        private bool VerifyServerCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            var other = certificate as X509Certificate2;

            return this.expectedServerCertificate.SerialNumber == other?.SerialNumber
                   && this.expectedServerCertificate.Thumbprint == other?.Thumbprint;
        }

        private void HandleStatusMessage(StatusMessage statusMessage)
        {
            if (statusMessage.Type == StatusMessageType.Error)
            {
                Console.Error.WriteLine("[SERVER] " + statusMessage.Text);
            }
            else
            {
                Console.WriteLine("[SERVER] " + statusMessage.Text);
            }
        }
    }
}
