// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Entities.Tabs
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Xml;

    using DotNetNuke.Abstractions.Modules;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Services.Localization;

    using Microsoft.Extensions.DependencyInjection;

    /// <content>Deprecated TabController methods.</content>
    public partial class TabController
    {
        /// <summary>Processes all panes and modules in the template file.</summary>
        /// <param name="nodePanes">Template file node for the panes is current tab.</param>
        /// <param name="portalId">PortalId of the new portal.</param>
        /// <param name="tabId">Tab being processed.</param>
        /// <param name="mergeTabs">Tabs need to merge.</param>
        /// <param name="hModules">Modules Hashtable.</param>
        [Obsolete("Deprecated in DotNetNuke 10.0.0. Please use overload with IBusinessControllerProvider. Scheduled removal in v12.0.0.")]
        public static void DeserializePanes(
            XmlNode nodePanes,
            int portalId,
            int tabId,
            PortalTemplateModuleAction mergeTabs,
            Hashtable hModules)
        {
            using var scope = Globals.GetOrCreateServiceScope();
            DeserializePanes(scope.ServiceProvider.GetRequiredService<IBusinessControllerProvider>(), nodePanes, portalId, tabId, mergeTabs, hModules);
        }

        /// <summary>Deserializes the tab.</summary>
        /// <param name="tabNode">The node tab.</param>
        /// <param name="tab">The obj tab.</param>
        /// <param name="portalId">The portal id.</param>
        /// <param name="mergeTabs">The merge tabs.</param>
        /// <returns>The deserialized <see cref="TabInfo"/> instance.</returns>
        [Obsolete("Deprecated in DotNetNuke 10.0.0. Please use overload with IBusinessControllerProvider. Scheduled removal in v12.0.0.")]
        public static TabInfo DeserializeTab(
            XmlNode tabNode,
            TabInfo tab,
            int portalId,
            PortalTemplateModuleAction mergeTabs)
        {
            using var scope = Globals.GetOrCreateServiceScope();
            return DeserializeTab(scope.ServiceProvider.GetRequiredService<IBusinessControllerProvider>(), tabNode, tab, portalId, mergeTabs);
        }

        /// <summary>Deserializes the tab.</summary>
        /// <param name="tabNode">The node tab.</param>
        /// <param name="tab">The obj tab.</param>
        /// <param name="tabs">The h tabs.</param>
        /// <param name="portalId">The portal id.</param>
        /// <param name="isAdminTemplate">if set to <c>true</c> [is admin template].</param>
        /// <param name="mergeTabs">The merge tabs.</param>
        /// <param name="modules">The h modules.</param>
        /// <returns>The deserialized <see cref="TabInfo"/> instance.</returns>
        [Obsolete("Deprecated in DotNetNuke 10.0.0. Please use overload with IBusinessControllerProvider. Scheduled removal in v12.0.0.")]
        public static TabInfo DeserializeTab(
            XmlNode tabNode,
            TabInfo tab,
            Hashtable tabs,
            int portalId,
            bool isAdminTemplate,
            PortalTemplateModuleAction mergeTabs,
            Hashtable modules)
        {
            using var scope = Globals.GetOrCreateServiceScope();
            return DeserializeTab(scope.ServiceProvider.GetRequiredService<IBusinessControllerProvider>(), tabNode, tab, tabs, portalId, isAdminTemplate, mergeTabs, modules);
        }

        /// <summary>Serializes the metadata of a page and its modules (and optionally the modules' contents) to an XML node.</summary>
        /// <param name="tabXml">The Xml Document to use for the Tab.</param>
        /// <param name="objTab">The TabInfo object to serialize.</param>
        /// <param name="includeContent">A flag used to determine if the Module content is included.</param>
        /// <returns>An <see cref="XmlNode"/> representing the page's data.</returns>
        [Obsolete("Deprecated in DotNetNuke 10.0.0. Please use overload with IBusinessControllerProvider. Scheduled removal in v12.0.0.")]
        public static XmlNode SerializeTab(XmlDocument tabXml, TabInfo objTab, bool includeContent)
        {
            using var scope = Globals.GetOrCreateServiceScope();
            return SerializeTab(
                scope.ServiceProvider.GetRequiredService<IBusinessControllerProvider>(),
                tabXml,
                objTab,
                includeContent);
        }

        /// <summary>Serializes the metadata of a page and its modules (and optionally the modules' contents) to an XML node.</summary>
        /// <param name="tabXml">The Xml Document to use for the Tab.</param>
        /// <param name="tabs">A Hashtable used to store the names of the tabs.</param>
        /// <param name="tab">The TabInfo object to serialize.</param>
        /// <param name="portal">The Portal object to which the tab belongs.</param>
        /// <param name="includeContent">A flag used to determine if the Module content is included.</param>
        /// <returns>An <see cref="XmlNode"/> representing the page's data.</returns>
        [Obsolete("Deprecated in DotNetNuke 10.0.0. Please use overload with IBusinessControllerProvider. Scheduled removal in v12.0.0.")]
        public static XmlNode SerializeTab(
            XmlDocument tabXml,
            Hashtable tabs,
            TabInfo tab,
            PortalInfo portal,
            bool includeContent)
        {
            using var scope = Globals.GetOrCreateServiceScope();
            return SerializeTab(
                scope.ServiceProvider.GetRequiredService<IBusinessControllerProvider>(),
                tabXml,
                tabs,
                tab,
                portal,
                includeContent);
        }

        /// <summary>Adds localized copies of the page in all missing languages.</summary>
        /// <param name="portalId"></param>
        /// <param name="tabId"></param>
        [Obsolete("This has been deprecated in favor of AddMissingLanguagesWithWarnings. Scheduled for removal in v11.0.0")]
        public void AddMissingLanguages(int portalId, int tabId)
        {
            this.AddMissingLanguagesWithWarnings(portalId, tabId);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Deprecated in DotNetNuke 7.3. RUse alternate overload. Scheduled removal in v10.0.0.")]
        public void CreateLocalizedCopy(List<TabInfo> tabs, Locale locale)
        {
            foreach (TabInfo t in tabs)
            {
                this.CreateLocalizedCopy(t, locale, true);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Deprecated in DotNetNuke 7.3. RUse alternate overload. Scheduled removal in v10.0.0.")]
        public void CreateLocalizedCopy(TabInfo originalTab, Locale locale)
        {
            this.CreateLocalizedCopy(originalTab, locale, true);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Deprecated in DNN 7.3. Method is not scalable. Use GetTabsByPortal. Scheduled removal in v10.0.0.")]
        public ArrayList GetAllTabs()
        {
            return CBO.FillCollection(this.dataProvider.GetAllTabs(), typeof(TabInfo));
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Deprecated in DNN 7.3. Method is not neccessary.  Use LINQ and GetPortalTabs(). Scheduled removal in v10.0.0.")]
        public List<TabInfo> GetCultureTabList(int portalid)
        {
            return (from kvp in this.GetTabsByPortal(portalid)
                    where !kvp.Value.TabPath.StartsWith("//Admin")
                          && kvp.Value.CultureCode == PortalController.Instance.GetCurrentPortalSettings().DefaultLanguage
                          && !kvp.Value.IsDeleted
                    select kvp.Value).ToList();
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Deprecated in DNN 7.3. Method is not neccessary.  Use LINQ and GetPortalTabs(). Scheduled removal in v10.0.0.")]
        public List<TabInfo> GetDefaultCultureTabList(int portalid)
        {
            return (from kvp in this.GetTabsByPortal(portalid)
                    where !kvp.Value.TabPath.StartsWith("//Admin")
                          && !kvp.Value.IsDeleted
                    select kvp.Value).ToList();
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("This method is obsolete.  It has been replaced by GetTab(ByVal TabId As Integer, ByVal PortalId As Integer, ByVal ignoreCache As Boolean) . Scheduled removal in v10.0.0.")]
        public TabInfo GetTab(int tabId)
        {
            return this.GetTab(tabId, GetPortalId(tabId, Null.NullInteger), false);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Deprecated in DNN 7.3. Use LINQ queries on tab collections thata re cached. Scheduled removal in v10.0.0.")]
        public TabInfo GetTabByUniqueID(Guid uniqueID)
        {
            return CBO.FillObject<TabInfo>(this.dataProvider.GetTabByUniqueID(uniqueID));
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Deprecated in DNN 7.3. Use GetTabsByPortal(portalId).Count. Scheduled removal in v10.0.0.")]
        public int GetTabCount(int portalId)
        {
            return this.GetTabsByPortal(portalId).Count;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("This method is obsolete.  It has been replaced by GetTabsByParent(ByVal ParentId As Integer, ByVal PortalId As Integer) . Scheduled removal in v10.0.0.")]
        public ArrayList GetTabsByParentId(int parentId)
        {
            return new ArrayList(GetTabsByParent(parentId, GetPortalId(parentId, Null.NullInteger)));
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Deprecated in DNN 7.3. Use one of the alternate MoveTabxxx methods). Scheduled removal in v10.0.0.")]
        public void MoveTab(TabInfo tab, TabMoveType type)
        {
            // Get the List of tabs with the same parent
            IOrderedEnumerable<TabInfo> siblingTabs = this.GetSiblingTabs(tab).OrderBy(t => t.TabOrder);
            int tabIndex = GetIndexOfTab(tab, siblingTabs);
            switch (type)
            {
                case TabMoveType.Top:
                    this.MoveTabBefore(tab, siblingTabs.First().TabID);
                    break;
                case TabMoveType.Bottom:
                    this.MoveTabAfter(tab, siblingTabs.Last().TabID);
                    break;
                case TabMoveType.Up:
                    this.MoveTabBefore(tab, siblingTabs.ElementAt(tabIndex - 1).TabID);
                    break;
                case TabMoveType.Down:
                    this.MoveTabAfter(tab, siblingTabs.ElementAt(tabIndex + 1).TabID);
                    break;
                case TabMoveType.Promote:
                    this.MoveTabAfter(tab, tab.ParentId);
                    break;
                case TabMoveType.Demote:
                    this.MoveTabToParent(tab, siblingTabs.ElementAt(tabIndex - 1).TabID);
                    break;
            }

            this.ClearCache(tab.PortalID);
        }
    }
}
