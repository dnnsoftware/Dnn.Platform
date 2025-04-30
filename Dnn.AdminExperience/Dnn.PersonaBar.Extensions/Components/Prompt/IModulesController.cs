﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace Dnn.PersonaBar.Prompt.Components
{
    using System.Collections.Generic;
    using System.Net;

    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Internal.SourceGenerators;

    /// <inheritdoc cref="Dnn.PersonaBar.Library.Controllers.IModulesController"/>
    [DnnDeprecated(9, 2, 1, "Moved to Dnn.PersonaBar.Library.Controllers because of multiple dependency.")]
    public partial interface IModulesController
    {
        List<ModuleInfo> AddNewModule(PortalSettings portalSettings, string title, int desktopModuleId, int tabId, string paneName, int position, int permissionType, string align, out KeyValuePair<HttpStatusCode, string> message);

        ModuleInfo CopyModule(PortalSettings portalSettings, int moduleId, int sourcePageId, int targetPageId, string pane, bool includeSettings, out KeyValuePair<HttpStatusCode, string> message, bool moveBahaviour = false);

        void DeleteModule(PortalSettings portalSettings, int moduleId, int pageId, out KeyValuePair<HttpStatusCode, string> message);

        ModuleInfo GetModule(PortalSettings portalSettings, int moduleId, int? pageId, out KeyValuePair<HttpStatusCode, string> message);

        IEnumerable<ModuleInfo> GetModules(PortalSettings portalSettings, bool? deleted, out int total, string moduleName = null, string moduleTitle = null, int? pageId = null, int pageIndex = 0, int max = 10);
    }
}
