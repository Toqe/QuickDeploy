using System;

namespace QuickDeploy.Common.Process
{
    public class ProcessLogEntry
    {
        public ProcessLogType Type { get; set; }

        public DateTime Timestamp { get; set; }

        public string Text { get; set; }

        public override string ToString()
        {
            return $"[{this.Type} {this.Timestamp}] {this.Text}";
        }
    }
}
