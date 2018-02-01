using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QuickDeploy.Common.FileFinder;

namespace QuickDeploy.Common.Messages
{
    [Serializable]
    public class AnalyzeDirectoryResponse : BaseResponse
    {
        public FileFindResult Contents { get; set; }
    }
}
