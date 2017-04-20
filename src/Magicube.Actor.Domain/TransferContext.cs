using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Magicube.Actor.Domain {
    [Serializable]
    public class TransferContext : IEnumerable {
        private readonly ConcurrentDictionary<string, object> _cacheDict;
        public TransferContext() {
            _cacheDict = new ConcurrentDictionary<string, object>();
        }

        public TransferContext TryAdd(string key, object value) {
            if (!_cacheDict.ContainsKey(key)) {
                if (value.GetType().IsSerializable && !IsActionDelegate(value.GetType())) {
                    _cacheDict.TryAdd(key, value);
                }
            }
            return this;
        }

        public T TryGet<T>(string key) {
            object defaultValue;
            if (_cacheDict.TryGetValue(key, out defaultValue)) {
                return (T)defaultValue;
            }
            return default(T);
        }

        public bool IsEmpty => _cacheDict.IsEmpty;

        public IEnumerator GetEnumerator() {
            return _cacheDict.GetEnumerator();
        }

        public IDictionary<string, object> ToDictionary() {
            return _cacheDict;
        }

        private bool IsActionDelegate(Type sourceType) {
            if (sourceType.IsSubclassOf(typeof(MulticastDelegate)) && sourceType.GetMethod("Invoke").ReturnType == typeof(void))
                return true;
            return false;
        }
    }
}