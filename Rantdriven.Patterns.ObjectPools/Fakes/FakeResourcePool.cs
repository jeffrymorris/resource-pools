using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rantdriven.Patterns.ObjectPools.Fakes
{
    public class FakeResourcePool : IResourcePool
    {
        private IStoreStrategy _storeStrategy;
        private readonly Func<IResourcePool, IResource> _factory;

        public FakeResourcePool(Func<IResourcePool, IResource> factory, IStoreStrategy storeStrategy)
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
