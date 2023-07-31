// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Entities.Modules
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Data;
    using System.Linq;
    using System.Xml;

    using DotNetNuke.Abstractions.Modules;
    using DotNetNuke.Common;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Internal.SourceGenerators;
    using DotNetNuke.Services.Exceptions;
    using DotNetNuke.Services.Log.EventLog;

    using Microsoft.Extensions.DependencyInjection;

    /// <content>The deprecated methods for ModuleController.</content>
    public partial class ModuleController
    {
        /// <summary>Deserializes the module.</summary>
        /// <param name="nodeModule">The node module.</param>
        /// <param name="module">ModuleInfo of current module.</param>
        /// <param name="portalId">The portal id.</param>
        /// <param name="tabId">The tab id.</param>
        [DnnDeprecated(10, 0, 0, "Please use overload with IBusinessControllerProvider")]
        public static partial void DeserializeModule(
            XmlNode nodeModule,
            ModuleInfo module,
            int portalId,
            int tabId)
        {
            using var scope = Globals.GetOrCreateServiceScope();
            DeserializeModule(scope.ServiceProvider.GetRequiredService<IBusinessControllerProvider>(), nodeModule, module, portalId, tabId);
        }

        /// <summary>Deserializes the module.</summary>
        /// <param name="nodeModule">The node module.</param>
        /// <param name="nodePane">The node pane.</param>
        /// <param name="portalId">The portal id.</param>
        /// <param name="tabId">The tab id.</param>
        /// <param name="mergeTabs">The merge tabs.</param>
        /// <param name="hModules">The modules.</param>
        [DnnDeprecated(10, 0, 0, "Please use overload with IBusinessControllerProvider")]
        public static partial void DeserializeModule(
            XmlNode nodeModule,
            XmlNode nodePane,
            int portalId,
            int tabId,
            PortalTemplateModuleAction mergeTabs,
            Hashtable hModules)
        {
            using var scope = Globals.GetOrCreateServiceScope();
            DeserializeModule(scope.ServiceProvider.GetRequiredService<IBusinessControllerProvider>(), nodeModule, nodePane, portalId, tabId, mergeTabs, hModules);
        }

        /// <summary>Serializes the metadata of a module (and optionally its contents) to an XML node.</summary>
        /// <param name="xmlModule">The XML Document to use for the Module.</param>
        /// <param name="module">The ModuleInfo object to serialize.</param>
        /// <param name="includeContent">A flag that determines whether the content of the module is serialized.</param>
        /// <returns>An <see cref="XmlNode"/> representing the module.</returns>
        [DnnDeprecated(10, 0, 0, "Please use overload with IBusinessControllerProvider")]
        public static partial XmlNode SerializeModule(XmlDocument xmlModule, ModuleInfo module, bool includeContent)
        {
            using var scope = Globals.GetOrCreateServiceScope();
            return SerializeModule(scope.ServiceProvider.GetRequiredService<IBusinessControllerProvider>(), xmlModule, module, includeContent);
        }
    }
}
