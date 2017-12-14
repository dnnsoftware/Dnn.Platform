#region Copyright
// 
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
#endregion
using System;
using System.IO;
using System.Web;
using System.Web.UI;

using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Services.Localization;
using DotNetNuke.UI.ControlPanels;
using DotNetNuke.UI.Modules;
using DotNetNuke.UI.Skins;

namespace DotNetNuke.UI
{
    public class UIUtilities
    {
        internal static string GetControlKey()
        {
            HttpRequest request = HttpContext.Current.Request;

            string key = "";
            if (request.QueryString["ctl"] != null)
            {
                key = request.QueryString["ctl"];
            }
            return key;
        }

        internal static int GetModuleId(string key)
        {
            HttpRequest request = HttpContext.Current.Request;

            int moduleId = -1;
            if (request.QueryString["mid"] != null)
            {
                Int32.TryParse(request.QueryString["mid"], out moduleId);
            }
            if (request.QueryString["moduleid"] != null && (key.ToLower() == "module" || key.ToLower() == "help"))
            {
                Int32.TryParse(request.QueryString["moduleid"], out moduleId);
            }

            return moduleId;
        }

        internal static string GetRenderMode()
        {
            HttpRequest request = HttpContext.Current.Request;

            var renderMode = "";
            if (request.QueryString["render"] != null)
            {
                renderMode = request.QueryString["render"];
            }
            return renderMode;
        }

        internal static ModuleInfo GetSlaveModule(int moduleId, string key, int tabId)
        {
            HttpRequest request = HttpContext.Current.Request;

            ModuleInfo slaveModule = null;
            if (moduleId != -1)
            {
                ModuleInfo module = ModuleController.Instance.GetModule(moduleId, tabId, false);
                if (module != null)
                {
                    slaveModule = module.Clone();
                }
            }

            if (slaveModule == null)
            {
                slaveModule = (new ModuleInfo {ModuleID = moduleId, ModuleDefID = -1, TabID = tabId, InheritViewPermissions = true});
            }

            if (request.QueryString["moduleid"] != null && (key.ToLower() == "module" || key.ToLower() == "help"))
            {
                slaveModule.ModuleDefID = -1;
            }

            if (request.QueryString["dnnprintmode"] != "true")
            {
                slaveModule.ModuleTitle = "";
            }

            slaveModule.Header = "";
            slaveModule.Footer = "";
            slaveModule.StartDate = DateTime.MinValue;
            slaveModule.EndDate = DateTime.MaxValue;
            slaveModule.Visibility = VisibilityState.None;
            slaveModule.Color = "";
            slaveModule.Border = "";
            slaveModule.DisplayTitle = true;
            slaveModule.DisplayPrint = false;
            slaveModule.DisplaySyndicate = false;

            return slaveModule;
        }

        public static ModuleInfo GetSlaveModule(int tabId)
        {
            var key = GetControlKey();
            var moduleId = GetModuleId(key);
            
            ModuleInfo slaveModule =  GetSlaveModule(moduleId, key, tabId);
            if (slaveModule != null)
            {
                var moduleControl = ModuleControlController.GetModuleControlByControlKey(key, slaveModule.ModuleDefID) ??
                                    ModuleControlController.GetModuleControlByControlKey(key, Null.NullInteger);
                if (moduleControl != null)
                {
                    slaveModule.ModuleControlId = moduleControl.ModuleControlID;
                }
            }

            return slaveModule;
        }

        public static bool IsLegacyUI(int moduleId, string key, int portalId)
        {
            var request = HttpContext.Current.Request;
            var isLegacyUi = true;
            var settings = PortalController.Instance.GetCurrentPortalSettings();
            if (settings != null)
            {
                isLegacyUi = !(settings.EnablePopUps && !request.Browser.Crawler && request.Browser.EcmaScriptVersion >= new Version(1, 0));

                if (!isLegacyUi && !String.IsNullOrEmpty(key))
                {
                    var slaveModule = GetSlaveModule(moduleId, key, settings.ActiveTab.TabID);
                    if (slaveModule != null)
                    {
                        var moduleControl = ModuleControlController.GetModuleControlByControlKey(key, slaveModule.ModuleDefID) ??
                                            ModuleControlController.GetModuleControlByControlKey(key, Null.NullInteger);
                        if (moduleControl != null)
                        {
                            isLegacyUi = !moduleControl.SupportsPopUps;
                        }
                    }
                }
            }

            return isLegacyUi;
        }

        public static bool IsLegacyUI(int portalId)
        {
            var key = GetControlKey();
            var moduleId = GetModuleId(key);

            return IsLegacyUI(moduleId, key, portalId);
        }

        internal static string GetLocalResourceFile(Control ctrl)
        {
            string resourceFileName = Null.NullString;

            while (ctrl != null)
            {
                if (ctrl is UserControl)
                {
                    resourceFileName = string.Format("{0}/{1}/{2}.ascx.resx", ctrl.TemplateSourceDirectory, Localization.LocalResourceDirectory, ctrl.GetType().BaseType.Name);
                    if ((File.Exists(ctrl.Page.Server.MapPath(resourceFileName))))
                    {
                        break;
                    }
                }

                if (ctrl is IModuleControl)
                {
                    resourceFileName = ((IModuleControl)ctrl).LocalResourceFile;
                    break;
                }

                if (ctrl is ControlPanelBase)
                {
                    resourceFileName = ((ControlPanelBase)ctrl).LocalResourceFile;
                    break;
                }

                if (ctrl is Page)
                {
                    resourceFileName = string.Format("{0}/{1}/{2}.aspx.resx", ctrl.TemplateSourceDirectory, Localization.LocalResourceDirectory, ctrl.GetType().BaseType.Name);
                }

                ctrl = ctrl.Parent;
            }

            return resourceFileName;
        }
    }
}