using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickDeploy.Common.Messages
{
    [Serializable]
    public class ExtractZipRequest : AuthorizedRequest
    {
        public string ZipFileName { get; set; }

        public string DestinationDirectory { get; set; }
    }
}
