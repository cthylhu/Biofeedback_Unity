using System;
using System.Collections.Generic;

namespace PhySigTK
{
	/// <summary>
	/// Threadsafe Queue, generic, first in first out,
	/// limits max number of entries to 100 by default, allows for bulk dequeue with DequeueAll.
	/// </summary>
	class LockedQueue<T>
	{
		private readonly object syncLock = new object();
		private Queue<T> queue;
		public int maxCount = 100;

		public LockedQueue()
		{
			this.queue = new Queue<T>();
		}

		public int Count
		{
			get
			{
				lock (syncLock) {
					return queue.Count;
				}
			}
		}

		public T Peek()
		{
			lock (syncLock) {
				return queue.Peek();
			}
		}

		public void Enqueue(T obj)
		{
			lock (syncLock) {
				queue.Enqueue(obj);
				if (queue.Count > maxCount) {
					queue.Dequeue(); // discard oldest object
				}
			}
		}

		public T Dequeue()
		{
			lock (syncLock) {
				return queue.Dequeue();
			}
		}

		public T[] DequeueAll()
		{
			lock (syncLock) {
				T[] values = queue.ToArray();
				queue.Clear();
				return values;
			}
		}

		public void Clear()
		{
			lock (syncLock) {
				queue.Clear();
			}
		}
	}
}
