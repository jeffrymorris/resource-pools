using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Rantdriven.Patterns.ObjectPools.Stubs;

namespace Rantdriven.Patterns.ObjectPools
{
    [TestFixture]
    public class QueueStoreTests
    {
        private IStoreStrategy _store;

        [SetUp]
        public void SetUp()
        {
            _store = new QueueStore();
        }

        [Test]
        public void TestRelease()
        {
            var resource = new FakeResource();
            Assert.AreEqual(0, _store.Count);
            _store.Release(resource);
            Assert.AreEqual(1, _store.Count);
        }

        [Test]
        public void TestAcquire()
        {
            Assert.AreEqual(1, _store.Count);
            var resource = _store.Acquire();
            Assert.AreEqual(0, _store.Count);
            Assert.IsNotNull(resource);
        }
    }
}
