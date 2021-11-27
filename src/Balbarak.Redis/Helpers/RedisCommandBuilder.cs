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

        public byte[] Build()
        {
            _fullCommand.Append($"*{_numberOfSegments}\r\n");

            _fullCommand.Append($"${_command.Length}\r\n");
            _fullCommand.Append($"{_command}\r\n");

            if (!string.IsNullOrWhiteSpace(_key))
            {
                _fullCommand.Append($"${_key.Length}\r\n");
                _fullCommand.Append($"{_key}\r\n");
            }

            if (!string.IsNullOrWhiteSpace(_value))
            {
                _fullCommand.Append($"${_value.Length}\r\n");
                _fullCommand.Append($"{_value}\r\n");
            }

            var cmd = _fullCommand.ToString();

            return Encoding.UTF8.GetBytes(cmd);
        }
    }
}
