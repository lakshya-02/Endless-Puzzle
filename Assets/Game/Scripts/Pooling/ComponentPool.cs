using System.Collections.Generic;
using UnityEngine;

namespace EndlessPuzzle.Pooling
{
    public sealed class ComponentPool<T> where T : Component
    {
        private readonly Stack<T> _available;
        private readonly T _prototype;
        private readonly Transform _poolRoot;

        public ComponentPool(T prototype, int prewarmCount, Transform poolRoot)
        {
            _prototype = prototype;
            _poolRoot = poolRoot;
            _available = new Stack<T>(prewarmCount);

            Expand(prewarmCount);
        }

        public T Get()
        {
            if (_available.Count == 0)
            {
                Expand(1);
            }

            T instance = _available.Pop();
            instance.transform.SetParent(null, false);
            instance.gameObject.SetActive(true);
            return instance;
        }

        public void Release(T instance)
        {
            instance.gameObject.SetActive(false);
            instance.transform.SetParent(_poolRoot, false);
            _available.Push(instance);
        }

        private void Expand(int count)
        {
            for (int i = 0; i < count; i++)
            {
                T instance = Object.Instantiate(_prototype, _poolRoot);
                instance.gameObject.SetActive(false);
                _available.Push(instance);
            }
        }
    }
}
