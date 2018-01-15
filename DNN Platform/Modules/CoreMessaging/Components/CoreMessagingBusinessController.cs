#region Copyright

// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2018
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

#endregion

using System;
using System.Collections.Generic;

using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Modules.Definitions;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Instrumentation;
using DotNetNuke.Services.Upgrade;

namespace DotNetNuke.Modules.CoreMessaging.Components
{
    public class CoreMessagingBusinessController : IUpgradeable
    {
    	private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof (CoreMessagingBusinessController));
        #region Implementation of IUpgradeable

        public string UpgradeModule(string Version)
        {
            try
            {
                switch (Version)
                {
                    case "06.02.00":
                        var moduleDefinition = ModuleDefinitionController.GetModuleDefinitionByFriendlyName("Message Center");
                        if (moduleDefinition != null)
                        {
                            var portals = PortalController.Instance.GetPortals();
                            foreach (PortalInfo portal in portals)
                            {
                                if (portal.UserTabId > Null.NullInteger)
                                {
                                    //Find TabInfo
                                    var tab = TabController.Instance.GetTab(portal.UserTabId, portal.PortalID, true);
                                    if (tab != null)
                                    {
                                        foreach (var module in ModuleController.Instance.GetTabModules(portal.UserTabId).Values)
                                        {
                                            if (module.DesktopModule.FriendlyName == "Messaging")
                                            {
                                                //Delete the Module from the Modules list
                                                ModuleController.Instance.DeleteTabModule(module.TabID, module.ModuleID, false);

                                                //Add new module to the page
                                                Upgrade.AddModuleToPage(tab, moduleDefinition.ModuleDefID, "Message Center", "", true);

                                                break;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        break;
                }
                return "Success";
            }
            catch (Exception exc)
            {
                Logger.Error(exc);

                return "Failed";
            }
        }

        #endregion
    }
}