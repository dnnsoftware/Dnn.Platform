// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2017
// by DotNetNuke Corporation
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation 
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and 
// to permit persons to whom the Software is furnished to do so, subject to the following conditions:
//  
// The above copyright notice and this permission notice shall be included in all copies or substantial portions 
// of the Software.
//  
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
// TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.

using System.Globalization;
using System.Linq;
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

namespace DotNetNuke.Web.Api.Internal
{
    public class DnnContextMessageHandler : MessageProcessingHandler
    {
        protected override HttpRequestMessage ProcessRequest(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var portalSettings = SetupPortalSettings(request);
            SetThreadCulture(portalSettings);

            return request;
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

            int tabId = request.FindTabId();
            if(tabId != Null.NullInteger)
            {
                //check to see if the portal exists on the same portal as detecte
                //by the alias. This can be wrong if the calling portal is a child
                //portal
                var tab = TabController.Instance.GetTab(tabId, -1);
                if (tab != null)
                {
                    if (tab.PortalID != alias.PortalID)
                    {
                        //the portal we got from the alias isn't correct. 
                        //let check to see we have alias for the child
                        //that would do this, if so lets just correct the
                        //portal alias otherwise we can have authenication
                        //issues see Bug7858

                        var childPortalAlias = PortalAliasController
                                                        .Instance
                                                        .GetPortalAliasesByPortalId(tab.PortalID);

                        //check we have something which starts with the domainName as a doublecheck
                        if(childPortalAlias.Any(a => a.HTTPAlias.Contains(domainName)))
                        {
                            //we do have a match so lets based on the partial match and the fact
                            //the tabId object is telling us that this is the correct portalId lets
                            //just use that
                            alias = childPortalAlias.First();
                        }
                    }
                }

            }



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

        protected override HttpResponseMessage ProcessResponse(HttpResponseMessage response, CancellationToken cancellationToken)
        {
            return response;
        }
    }
}