using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickDeploy.Common.Messages
{
    [Serializable]
    public class SyncDirectoryRequest : AuthorizedRequest
    {
        public string Directory { get; set; }

        public byte[] Archive { get; set; }

        public List<string> FilesToDelete { get; set; }
    }
}
