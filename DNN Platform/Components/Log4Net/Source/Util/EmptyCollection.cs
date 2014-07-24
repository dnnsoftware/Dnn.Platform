using System;
using System.Collections;

namespace log4net.Util
{
	[Serializable]
	public sealed class EmptyCollection : ICollection, IEnumerable
	{
		private readonly static EmptyCollection s_instance;

		public int Count
		{
			get
			{
				return 0;
			}
		}

		public static EmptyCollection Instance
		{
			get
			{
				return EmptyCollection.s_instance;
			}
		}

		public bool IsSynchronized
		{
			get
			{
				return true;
			}
		}

		public object SyncRoot
		{
			get
			{
				return this;
			}
		}

		static EmptyCollection()
		{
			EmptyCollection.s_instance = new EmptyCollection();
		}

		private EmptyCollection()
		{
		}

		public void CopyTo(Array array, int index)
		{
		}

		public IEnumerator GetEnumerator()
		{
			return NullEnumerator.Instance;
		}
	}
}