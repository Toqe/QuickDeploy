using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickDeploy.Common.Messages
{
    [Serializable]
    public class ChangeServiceStatusRequest : AuthorizedRequest
    {
        public string ServiceName { get; set; }

        public ServiceStatus DesiredServiceStatus { get; set; }
    }
}
