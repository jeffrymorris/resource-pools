using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Rantdriven.Patterns.ObjectPools
{
    [TestFixture]
    public class ConnectionPoolTests
    {
        private IResourcePool _pool;

        [SetUp]
        public void SetUp()
        {
            var factory = new Func<IResourcePool, Connection>(x => null);
            var store = new QueueStore();
            _pool = new ConnectionPool(factory, store);
        }

        [Test]
        public void TestAcquire()
        {
            var connection = _pool.Acquire();
            Assert.IsNotNull(connection);
        }

        [TearDown]
        public void TearDown()
        {
            _pool.Dispose();
        }
    }
}
