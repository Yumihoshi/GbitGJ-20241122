using System;
using System.Collections.Generic;

namespace GCFramework.Utility.ObjectPool
{
    public class EasyObjPool<T>
    {
        private readonly Queue<T> _queue;
        private readonly Func<T> _onCreate;
        private readonly Action<T> _onReturn;
        private readonly Action<T> _onRelease;

        private int _maxSize;
        private int _usingCount;

        public EasyObjPool(Func<T> onCreate = null, Action<T> onReturn = null, Action<T> onRelease = null, int size = 50)
        {
            _queue = new Queue<T>(size);
            _maxSize = size;

            _onCreate = onCreate;
            _onReturn = onReturn;
            _onRelease = onRelease;
        }

        public T Get()
        {
            T obj;
            if (_queue.Count == 0)
            {
                if (_onCreate != null)
                {
                    obj = _onCreate();
                }
                else
                {
                    obj = Activator.CreateInstance<T>();
                }
            }
            else
            {
                obj = _queue.Dequeue();
            }

            _usingCount++;
            return obj;
        }

        public void Return(T obj)
        {
            if (_onReturn != null)
                _onReturn(obj);

            if (_queue.Count < _maxSize)
                _queue.Enqueue(obj);
            else
            {
                if (_onRelease != null)
                    _onRelease(obj);
            }

            _usingCount--;
        }

        public void Release()
        {
            while (_queue.Count > 0)
            {
                var obj = _queue.Dequeue();
                if (_onRelease != null)
                    _onRelease(obj);
            }
        }

        public override string ToString()
        {
            return $"EasyObjPool: objType:{typeof(T)}, usingCount:{_usingCount}, maxSize:{_maxSize}, usingPercent:{_usingCount/_maxSize}";
        }
    }
}