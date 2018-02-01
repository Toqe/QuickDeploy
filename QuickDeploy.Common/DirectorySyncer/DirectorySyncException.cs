using System;

namespace QuickDeploy.Common.DirectorySyncer
{
    public class DirectorySyncException : Exception
    {
        public DirectorySyncException(string message) : base(message)
        {
        }
    }
}
