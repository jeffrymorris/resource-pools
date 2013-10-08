using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Rantdriven.Patterns.ObjectPools.Fakes;
using Rantdriven.Patterns.ObjectPools.Stubs;

namespace Rantdriven.Patterns.ObjectPools
{
    [TestFixture]
    public class FakeResourcePoolTests
    {
        private IResourcePool _pool;

        [Test]
        public void Test_Construction()
        {
            var factory = new Func<IResourcePool, IResource>(x => new FakeResource());
            var store = new FakeStoreStrategy();
            _pool = new FakeResourcePool(factory, store);
        }
    }
}
