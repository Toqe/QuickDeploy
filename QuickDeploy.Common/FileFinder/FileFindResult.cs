using System;
using System.Collections.Generic;

namespace QuickDeploy.Common.FileFinder
{
    [Serializable]
    public class FileFindResult
    {
        public List<FileFindFile> Files { get; set; } = new List<FileFindFile>();

        public List<FileFindDirectory> Directories { get; set; } = new List<FileFindDirectory>();
    }
}
