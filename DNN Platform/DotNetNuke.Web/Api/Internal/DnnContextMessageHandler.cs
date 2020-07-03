// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.Api.Internal
{
    using System.Globalization;
    using System.Net;
    using System.Net.Http;
    using System.Threading;
    using System.Web.Http;

    using DotNetNuke.Common.Internal;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.Services.Localization.Internal;

    public class DnnContextMessageHandler : MessageProcessingHandler
    {
        protected override HttpRequestMessage ProcessRequest(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var portalSettings = SetupPortalSettings(request);
            SetThreadCulture(portalSettings);

            return request;
        }

        protected override HttpResponseMessage ProcessResponse(HttpResponseMessage response, CancellationToken cancellationToken)
        {
            return response;
        }

        private static void SetThreadCulture(PortalSettings portalSettings)
        {
            CultureInfo pageLocale = TestableLocalization.Instance.GetPageLocale(portalSettings);
            if (pageLocale != null)
            {
                TestableLocalization.Instance.SetThreadCultures(pageLocale, portalSettings);
            }
        }

        private static PortalSettings SetupPortalSettings(HttpRequestMessage request)
        {
            var domainName = TestableGlobals.Instance.GetDomainName(request.RequestUri);
            var alias = PortalAliasController.Instance.GetPortalAlias(domainName);

            int tabId;
            ValidateTabAndModuleContext(request, alias.PortalID, out tabId);

            var portalSettings = new PortalSettings(tabId, alias);

            request.GetHttpContext().Items["PortalSettings"] = portalSettings;
            return portalSettings;
        }

        private static bool TabIsInPortalOrHost(int tabId, int portalId)
        {
            var tab = TabController.Instance.GetTab(tabId, portalId);

            return tab != null && (IsHostTab(tab) || tab.PortalID == portalId || InSamePortalGroup(tab.PortalID, portalId));
        }

        private static bool InSamePortalGroup(int portalId1, int portalId2)
        {
            var portal1 = PortalController.Instance.GetPortal(portalId1);
            var portal2 = PortalController.Instance.GetPortal(portalId2);

            return portal1 != null
                       && portal2 != null
                       && portal1.PortalGroupID > Null.NullInteger
                       && portal2.PortalGroupID > Null.NullInteger
                       && portal1.PortalGroupID == portal2.PortalGroupID;
        }

        private static bool IsHostTab(TabInfo tab)
        {
            return tab.PortalID == Null.NullInteger;
        }

        private static void ValidateTabAndModuleContext(HttpRequestMessage request, int portalId, out int tabId)
        {
            tabId = request.FindTabId();

            if (tabId != Null.NullInteger)
            {
                if (!TabIsInPortalOrHost(tabId, portalId))
                {
                    throw new HttpResponseException(request.CreateErrorResponse(HttpStatusCode.BadRequest, Localization.GetString("TabNotInPortal", Localization.ExceptionsResourceFile)));
                }

                int moduleId = request.FindModuleId();

                if (moduleId != Null.NullInteger)
                {
                    var module = ModuleController.Instance.GetModule(moduleId, tabId, false);
                    if (module == null)
                    {
                        throw new HttpResponseException(request.CreateErrorResponse(HttpStatusCode.BadRequest, Localization.GetString("TabModuleNotExist", Localization.ExceptionsResourceFile)));
                    }
                }
            }
        }
    }
}
