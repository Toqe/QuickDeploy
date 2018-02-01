using System;

namespace QuickDeploy.Common.FileFinder
{
    [Serializable]
    public class FileFindFile
    {
        public string FileName { get; set; }

        public DateTime ChangeTimestamp { get; set; }

        public string Md5 { get; set; }
    }
}
