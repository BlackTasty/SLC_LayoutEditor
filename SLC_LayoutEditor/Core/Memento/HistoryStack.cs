using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SLC_LayoutEditor.Core.Memento
{
    class HistoryStack<T>
    {
        private readonly List<T> stack;
        private int capacity = 30;

        public int Capacity { get => capacity; set => capacity = value; }

        public HistoryStack() : this(-1) { }

        public HistoryStack(int capacity) : base()
        {
            if (capacity > -1)
            {
                stack = new List<T>(capacity);
                this.capacity = capacity;
            }
            else
            {
                stack = new List<T>();
            }
        }

        public int Count => stack.Count;

        public void Clear()
        {
            stack.Clear();
        }

        public bool Contains(T item)
        {
            return stack.Contains(item);
        }

        public T Peek()
        {
            return stack.FirstOrDefault();
        }

        public T Pop()
        {
            T popped = Peek();

            if (popped != null)
            {
                stack.RemoveAt(0);
            }

            return popped;
        }

        public void Push(T item)
        {
            if (item == null)
            {
                return;
            }

            stack.Insert(0, item);
            TrimExcess();
        }

        public void TrimExcess()
        {
            if (Count > capacity)
            {
                stack.RemoveRange(capacity, Count - capacity);
            }
        }
    }
}
