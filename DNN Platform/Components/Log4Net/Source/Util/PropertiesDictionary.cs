using System;
using System.Collections;
using System.Runtime.Serialization;

namespace log4net.Util
{
	[Serializable]
	public sealed class PropertiesDictionary : ReadOnlyPropertiesDictionary, ISerializable, IDictionary, ICollection, IEnumerable
	{
		public override object this[string key]
		{
			get
			{
				return base.InnerHashtable[key];
			}
			set
			{
				base.InnerHashtable[key] = value;
			}
		}

		bool System.Collections.ICollection.IsSynchronized
		{
			get
			{
				return base.InnerHashtable.IsSynchronized;
			}
		}

		object System.Collections.ICollection.SyncRoot
		{
			get
			{
				return base.InnerHashtable.SyncRoot;
			}
		}

		bool System.Collections.IDictionary.IsFixedSize
		{
			get
			{
				return false;
			}
		}

		bool System.Collections.IDictionary.IsReadOnly
		{
			get
			{
				return false;
			}
		}

		object System.Collections.IDictionary.this[object key]
		{
			get
			{
				if (!(key is string))
				{
					throw new ArgumentException("key must be a string", "key");
				}
				return base.InnerHashtable[key];
			}
			set
			{
				if (!(key is string))
				{
					throw new ArgumentException("key must be a string", "key");
				}
				base.InnerHashtable[key] = value;
			}
		}

		ICollection System.Collections.IDictionary.Keys
		{
			get
			{
				return base.InnerHashtable.Keys;
			}
		}

		ICollection System.Collections.IDictionary.Values
		{
			get
			{
				return base.InnerHashtable.Values;
			}
		}

		public PropertiesDictionary()
		{
		}

		public PropertiesDictionary(ReadOnlyPropertiesDictionary propertiesDictionary) : base(propertiesDictionary)
		{
		}

		private PropertiesDictionary(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}

		public override void Clear()
		{
			base.InnerHashtable.Clear();
		}

		public void Remove(string key)
		{
			base.InnerHashtable.Remove(key);
		}

		void System.Collections.ICollection.CopyTo(Array array, int index)
		{
			base.InnerHashtable.CopyTo(array, index);
		}

		void System.Collections.IDictionary.Add(object key, object value)
		{
			if (!(key is string))
			{
				throw new ArgumentException("key must be a string", "key");
			}
			base.InnerHashtable.Add(key, value);
		}

		bool System.Collections.IDictionary.Contains(object key)
		{
			return base.InnerHashtable.Contains(key);
		}

		IDictionaryEnumerator System.Collections.IDictionary.GetEnumerator()
		{
			return base.InnerHashtable.GetEnumerator();
		}

		void System.Collections.IDictionary.Remove(object key)
		{
			base.InnerHashtable.Remove(key);
		}

		IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return ((IEnumerable)base.InnerHashtable).GetEnumerator();
		}
	}
}