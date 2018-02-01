using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickDeploy.Common.Messages
{
    [Serializable]
    public class ExecuteCommandResponse : BaseResponse
    {
        public int? ExitCode { get; set; }
    }
}
