// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Entities.Tabs
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Portals;

    public class PageHeaderTagInfo
    {
        public const string SettingPrefix = "PageHeaderTag_";

        public string Name { get; set; }

        public string Content { get; set; }

        public string SettingName => SettingPrefix + this.Name;

        public static IList<PageHeaderTagInfo> FromSettings(IDictionary settings)
        {
            var items = new List<PageHeaderTagInfo>();
            if (settings == null)
            {
                return items;
            }

            foreach (DictionaryEntry entry in settings)
            {
                var key = Convert.ToString(entry.Key, CultureInfo.InvariantCulture);
                if (string.IsNullOrEmpty(key) || !key.StartsWith(SettingPrefix, StringComparison.Ordinal))
                {
                    continue;
                }

                var name = key.Substring(SettingPrefix.Length);
                if (string.IsNullOrWhiteSpace(name))
                {
                    continue;
                }

                items.Add(new PageHeaderTagInfo
                {
                    Name = name,
                    Content = Convert.ToString(entry.Value, CultureInfo.InvariantCulture),
                });
            }

            return items.OrderBy(item => item.Name, StringComparer.OrdinalIgnoreCase).ToList();
        }

        public static IList<PageHeaderTagInfo> GetTabItems(int tabId)
        {
            return FromSettings(TabController.Instance.GetTabSettings(tabId));
        }

        public static IList<PageHeaderTagInfo> GetPortalItems(int portalId, string cultureCode = null)
        {
            var settings = string.IsNullOrEmpty(cultureCode)
                ? PortalController.Instance.GetPortalSettings(portalId)
                : PortalController.Instance.GetPortalSettings(portalId, cultureCode);

            return FromSettings(new Hashtable(settings));
        }

        public static string Render(IEnumerable<PageHeaderTagInfo> items)
        {
            if (items == null)
            {
                return string.Empty;
            }

            return string.Join(Environment.NewLine, items.Select(item => item.Content).Where(content => !string.IsNullOrWhiteSpace(content)));
        }

        public static void SaveTabItems(int tabId, IEnumerable<PageHeaderTagInfo> items)
        {
            var normalizedItems = Normalize(items);
            var targetSettingNames = new HashSet<string>(normalizedItems.Select(item => item.SettingName), StringComparer.OrdinalIgnoreCase);
            var existingSettingNames = TabController.Instance.GetTabSettings(tabId)
                .Cast<DictionaryEntry>()
                .Select(entry => Convert.ToString(entry.Key, CultureInfo.InvariantCulture))
                .Where(key => !string.IsNullOrEmpty(key))
                .ToList();

            foreach (var key in existingSettingNames)
            {
                if (key.StartsWith(SettingPrefix, StringComparison.Ordinal) && !targetSettingNames.Contains(key))
                {
                    TabController.Instance.DeleteTabSetting(tabId, key);
                }
            }

            foreach (var item in normalizedItems)
            {
                TabController.Instance.UpdateTabSetting(tabId, item.SettingName, item.Content);
            }
        }

        public static void SavePortalItems(int portalId, IEnumerable<PageHeaderTagInfo> items, string cultureCode = null)
        {
            var normalizedItems = Normalize(items);
            var targetSettingNames = new HashSet<string>(normalizedItems.Select(item => item.SettingName), StringComparer.OrdinalIgnoreCase);
            var existingSettingNames = (string.IsNullOrEmpty(cultureCode)
                ? PortalController.Instance.GetPortalSettings(portalId)
                : PortalController.Instance.GetPortalSettings(portalId, cultureCode))
                .Keys
                .Where(key => !string.IsNullOrEmpty(key))
                .ToList();

            foreach (var key in existingSettingNames)
            {
                if (key.StartsWith(SettingPrefix, StringComparison.Ordinal) && !targetSettingNames.Contains(key))
                {
                    PortalController.DeletePortalSetting(portalId, key);
                }
            }

            foreach (var item in normalizedItems)
            {
                PortalController.Instance.UpdatePortalSetting(portalId, item.SettingName, item.Content, true, cultureCode ?? Null.NullString);
            }
        }

        private static List<PageHeaderTagInfo> Normalize(IEnumerable<PageHeaderTagInfo> items)
        {
            if (items == null)
            {
                return new List<PageHeaderTagInfo>();
            }

            return items
                .Where(item => item != null)
                .Select(item => new PageHeaderTagInfo
                {
                    Name = (item.Name ?? string.Empty).Trim(),
                    Content = item.Content ?? string.Empty,
                })
                .Where(item => !string.IsNullOrWhiteSpace(item.Name) && !string.IsNullOrWhiteSpace(item.Content))
                .GroupBy(item => item.Name, StringComparer.OrdinalIgnoreCase)
                .Select(group => group.Last())
                .OrderBy(item => item.Name, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }
    }
}
