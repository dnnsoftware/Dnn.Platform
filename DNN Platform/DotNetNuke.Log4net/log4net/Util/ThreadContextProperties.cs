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
#if NETCF
using System.Collections;
#endif

namespace log4net.Util
{
	/// <summary>
	/// Implementation of Properties collection for the <see cref="log4net.ThreadContext"/>
	/// </summary>
	/// <remarks>
	/// <para>
	/// Class implements a collection of properties that is specific to each thread.
	/// The class is not synchronized as each thread has its own <see cref="PropertiesDictionary"/>.
	/// </para>
	/// </remarks>
	/// <author>Nicko Cadell</author>
	public sealed class ThreadContextProperties : ContextPropertiesBase
	{
		#region Private Instance Fields

#if NETCF
		/// <summary>
		/// The thread local data slot to use to store a PropertiesDictionary.
		/// </summary>
		private readonly static LocalDataStoreSlot s_threadLocalSlot = System.Threading.Thread.AllocateDataSlot();
#else
		/// <summary>
		/// Each thread will automatically have its instance.
		/// </summary>
		[ThreadStatic]
		private static PropertiesDictionary _dictionary;
#endif

		#endregion Private Instance Fields

		#region Public Instance Constructors

		/// <summary>
		/// Internal constructor
		/// </summary>
		/// <remarks>
		/// <para>
		/// Initializes a new instance of the <see cref="ThreadContextProperties" /> class.
		/// </para>
		/// </remarks>
		internal ThreadContextProperties()
		{
		}

		#endregion Public Instance Constructors

		#region Public Instance Properties

		/// <summary>
		/// Gets or sets the value of a property
		/// </summary>
		/// <value>
		/// The value for the property with the specified key
		/// </value>
		/// <remarks>
		/// <para>
		/// Gets or sets the value of a property
		/// </para>
		/// </remarks>
		override public object this[string key]
		{
			get
			{
#if NETCF
				PropertiesDictionary _dictionary = GetProperties(false);
#endif
				if (_dictionary != null)
				{
					return _dictionary[key];
				}
				return null;
			}
			set
			{
				GetProperties(true)[key] = value;
			}
		}

		#endregion Public Instance Properties

		#region Public Instance Methods

		/// <summary>
		/// Remove a property
		/// </summary>
		/// <param name="key">the key for the entry to remove</param>
		/// <remarks>
		/// <para>
		/// Remove a property
		/// </para>
		/// </remarks>
		public void Remove(string key)
		{
#if NETCF
			PropertiesDictionary _dictionary = GetProperties(false);
#endif
			if (_dictionary != null)
			{
				_dictionary.Remove(key);
			}
		}

		/// <summary>
		/// Get the keys stored in the properties.
		/// </summary>
		/// <para>
		/// Gets the keys stored in the properties.
		/// </para>
		/// <returns>a set of the defined keys</returns>
		public string[] GetKeys()
		{
#if NETCF
			PropertiesDictionary _dictionary = GetProperties(false);
#endif
			if (_dictionary != null)
			{
				return _dictionary.GetKeys();
			}
			return null;
		}

		/// <summary>
		/// Clear all properties
		/// </summary>
		/// <remarks>
		/// <para>
		/// Clear all properties
		/// </para>
		/// </remarks>
		public void Clear()
		{
#if NETCF
			PropertiesDictionary _dictionary = GetProperties(false);
#endif
			if (_dictionary != null)
			{
				_dictionary.Clear();
			}
		}

		#endregion Public Instance Methods

		#region Internal Instance Methods

		/// <summary>
		/// Get the <c>PropertiesDictionary</c> for this thread.
		/// </summary>
		/// <param name="create">create the dictionary if it does not exist, otherwise return null if does not exist</param>
		/// <returns>the properties for this thread</returns>
		/// <remarks>
		/// <para>
		/// The collection returned is only to be used on the calling thread. If the
		/// caller needs to share the collection between different threads then the 
		/// caller must clone the collection before doing so.
		/// </para>
		/// </remarks>
		internal PropertiesDictionary GetProperties(bool create)
		{
#if NETCF
			PropertiesDictionary _dictionary = (PropertiesDictionary)System.Threading.Thread.GetData(s_threadLocalSlot);
#endif
			if (_dictionary == null && create)
			{
				_dictionary  = new PropertiesDictionary();
#if NETCF
				System.Threading.Thread.SetData(s_threadLocalSlot, _dictionary);
#endif
			}
			return _dictionary;
		}

		#endregion Internal Instance Methods
	}
}

