using System;
using System.Collections;

namespace log4net.Util
{
	public sealed class NullEnumerator : IEnumerator
	{
		private readonly static NullEnumerator s_instance;

		public object Current
		{
			get
			{
				throw new InvalidOperationException();
			}
		}

		public static NullEnumerator Instance
		{
			get
			{
				return NullEnumerator.s_instance;
			}
		}

		static NullEnumerator()
		{
			NullEnumerator.s_instance = new NullEnumerator();
		}

		private NullEnumerator()
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