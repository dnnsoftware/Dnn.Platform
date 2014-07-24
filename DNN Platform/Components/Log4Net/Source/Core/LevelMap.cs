using log4net.Util;
using System;
using System.Collections;

namespace log4net.Core
{
	public sealed class LevelMap
	{
		private Hashtable m_mapName2Level = SystemInfo.CreateCaseInsensitiveHashtable();

		public LevelCollection AllLevels
		{
			get
			{
				LevelCollection levelCollections;
				lock (this)
				{
					levelCollections = new LevelCollection(this.m_mapName2Level.Values);
				}
				return levelCollections;
			}
		}

		public Level this[string name]
		{
			get
			{
				Level item;
				if (name == null)
				{
					throw new ArgumentNullException("name");
				}
				lock (this)
				{
					item = (Level)this.m_mapName2Level[name];
				}
				return item;
			}
		}

		public LevelMap()
		{
		}

		public void Add(string name, int value)
		{
			this.Add(name, value, null);
		}

		public void Add(string name, int value, string displayName)
		{
			if (name == null)
			{
				throw new ArgumentNullException("name");
			}
			if (name.Length == 0)
			{
				throw SystemInfo.CreateArgumentOutOfRangeException("name", name, string.Concat("Parameter: name, Value: [", name, "] out of range. Level name must not be empty"));
			}
			if (displayName == null || displayName.Length == 0)
			{
				displayName = name;
			}
			this.Add(new Level(value, name, displayName));
		}

		public void Add(Level level)
		{
			if (level == null)
			{
				throw new ArgumentNullException("level");
			}
			lock (this)
			{
				this.m_mapName2Level[level.Name] = level;
			}
		}

		public void Clear()
		{
			this.m_mapName2Level.Clear();
		}

		public Level LookupWithDefault(Level defaultLevel)
		{
			Level level;
			if (defaultLevel == null)
			{
				throw new ArgumentNullException("defaultLevel");
			}
			lock (this)
			{
				Level item = (Level)this.m_mapName2Level[defaultLevel.Name];
				if (item != null)
				{
					level = item;
				}
				else
				{
					this.m_mapName2Level[defaultLevel.Name] = defaultLevel;
					level = defaultLevel;
				}
			}
			return level;
		}
	}
}