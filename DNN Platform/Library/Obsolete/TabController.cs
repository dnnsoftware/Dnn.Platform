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
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Internal.SourceGenerators;
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
        [DnnDeprecated(10, 0, 0, "Please use overload with IBusinessControllerProvider")]
        public static partial void DeserializePanes(
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
        [DnnDeprecated(10, 0, 0, "Please use overload with IBusinessControllerProvider")]
        public static partial TabInfo DeserializeTab(
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
        /// <param name="isAdminTemplate">if set to <see langword="true"/> [is admin template].</param>
        /// <param name="mergeTabs">The merge tabs.</param>
        /// <param name="modules">The h modules.</param>
        /// <returns>The deserialized <see cref="TabInfo"/> instance.</returns>
        [DnnDeprecated(10, 0, 0, "Please use overload with IBusinessControllerProvider")]
        public static partial TabInfo DeserializeTab(
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
        [DnnDeprecated(10, 0, 0, "Please use overload with IBusinessControllerProvider")]
        public static partial XmlNode SerializeTab(XmlDocument tabXml, TabInfo objTab, bool includeContent)
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
        [DnnDeprecated(10, 0, 0, "Please use overload with IBusinessControllerProvider")]
        public static partial XmlNode SerializeTab(
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
        /// <param name="portalId">The portal ID.</param>
        /// <param name="tabId">The tab ID.</param>
        [DnnDeprecated(9, 11, 1, "Use AddMissingLanguagesWithWarnings")]
        public partial void AddMissingLanguages(int portalId, int tabId)
        {
            this.AddMissingLanguagesWithWarnings(portalId, tabId);
        }
    }
}
