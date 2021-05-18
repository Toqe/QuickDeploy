using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickDeploy.Common
{
    public class ProxyException : Exception
    {
        public ProxyException(string message)
            : base(message)
        {
        }
    }
}
