#region Copyright
// 
// DotNetNuke� - http://www.dotnetnuke.com
// Copyright (c) 2002-2016
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
#region Usings

using System;

using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Modules.Definitions;
using DotNetNuke.Instrumentation;
using DotNetNuke.Services.Upgrade;


#endregion

namespace DotNetNuke.Modules.Taxonomy
{
    public class TaxonomyController : IUpgradeable
    {
    	private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof (TaxonomyController));
        #region IUpgradeable Members

        public string UpgradeModule(string Version)
        {
            try
            {
                switch (Version)
                {
                    case "01.00.00":
                        ModuleDefinitionInfo moduleDefinition = ModuleDefinitionController.GetModuleDefinitionByFriendlyName("Taxonomy Manager");

                        if (moduleDefinition != null)
                        {
                            //Add Module to Admin Page for all Portals
                            Upgrade.AddAdminPages("Taxonomy",
                                                  "Manage the Taxonomy for your Site",
                                                  "~/images/icon_tag_16px.gif",
                                                  "~/images/icon_tag_32px.gif",
                                                  true,
                                                  moduleDefinition.ModuleDefID,
                                                  "Taxonomy Manager",
                                                  "~/images/icon_tag_32px.gif",
                                                  true);
                        }
                        break;

                    case "06.00.00":
                        DesktopModuleInfo desktopModule = DesktopModuleController.GetDesktopModuleByModuleName("DotNetNuke.Taxonomy", Null.NullInteger);
                        desktopModule.Category = "Admin";
                        DesktopModuleController.SaveDesktopModule(desktopModule, false, false);
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