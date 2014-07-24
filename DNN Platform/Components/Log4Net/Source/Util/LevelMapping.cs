using log4net.Core;
using System;
using System.Collections;

namespace log4net.Util
{
	public sealed class LevelMapping : IOptionHandler
	{
		private Hashtable m_entriesMap = new Hashtable();

		private LevelMappingEntry[] m_entries;

		public LevelMapping()
		{
		}

		public void ActivateOptions()
		{
			Level[] levelArray = new Level[this.m_entriesMap.Count];
			LevelMappingEntry[] levelMappingEntryArray = new LevelMappingEntry[this.m_entriesMap.Count];
			this.m_entriesMap.Keys.CopyTo(levelArray, 0);
			this.m_entriesMap.Values.CopyTo(levelMappingEntryArray, 0);
			Array.Sort<Level, LevelMappingEntry>(levelArray, levelMappingEntryArray, 0, (int)levelArray.Length, null);
			Array.Reverse(levelMappingEntryArray, 0, (int)levelMappingEntryArray.Length);
			LevelMappingEntry[] levelMappingEntryArray1 = levelMappingEntryArray;
			for (int i = 0; i < (int)levelMappingEntryArray1.Length; i++)
			{
				levelMappingEntryArray1[i].ActivateOptions();
			}
			this.m_entries = levelMappingEntryArray;
		}

		public void Add(LevelMappingEntry entry)
		{
			if (this.m_entriesMap.ContainsKey(entry.Level))
			{
				this.m_entriesMap.Remove(entry.Level);
			}
			this.m_entriesMap.Add(entry.Level, entry);
		}

		public LevelMappingEntry Lookup(Level level)
		{
			if (this.m_entries != null)
			{
				LevelMappingEntry[] mEntries = this.m_entries;
				for (int i = 0; i < (int)mEntries.Length; i++)
				{
					LevelMappingEntry levelMappingEntry = mEntries[i];
					if (level >= levelMappingEntry.Level)
					{
						return levelMappingEntry;
					}
				}
			}
			return null;
		}
	}
}