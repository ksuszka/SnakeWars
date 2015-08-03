using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace SnakeWars.ContestRunner
{
    public class ViewersPool<TKey>
    {
        private readonly ConcurrentDictionary<TKey, Action<string>> _viewers =
            new ConcurrentDictionary<TKey, Action<string>>();

        public IEnumerable<Action<string>> CurrentViewers => _viewers.Values;

        public void Add(TKey key, Action<string> action)
        {
            _viewers.TryAdd(key, action);
        }

        public void Remove(TKey key)
        {
            Action<string> handler;
            _viewers.TryRemove(key, out handler);
        }
    }
}