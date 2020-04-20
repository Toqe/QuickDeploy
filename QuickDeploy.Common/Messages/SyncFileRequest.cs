using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickDeploy.Common.Messages
{
    [Serializable]
    public class SyncFileRequest : AuthorizedRequest
    {
        public string Filename { get; set; }

        public byte[] GzippedFile { get; set; }
    }
}
