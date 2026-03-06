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

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Common;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Tabs;

    using Microsoft.Extensions.DependencyInjection;

    /// <summary>Provides DNN context for the DDR Menu.</summary>
    public class DNNContext : IDisposable
    {
        private static string moduleName;
        private static string moduleFolder;
        private static string dataName;

        private readonly DNNContext savedContext;
        private bool isDisposed;
        private Page page;
        private PortalSettings portalSettings;
        private TabInfo activeTab;
        private string skinPath;

        /// <summary>Initializes a new instance of the <see cref="DNNContext"/> class.</summary>
        /// <param name="hostControl">The control that hosts the menu.</param>
        public DNNContext(Control hostControl)
        {
            this.HostControl = hostControl;

            this.savedContext = Current;
            Current = this;
        }

        /// <summary>Gets the module name.</summary>
        public static string ModuleName => moduleName ??= GetModuleNameFromAssembly();

        /// <summary>Gets the module folder.</summary>
        [Obsolete("Deprecated in DotNetNuke 10.2.4. Please use GetModuleFolder(IHostSettings). Scheduled removal in v12.0.0.")]
        public static string ModuleFolder => GetModuleFolder(Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettings>());

        /// <summary>Gets the current Dnn context.</summary>
        public static DNNContext Current
        {
            get { return (DNNContext)HttpContext.Current.Items[DataName]; }
            private set { HttpContext.Current.Items[DataName] = value; }
        }

        /// <summary>Gets a reference to the page.</summary>
        public Page Page => this.page ??= this.HostControl.Page;

        /// <summary>Gets the current portal settings.</summary>
        public PortalSettings PortalSettings => this.portalSettings ??= (PortalSettings)PortalController.Instance.GetCurrentSettings();

        /// <summary>Gets the currently active tab (page).</summary>
        public TabInfo ActiveTab => this.activeTab ??= this.PortalSettings.ActiveTab;

        /// <summary>Gets the path to the skin (theme).</summary>
        public string SkinPath => this.skinPath ??= this.ActiveTab.SkinPath;

        /// <summary>Gets the host control.</summary>
        public Control HostControl { get; private set; }

        private static string DataName => dataName ??= "DDRMenu.DNNContext." + ModuleName;

        /// <summary>Gets the module folder.</summary>
        /// <param name="hostSettings">The host settings.</param>
        /// <returns>The virtual rooted path.</returns>
        public static string GetModuleFolder(IHostSettings hostSettings)
        {
            var desktopModule = DesktopModuleController.GetDesktopModuleByModuleName(hostSettings, ModuleName, PortalSettings.Current.PortalId);
            return moduleFolder ??= $"~/DesktopModules/{desktopModule.FolderName}/";
        }

        /// <summary>Converts a url into one that is usable on the requesting Client.</summary>
        /// <param name="relativeUrl">The relative url.</param>
        /// <returns>The converted url.</returns>
        public string ResolveUrl(string relativeUrl)
        {
            return this.HostControl.ResolveUrl(relativeUrl);
        }

        /// <inheritdoc />
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>Disposes this instance resources.</summary>
        /// <param name="disposing">A value indicating if the current instance is disposing.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (this.isDisposed)
            {
                return;
            }

            if (disposing)
            {
                // free managed resources.
            }

            Current = this.savedContext;
            this.isDisposed = true;
        }

        private static string GetModuleNameFromAssembly()
        {
            var moduleFullName = Path.GetFileNameWithoutExtension(Assembly.GetExecutingAssembly().CodeBase);

            return moduleFullName.Substring(moduleFullName.LastIndexOf('.') + 1);
        }
    }
}
