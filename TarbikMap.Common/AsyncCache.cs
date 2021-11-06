namespace TarbikMap.Common
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    public class AsyncCache<T>
    {
        private const int Capacity = 100;

        private readonly Dictionary<string, KeyUsage> keyUsages = new Dictionary<string, KeyUsage>();

        private readonly ConcurrentDictionary<string, SemaphoreSlim> keyLocksForWaiting = new ConcurrentDictionary<string, SemaphoreSlim>();
        private readonly ConcurrentDictionary<string, T> cache = new ConcurrentDictionary<string, T>();

        public async Task<T> Get(string key, Func<Task<T>> creator, bool onlyTryToAddToCache)
        {
            if (creator == null)
            {
                throw new ArgumentNullException(nameof(creator));
            }

            lock (this.keyUsages)
            {
                KeyUsage keyUsage;

                if (!this.keyUsages.TryGetValue(key, out keyUsage))
                {
                    keyUsage = new KeyUsage();
                    this.keyUsages.Add(key, keyUsage);
                }

                ++keyUsage.Count;
                keyUsage.LastAccess = DateTime.UtcNow;

                this.CleanupInLock();
            }

            try
            {
                var keyLock = this.keyLocksForWaiting.GetOrAdd(key, x => new SemaphoreSlim(1));

                if (onlyTryToAddToCache)
                {
                    if (!await keyLock.WaitAsync(0).ConfigureAwait(false))
                    {
                        return await creator().ConfigureAwait(false);
                    }
                }
                else
                {
                    await keyLock.WaitAsync().ConfigureAwait(false);
                }

                try
                {
                    T value;

                    if (!this.cache.TryGetValue(key, out value))
                    {
                        value = await creator().ConfigureAwait(false);

                        this.cache.TryAdd(key, value);
                    }

                    return value;
                }
                finally
                {
                    keyLock.Release();
                }
            }
            finally
            {
                lock (this.keyUsages)
                {
                    --this.keyUsages[key].Count;
                }
            }
        }

        private void CleanupInLock()
        {
            if (this.keyUsages.Count <= Capacity)
            {
                return;
            }

            int toRemove = this.keyUsages.Count - Capacity;

            var keysToRemove = this.keyUsages.Where(kv => kv.Value.Count == 0).OrderBy(kv => kv.Value.LastAccess).Take(toRemove).Select(kv => kv.Key);
            foreach (string key in keysToRemove)
            {
                this.keyUsages.Remove(key);

                SemaphoreSlim semaphore;
                if (this.keyLocksForWaiting.TryRemove(key, out semaphore))
                {
                    semaphore.Dispose();
                }

                this.cache.TryRemove(key, out T unused);
            }
        }

        private class KeyUsage
        {
            public uint Count { get; set; }

            public DateTime LastAccess { get; set; }
        }
    }
}