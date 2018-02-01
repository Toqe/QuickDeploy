using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickDeploy.Common.Messages
{
    [Serializable]
    public class Credentials
    {
        public string Domain { get; set; }

        public string Username { get; set; }

        public string Password { get; set; }
    }
}
