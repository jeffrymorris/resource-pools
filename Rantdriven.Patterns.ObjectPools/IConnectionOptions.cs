using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Rantdriven.Patterns.ObjectPools
{
    public interface IConnectionOptions
    {
        IPEndPoint EndPoint { get; set; }
        int SendTimeout { get; set; }
        int RecieveTimeout { get; set; }
        SocketOptionLevel OptionLevel { get; set; }
        SocketOptionName OptionName { get; set; }
        bool OptionValue { get; set; }
        bool UseNagle { get; set; }
    }
}
