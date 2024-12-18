using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;

public static class QueueUtils
{
    public static void Clear<T>(this ConcurrentQueue<T> queue)
    {
        T item;
        while (queue.TryDequeue(out item))
        {
            
        }
    }
}

