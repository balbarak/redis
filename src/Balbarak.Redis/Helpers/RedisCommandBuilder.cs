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
            _fullCommand.Append($"*{_numberOfSegments}\r\n");
            _fullCommand.Append($"${_command.Length}\r\n");
            _fullCommand.Append($"{_command}\r\n");

            if (!string.IsNullOrWhiteSpace(_key))
            {
                var size = Encoding.UTF8.GetByteCount(_key);

                _fullCommand.Append($"${size}\r\n");

                _fullCommand.Append($"{_key}\r\n");
            }

            if (!string.IsNullOrWhiteSpace(_value))
            {
                var size = Encoding.UTF8.GetByteCount(_value);
                _fullCommand.Append($"${size}\r\n");
                _fullCommand.Append($"{_value}\r\n");
            }

            //if (_valueBytes != null)
            //{
            //    var base64 = Convert.ToBase64String(_valueBytes);
            //    _fullCommand.Append($"${base64.Length}\r\n");
            //    _fullCommand.Append($"{base64}\r\n");
            //}

            if (_valueBytes != null)
            {
                List<byte> result = new List<byte>();

                _fullCommand.Append($"${_valueBytes.Length}\r\n");

                var firstCmd = _fullCommand.ToString();

                result.AddRange(Encoding.UTF8.GetBytes(firstCmd));
                result.AddRange(_valueBytes);

                result.Add((byte)'\r');
                result.Add((byte)'\n');

                return result.ToArray();
                //_fullCommand.Append($"${base64.Length}\r\n");
                //_fullCommand.Append($"{base64}\r\n");
            }

            var cmd = _fullCommand.ToString();

            return Encoding.UTF8.GetBytes(cmd);
        }
    }
}
