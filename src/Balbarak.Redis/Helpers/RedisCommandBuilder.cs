using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Balbarak.Redis
{
    internal class RedisCommandBuilder
    {
        private StringBuilder _fullCommand;
        private string _command;
        private string _key;
        private string _value;
        private byte[] _valueBytes;
        private int _numberOfSegments = 1;

        public RedisCommandBuilder(string command)
        {
            _fullCommand = new StringBuilder();
            _command = command;
        }

        public RedisCommandBuilder WithKey(string key)
        {
            _key = key;

            _numberOfSegments++;

            return this;
        }

        public RedisCommandBuilder WithValue(string value)
        {
            _value = value;
            _numberOfSegments++;

            return this;
        }

        public RedisCommandBuilder WithValue(byte[] value)
        {
            _valueBytes = value;
            _numberOfSegments++;

            return this;
        }

        public byte[] Build()
        {
            byte[] result = null;

            using (MemoryStream ms = new MemoryStream())
            {
                var command = Encoding.ASCII.GetBytes($"*{_numberOfSegments}\r\n${_command.Length}\r\n{_command}\r\n");

                ms.Write(command,0,command.Length);

                if (!string.IsNullOrWhiteSpace(_key))
                {
                    var keySize = Encoding.UTF8.GetByteCount(_key);

                    var keyData = Encoding.UTF8.GetBytes($"${keySize}\r\n{_key}\r\n");

                    ms.Write(keyData,0,keyData.Length);
                }

                if (!string.IsNullOrWhiteSpace(_value))
                {
                    var valueSize = Encoding.UTF8.GetByteCount(_value);

                    var valueData = Encoding.UTF8.GetBytes($"${valueSize}\r\n{_value}\r\n");

                    ms.Write(valueData,0,valueData.Length);
                }

                if (_valueBytes != null && _valueBytes.Length > 0)
                {
                    var sizeData = Encoding.ASCII.GetBytes($"${_valueBytes.Length}\r\n");

                    ms.Write(sizeData,0,sizeData.Length);

                    ms.Write(_valueBytes,0,_valueBytes.Length);

                    ms.WriteByte((byte)'\r');
                    ms.WriteByte((byte)'\n');
                }

                result = ms.ToArray();
            }

            return result;
        }
    }
}
