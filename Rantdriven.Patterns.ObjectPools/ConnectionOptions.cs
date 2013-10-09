using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Rantdriven.Patterns.ObjectPools
{
    public class ConnectionOptions : IConnectionOptions
    {
        public System.Net.IPEndPoint EndPoint { get; set; }
        public int SendTimeout { get; set; }
        public int RecieveTimeout { get; set; }
        public SocketOptionLevel OptionLevel { get; set; }
        public SocketOptionName OptionName { get; set; }
        public bool OptionValue { get; set; }
        public bool UseNagle { get; set; }
        public int ConnectionTimeout { get; set; }
        public int DeadTimeout { get; set; }
        public int MaxPoolSize { get; set; }
        public int MinPoolSize { get; set; }
    }
}
