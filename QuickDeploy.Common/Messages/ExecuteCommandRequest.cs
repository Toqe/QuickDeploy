using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickDeploy.Common.Messages
{
    [Serializable]
    public class ExecuteCommandRequest : AuthorizedRequest
    {
        public string Command { get; set; }

        public string Arguments { get; set; }
    }
}
