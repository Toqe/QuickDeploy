using System;
using System.Diagnostics;
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

        private readonly string certificateFilename;

        private X509Certificate serverCertificate;

        private bool running;

        private TcpListener tcpListener;

        public QuickDeployTcpSslServer(int port, string certificateFilename)
        {
            this.port = port;
            this.certificateFilename = certificateFilename;
        }

        public void Start()
        {
            try
            {
                if (this.running)
                {
                    throw new InvalidOperationException("Server is already running.");
                }

                if (this.serverCertificate == null)
                {
                    this.serverCertificate = new X509Certificate2(this.certificateFilename, "", X509KeyStorageFlags.Exportable);
                }

                this.tcpListener = new TcpListener(IPAddress.Any, this.port);

                this.tcpListener.Start();
                this.running = true;

                Task.Run(() =>
                {
                    while (this.running)
                    {
                        var newClient = this.tcpListener.AcceptTcpClient();

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
                this.running = false;
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
                using (var sslStream = new SslStream(client.GetStream(), false))
                {
                    try
                    {
                        sslStream.AuthenticateAsServer(this.serverCertificate, false, SslProtocols.Tls, true);

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
    }
}
