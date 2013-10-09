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
        private IResourcePool _pool;
        private Socket _socket;
        private bool _disposed;
        private bool _isUsed;

        public Connection(IResourcePool pool, Socket socket)
        {
            _pool = pool;
            _socket = socket;
        }

        /// <summary>
        /// Releases the resource back into the pool
        /// </summary>
        public void Dispose()
        {
            _pool.Release(this);
        }

        ~Connection()
        {
            Close(false);
        }

        void Close(bool disposing)
        {
            if (disposing && !_disposed)
            {
                GC.SuppressFinalize(this);
            }
 
            _socket.Close();
            _socket.Dispose();
            _disposed = true;
        }

        /// <summary>
        /// Closes the connection and removes the resource from the pool
        /// </summary>
        public void Close()
        {
           Close(true);
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
