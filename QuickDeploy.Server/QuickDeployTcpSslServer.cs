using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using QuickDeploy.Common;

namespace QuickDeploy.Server
{
    public class QuickDeployTcpSslServer
    {
        private readonly StreamHelper streamHelper = new StreamHelper();

        private readonly int port;

        private string serverCertificateFilename;

        private string serverCertificatePassword;

        private X509Certificate2 serverCertificate;

        private string expectedClientCertificateFilename;

        private X509Certificate2 expectedClientCertificate;

        private bool isRunning;

        private TcpListener tcpListener;

        public QuickDeployTcpSslServer(
            int port, 
            string serverCertificateFilename, 
            string serverCertificatePassword,
            string expectedClientCertificateFilename)
        {
            this.port = port;
            this.serverCertificateFilename = serverCertificateFilename;
            this.serverCertificatePassword = serverCertificatePassword;
            this.expectedClientCertificateFilename = expectedClientCertificateFilename;
        }

        public void Start()
        {
            try
            {
                if (this.isRunning)
                {
                    throw new InvalidOperationException("Server is already running.");
                }

                if (!File.Exists(this.serverCertificateFilename))
                {
                    this.serverCertificateFilename = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, this.serverCertificateFilename);
                }

                if (!File.Exists(this.serverCertificateFilename))
                {
                    throw new InvalidOperationException($"Server certificate file '{this.serverCertificateFilename}' not found.");
                }

                if (!File.Exists(this.expectedClientCertificateFilename))
                {
                    this.expectedClientCertificateFilename = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, this.expectedClientCertificateFilename);
                }

                if (!File.Exists(this.expectedClientCertificateFilename))
                {
                    throw new InvalidOperationException($"Expected client certificate file '{this.expectedClientCertificateFilename}' not found.");
                }

                this.serverCertificate = new X509Certificate2(this.serverCertificateFilename, this.serverCertificatePassword, X509KeyStorageFlags.Exportable);
                this.expectedClientCertificate = new X509Certificate2(this.expectedClientCertificateFilename);

                this.tcpListener = new TcpListener(IPAddress.Any, this.port);

                this.tcpListener.Start();
                this.isRunning = true;

                Task.Run(() =>
                {
                    while (this.isRunning)
                    {
                        var newClient = this.tcpListener.AcceptTcpClient();

                        if (newClient == null)
                        {
                            continue;
                        }

                        ThreadPool.QueueUserWorkItem(state =>
                        {
                            var client = (TcpClient)state;
                            this.HandleClient(client);
                        }, newClient);
                    }
                });
            }
            catch (Exception ex)
            {
                Trace.TraceError(ex.ToString());
                throw;
            }
        }

        public void Stop()
        {
            try
            {
                this.isRunning = false;
                this.tcpListener.Stop();
            }
            catch (Exception ex)
            {
                Trace.TraceError(ex.ToString());
            }
        }

        private void HandleClient(TcpClient client)
        {
            try
            {
                using (client)
                using (var sslStream = new SslStream(client.GetStream(), false, this.VerifyClientCertificate))
                {
                    try
                    {
                        sslStream.AuthenticateAsServer(this.serverCertificate, true, SslProtocols.Tls12, true);

                        var request = this.streamHelper.Receive(sslStream);
                        var statusMessageSender = new StatusMessageSender(m => this.streamHelper.Send(sslStream, m));
                        var controller = new Controller(statusMessageSender);
                        var response = controller.Handle(request);
                        this.streamHelper.Send(sslStream, response);
                    }
                    catch (AuthenticationException ex)
                    {
                        Trace.TraceWarning("SSL error: " + ex);
                    }
                    catch (Exception ex)
                    {
                        Trace.TraceError("Error during request: " + ex);
                    }
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError(ex.ToString());
                throw;
            }
        }

        private bool VerifyClientCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            var other = certificate as X509Certificate2;

            return this.expectedClientCertificate.SerialNumber == other?.SerialNumber
                   && this.expectedClientCertificate.Thumbprint == other?.Thumbprint;
        }
    }
}
