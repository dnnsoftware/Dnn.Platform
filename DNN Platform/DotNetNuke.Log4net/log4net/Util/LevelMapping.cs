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

using log4net.Core;

namespace log4net.Util
{
	/// <summary>
	/// Manages a mapping from levels to <see cref="LevelMappingEntry"/>
	/// </summary>
	/// <remarks>
	/// <para>
	/// Manages an ordered mapping from <see cref="Level"/> instances 
	/// to <see cref="LevelMappingEntry"/> subclasses.
	/// </para>
	/// </remarks>
	/// <author>Nicko Cadell</author>
	public sealed class LevelMapping : IOptionHandler
	{
		#region Public Instance Constructors

		/// <summary>
		/// Default constructor
		/// </summary>
		/// <remarks>
		/// <para>
		/// Initialise a new instance of <see cref="LevelMapping"/>.
		/// </para>
		/// </remarks>
		public LevelMapping() 
		{
		}

		#endregion // Public Instance Constructors

		#region Public Instance Methods
	
		/// <summary>
		/// Add a <see cref="LevelMappingEntry"/> to this mapping
		/// </summary>
		/// <param name="entry">the entry to add</param>
		/// <remarks>
		/// <para>
		/// If a <see cref="LevelMappingEntry"/> has previously been added
		/// for the same <see cref="Level"/> then that entry will be 
		/// overwritten.
		/// </para>
		/// </remarks>
		public void Add(LevelMappingEntry entry)
		{
			if (m_entriesMap.ContainsKey(entry.Level))
			{
				m_entriesMap.Remove(entry.Level);
			}
			m_entriesMap.Add(entry.Level, entry);
		}

		/// <summary>
		/// Lookup the mapping for the specified level
		/// </summary>
		/// <param name="level">the level to lookup</param>
		/// <returns>the <see cref="LevelMappingEntry"/> for the level or <c>null</c> if no mapping found</returns>
		/// <remarks>
		/// <para>
		/// Lookup the value for the specified level. Finds the nearest
		/// mapping value for the level that is equal to or less than the
		/// <paramref name="level"/> specified.
		/// </para>
		/// <para>
		/// If no mapping could be found then <c>null</c> is returned.
		/// </para>
		/// </remarks>
		public LevelMappingEntry Lookup(Level level)
		{
			if (m_entries != null)
			{
				foreach(LevelMappingEntry entry in m_entries)
				{
					if (level >= entry.Level)
					{
						return entry;
					}
				}
			}
			return null;
		}

		#endregion // Public Instance Methods

		#region IOptionHandler Members

		/// <summary>
		/// Initialize options
		/// </summary>
		/// <remarks>
		/// <para>
		/// Caches the sorted list of <see cref="LevelMappingEntry"/> in an array
		/// </para>
		/// </remarks>
		public void ActivateOptions()
		{
			Level[] sortKeys = new Level[m_entriesMap.Count];
			LevelMappingEntry[] sortValues = new LevelMappingEntry[m_entriesMap.Count];

			m_entriesMap.Keys.CopyTo(sortKeys, 0);
			m_entriesMap.Values.CopyTo(sortValues, 0);

			// Sort in level order
			Array.Sort(sortKeys, sortValues, 0, sortKeys.Length, null);

			// Reverse list so that highest level is first
			Array.Reverse(sortValues, 0, sortValues.Length);

			foreach(LevelMappingEntry entry in sortValues)
			{
				entry.ActivateOptions();
			}

			 m_entries = sortValues;
		}

		#endregion // IOptionHandler Members

		#region Private Instance Fields

		private Hashtable m_entriesMap = new Hashtable();
		private LevelMappingEntry[] m_entries = null;

		#endregion // Private Instance Fields
	}
}
