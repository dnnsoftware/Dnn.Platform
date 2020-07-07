// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.DDRMenu.DNNCommon
{
    using System;
    using System.IO;
    using System.Reflection;
    using System.Web;
    using System.Web.UI;

    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Tabs;

    public class DNNContext : IDisposable
    {
        private static string _ModuleName;

        private static string _ModuleFolder;

        private static string _DataName;

        private readonly DNNContext savedContext;

        private Page _Page;

        private PortalSettings _PortalSettings;

        private TabInfo _ActiveTab;

        private string _SkinPath;

        public DNNContext(Control hostControl)
        {
            this.HostControl = hostControl;

            this.savedContext = Current;
            Current = this;
        }

        public static string ModuleName
        {
            get { return _ModuleName ?? (_ModuleName = GetModuleNameFromAssembly()); }
        }

        public static string ModuleFolder
        {
            get
            {
                return _ModuleFolder ??
                       (_ModuleFolder =
                        string.Format(
                            "~/DesktopModules/{0}/", DesktopModuleController.GetDesktopModuleByModuleName(ModuleName, PortalSettings.Current.PortalId).FolderName));
            }
        }

        public static DNNContext Current
        {
            get { return (DNNContext)HttpContext.Current.Items[DataName]; }
            private set { HttpContext.Current.Items[DataName] = value; }
        }

        public Page Page
        {
            get { return this._Page ?? (this._Page = this.HostControl.Page); }
        }

        public PortalSettings PortalSettings
        {
            get { return this._PortalSettings ?? (this._PortalSettings = PortalController.Instance.GetCurrentPortalSettings()); }
        }

        public TabInfo ActiveTab
        {
            get { return this._ActiveTab ?? (this._ActiveTab = this.PortalSettings.ActiveTab); }
        }

        public string SkinPath
        {
            get { return this._SkinPath ?? (this._SkinPath = this.ActiveTab.SkinPath); }
        }

        public Control HostControl { get; private set; }

        private static string DataName
        {
            get { return _DataName ?? (_DataName = "DDRMenu.DNNContext." + ModuleName); }
        }

        public string ResolveUrl(string relativeUrl)
        {
            return this.HostControl.ResolveUrl(relativeUrl);
        }

        public void Dispose()
        {
            Current = this.savedContext;
        }

        private static string GetModuleNameFromAssembly()
        {
            var moduleFullName = Path.GetFileNameWithoutExtension(Assembly.GetExecutingAssembly().CodeBase);

            // ReSharper disable PossibleNullReferenceException
            return moduleFullName.Substring(moduleFullName.LastIndexOf('.') + 1);

            // ReSharper restore PossibleNullReferenceException
        }
    }
}
