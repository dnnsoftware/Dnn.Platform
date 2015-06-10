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

namespace log4net.Util
{
	/// <summary>
	/// An always empty <see cref="IDictionaryEnumerator"/>.
	/// </summary>
	/// <remarks>
	/// <para>
	/// A singleton implementation of the <see cref="IDictionaryEnumerator"/> over a collection
	/// that is empty and not modifiable.
	/// </para>
	/// </remarks>
	/// <author>Nicko Cadell</author>
	/// <author>Gert Driesen</author>
	public sealed class NullDictionaryEnumerator : IDictionaryEnumerator
	{
		#region Private Instance Constructors

		/// <summary>
		/// Initializes a new instance of the <see cref="NullDictionaryEnumerator" /> class. 
		/// </summary>
		/// <remarks>
		/// <para>
		/// Uses a private access modifier to enforce the singleton pattern.
		/// </para>
		/// </remarks>
		private NullDictionaryEnumerator()
		{
		}

		#endregion Private Instance Constructors

		#region Public Static Properties
  
		/// <summary>
		/// Gets the singleton instance of the <see cref="NullDictionaryEnumerator" />.
		/// </summary>
		/// <returns>The singleton instance of the <see cref="NullDictionaryEnumerator" />.</returns>
		/// <remarks>
		/// <para>
		/// Gets the singleton instance of the <see cref="NullDictionaryEnumerator" />.
		/// </para>
		/// </remarks>
		public static NullDictionaryEnumerator Instance
		{
			get { return s_instance; }
		}

		#endregion Public Static Properties

		#region Implementation of IEnumerator

		/// <summary>
		/// Gets the current object from the enumerator.
		/// </summary>
		/// <remarks>
		/// Throws an <see cref="InvalidOperationException" /> because the 
		/// <see cref="NullDictionaryEnumerator" /> never has a current value.
		/// </remarks>
		/// <remarks>
		/// <para>
		/// As the enumerator is over an empty collection its <see cref="Current"/>
		/// value cannot be moved over a valid position, therefore <see cref="Current"/>
		/// will throw an <see cref="InvalidOperationException"/>.
		/// </para>
		/// </remarks>
		/// <exception cref="InvalidOperationException">The collection is empty and <see cref="Current"/> 
		/// cannot be positioned over a valid location.</exception>
		public object Current 
		{
			get	{ throw new InvalidOperationException(); }
		}
  
		/// <summary>
		/// Test if the enumerator can advance, if so advance.
		/// </summary>
		/// <returns><c>false</c> as the <see cref="NullDictionaryEnumerator" /> cannot advance.</returns>
		/// <remarks>
		/// <para>
		/// As the enumerator is over an empty collection its <see cref="Current"/>
		/// value cannot be moved over a valid position, therefore <see cref="MoveNext"/>
		/// will always return <c>false</c>.
		/// </para>
		/// </remarks>
		public bool MoveNext()
		{
			return false;
		}
  
		/// <summary>
		/// Resets the enumerator back to the start.
		/// </summary>
		/// <remarks>
		/// <para>
		/// As the enumerator is over an empty collection <see cref="Reset"/> does nothing.
		/// </para>
		/// </remarks>
		public void Reset() 
		{
		}

		#endregion Implementation of IEnumerator

		#region Implementation of IDictionaryEnumerator

		/// <summary>
		/// Gets the current key from the enumerator.
		/// </summary>
		/// <remarks>
		/// Throws an exception because the <see cref="NullDictionaryEnumerator" />
		/// never has a current value.
		/// </remarks>
		/// <remarks>
		/// <para>
		/// As the enumerator is over an empty collection its <see cref="Current"/>
		/// value cannot be moved over a valid position, therefore <see cref="Key"/>
		/// will throw an <see cref="InvalidOperationException"/>.
		/// </para>
		/// </remarks>
		/// <exception cref="InvalidOperationException">The collection is empty and <see cref="Current"/> 
		/// cannot be positioned over a valid location.</exception>
		public object Key 
		{
			get	{ throw new InvalidOperationException(); }
		}

		/// <summary>
		/// Gets the current value from the enumerator.
		/// </summary>
		/// <value>The current value from the enumerator.</value>
		/// <remarks>
		/// Throws an <see cref="InvalidOperationException" /> because the 
		/// <see cref="NullDictionaryEnumerator" /> never has a current value.
		/// </remarks>
		/// <remarks>
		/// <para>
		/// As the enumerator is over an empty collection its <see cref="Current"/>
		/// value cannot be moved over a valid position, therefore <see cref="Value"/>
		/// will throw an <see cref="InvalidOperationException"/>.
		/// </para>
		/// </remarks>
		/// <exception cref="InvalidOperationException">The collection is empty and <see cref="Current"/> 
		/// cannot be positioned over a valid location.</exception>
		public object Value 
		{
			get	{ throw new InvalidOperationException(); }
		}

		/// <summary>
		/// Gets the current entry from the enumerator.
		/// </summary>
		/// <remarks>
		/// Throws an <see cref="InvalidOperationException" /> because the 
		/// <see cref="NullDictionaryEnumerator" /> never has a current entry.
		/// </remarks>
		/// <remarks>
		/// <para>
		/// As the enumerator is over an empty collection its <see cref="Current"/>
		/// value cannot be moved over a valid position, therefore <see cref="Entry"/>
		/// will throw an <see cref="InvalidOperationException"/>.
		/// </para>
		/// </remarks>
		/// <exception cref="InvalidOperationException">The collection is empty and <see cref="Current"/> 
		/// cannot be positioned over a valid location.</exception>
		public DictionaryEntry Entry 
		{
			get	{ throw new InvalidOperationException(); }
		}
  
		#endregion Implementation of IDictionaryEnumerator

		#region Private Static Fields

		/// <summary>
		/// The singleton instance of the <see cref="NullDictionaryEnumerator" />.
		/// </summary>
		private readonly static NullDictionaryEnumerator s_instance = new NullDictionaryEnumerator();
  
		#endregion Private Static Fields
	}
}
