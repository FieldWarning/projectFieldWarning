using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared
{
    /// <summary>
    /// Add is quick, delete is slow, lock is not guaranteed
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class LockList<T>
    {
        private List<T> Items = new List<T>();
        object CurrentLock = new object();

        public List<T> Get(Predicate<T> exp) => Items.FindAll(exp);
        public T Single(Predicate<T> exp) => Items.Find(exp);

        public void Add(IEnumerable<T> items)
        {
            lock (CurrentLock)
                Items.AddRange(items);
        }
        public void Add(T item)
        {
            lock (CurrentLock)
            {
                Items.Add(item);
            }
        }

        public void Delete(Predicate<T> exp)
        {
            lock (CurrentLock)
            {
                Items.RemoveAll(exp);
            }
        }
    }
}
