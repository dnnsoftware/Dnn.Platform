using System;
using System.Collections;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Xml;

namespace log4net.Util
{
	[Serializable]
	public class ReadOnlyPropertiesDictionary : ISerializable, IDictionary, ICollection, IEnumerable
	{
		private Hashtable m_hashtable = new Hashtable();

		public int Count
		{
			get
			{
				return this.InnerHashtable.Count;
			}
		}

		protected Hashtable InnerHashtable
		{
			get
			{
				return this.m_hashtable;
			}
		}

		public virtual object this[string key]
		{
			get
			{
				return this.InnerHashtable[key];
			}
			set
			{
				throw new NotSupportedException("This is a Read Only Dictionary and can not be modified");
			}
		}

		bool System.Collections.ICollection.IsSynchronized
		{
			get
			{
				return this.InnerHashtable.IsSynchronized;
			}
		}

		object System.Collections.ICollection.SyncRoot
		{
			get
			{
				return this.InnerHashtable.SyncRoot;
			}
		}

		bool System.Collections.IDictionary.IsFixedSize
		{
			get
			{
				return this.InnerHashtable.IsFixedSize;
			}
		}

		bool System.Collections.IDictionary.IsReadOnly
		{
			get
			{
				return true;
			}
		}

		object System.Collections.IDictionary.this[object key]
		{
			get
			{
				if (!(key is string))
				{
					throw new ArgumentException("key must be a string");
				}
				return this.InnerHashtable[key];
			}
			set
			{
				throw new NotSupportedException("This is a Read Only Dictionary and can not be modified");
			}
		}

		ICollection System.Collections.IDictionary.Keys
		{
			get
			{
				return this.InnerHashtable.Keys;
			}
		}

		ICollection System.Collections.IDictionary.Values
		{
			get
			{
				return this.InnerHashtable.Values;
			}
		}

		public ReadOnlyPropertiesDictionary()
		{
		}

		public ReadOnlyPropertiesDictionary(ReadOnlyPropertiesDictionary propertiesDictionary)
		{
			foreach (DictionaryEntry dictionaryEntry in (IEnumerable)propertiesDictionary)
			{
				this.InnerHashtable.Add(dictionaryEntry.Key, dictionaryEntry.Value);
			}
		}

		protected ReadOnlyPropertiesDictionary(SerializationInfo info, StreamingContext context)
		{
			SerializationInfoEnumerator enumerator = info.GetEnumerator();
			while (enumerator.MoveNext())
			{
				SerializationEntry current = enumerator.Current;
				this.InnerHashtable[XmlConvert.DecodeName(current.Name)] = current.Value;
			}
		}

		public virtual void Clear()
		{
			throw new NotSupportedException("This is a Read Only Dictionary and can not be modified");
		}

		public bool Contains(string key)
		{
			return this.InnerHashtable.Contains(key);
		}

		public string[] GetKeys()
		{
			string[] strArrays = new string[this.InnerHashtable.Count];
			this.InnerHashtable.Keys.CopyTo(strArrays, 0);
			return strArrays;
		}

		[SecurityPermission(SecurityAction.Demand, SerializationFormatter=true)]
		public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			foreach (DictionaryEntry innerHashtable in this.InnerHashtable)
			{
				string key = innerHashtable.Key as string;
				object value = innerHashtable.Value;
				if (key == null || value == null || !value.GetType().IsSerializable)
				{
					continue;
				}
				info.AddValue(XmlConvert.EncodeLocalName(key), value);
			}
		}

		void System.Collections.ICollection.CopyTo(Array array, int index)
		{
			this.InnerHashtable.CopyTo(array, index);
		}

		void System.Collections.IDictionary.Add(object key, object value)
		{
			throw new NotSupportedException("This is a Read Only Dictionary and can not be modified");
		}

		bool System.Collections.IDictionary.Contains(object key)
		{
			return this.InnerHashtable.Contains(key);
		}

		IDictionaryEnumerator System.Collections.IDictionary.GetEnumerator()
		{
			return this.InnerHashtable.GetEnumerator();
		}

		void System.Collections.IDictionary.Remove(object key)
		{
			throw new NotSupportedException("This is a Read Only Dictionary and can not be modified");
		}

		IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return ((IEnumerable)this.InnerHashtable).GetEnumerator();
		}
	}
}