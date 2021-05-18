using System;
using QuickDeploy.Common.Messages;

namespace QuickDeploy.Common
{
    public class StatusMessageSender
    {
        private readonly Action<StatusMessage> sendStatusMessageCallback;

        public StatusMessageSender(Action<StatusMessage> sendStatusMessageCallback)
        {
            this.sendStatusMessageCallback = sendStatusMessageCallback;
        }

        public void SendInfo(string text)
        {
            var statusMessage = new StatusMessage { Type = StatusMessageType.Info, Text = text };
            this.sendStatusMessageCallback(statusMessage);
        }

        public void SendError(string text)
        {
            var statusMessage = new StatusMessage { Type = StatusMessageType.Error, Text = text };
            this.sendStatusMessageCallback(statusMessage);
        }
    }
}
