using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickDeploy.Common.Messages
{
    [Serializable]
    public class ChangeIisAppPoolStatusRequest : AuthorizedRequest
    {
        public string IisAppPoolName { get; set; }

        public ServiceStatus DesiredServiceStatus { get; set; }
    }
}
