#region Apache License
//
// Licensed to the Apache Software Foundation (ASF) under one or more 
// contributor license agreements. See the NOTICE file distributed with
// this work for additional information regarding copyright ownership. 
// The ASF licenses this file to you under the Apache License, Version 2.0
// (the "License"); you may not use this file except in compliance with 
// the License. You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
#endregion

using System;
using System.Collections;
#if !NETCF
using System.Runtime.Serialization;
using System.Xml;
#endif

namespace log4net.Util
{
	/// <summary>
	/// String keyed object map that is read only.
	/// </summary>
	/// <remarks>
	/// <para>
	/// This collection is readonly and cannot be modified.
	/// </para>
	/// <para>
	/// While this collection is serializable only member 
	/// objects that are serializable will
	/// be serialized along with this collection.
	/// </para>
	/// </remarks>
	/// <author>Nicko Cadell</author>
	/// <author>Gert Driesen</author>
#if NETCF
	public class ReadOnlyPropertiesDictionary : IDictionary
#else
	[Serializable] public class ReadOnlyPropertiesDictionary : ISerializable, IDictionary
#endif
	{
		#region Private Instance Fields

		/// <summary>
		/// The Hashtable used to store the properties data
		/// </summary>
		private Hashtable m_hashtable = new Hashtable();

		#endregion Private Instance Fields

		#region Public Instance Constructors

		/// <summary>
		/// Constructor
		/// </summary>
		/// <remarks>
		/// <para>
		/// Initializes a new instance of the <see cref="ReadOnlyPropertiesDictionary" /> class.
		/// </para>
		/// </remarks>
		public ReadOnlyPropertiesDictionary()
		{
		}

		/// <summary>
		/// Copy Constructor
		/// </summary>
		/// <param name="propertiesDictionary">properties to copy</param>
		/// <remarks>
		/// <para>
		/// Initializes a new instance of the <see cref="ReadOnlyPropertiesDictionary" /> class.
		/// </para>
		/// </remarks>
		public ReadOnlyPropertiesDictionary(ReadOnlyPropertiesDictionary propertiesDictionary)
		{
			foreach(DictionaryEntry entry in propertiesDictionary)
			{
				InnerHashtable.Add(entry.Key, entry.Value);
			}
		}

		#endregion Public Instance Constructors

		#region Private Instance Constructors

#if !NETCF
		/// <summary>
		/// Deserialization constructor
		/// </summary>
		/// <param name="info">The <see cref="SerializationInfo" /> that holds the serialized object data.</param>
		/// <param name="context">The <see cref="StreamingContext" /> that contains contextual information about the source or destination.</param>
		/// <remarks>
		/// <para>
		/// Initializes a new instance of the <see cref="ReadOnlyPropertiesDictionary" /> class 
		/// with serialized data.
		/// </para>
		/// </remarks>
		protected ReadOnlyPropertiesDictionary(SerializationInfo info, StreamingContext context)
		{
			foreach(SerializationEntry entry in info)
			{
				// The keys are stored as Xml encoded names
				InnerHashtable[XmlConvert.DecodeName(entry.Name)] = entry.Value;
			}
		}
#endif

		#endregion Protected Instance Constructors

		#region Public Instance Properties

		/// <summary>
		/// Gets the key names.
		/// </summary>
		/// <returns>An array of all the keys.</returns>
		/// <remarks>
		/// <para>
		/// Gets the key names.
		/// </para>
		/// </remarks>
		public string[] GetKeys()
		{
			string[] keys = new String[InnerHashtable.Count];
			InnerHashtable.Keys.CopyTo(keys, 0);
			return keys;
		}

		/// <summary>
		/// Gets or sets the value of the  property with the specified key.
		/// </summary>
		/// <value>
		/// The value of the property with the specified key.
		/// </value>
		/// <param name="key">The key of the property to get or set.</param>
		/// <remarks>
		/// <para>
		/// The property value will only be serialized if it is serializable.
		/// If it cannot be serialized it will be silently ignored if
		/// a serialization operation is performed.
		/// </para>
		/// </remarks>
		public virtual object this[string key]
		{
			get { return InnerHashtable[key]; }
			set { throw new NotSupportedException("This is a Read Only Dictionary and can not be modified"); }
		}

		#endregion Public Instance Properties

		#region Public Instance Methods

		/// <summary>
		/// Test if the dictionary contains a specified key
		/// </summary>
		/// <param name="key">the key to look for</param>
		/// <returns>true if the dictionary contains the specified key</returns>
		/// <remarks>
		/// <para>
		/// Test if the dictionary contains a specified key
		/// </para>
		/// </remarks>
		public bool Contains(string key)
		{
			return InnerHashtable.Contains(key);
		}

		#endregion

		/// <summary>
		/// The hashtable used to store the properties
		/// </summary>
		/// <value>
		/// The internal collection used to store the properties
		/// </value>
		/// <remarks>
		/// <para>
		/// The hashtable used to store the properties
		/// </para>
		/// </remarks>
		protected Hashtable InnerHashtable
		{
			get { return m_hashtable; }
		}

		#region Implementation of ISerializable

#if !NETCF
		/// <summary>
		/// Serializes this object into the <see cref="SerializationInfo" /> provided.
		/// </summary>
		/// <param name="info">The <see cref="SerializationInfo" /> to populate with data.</param>
		/// <param name="context">The destination for this serialization.</param>
		/// <remarks>
		/// <para>
		/// Serializes this object into the <see cref="SerializationInfo" /> provided.
		/// </para>
		/// </remarks>
#if NET_4_0
        [System.Security.SecurityCritical]
#else
		[System.Security.Permissions.SecurityPermissionAttribute(System.Security.Permissions.SecurityAction.Demand, SerializationFormatter=true)]
#endif
        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			foreach(DictionaryEntry entry in InnerHashtable)
			{
				string entryKey = entry.Key as string;
				object entryValue = entry.Value;

				// If value is serializable then we add it to the list
				if (entryKey != null && entryValue != null && entryValue.GetType().IsSerializable)
				{
					// Store the keys as an Xml encoded local name as it may contain colons (':') 
					// which are NOT escaped by the Xml Serialization framework.
					// This must be a bug in the serialization framework as we cannot be expected
					// to know the implementation details of all the possible transport layers.
					info.AddValue(XmlConvert.EncodeLocalName(entryKey), entryValue);
				}
			}
		}
#endif

		#endregion Implementation of ISerializable

		#region Implementation of IDictionary

		/// <summary>
		/// See <see cref="IDictionary.GetEnumerator"/>
		/// </summary>
		IDictionaryEnumerator IDictionary.GetEnumerator()
		{
			return InnerHashtable.GetEnumerator();
		}

		/// <summary>
		/// See <see cref="IDictionary.Remove"/>
		/// </summary>
		/// <param name="key"></param>
		void IDictionary.Remove(object key)
		{
			throw new NotSupportedException("This is a Read Only Dictionary and can not be modified");
		}

		/// <summary>
		/// See <see cref="IDictionary.Contains"/>
		/// </summary>
		/// <param name="key"></param>
		/// <returns></returns>
		bool IDictionary.Contains(object key)
		{
			return InnerHashtable.Contains(key);
		}

		/// <summary>
		/// Remove all properties from the properties collection
		/// </summary>
		public virtual void Clear()
		{
			throw new NotSupportedException("This is a Read Only Dictionary and can not be modified");
		}

		/// <summary>
		/// See <see cref="IDictionary.Add"/>
		/// </summary>
		/// <param name="key"></param>
		/// <param name="value"></param>
		void IDictionary.Add(object key, object value)
		{
			throw new NotSupportedException("This is a Read Only Dictionary and can not be modified");
		}

		/// <summary>
		/// See <see cref="IDictionary.IsReadOnly"/>
		/// </summary>
		bool IDictionary.IsReadOnly
		{
			get { return true; }
		}

		/// <summary>
		/// See <see cref="IDictionary.this[object]"/>
		/// </summary>
		object IDictionary.this[object key]
		{
			get
			{
				if (!(key is string)) throw new ArgumentException("key must be a string");
				return InnerHashtable[key];
			}
			set
			{
				throw new NotSupportedException("This is a Read Only Dictionary and can not be modified");
			}
		}

		/// <summary>
		/// See <see cref="IDictionary.Values"/>
		/// </summary>
		ICollection IDictionary.Values
		{
			get { return InnerHashtable.Values; }
		}

		/// <summary>
		/// See <see cref="IDictionary.Keys"/>
		/// </summary>
		ICollection IDictionary.Keys
		{
			get { return InnerHashtable.Keys; }
		}

		/// <summary>
		/// See <see cref="IDictionary.IsFixedSize"/>
		/// </summary>
		bool IDictionary.IsFixedSize
		{
			get { return InnerHashtable.IsFixedSize; }
		}

		#endregion

		#region Implementation of ICollection

		/// <summary>
		/// See <see cref="ICollection.CopyTo"/>
		/// </summary>
		/// <param name="array"></param>
		/// <param name="index"></param>
		void ICollection.CopyTo(Array array, int index)
		{
			InnerHashtable.CopyTo(array, index);
		}

		/// <summary>
		/// See <see cref="ICollection.IsSynchronized"/>
		/// </summary>
		bool ICollection.IsSynchronized
		{
			get { return InnerHashtable.IsSynchronized; }
		}

		/// <summary>
		/// The number of properties in this collection
		/// </summary>
		public int Count
		{
			get { return InnerHashtable.Count; }
		}

		/// <summary>
		/// See <see cref="ICollection.SyncRoot"/>
		/// </summary>
		object ICollection.SyncRoot
		{
			get { return InnerHashtable.SyncRoot; }
		}

		#endregion

		#region Implementation of IEnumerable

		/// <summary>
		/// See <see cref="IEnumerable.GetEnumerator"/>
		/// </summary>
		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable)InnerHashtable).GetEnumerator();
		}

		#endregion
	}
}

