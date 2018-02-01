using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace QuickDeploy.Common
{
    public class StreamHelper
    {
        private readonly BinaryFormatter binaryFormatter = new BinaryFormatter();

        private readonly int intLength = BitConverter.GetBytes(0).Length;

        public void Send(Stream stream, object o)
        {
            var serialized = this.Serialize(o);
            var lengthSerialized = BitConverter.GetBytes(serialized.Length);
            stream.Write(lengthSerialized, 0, lengthSerialized.Length);
            stream.Write(serialized, 0, serialized.Length);
            stream.Flush();
        }

        public object Receive(Stream stream)
        {
            var lengthSerialized = new byte[this.intLength];
            this.ReadToBuffer(stream, lengthSerialized, 0, this.intLength);
            var length = BitConverter.ToInt32(lengthSerialized, 0);
            var serialized = new byte[length];
            this.ReadToBuffer(stream, serialized, 0, length);
            return this.Deserialize(serialized);
        }

        private void ReadToBuffer(Stream stream, byte[] buffer, int start, int length)
        {
            int dataRead = 0;

            do
            {
                dataRead += stream.Read(buffer, start + dataRead, length - dataRead);
            }
            while (dataRead < length);
        }

        private byte[] Serialize(object o)
        {
            using (var memoryStream = new MemoryStream())
            {
                this.binaryFormatter.Serialize(memoryStream, new Message { Data = o });
                return memoryStream.ToArray();
            }
        }

        private object Deserialize(byte[] data)
        {
            using (var memoryStream = new MemoryStream(data))
            {
                var message = this.binaryFormatter.Deserialize(memoryStream) as Message;
                return message?.Data;
            }
        }

        [Serializable]
        public class Message
        {
            public object Data { get; set; }
        }
    }
}
