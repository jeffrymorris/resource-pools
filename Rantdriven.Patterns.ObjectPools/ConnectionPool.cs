using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Rantdriven.Patterns.ObjectPools
{
    public class ConnectionPool : IResourcePool
    {
        private readonly IStoreStrategy _storeStrategy;
        private readonly Func<IResourcePool, IResource> _factory;
        private readonly SemaphoreSlim _syncObj;
        private readonly IConnectionOptions _options;
        private bool _isDisposed;
        private int _count;

        public ConnectionPool(Func<IResourcePool, IResource> factory, IStoreStrategy storeStrategy, IConnectionOptions options)
        {
            _storeStrategy = storeStrategy;
            _factory = factory;
            _syncObj = new SemaphoreSlim(0, options.MaxPoolSize);
            _options = options;
            Initialize();
        }

        void Initialize()
        {
            CheckDisposed();
            lock (_storeStrategy)
            {
                for (int i = 0; i < _options.MinPoolSize; i++)
                {
                    var resource = _factory(this);
                    _storeStrategy.Release(resource);
                }
            }
            _count = _options.MinPoolSize;
            _syncObj.Release();
        }

        public IResource Acquire()
        {
            Console.WriteLine(this);
            CheckDisposed();
            _syncObj.Wait();

            Interlocked.Increment(ref _count);
            lock (_storeStrategy)
            {
                if (_storeStrategy.Count > 0)
                {
                    return _storeStrategy.Acquire();
                }
            }
            return _factory(this);
        }

        //TODO should release close connections that go over minPoolSize when they are released?
        public void Release(IResource resource)
        {
            CheckDisposed();
            lock (_storeStrategy)
            {
                _storeStrategy.Release(resource);
            }
            Interlocked.Decrement(ref _count);
            Console.WriteLine("Releaseing {0}", _syncObj.Release());
            Console.WriteLine(this);
        }

        //TODO this is a leaky abstraction but required for semaphore management
        public void Close(IResource resource)
        {
            resource.Close();
            _syncObj.Release();
        }

        void CheckDisposed()
        {
            if (_isDisposed)
            {
                throw new ObjectDisposedException("pool");
            }
        }

        ~ConnectionPool()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
        }

        void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                lock (_storeStrategy)
                {
                    while (_storeStrategy.Count > 0)
                    {
                        var item = _storeStrategy.Acquire();
                        item.Close();
                    }
                }
                _syncObj.Dispose();
            }
            if (disposing && !_isDisposed)
            {
                GC.SuppressFinalize(this);
                _isDisposed = true;
            }
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendFormat("In {0}()", GetCurrentMethod());
            sb.AppendFormat("Reference count {0}", _count);
            sb.AppendFormat("Queue count {0}", _storeStrategy.Count);
            return sb.ToString();
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public string GetCurrentMethod()
        {
            var stackTrace = new StackTrace();
            var stackFrame = stackTrace.GetFrame(1);
            return stackFrame.GetMethod().Name;
        }
    }
}
