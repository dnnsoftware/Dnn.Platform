using System;
using System.Collections;

namespace log4net.Util
{
	public sealed class NullDictionaryEnumerator : IDictionaryEnumerator, IEnumerator
	{
		private readonly static NullDictionaryEnumerator s_instance;

		public object Current
		{
			get
			{
				throw new InvalidOperationException();
			}
		}

		public DictionaryEntry Entry
		{
			get
			{
				throw new InvalidOperationException();
			}
		}

		public static NullDictionaryEnumerator Instance
		{
			get
			{
				return NullDictionaryEnumerator.s_instance;
			}
		}

		public object Key
		{
			get
			{
				throw new InvalidOperationException();
			}
		}

		public object Value
		{
			get
			{
				throw new InvalidOperationException();
			}
		}

		static NullDictionaryEnumerator()
		{
			NullDictionaryEnumerator.s_instance = new NullDictionaryEnumerator();
		}

		private NullDictionaryEnumerator()
		{
		}

		public bool MoveNext()
		{
			return false;
		}

		public void Reset()
		{
		}
	}
}