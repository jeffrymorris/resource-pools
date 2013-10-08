using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Rantdriven.Patterns.ObjectPools
{
    [TestFixture]
    public class PoolTest
    {
        private Pool<Connection> _pool;
        private new Func<Pool<Connection>, Connection> _factory;

        [SetUp]
        public void SetUp()
        {
            var port = 8091;
            var address = "127.0.0.1";
            var recieveTimeout = 1000;
            var sendTimeout = 1000;

            _factory = new Func<Pool<Connection>, Connection>((p) =>
            {
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
                    RecieveTimeout = recieveTimeout,//one second
                    SendTimeout = sendTimeout,
                    UseNagle = true
                };

                var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
                {
                    SendTimeout = options.SendTimeout,
                    ReceiveTimeout = options.RecieveTimeout,
                    NoDelay = options.UseNagle
                };

                socket.SetSocketOption(options.OptionLevel, options.OptionName, options.OptionValue);
                socket.Connect(options.EndPoint);
                return new Connection(p, socket);
            });
        }

        [Test]
        public void TestConstruct()
        {
            _pool = new Pool<Connection>(10, _factory, LoadingMode.Lazy, AccessMode.FIFO);
            Assert.IsNotNull(_pool);
        }

        [Test]
        public void TestAcquire()
        {
            _pool = new Pool<Connection>(10, _factory, LoadingMode.Lazy, AccessMode.FIFO);
            var connection = _pool.Acquire();
            Assert.IsNotNull(connection);
        }

        [Test]
        public void TestRelease()
        {
            _pool = new Pool<Connection>(10, _factory, LoadingMode.Lazy, AccessMode.FIFO);
            var connection = _pool.Acquire();
            Assert.IsNotNull(connection);
            _pool.Release(connection);
        }

        [Test]
        public void Test_That_Twenty_Items_Are_Acquired()
        {
            var count = 0;
            _pool = new Pool<Connection>(10, _factory, LoadingMode.LazyExpanding, AccessMode.FIFO);
            for (int i = 0; i < 20; i++)
            {
                var connection = _pool.Acquire();
                if (connection != null)
                {
                    count++;
                }
                //_pool.Release(connection);
            }
            Assert.AreEqual(20, count);
        }

        [TearDown]
        public void TearDown()
        {
            _pool.Dispose();  
        }
    }
}
