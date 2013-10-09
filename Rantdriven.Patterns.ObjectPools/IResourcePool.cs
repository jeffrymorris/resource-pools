using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rantdriven.Patterns.ObjectPools
{
    public interface IResourcePool : IDisposable
    {
        IResource Acquire();
        void Release(IResource resource);
        void Close(IResource resource);
    }
}
