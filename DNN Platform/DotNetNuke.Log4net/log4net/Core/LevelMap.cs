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
using System.Collections.Specialized;

using log4net.Util;

namespace log4net.Core
{
	/// <summary>
	/// Mapping between string name and Level object
	/// </summary>
	/// <remarks>
	/// <para>
	/// Mapping between string name and <see cref="Level"/> object.
	/// This mapping is held separately for each <see cref="log4net.Repository.ILoggerRepository"/>.
	/// The level name is case insensitive.
	/// </para>
	/// </remarks>
	/// <author>Nicko Cadell</author>
	public sealed class LevelMap
	{
		#region Member Variables

		/// <summary>
		/// Mapping from level name to Level object. The
		/// level name is case insensitive
		/// </summary>
		private Hashtable m_mapName2Level = SystemInfo.CreateCaseInsensitiveHashtable();

		#endregion

		/// <summary>
		/// Construct the level map
		/// </summary>
		/// <remarks>
		/// <para>
		/// Construct the level map.
		/// </para>
		/// </remarks>
		public LevelMap()
		{
		}

		/// <summary>
		/// Clear the internal maps of all levels
		/// </summary>
		/// <remarks>
		/// <para>
		/// Clear the internal maps of all levels
		/// </para>
		/// </remarks>
		public void Clear()
		{
			// Clear all current levels
			m_mapName2Level.Clear();
		}

		/// <summary>
		/// Lookup a <see cref="Level"/> by name
		/// </summary>
		/// <param name="name">The name of the Level to lookup</param>
		/// <returns>a Level from the map with the name specified</returns>
		/// <remarks>
		/// <para>
		/// Returns the <see cref="Level"/> from the
		/// map with the name specified. If the no level is
		/// found then <c>null</c> is returned.
		/// </para>
		/// </remarks>
		public Level this[string name]
		{
			get
			{
				if (name == null)
				{
					throw new ArgumentNullException("name");
				}

				lock(this)
				{
					return (Level)m_mapName2Level[name];
				}
			}
		}

		/// <summary>
		/// Create a new Level and add it to the map
		/// </summary>
		/// <param name="name">the string to display for the Level</param>
		/// <param name="value">the level value to give to the Level</param>
		/// <remarks>
		/// <para>
		/// Create a new Level and add it to the map
		/// </para>
		/// </remarks>
		/// <seealso cref="M:Add(string,int,string)"/>
		public void Add(string name, int value)
		{
			Add(name, value, null);
		}

		/// <summary>
		/// Create a new Level and add it to the map
		/// </summary>
		/// <param name="name">the string to display for the Level</param>
		/// <param name="value">the level value to give to the Level</param>
		/// <param name="displayName">the display name to give to the Level</param>
		/// <remarks>
		/// <para>
		/// Create a new Level and add it to the map
		/// </para>
		/// </remarks>
		public void Add(string name, int value, string displayName)
		{
			if (name == null)
			{
				throw new ArgumentNullException("name");
			}
			if (name.Length == 0)
			{
				throw log4net.Util.SystemInfo.CreateArgumentOutOfRangeException("name", name, "Parameter: name, Value: ["+name+"] out of range. Level name must not be empty");
			}

			if (displayName == null || displayName.Length == 0)
			{
				displayName = name;
			}

			Add(new Level(value, name, displayName));
		}

		/// <summary>
		/// Add a Level to the map
		/// </summary>
		/// <param name="level">the Level to add</param>
		/// <remarks>
		/// <para>
		/// Add a Level to the map
		/// </para>
		/// </remarks>
		public void Add(Level level)
		{
			if (level == null)
			{
				throw new ArgumentNullException("level");
			}
			lock(this)
			{
				m_mapName2Level[level.Name] = level;
			}
		}

		/// <summary>
		/// Return all possible levels as a list of Level objects.
		/// </summary>
		/// <returns>all possible levels as a list of Level objects</returns>
		/// <remarks>
		/// <para>
		/// Return all possible levels as a list of Level objects.
		/// </para>
		/// </remarks>
		public LevelCollection AllLevels
		{
			get
			{
				lock(this)
				{
					return new LevelCollection(m_mapName2Level.Values);
				}
			}
		}

		/// <summary>
		/// Lookup a named level from the map
		/// </summary>
		/// <param name="defaultLevel">the name of the level to lookup is taken from this level. 
		/// If the level is not set on the map then this level is added</param>
		/// <returns>the level in the map with the name specified</returns>
		/// <remarks>
		/// <para>
		/// Lookup a named level from the map. The name of the level to lookup is taken
		/// from the <see cref="Level.Name"/> property of the <paramref name="defaultLevel"/>
		/// argument.
		/// </para>
		/// <para>
		/// If no level with the specified name is found then the 
		/// <paramref name="defaultLevel"/> argument is added to the level map
		/// and returned.
		/// </para>
		/// </remarks>
		public Level LookupWithDefault(Level defaultLevel)
		{
			if (defaultLevel == null)
			{
				throw new ArgumentNullException("defaultLevel");
			}

			lock(this)
			{
				Level level = (Level)m_mapName2Level[defaultLevel.Name];
				if (level == null)
				{
					m_mapName2Level[defaultLevel.Name] = defaultLevel;
					return defaultLevel;
				}
				return level;
			}
		}
	}
}
