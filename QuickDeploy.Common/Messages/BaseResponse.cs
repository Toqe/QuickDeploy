﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickDeploy.Common.Messages
{
    [Serializable]
    public class BaseResponse
    {
        public bool Success { get; set; }

        public string ErrorMessage { get; set; }
    }
}
