namespace Supercell.Magic.Titan.Util
{
    using System;

    public class LogicArrayList<T>
    {
        private T[] m_items;
        private int m_size;

        public LogicArrayList()
        {
            this.m_items = new T[0];
        }

        public LogicArrayList(int initialCapacity)
        {
            this.m_items = new T[initialCapacity];
        }

        public T this[int index]
        {
            get
            {
                return this.m_items[index];
            }
            set
            {
                this.m_items[index] = value;
            }
        }

        public void Add(T item)
        {
            int size = this.m_items.Length;

            if (size == this.m_size)
            {
                this.EnsureCapacity(size != 0 ? size * 2 : 5);
            }

            this.m_items[this.m_size++] = item;
        }

        public void Add(int index, T item)
        {
            int size = this.m_items.Length;

            if (size == this.m_size)
            {
                this.EnsureCapacity(size != 0 ? size * 2 : 5);
            }

            if (this.m_size > index)
            {
                Array.Copy(this.m_items, index, this.m_items, index + 1, this.m_size - index);
            }

            this.m_items[index] = item;
            this.m_size += 1;
        }

        public void AddAll(LogicArrayList<T> array)
        {
            this.EnsureCapacity(this.m_size + array.m_size);

            for (int i = 0, cnt = array.m_size; i < cnt; i++)
            {
                this.m_items[this.m_size++] = array[i];
            }
        }

        public int IndexOf(T item)
        {
            return Array.IndexOf(this.m_items, item, 0, this.m_size);
        }

        public void Remove(int index)
        {
            if ((uint) index < this.m_size)
            {
                this.m_size -= 1;

                if (index != this.m_size)
                {
                    Array.Copy(this.m_items, index + 1, this.m_items, index, this.m_size - index);
                }
            }
        }

        public void EnsureCapacity(int count)
        {
            int size = this.m_items.Length;

            if (size < count)
            {
                Array.Resize(ref this.m_items, count);
            }
        }

        public int Size()
        {
            return this.m_size;
        }

        public void Clear()
        {
            this.m_size = 0;
        }

        public void Destruct()
        {
            this.m_items = null;
            this.m_size = 0;
        }
    }
}