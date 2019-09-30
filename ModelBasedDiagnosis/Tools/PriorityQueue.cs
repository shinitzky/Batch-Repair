using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelBasedDiagnosis
{
    class PriorityQueue<T>
    {
        int total_size;
        SortedDictionary<double, Queue<T>> storage;

        public PriorityQueue()
        {
            this.storage = new SortedDictionary<double, Queue<T>>();
            this.total_size = 0;
        }

        public bool IsEmpty()
        {
            return (total_size == 0);
        }

        public object Dequeue()
        {
            if (IsEmpty())
            {
                throw new Exception("Please check that priorityQueue is not empty before dequeing");
            }
            else
                foreach (Queue<T> q in storage.Values)
                {
                    // we use a sorted dictionary
                    if (q.Count > 0)
                    {
                        total_size--;
                        return q.Dequeue();
                    }
                }

            //Debug.Assert(false, "not supposed to reach here. problem with changing total_size");

            return null; // not supposed to reach here.
        }

        // same as above, except for peek.

        public object Peek()
        {
            if (IsEmpty())
                throw new Exception("Please check that priorityQueue is not empty before peeking");
            else
                foreach (Queue<T> q in storage.Values)
                {
                    if (q.Count > 0)
                        return q.Peek();
                }

          //  Debug.Assert(false, "not supposed to reach here. problem with changing total_size");

            return null; // not supposed to reach here.
        }

        public object Dequeue(double prio)
        {
            total_size--;
            return storage[prio].Dequeue();
        }

        public void Enqueue(T item, double prio)
        {
            if (!storage.ContainsKey(prio))
            {
                storage.Add(prio, new Queue<T>());
            }
            storage[prio].Enqueue(item);
            total_size++;

        }
    }
}
