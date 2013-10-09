using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Rantdriven.Patterns.ObjectPools
{
    [TestFixture]
    public class ConnectionPoolTests
    {
        private IResourcePool _pool;
        private Func<IResourcePool, Connection> _factory;

        [SetUp]
        public void SetUp()
        {
            var port = 8091;
            var address = "127.0.0.1";

            IPAddress ipAddress;
            if (!IPAddress.TryParse(address, out ipAddress))
            {
                throw new ArgumentException("endpoint");
            }

            var options = new ConnectionOptions
            {
                EndPoint = new IPEndPoint(ipAddress, port),
                OptionLevel = SocketOptionLevel.Socket,
                OptionName = SocketOptionName.KeepAlive,
                OptionValue = true,
                RecieveTimeout = 1000, //one second
                SendTimeout = 1000,
                ConnectionTimeout = 1000,
                DeadTimeout = 1000,
                UseNagle = true,
                MaxPoolSize = 20,
                MinPoolSize = 10
            };

            _factory = p =>
            {
                var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
                {
                    SendTimeout = options.SendTimeout,
                    ReceiveTimeout = options.RecieveTimeout,
                    NoDelay = options.UseNagle
                };

                socket.SetSocketOption(options.OptionLevel, options.OptionName, options.OptionValue);
                socket.Connect(options.EndPoint);
                return new Connection(p, socket);
            };

            var store = new QueueStore();
            _pool = new ConnectionPool(_factory, store, options);
        }

        [Test]
        public void TestAcquire()
        {
            using (var resource = _pool.Acquire())
            {
                Assert.IsNotNull(resource);
            }
        }

        [Test]
        public void TestRelease()
        {
            var resource = _pool.Acquire();
            _pool.Release(resource);
        }

        [Test]
        public void TestClose()
        {
            var resource = _pool.Acquire();
            _pool.Close(resource);
        }

        [Test]
        public void TestCloseFive()
        {
            for (int i = 0; i < 10; i++)
            {
                var resource = _pool.Acquire();
                _pool.Close(resource);
            }

            for (int i = 0; i < 20; i++)
            {
                using (_pool.Acquire())
                {
                    
                }
            }
        }

        [Test]
        public void TestAcquireEleven()
        {
            var resources = new List<IResource>();
            for (int i = 0; i < 18; i++)
            {
                using (var resource = _pool.Acquire())
                {
                    resources.Add(resource);
                }
            }
            Assert.AreEqual(resources.Count, 18);
        }

        [Test]
        public void TestThreaded()
        {
            for (int i = 0; i < 100; i++)
            {
                Task.Factory.StartNew(() =>
                {
                    if(i%4==0)Thread.Sleep(100);
                    using (var resource = _pool.Acquire())
                    {
                        Console.WriteLine("Not blocked!");
                    }
                });
            }
            Thread.Sleep(10000);
        }

        [TearDown]
        public void TearDown()
        {
            _pool.Dispose();
        }
    }
}
