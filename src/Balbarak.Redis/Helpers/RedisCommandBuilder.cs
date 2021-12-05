using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Balbarak.Redis
{
    internal class RedisCommandBuilder
    {
        private string _command;
        private string _key;
        private List<string> _values;
        private byte[] _valueBytes;
        private int _numberOfSegments = 1;
        private TimeSpan? _expire = null;

        public RedisCommandBuilder(string command)
        {
            _command = command;
            _values = new List<string>();
        }

        public RedisCommandBuilder WithKey(string key)
        {
            _key = key;

            _numberOfSegments++;

            return this;
        }

        public RedisCommandBuilder WithValue(string value)
        {
            _values.Add(value);

            _numberOfSegments++;

            return this;
        }

        public RedisCommandBuilder WithValue(byte[] value)
        {
            _valueBytes = value;
            _numberOfSegments++;

            return this;
        }

        public RedisCommandBuilder WithExpiration(TimeSpan expire)
        {
            _expire = expire;
            _numberOfSegments +=2;

            return this;
        }

        public byte[] Build()
        {
            byte[] result = null;

            using (MemoryStream ms = new MemoryStream())
            {
                var command = Encoding.ASCII.GetBytes($"*{_numberOfSegments}\r\n${_command.Length}\r\n{_command}\r\n");

                ms.Write(command, 0, command.Length);

                if (!string.IsNullOrWhiteSpace(_key))
                {
                    var keySize = Encoding.UTF8.GetByteCount(_key);

                    var keyData = Encoding.UTF8.GetBytes($"${keySize}\r\n{_key}\r\n");

                    ms.Write(keyData, 0, keyData.Length);
                }

                if (_values.Count > 0)
                {
                    foreach (var value in _values)
                    {
                        var valueSize = Encoding.UTF8.GetByteCount(value);

                        var valueData = Encoding.UTF8.GetBytes($"${valueSize}\r\n{value}\r\n");

                        ms.Write(valueData, 0, valueData.Length);
                    }
                    
                }

                if (_valueBytes != null && _valueBytes.Length > 0)
                {
                    var sizeData = Encoding.ASCII.GetBytes($"${_valueBytes.Length}\r\n");

                    ms.Write(sizeData, 0, sizeData.Length);

                    ms.Write(_valueBytes, 0, _valueBytes.Length);

                    ms.WriteByte((byte)'\r');
                    ms.WriteByte((byte)'\n');
                }

                if (_expire != null)
                {
                    var totalSeconds = ((int)_expire.Value.TotalSeconds).ToString();

                    var totalSecondsSize = Encoding.ASCII.GetByteCount(totalSeconds);

                    var expireData = Encoding.ASCII.GetBytes($"$2\r\nEX\r\n${totalSecondsSize}\r\n{totalSeconds}\r\n");

                    ms.Write(expireData, 0, expireData.Length);
                }

                result = ms.ToArray();
            }

            return result;
        }
    }
}
