// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Entities.Tabs
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.Serialization;
    using System.Security;

    using DotNetNuke.Collections;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Services.Localization;

    /// <summary>
    /// Represents the collection of Tabs for a portal.
    /// </summary>
    /// <remarks></remarks>
    [Serializable]
    public class TabCollection : Dictionary<int, TabInfo>
    {
        // This is used to provide a collection of children
        [NonSerialized]
        private readonly Dictionary<int, List<TabInfo>> _children;

        // This is used to return a sorted List
        [NonSerialized]
        private readonly List<TabInfo> _list;

        // This is used to provide a culture based set of tabs
        [NonSerialized]
        private readonly Dictionary<string, List<TabInfo>> _localizedTabs;

        public TabCollection()
        {
            this._list = new List<TabInfo>();
            this._children = new Dictionary<int, List<TabInfo>>();
            this._localizedTabs = new Dictionary<string, List<TabInfo>>();
        }

        // The special constructor is used to deserialize values.
        public TabCollection(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            this._list = new List<TabInfo>();
            this._children = new Dictionary<int, List<TabInfo>>();
            this._localizedTabs = new Dictionary<string, List<TabInfo>>();
        }

        public TabCollection(IEnumerable<TabInfo> tabs)
            : this()
        {
            this.AddRange(tabs);
        }

        public override void OnDeserialization(object sender)
        {
            base.OnDeserialization(sender);

            foreach (var tab in this.Values)
            {
                // Update all child collections
                this.AddInternal(tab);
            }
        }

        public void Add(TabInfo tab)
        {
            // Call base class to add to base Dictionary
            this.Add(tab.TabID, tab);

            // Update all child collections
            this.AddInternal(tab);
        }

        public void AddRange(IEnumerable<TabInfo> tabs)
        {
            foreach (TabInfo tab in tabs)
            {
                this.Add(tab);
            }
        }

        public List<TabInfo> AsList()
        {
            return this._list;
        }

        public List<TabInfo> DescendentsOf(int tabId)
        {
            return this.GetDescendants(tabId, Null.NullInteger);
        }

        public List<TabInfo> DescendentsOf(int tabId, int originalTabLevel)
        {
            return this.GetDescendants(tabId, originalTabLevel);
        }

        public bool IsDescendentOf(int ancestorId, int testTabId)
        {
            return this.DescendentsOf(ancestorId).Any(tab => tab.TabID == testTabId);
        }

        public ArrayList ToArrayList()
        {
            return new ArrayList(this._list);
        }

        public TabCollection WithCulture(string cultureCode, bool includeNeutral)
        {
            return this.WithCulture(cultureCode, includeNeutral, IsLocalizationEnabled());
        }

        public TabCollection WithCulture(string cultureCode, bool includeNeutral, bool localizationEnabled)
        {
            TabCollection collection;
            if (localizationEnabled)
            {
                if (string.IsNullOrEmpty(cultureCode))
                {
                    // No culture passed in - so return all tabs
                    collection = this;
                }
                else
                {
                    cultureCode = cultureCode.ToLowerInvariant();
                    List<TabInfo> tabs;
                    if (!this._localizedTabs.TryGetValue(cultureCode, out tabs))
                    {
                        collection = new TabCollection(new List<TabInfo>());
                    }
                    else
                    {
                        collection = !includeNeutral
                                        ? new TabCollection(from t in tabs
                                                            where t.CultureCode.ToLowerInvariant() == cultureCode
                                                            select t)
                                        : new TabCollection(tabs);
                    }
                }
            }
            else
            {
                // Return all tabs
                collection = this;
            }

            return collection;
        }

        public List<TabInfo> WithParentId(int parentId)
        {
            List<TabInfo> tabs;
            if (!this._children.TryGetValue(parentId, out tabs))
            {
                tabs = new List<TabInfo>();
            }

            return tabs;
        }

        public TabInfo WithTabId(int tabId)
        {
            TabInfo t = null;
            if (this.ContainsKey(tabId))
            {
                t = this[tabId];
            }

            return t;
        }

        public TabInfo WithTabNameAndParentId(string tabName, int parentId)
        {
            return (from t in this._list where t.TabName.Equals(tabName, StringComparison.InvariantCultureIgnoreCase) && t.ParentId == parentId select t).SingleOrDefault();
        }

        public TabInfo WithTabName(string tabName)
        {
            return (from t in this._list where !string.IsNullOrEmpty(t.TabName) && t.TabName.Equals(tabName, StringComparison.InvariantCultureIgnoreCase) select t).FirstOrDefault();
        }

        internal void RefreshCache(int tabId, TabInfo updatedTab)
        {
            if (this.ContainsKey(tabId))
            {
                if (updatedTab == null) // the tab has been deleted
                {
                    this.Remove(tabId);
                    this._list.RemoveAll(t => t.TabID == tabId);
                    this._localizedTabs.ForEach(kvp =>
                    {
                        kvp.Value.RemoveAll(t => t.TabID == tabId);
                    });
                    this._children.Remove(tabId);
                }
                else
                {
                    this[tabId] = updatedTab;
                    var index = this._list.FindIndex(t => t.TabID == tabId);
                    if (index > Null.NullInteger)
                    {
                        this._list[index] = updatedTab;
                    }

                    this._localizedTabs.ForEach(kvp =>
                    {
                        var localizedIndex = kvp.Value.FindIndex(t => t.TabID == tabId);
                        if (localizedIndex > Null.NullInteger)
                        {
                            kvp.Value[localizedIndex] = updatedTab;
                        }
                    });
                }
            }
        }

        private static bool IsLocalizationEnabled()
        {
            var portalSettings = PortalController.Instance.GetCurrentPortalSettings();
            return (portalSettings != null) ? portalSettings.ContentLocalizationEnabled : Null.NullBoolean;
        }

        private static bool IsLocalizationEnabled(int portalId)
        {
            return PortalController.GetPortalSettingAsBoolean("ContentLocalizationEnabled", portalId, false);
        }

        private void AddInternal(TabInfo tab)
        {
            if (tab.ParentId == Null.NullInteger)
            {
                // Add tab to Children collection
                this.AddToChildren(tab);

                // Add to end of List as all zero-level tabs are returned in order first
                this._list.Add(tab);
            }
            else
            {
                // Find Parent in list
                for (int index = 0; index <= this._list.Count - 1; index++)
                {
                    TabInfo parentTab = this._list[index];
                    if (parentTab.TabID == tab.ParentId)
                    {
                        int childCount = this.AddToChildren(tab);

                        // Insert tab in master List
                        this._list.Insert(index + childCount, tab);
                    }
                }
            }

            // Add to localized tabs
            if (tab.PortalID == Null.NullInteger || IsLocalizationEnabled(tab.PortalID))
            {
                this.AddToLocalizedTabs(tab);
            }
        }

        private int AddToChildren(TabInfo tab)
        {
            List<TabInfo> childList;
            if (!this._children.TryGetValue(tab.ParentId, out childList))
            {
                childList = new List<TabInfo>();
                this._children.Add(tab.ParentId, childList);
            }

            // Add tab to end of child list as children are returned in order
            childList.Add(tab);
            return childList.Count;
        }

        private void AddToLocalizedTabCollection(TabInfo tab, string cultureCode)
        {
            List<TabInfo> localizedTabCollection;

            var key = cultureCode.ToLowerInvariant();
            if (!this._localizedTabs.TryGetValue(key, out localizedTabCollection))
            {
                localizedTabCollection = new List<TabInfo>();
                this._localizedTabs.Add(key, localizedTabCollection);
            }

            // Add tab to end of localized tabs
            localizedTabCollection.Add(tab);
        }

        private void AddToLocalizedTabs(TabInfo tab)
        {
            if (string.IsNullOrEmpty(tab.CultureCode))
            {
                // Add to all cultures
                foreach (var locale in LocaleController.Instance.GetLocales(tab.PortalID).Values)
                {
                    this.AddToLocalizedTabCollection(tab, locale.Code);
                }
            }
            else
            {
                this.AddToLocalizedTabCollection(tab, tab.CultureCode);
            }
        }

        private List<TabInfo> GetDescendants(int tabId, int tabLevel)
        {
            var descendantTabs = new List<TabInfo>();
            for (int index = 0; index <= this._list.Count - 1; index++)
            {
                TabInfo parentTab = this._list[index];
                if (parentTab.TabID == tabId)
                {
                    // Found Parent - so add descendents
                    for (int descendantIndex = index + 1; descendantIndex <= this._list.Count - 1; descendantIndex++)
                    {
                        TabInfo descendantTab = this._list[descendantIndex];

                        if (tabLevel == Null.NullInteger)
                        {
                            tabLevel = parentTab.Level;
                        }

                        if (descendantTab.Level > tabLevel)
                        {
                            // Descendant so add to collection
                            descendantTabs.Add(descendantTab);
                        }
                        else
                        {
                            break;
                        }
                    }

                    break;
                }
            }

            return descendantTabs;
        }
    }
}
