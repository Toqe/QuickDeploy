using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickDeploy.Common.Messages
{
    [Serializable]
    public class ProxyResponse : BaseResponse
    {
        public BaseResponse Response { get; set; }
    }
}
