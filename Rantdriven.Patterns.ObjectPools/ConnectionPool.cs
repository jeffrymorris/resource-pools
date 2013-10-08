using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Rantdriven.Patterns.ObjectPools
{
    public class ConnectionPool : IResourcePool
    {
        private readonly IStoreStrategy _storeStrategy;
        private readonly int _size;
        private int _count;
        private readonly Func<IResourcePool, IResource> _factory;
        private readonly LoadingMode _loadingMode;
        private readonly Semaphore _syncObj;
        private bool _isDisposed;

        public ConnectionPool(Func<IResourcePool, IResource> factory, IStoreStrategy storeStrategy)
        {
            _storeStrategy = storeStrategy;
            _factory = factory;
        }

        public IConnection Acquire()
        {
            throw new NotImplementedException();
        }

        public void Release(IResource resource)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
