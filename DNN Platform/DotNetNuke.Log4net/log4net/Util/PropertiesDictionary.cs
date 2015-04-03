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
	/// String keyed object map.
	/// </summary>
	/// <remarks>
	/// <para>
	/// While this collection is serializable only member 
	/// objects that are serializable will
	/// be serialized along with this collection.
	/// </para>
	/// </remarks>
	/// <author>Nicko Cadell</author>
	/// <author>Gert Driesen</author>
#if NETCF
	public sealed class PropertiesDictionary : ReadOnlyPropertiesDictionary, IDictionary
#else
	[Serializable] public sealed class PropertiesDictionary : ReadOnlyPropertiesDictionary, ISerializable, IDictionary
#endif
	{
		#region Public Instance Constructors

		/// <summary>
		/// Constructor
		/// </summary>
		/// <remarks>
		/// <para>
		/// Initializes a new instance of the <see cref="PropertiesDictionary" /> class.
		/// </para>
		/// </remarks>
		public PropertiesDictionary()
		{
		}

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="propertiesDictionary">properties to copy</param>
		/// <remarks>
		/// <para>
		/// Initializes a new instance of the <see cref="PropertiesDictionary" /> class.
		/// </para>
		/// </remarks>
		public PropertiesDictionary(ReadOnlyPropertiesDictionary propertiesDictionary) : base(propertiesDictionary)
		{
		}

		#endregion Public Instance Constructors

		#region Private Instance Constructors

#if !NETCF
		/// <summary>
		/// Initializes a new instance of the <see cref="PropertiesDictionary" /> class 
		/// with serialized data.
		/// </summary>
		/// <param name="info">The <see cref="SerializationInfo" /> that holds the serialized object data.</param>
		/// <param name="context">The <see cref="StreamingContext" /> that contains contextual information about the source or destination.</param>
		/// <remarks>
		/// <para>
		/// Because this class is sealed the serialization constructor is private.
		/// </para>
		/// </remarks>
		private PropertiesDictionary(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}
#endif

		#endregion Protected Instance Constructors

		#region Public Instance Properties

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
		override public object this[string key]
		{
			get { return InnerHashtable[key]; }
			set { InnerHashtable[key] = value; }
		}

		#endregion Public Instance Properties

		#region Public Instance Methods

		/// <summary>
		/// Remove the entry with the specified key from this dictionary
		/// </summary>
		/// <param name="key">the key for the entry to remove</param>
		/// <remarks>
		/// <para>
		/// Remove the entry with the specified key from this dictionary
		/// </para>
		/// </remarks>
		public void Remove(string key)
		{
			InnerHashtable.Remove(key);
		}

		#endregion Public Instance Methods

		#region Implementation of IDictionary

		/// <summary>
		/// See <see cref="IDictionary.GetEnumerator"/>
		/// </summary>
		/// <returns>an enumerator</returns>
		/// <remarks>
		/// <para>
		/// Returns a <see cref="IDictionaryEnumerator"/> over the contest of this collection.
		/// </para>
		/// </remarks>
		IDictionaryEnumerator IDictionary.GetEnumerator()
		{
			return InnerHashtable.GetEnumerator();
		}

		/// <summary>
		/// See <see cref="IDictionary.Remove"/>
		/// </summary>
		/// <param name="key">the key to remove</param>
		/// <remarks>
		/// <para>
		/// Remove the entry with the specified key from this dictionary
		/// </para>
		/// </remarks>
		void IDictionary.Remove(object key)
		{
			InnerHashtable.Remove(key);
		}

		/// <summary>
		/// See <see cref="IDictionary.Contains"/>
		/// </summary>
		/// <param name="key">the key to lookup in the collection</param>
		/// <returns><c>true</c> if the collection contains the specified key</returns>
		/// <remarks>
		/// <para>
		/// Test if this collection contains a specified key.
		/// </para>
		/// </remarks>
		bool IDictionary.Contains(object key)
		{
			return InnerHashtable.Contains(key);
		}

		/// <summary>
		/// Remove all properties from the properties collection
		/// </summary>
		/// <remarks>
		/// <para>
		/// Remove all properties from the properties collection
		/// </para>
		/// </remarks>
		public override void Clear()
		{
			InnerHashtable.Clear();
		}

		/// <summary>
		/// See <see cref="IDictionary.Add"/>
		/// </summary>
		/// <param name="key">the key</param>
		/// <param name="value">the value to store for the key</param>
		/// <remarks>
		/// <para>
		/// Store a value for the specified <see cref="String"/> <paramref name="key"/>.
		/// </para>
		/// </remarks>
		/// <exception cref="ArgumentException">Thrown if the <paramref name="key"/> is not a string</exception>
		void IDictionary.Add(object key, object value)
		{
			if (!(key is string))
			{
				throw new ArgumentException("key must be a string", "key");
			}
			InnerHashtable.Add(key, value);
		}

		/// <summary>
		/// See <see cref="IDictionary.IsReadOnly"/>
		/// </summary>
		/// <value>
		/// <c>false</c>
		/// </value>
		/// <remarks>
		/// <para>
		/// This collection is modifiable. This property always
		/// returns <c>false</c>.
		/// </para>
		/// </remarks>
		bool IDictionary.IsReadOnly
		{
			get { return false; }
		}

		/// <summary>
		/// See <see cref="IDictionary.this"/>
		/// </summary>
		/// <value>
		/// The value for the key specified.
		/// </value>
		/// <remarks>
		/// <para>
		/// Get or set a value for the specified <see cref="String"/> <paramref name="key"/>.
		/// </para>
		/// </remarks>
		/// <exception cref="ArgumentException">Thrown if the <paramref name="key"/> is not a string</exception>
		object IDictionary.this[object key]
		{
			get
			{
				if (!(key is string))
				{
					throw new ArgumentException("key must be a string", "key");
				}
				return InnerHashtable[key];
			}
			set
			{
				if (!(key is string))
				{
					throw new ArgumentException("key must be a string", "key");
				}
				InnerHashtable[key] = value;
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
			get { return false; }
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

