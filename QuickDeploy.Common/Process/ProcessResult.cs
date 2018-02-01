using System;
using System.Collections.Generic;
using System.Text;

namespace QuickDeploy.Common.Process
{
    public class ProcessResult
    {
        private object lockObject = new object();

        public int? ExitCode { get; set; }

        public Exception Exception { get; set; }

        public List<ProcessLogEntry> Logs { get; set; } = new List<ProcessLogEntry>();

        public ProcessLogEntry Log(ProcessLogType type, string text = null)
        {
            lock (this.lockObject)
            {
                var logEntry = new ProcessLogEntry { Type = type, Text = text, Timestamp = DateTime.UtcNow };
                this.Logs.Add(logEntry);
                return logEntry;
            }
        }

        public override string ToString()
        {
            var sb = new StringBuilder();

            foreach (var log in this.Logs)
            {
                sb.AppendLine(log.ToString());
            }

            return sb.ToString();
        }
    }
}
