using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rantdriven.Patterns.ObjectPools.Fakes
{
    public class FakeStoreStrategy : IStoreStrategy
    {
        public IResource Acquire()
        {
            throw new NotImplementedException();
        }

        public void Release(IResource resource)
        {
            throw new NotImplementedException();
        }

        public int Count
        {
            get { throw new NotImplementedException(); }
        }
    }
}
