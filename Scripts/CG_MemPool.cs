using System.Collections.Generic;
using UnityEngine;

// Memory Pools.
// Originally written from TEXDraw pooling system.

public static partial class CoreGUI
{
    public static class ArrayPool<T>
    {
        const int kMaxCount = 5;

        // Object pool to avoid allocations.
        private static readonly Stack<T[]>[] s_ArrayPool;

        static ArrayPool()
        {
            s_ArrayPool = new Stack<T[]>[kMaxCount];
            for (int i = 0; i < kMaxCount; i++)
            {
                s_ArrayPool[i] = new Stack<T[]>();
            }
        }
        
        public static T[] Get(T a)
        {
            if (s_ArrayPool[0].Count == 0)
                return new T[] { a };
            else
            {
                var arr = s_ArrayPool[0].Pop();
                arr[0] = a;
                return arr;
            }                 
        }

        public static T[] Get(T a, T b)
        {
            if (s_ArrayPool[1].Count == 0)
                return new T[] { a, b };
            else
            {
                var arr = s_ArrayPool[1].Pop();
                arr[0] = a;
                arr[1] = b;
                return arr;
            }
        }
        
        public static T[] Get(T a, T b, T c)
        {
            if (s_ArrayPool[2].Count == 0)
                return new T[] { a, b, c };
            else
            {
                var arr = s_ArrayPool[2].Pop();
                arr[0] = a;
                arr[1] = b;
                arr[2] = c;
                return arr;
            }
        }

        public static T[] Get(T a, T b, T c, T d)
        {
            if (s_ArrayPool[3].Count == 0)
                return new T[] { a, b, c, d };
            else
            {
                var arr = s_ArrayPool[3].Pop();
                arr[0] = a;
                arr[1] = b;
                arr[2] = c;
                arr[3] = d;
                return arr;
            }
        }

        public static T[] Get(T a, T b, T c, T d, T e)
        {
            if (s_ArrayPool[4].Count == 0)
                return new T[] { a, b, c, d, e };
            else
            {
                var arr = s_ArrayPool[4].Pop();
                arr[0] = a;
                arr[1] = b;
                arr[2] = c;
                arr[3] = d;
                arr[4] = e;
                return arr;
            }
        }

        public static void Release(T[] arr)
        {
            if (arr.Length >= kMaxCount)
                // No enough room for this
                return;

            s_ArrayPool[arr.Length - 1].Push(arr);
        }
    }

    //This one is used for getting a List class
    public static class ListPool<T>
    {
        // Object pool to avoid allocations.
        private static readonly ObjectPool<List<T>> s_ListPool = new ObjectPool<List<T>>();

        private static bool m_IsImplementIFlusable = typeof(T).IsSubclassOf(typeof(IFlushable));

        /// Get a new list instance
        /// Replacement for new List<T>()
        public static List<T> Get()
        {
            //if(typeof(T) == typeof(Box))
            //   Debug.LogWarning("POP " + Time.frameCount);

            return s_ListPool.Get();
        }

        /// Releasing this list with its children if possible
        public static void Release(List<T> toRelease)
        {
            if (m_IsImplementIFlusable && toRelease.Count > 0)
            {
                for (int i = 0; i < toRelease.Count; i++)
                {
                    ((IFlushable)toRelease[i]).Flush();
                }
            }
            toRelease.Clear();
            //if(typeof(T) == typeof(Box))
            //    Debug.Log("PUSH " + Time.frameCount);
            s_ListPool.Release(toRelease);
        }

        /// Releasing this list without flushing the childs
        /// used if reference child is still used somewhere
        public static void ReleaseNoFlush(List<T> toRelease)
        {
            toRelease.Clear();
            //if(typeof(T) == typeof(Box))
            //    Debug.Log("PUSH " + Time.frameCount);
            s_ListPool.Release(toRelease);
        }
    }

    public static class MemPool<T> where T : class, IFlushable, new()
    {
        // Object pool to avoid allocations.
        private static readonly ObjectPool<T> s_ObjPool = new ObjectPool<T>();

        public static T Get()
        {
            T obj = s_ObjPool.Get();
            obj.IsFlushed = false;
            return obj;
        }

        public static void Release(T toRelease)
        {
            if (!toRelease.IsFlushed)
            {
                toRelease.IsFlushed = true;
                s_ObjPool.Release(toRelease);
            }
        }
    }

    //Interface to get a class to be flushable (flush means to be released to the main class stack
    //when it's unused, later if code need a new instance, the main stack will give this class back
    //instead of creating a new instance (which later introducing Memory Garbages)).
    public interface IFlushable
    {
        bool IsFlushed { get; set; }

        void Flush();
    }


    public class ObjectPool<T> : IObjectPool where T : new()
    {

#if UNITY_EDITOR
        public static class ObjectPoolShared
        {
            public static List<IObjectPool> objectPools = new List<IObjectPool>();
        }

#endif

        private readonly Stack<T> m_Stack = new Stack<T>();

#if UNITY_EDITOR
        public int countAll { get; set; }

        public int countActive { get { return countAll - countInactive; } }

        public int countInactive { get { return m_Stack.Count; } }

        public bool hasRegistered = false;
#endif

        public T Get()
        {
            T element;
            if (m_Stack.Count == 0)
            {
                element = new T();
#if UNITY_EDITOR
                countAll++;
                // Debug.LogFormat( "Pop New {0}, Total {1}", typeof(T).Name, countAll);
                if (!hasRegistered)
                {
                    ObjectPoolShared.objectPools.Add(this);
                    hasRegistered = true;
                }
#endif
            }
            else
            {
                element = m_Stack.Pop();
            }
            return element;
        }

        public void Release(T element)
        {
#if UNITY_EDITOR
            if (m_Stack.Count > 0 && ReferenceEquals(m_Stack.Peek(), element))
                Debug.LogError("Internal error. Trying to destroy object that is already released to pool.");
#endif
            m_Stack.Push(element);
        }
    }

    public interface IObjectPool
    {
#if UNITY_EDITOR
        int countAll { get; set; }

        int countActive { get; }

        int countInactive { get; }
#endif
    }

}
