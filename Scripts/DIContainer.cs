using System;
using System.Collections.Generic;

namespace BaseCon
{
    public class DIContainer : IDisposable
    {
        private readonly DIContainer _parentContainer;
        private readonly Dictionary<(string, Type), DIEntry> _entriesMap = new();
        private readonly HashSet<(string, Type)> _resolutionsCache = new();
        
        public DIContainer(DIContainer parentContainer = null)
        {
            _parentContainer = parentContainer;
        }

        #region Registration Methods
        public DIEntry RegiserFactory<T>(Func<DIContainer, T> factory)
        {
            return RegiserFactory<T>(null, factory);
        }
        
        public DIEntry RegiserFactory<T>(string tag, Func<DIContainer, T> factory)
        {
            var key = (tag, typeof(T));
            if (_entriesMap.ContainsKey(key))
            {
                throw new Exception(
                    $"DI: Factory with tag {key.Item1} and type {key.Item2.FullName} has already registered");
            }

            var diEntry = new DIEntry<T>(this, factory);

            _entriesMap[key] = diEntry;

            return diEntry;
        }
        
        public void RegisterInstance<T>(T instance)
        {
            RegisterInstance(null, instance);
        }

        public void RegisterInstance<T>(string tag, T instance)
        {
            var key = (tag, typeof(T));

            if (_entriesMap.ContainsKey(key))
                throw new Exception($"DI: Instance with tag {key.Item1} and type {key.Item2.FullName} has already registered");

            var diEntry = new DIEntry<T>(instance);
            _entriesMap[key] = diEntry;
        }
        #endregion

        #region Fetch Methods

        public T Resolve<T>(string tag = null)
        {
            var key = (tag, typeof(T));

            if (!_resolutionsCache.Add(key))
                throw new Exception($"[DIContainer] Circular dependency detected while resolving type {key.Item2.FullName} with tag '{key.tag}'.");

            try
            {
                if (_entriesMap.TryGetValue(key, out var diEntry))
                    return diEntry.Resolve<T>();
                else if (_parentContainer != null)
                    return _parentContainer.Resolve<T>(tag);
            }
            finally
            {
                _resolutionsCache.Remove(key);
            }
            throw new Exception($"[DIContainer] No registration for type {typeof(T)} with tag '{tag}' found.");
        }
        #endregion

        public void Dispose()
        {
            var entries = _entriesMap.Values;
            foreach (var entry in entries)
                entry.Dispose();
        }
    }
}
