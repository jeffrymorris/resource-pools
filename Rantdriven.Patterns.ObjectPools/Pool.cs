using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Rantdriven.Patterns.ObjectPools
{
    public enum AccessMode
    {
        FIFO,
        LIFO,
        Circular
    };

    public enum LoadingMode
    {
        Lazy,
        Eager,
        LazyExpanding
    };

    public class Pool<T> : IDisposable where T : IDisposable
    {
        private readonly IITemStore _itemStore;
        private readonly int _size;
        private int _count;
        private readonly Func<Pool<T>, T> _factory;
        private readonly LoadingMode _loadingMode;
        private readonly Semaphore _syncObj;
        private bool _isDisposed;

        public Pool(int size, Func<Pool<T>, T> factory, LoadingMode loadingMode, AccessMode accessMode)
        {
            if(size <= 0) throw new ArgumentOutOfRangeException("size", "must be greater than 0");
            if(factory == null) throw new ArgumentNullException("factory");

            _size = size;
            _factory = factory;
            _syncObj = new Semaphore(_size, _size);
            _loadingMode = loadingMode;
            _itemStore = CreateITemStore(accessMode, _size);
           
            if (_loadingMode == LoadingMode.Eager)
            {
                PreloadItems();
            }
        }

        public T Acquire()
        {
            CheckDisposed();    
            T item;
            _syncObj.WaitOne();
            
            switch (_loadingMode)
            {
                case LoadingMode.Lazy:
                    item = AcquireLazy();
                    break;
                case LoadingMode.Eager:
                    item = AcquireEager();
                    break;
                case LoadingMode.LazyExpanding:
                    item = AcquireLazyExpanding();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return item;
        }

        public void Release(T item)
        {
            CheckDisposed();
            lock (_itemStore)
            {
                _itemStore.Release(item);
            }
            _syncObj.Release();
        }

        private void PreloadItems()
        {
            CheckDisposed();
            for (var i = 0; i < _size; i++)
            {
                T item = _factory(this);
                _itemStore.Release(item);
            }
            _count = _size;
        }

        private IITemStore CreateITemStore(AccessMode accessMode, int capacity)
        {
            CheckDisposed();
            IITemStore itemStore;
            switch (accessMode)
            {
                case AccessMode.FIFO:
                    itemStore = new QueueStore(capacity);
                    break;
                case AccessMode.LIFO:
                    itemStore = new StackStore(capacity);
                    break;
                case AccessMode.Circular:
                    itemStore = new CircularStore(capacity);
                    break;
                default:
                    throw new ArgumentOutOfRangeException("accessMode");
            }
            return itemStore;
        }

        private T AcquireEager()
        {
            CheckDisposed();
            lock (_itemStore)
            {
                return _itemStore.Acquire();
            }
        }

        private T AcquireLazy()
        {
            CheckDisposed();
            lock (_itemStore)
            {
                if (_itemStore.Count > 0)
                {
                    return _itemStore.Acquire();
                }
            }
            Interlocked.Increment(ref _count);
            return _factory(this);
        }

        private T AcquireLazyExpanding()
        {
            CheckDisposed();
            var shouldExpand = false;
            if (_count < _size)
            {
                var newCount = Interlocked.Increment(ref _count);
                if (newCount <= _size)
                {
                    shouldExpand = true;
                }
                else
                {
                    Interlocked.Decrement(ref _count);
                }
            }

            if (shouldExpand)
            {
                return _factory(this);
            }
                  
            lock (_itemStore)
            {
                return _itemStore.Acquire();
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                lock (_itemStore)
                {
                    while (_itemStore.Count > 0)
                    {
                        var item = _itemStore.Acquire();
                        item.Dispose();
                    }
                }
                _syncObj.Close();
            }
            if (disposing && !_isDisposed)
            {
                GC.SuppressFinalize(this);
                _isDisposed = true;
            }
        }

        ~Pool()
        {
            Dispose(false);
        }

        void CheckDisposed()
        {
            if (_isDisposed)
            {
                throw new ObjectDisposedException("pool");
            }
        }

        interface IITemStore
        {
            T Acquire();
            void Release(T item);
            int Count { get; }
        }

        class QueueStore : Queue<T>, IITemStore
        {
            public QueueStore(int capacity) 
                : base(capacity)
            {
            }

            public T Acquire()
            {
                return Dequeue();
            }

            public void Release(T item)
            {
               Enqueue(item);
            }
        }

        class StackStore : Stack<T>, IITemStore
        {
            public StackStore(int capacity) 
                : base(capacity)
            {
            }

            public T Acquire()
            {
                return Pop();
            }

            public void Release(T item)
            {
                Push(item);
            }
        }

        class CircularStore : IITemStore
        {
            private List<Slot> _slots;
            private int _freeSlotCount;
            private int _position = -1;

            public CircularStore(int capacity)
            {
                _slots = new List<Slot>(capacity);
            }
 
            public T Acquire()
            {
                if (Count == 0)
                {
                    throw new InvalidOperationException("The buffer is empty!");
                }

                var start = _position;
                do
                {
                    Advance();
                    var slot = _slots[_position];
                    if (!slot.IsUsed)
                    {
                        slot.IsUsed = true;
                        --_freeSlotCount;
                        return slot.Item;
                    }

                } while (start != _position);
                throw new InvalidOperationException("No free slots!");
            }

            void Advance()
            {
                _position = (_position + 1) % _slots.Count;
            }

            public void Release(T item)
            {
                var slot = _slots.Find(x => x.Item.Equals(item));
                if (slot == null)
                {
                    slot = new Slot(item);
                    _slots.Add(slot);
                }
                slot.IsUsed = false;
                ++_freeSlotCount;
            }

            public int Count
            {
                get { return _freeSlotCount; }
            }

            class Slot
            {
                public Slot(T item)
                {
                    Item = item;
                }
                
                public T Item { get; private set; }

                public bool IsUsed { get; set; }
            }
        }
    }
}
