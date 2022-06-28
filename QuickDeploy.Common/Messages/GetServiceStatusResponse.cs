using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickDeploy.Common.Messages
{
    [Serializable]
    public class GetServiceStatusResponse : BaseResponse
    {
        public string ServiceName { get; set; }

        public bool ServiceExists { get; set; }

        public bool IsRunning { get; set; }
    }
}
