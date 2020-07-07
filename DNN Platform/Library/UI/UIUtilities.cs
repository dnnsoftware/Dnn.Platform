// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.UI
{
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

    public class UIUtilities
    {
        public static ModuleInfo GetSlaveModule(int tabId)
        {
            var key = GetControlKey();
            var moduleId = GetModuleId(key);

            ModuleInfo slaveModule = GetSlaveModule(moduleId, key, tabId);
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

                if (!isLegacyUi && !string.IsNullOrEmpty(key))
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

        internal static string GetControlKey()
        {
            HttpRequest request = HttpContext.Current.Request;

            string key = string.Empty;
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
                int.TryParse(request.QueryString["mid"], out moduleId);
            }

            if (request.QueryString["moduleid"] != null && (key.Equals("module", StringComparison.InvariantCultureIgnoreCase) || key.Equals("help", StringComparison.InvariantCultureIgnoreCase)))
            {
                int.TryParse(request.QueryString["moduleid"], out moduleId);
            }

            return moduleId;
        }

        internal static string GetRenderMode()
        {
            HttpRequest request = HttpContext.Current.Request;

            var renderMode = string.Empty;
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
                slaveModule = new ModuleInfo { ModuleID = moduleId, ModuleDefID = -1, TabID = tabId, InheritViewPermissions = true };
            }

            if (request.QueryString["moduleid"] != null && (key.ToLowerInvariant() == "module" || key.ToLowerInvariant() == "help"))
            {
                slaveModule.ModuleDefID = -1;
            }

            if (request.QueryString["dnnprintmode"] != "true")
            {
                slaveModule.ModuleTitle = string.Empty;
            }

            slaveModule.Header = string.Empty;
            slaveModule.Footer = string.Empty;
            slaveModule.StartDate = DateTime.MinValue;
            slaveModule.EndDate = DateTime.MaxValue;
            slaveModule.Visibility = VisibilityState.None;
            slaveModule.Color = string.Empty;
            slaveModule.Border = string.Empty;
            slaveModule.DisplayTitle = true;
            slaveModule.DisplayPrint = false;
            slaveModule.DisplaySyndicate = false;

            return slaveModule;
        }

        internal static string GetLocalResourceFile(Control ctrl)
        {
            string resourceFileName = Null.NullString;

            while (ctrl != null)
            {
                if (ctrl is UserControl)
                {
                    resourceFileName = string.Format("{0}/{1}/{2}.ascx.resx", ctrl.TemplateSourceDirectory, Localization.LocalResourceDirectory, ctrl.GetType().BaseType.Name);
                    if (File.Exists(ctrl.Page.Server.MapPath(resourceFileName)))
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
