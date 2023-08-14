using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Solitaire
{
    internal class Pile<T> : IList<T>
    {
        public Pile()
        {
            data = new T[0];
        }

        private T[] data;
        public T this[int index] { get => this.data[index]; set => this.data[index] = value; }

        public int Count => data.Length;
        public int Length => data.Length;

        public bool IsReadOnly => throw new NotImplementedException();

        public void Add(T item)
        {
            int newSize = this.Count;
            T[] array = new T[newSize];
            Array.Copy(data, array, newSize);
        }

        public void Clear()
        {
            throw new NotImplementedException();
        }

        public bool Contains(T item)
        {
            throw new NotImplementedException();
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public IEnumerator<T> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        public int IndexOf(T item)
        {
            throw new NotImplementedException();
        }

        public void Insert(int index, T item)
        {
            throw new NotImplementedException();
        }

        public bool Remove(T item)
        {
            throw new NotImplementedException();
        }

        public void RemoveAt(int index)
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }
}
