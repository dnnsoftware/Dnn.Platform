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

// .NET Compact Framework 1.0 has no support for System.Runtime.Remoting.Messaging.CallContext
#if !NETCF

using System;
using System.Runtime.Remoting.Messaging;
using System.Security;

namespace log4net.Util
{
	/// <summary>
	/// Implementation of Properties collection for the <see cref="log4net.LogicalThreadContext"/>
	/// </summary>
	/// <remarks>
	/// <para>
	/// Class implements a collection of properties that is specific to each thread.
	/// The class is not synchronized as each thread has its own <see cref="PropertiesDictionary"/>.
	/// </para>
	/// <para>
	/// This class stores its properties in a slot on the <see cref="CallContext"/> named
	/// <c>log4net.Util.LogicalThreadContextProperties</c>.
	/// </para>
	/// <para>
	/// The <see cref="CallContext"/> requires a link time 
	/// <see cref="System.Security.Permissions.SecurityPermission"/> for the
	/// <see cref="System.Security.Permissions.SecurityPermissionFlag.Infrastructure"/>.
	/// If the calling code does not have this permission then this context will be disabled.
	/// It will not store any property values set on it.
	/// </para>
	/// </remarks>
	/// <author>Nicko Cadell</author>
	public sealed class LogicalThreadContextProperties : ContextPropertiesBase
	{
		private const string c_SlotName = "log4net.Util.LogicalThreadContextProperties";
		
		/// <summary>
		/// Flag used to disable this context if we don't have permission to access the CallContext.
		/// </summary>
		private bool m_disabled = false;
		
		#region Public Instance Constructors

		/// <summary>
		/// Constructor
		/// </summary>
		/// <remarks>
		/// <para>
		/// Initializes a new instance of the <see cref="LogicalThreadContextProperties" /> class.
		/// </para>
		/// </remarks>
		internal LogicalThreadContextProperties()
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
		/// Get or set the property value for the <paramref name="key"/> specified.
		/// </para>
		/// </remarks>
		override public object this[string key]
		{
			get 
			{ 
				// Don't create the dictionary if it does not already exist
				PropertiesDictionary dictionary = GetProperties(false);
				if (dictionary != null)
				{
					return dictionary[key]; 
				}
				return null;
			}
			set 
			{ 
				// Force the dictionary to be created
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
		/// Remove the value for the specified <paramref name="key"/> from the context.
		/// </para>
		/// </remarks>
		public void Remove(string key)
		{
			PropertiesDictionary dictionary = GetProperties(false);
			if (dictionary != null)
			{
				dictionary.Remove(key);
			}
		}

		/// <summary>
		/// Clear all the context properties
		/// </summary>
		/// <remarks>
		/// <para>
		/// Clear all the context properties
		/// </para>
		/// </remarks>
		public void Clear()
		{
			PropertiesDictionary dictionary = GetProperties(false);
			if (dictionary != null)
			{
				dictionary.Clear();
			}
		}

		#endregion Public Instance Methods

		#region Internal Instance Methods

		/// <summary>
		/// Get the PropertiesDictionary stored in the LocalDataStoreSlot for this thread.
		/// </summary>
		/// <param name="create">create the dictionary if it does not exist, otherwise return null if is does not exist</param>
		/// <returns>the properties for this thread</returns>
		/// <remarks>
		/// <para>
		/// The collection returned is only to be used on the calling thread. If the
		/// caller needs to share the collection between different threads then the 
		/// caller must clone the collection before doings so.
		/// </para>
		/// </remarks>
		internal PropertiesDictionary GetProperties(bool create)
		{
			if (!m_disabled)
			{
				try
				{
					PropertiesDictionary properties = GetCallContextData();
					if (properties == null && create)
					{
						properties = new PropertiesDictionary();
						SetCallContextData(properties);
					}
					return properties;
				}
				catch (SecurityException secEx)
				{
					m_disabled = true;
					
					// Thrown if we don't have permission to read or write the CallContext
					LogLog.Warn(declaringType, "SecurityException while accessing CallContext. Disabling LogicalThreadContextProperties", secEx);
				}
			}
			
			// Only get here is we are disabled because of a security exception
			if (create)
			{
				return new PropertiesDictionary();
			}
			return null;
		}

		#endregion Internal Instance Methods

        #region Private Static Methods

        /// <summary>
		/// Gets the call context get data.
		/// </summary>
		/// <returns>The peroperties dictionary stored in the call context</returns>
		/// <remarks>
		/// The <see cref="CallContext"/> method <see cref="CallContext.GetData"/> has a
		/// security link demand, therfore we must put the method call in a seperate method
		/// that we can wrap in an exception handler.
		/// </remarks>
#if NET_4_0
        [System.Security.SecuritySafeCritical]
#endif
        private static PropertiesDictionary GetCallContextData()
		{
			return CallContext.GetData(c_SlotName) as PropertiesDictionary;
		}

		/// <summary>
		/// Sets the call context data.
		/// </summary>
		/// <param name="properties">The properties.</param>
		/// <remarks>
		/// The <see cref="CallContext"/> method <see cref="CallContext.SetData"/> has a
		/// security link demand, therfore we must put the method call in a seperate method
		/// that we can wrap in an exception handler.
		/// </remarks>
#if NET_4_0
        [System.Security.SecuritySafeCritical]
#endif
        private static void SetCallContextData(PropertiesDictionary properties)
		{
			CallContext.SetData(c_SlotName, properties);
        }

        #endregion

	    #region Private Static Fields

	    /// <summary>
	    /// The fully qualified type of the LogicalThreadContextProperties class.
	    /// </summary>
	    /// <remarks>
	    /// Used by the internal logger to record the Type of the
	    /// log message.
	    /// </remarks>
	    private readonly static Type declaringType = typeof(LogicalThreadContextProperties);

	    #endregion Private Static Fields
    }
}

#endif
