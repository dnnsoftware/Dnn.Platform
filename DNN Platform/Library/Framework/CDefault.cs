// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Framework
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Web.UI;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Common.Internal;
    using DotNetNuke.Entities.Controllers;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.Framework.JavaScriptLibraries;
    using DotNetNuke.UI.Utilities;

    public class CDefault : PageBase
    {
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        public string Author = string.Empty;
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        public string Comment = string.Empty;
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        public string Copyright = string.Empty;
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        public string Description = string.Empty;
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        public string Generator = string.Empty;
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        public string KeyWords = string.Empty;
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        public new string Title = string.Empty;

        private static readonly object InstallerFilesRemovedLock = new object();

        /// <summary>Initializes a new instance of the <see cref="CDefault"/> class.</summary>
        [Obsolete("Deprecated in DotNetNuke 10.0.2. Please use overload with IPortalController. Scheduled removal in v12.0.0.")]
        public CDefault()
            : this(null, null, null)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="CDefault"/> class.</summary>
        /// <param name="portalController">The portal controller.</param>
        /// <param name="appStatus">The application status.</param>
        /// <param name="hostSettings">The host settings.</param>
        public CDefault(IPortalController portalController, IApplicationStatusInfo appStatus, IHostSettings hostSettings)
            : base(portalController, appStatus, hostSettings)
        {
        }

        protected string AdvancedSettingsPageUrl
        {
            get
            {
                string result;
                var tab = TabController.Instance.GetTabByName("Advanced Settings", this.PortalSettings.PortalId);
                var modules = ModuleController.Instance.GetTabModules(tab.TabID).Values;

                if (modules.Count > 0)
                {
                    var pmb = new PortalModuleBase();
                    result = pmb.EditUrl(tab.TabID, string.Empty, false, string.Concat("mid=", modules.ElementAt(0).ModuleID), "popUp=true", string.Concat("ReturnUrl=", this.Server.UrlEncode(TestableGlobals.Instance.NavigateURL())));
                }
                else
                {
                    result = TestableGlobals.Instance.NavigateURL(tab.TabID);
                }

                return result;
            }
        }

        /// <summary>Allows the scroll position on the page to be moved to the top of the passed in control.</summary>
        /// <param name="objControl">Control to scroll to.</param>
        public void ScrollToControl(Control objControl)
        {
            if (ClientAPI.BrowserSupportsFunctionality(ClientAPI.ClientFunctionality.Positioning))
            {
                JavaScript.RegisterClientReference(this, ClientAPI.ClientNamespaceReferences.dnn_dom_positioning);
                ClientAPI.RegisterClientVariable(this, "ScrollToControl", objControl.ClientID, true);
                DNNClientAPI.SetScrollTop(this.Page);
            }
        }

        /// <inheritdoc/>
        protected override void RegisterAjaxScript()
        {
            if (this.Page.Form != null)
            {
                if (ServicesFrameworkInternal.Instance.IsAjaxScriptSupportRequired)
                {
                    ServicesFrameworkInternal.Instance.RegisterAjaxScript(this.Page);
                }
            }
        }

        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Breaking change")]
        protected void ManageInstallerFiles()
        {
            if (!HostController.Instance.GetBoolean("InstallerFilesRemoved"))
            {
                lock (InstallerFilesRemovedLock)
                {
                    if (!HostController.Instance.GetBoolean("InstallerFilesRemoved"))
                    {
                        Services.Upgrade.Upgrade.DeleteInstallerFiles();
                        HostController.Instance.Update("InstallerFilesRemoved", "True", true);
                    }
                }
            }
        }
    }
}
