using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickDeploy.Common.Messages
{
    [Serializable]
    public class ProxyRequest : AuthorizedRequest
    {
        public string Hostname { get; set; }
        
        public int Port { get; set; }

        public byte[] ExpectedServerCertificate { get; set; }

        public byte[] ClientCertificate { get; set; }

        public string ClientCertificatePassword { get; set; }

        public AuthorizedRequest Request { get; set; }
    }
}
