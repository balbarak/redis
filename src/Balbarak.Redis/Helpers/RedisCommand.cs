using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Balbarak.Redis
{
    internal class RedisCommand
    {
        public string Command { get; private set; }

        public string Arguments { get; private set; }

        public RedisCommand(string command,string args)
        {
            Command = command;
            Arguments = args;
        }

        public byte[] GetBytesToSend()
        {
            return Encoding.UTF8.GetBytes(Command);
        }
    }
}
