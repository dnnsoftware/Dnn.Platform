using System;
using System.Collections;

namespace log4net.Util
{
	[Serializable]
	public sealed class EmptyDictionary : IDictionary, ICollection, IEnumerable
	{
		private readonly static EmptyDictionary s_instance;

		public int Count
		{
			get
			{
				return 0;
			}
		}

		public static EmptyDictionary Instance
		{
			get
			{
				return EmptyDictionary.s_instance;
			}
		}

		public bool IsFixedSize
		{
			get
			{
				return true;
			}
		}

		public bool IsReadOnly
		{
			get
			{
				return true;
			}
		}

		public bool IsSynchronized
		{
			get
			{
				return true;
			}
		}

		public object this[object key]
		{
			get
			{
				return null;
			}
			set
			{
				throw new InvalidOperationException();
			}
		}

		public ICollection Keys
		{
			get
			{
				return EmptyCollection.Instance;
			}
		}

		public object SyncRoot
		{
			get
			{
				return this;
			}
		}

		public ICollection Values
		{
			get
			{
				return EmptyCollection.Instance;
			}
		}

		static EmptyDictionary()
		{
			EmptyDictionary.s_instance = new EmptyDictionary();
		}

		private EmptyDictionary()
		{
		}

		public void Add(object key, object value)
		{
			throw new InvalidOperationException();
		}

		public void Clear()
		{
			throw new InvalidOperationException();
		}

		public bool Contains(object key)
		{
			return false;
		}

		public void CopyTo(Array array, int index)
		{
		}

		public IDictionaryEnumerator GetEnumerator()
		{
			return NullDictionaryEnumerator.Instance;
		}

		public void Remove(object key)
		{
			throw new InvalidOperationException();
		}

		IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return NullEnumerator.Instance;
		}
	}
}