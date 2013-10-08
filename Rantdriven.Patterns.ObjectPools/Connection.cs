using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Rantdriven.Patterns.ObjectPools
{
    public class Connection : IResource
    {
        private Pool<Connection> _pool;
        private Socket _socket;
        private bool _disposed;
        private bool _isUsed;

        public Connection(Pool<Connection> pool, Socket socket)
        {
            _pool = pool;
            _socket = socket;
        }

        public void Dispose()
        {
            Dispose(true);
        }

        void Dispose(bool disposing)
        {
            if (disposing && !_disposed)
            {
                GC.SuppressFinalize(this);
            }
            _socket.Dispose();
            _disposed = true;
        }

        ~Connection()
        {
            Dispose(false);
        }

        void CheckDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException("connection");
            }
        }
    }
}
