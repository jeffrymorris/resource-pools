using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rantdriven.Patterns.ObjectPools
{
    public class QueueStore : Queue<IResource>,IStoreStrategy
    {
        public IResource Acquire()
        {
            return Dequeue();
        }

        public void Release(IResource resource)
        {
            Enqueue(resource);
        }
    }
}
