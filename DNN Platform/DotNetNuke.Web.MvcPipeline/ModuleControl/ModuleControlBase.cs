// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.MvcPipeline.ModuleControl
{
    using System;
    using System.Collections;
    using System.IO;
    using System.Web.UI;

    using DotNetNuke.Common;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Modules.Actions;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.UI.Modules;

    public class ModuleControlBase : IModuleControl, IDisposable
    {
        /*
        protected static readonly Regex FileInfoRegex = new Regex(
             @"\.([a-z]{2,3}\-[0-9A-Z]{2,4}(-[A-Z]{2})?)(\.(Host|Portal-\d+))?\.resx$",
             RegexOptions.IgnoreCase | RegexOptions.Compiled,
             TimeSpan.FromSeconds(1));
        */

        private readonly ILog tracelLogger = LoggerSource.Instance.GetLogger("DNN.Trace");
        private readonly Lazy<ServiceScopeContainer> serviceScopeContainer = new Lazy<ServiceScopeContainer>(ServiceScopeContainer.GetRequestOrCreateScope);
        private string localResourceFile;
        private ModuleInstanceContext moduleContext;

        /*
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Control ContainerControl
        {
            get
            {
                return Globals.FindControlRecursive(this, "ctr" + this.ModuleId);
            }
        }
        */

        public bool IsHostMenu
        {
            get
            {
                return Globals.IsHostTab(this.PortalSettings.ActiveTab.TabID);
            }
        }

        public PortalSettings PortalSettings
        {
            get
            {
                return PortalController.Instance.GetCurrentPortalSettings();
            }
        }

        /// <summary>
        /// Gets a value indicating whether the EditMode property is used to determine whether the user is in the
        /// Administrator role
        /// Cache.
        /// </summary>
        public bool EditMode
        {
            get
            {
                return this.ModuleContext.EditMode;
            }
        }

        public bool IsEditable
        {
            get
            {
                return this.ModuleContext.IsEditable;
            }
        }

        public int PortalId
        {
            get
            {
                return this.ModuleContext.PortalId;
            }
        }

        public int TabId
        {
            get
            {
                return this.ModuleContext.TabId;
            }
        }

        public UserInfo UserInfo
        {
            get
            {
                return this.PortalSettings.UserInfo;
            }
        }

        public int UserId
        {
            get
            {
                return this.PortalSettings.UserId;
            }
        }

        public PortalAliasInfo PortalAlias
        {
            get
            {
                return this.PortalSettings.PortalAlias;
            }
        }

        public Hashtable Settings
        {
            get
            {
                return this.ModuleContext.Settings;
            }
        }

        /// <summary>Gets the underlying base control for this ModuleControl.</summary>
        /// <returns>A String.</returns>
        public Control Control
        {
            get
            {
                return null;
            }
        }

        public string ID { get; set; }

        /// <summary>Gets or Sets the Path for this control (used primarily for UserControls).</summary>
        /// <returns>A String.</returns>
        public string ControlPath { get; set; }

        /// <summary>Gets the Name for this control.</summary>
        /// <returns>A String.</returns>
        public string ControlName
        {
            get
            {
                return this.GetType().Name.Replace("_", ".");
            }
        }

        /// <summary>Gets the Module Context for this control.</summary>
        /// <returns>A ModuleInstanceContext.</returns>
        public ModuleInstanceContext ModuleContext
        {
            get
            {
                if (this.moduleContext == null)
                {
                    this.moduleContext = new ModuleInstanceContext(this);
                }

                return this.moduleContext;
            }
        }

        /*
        // CONVERSION: Remove obsoleted methods (FYI some core modules use these, such as Links)

        /// <summary>
        ///   Gets the CacheDirectory property is used to return the location of the "Cache"
        ///   Directory for the Module.
        /// </summary>
        [Obsolete("Deprecated in DotNetNuke 7.0.0. Please use ModuleController.CacheDirectory(). Scheduled removal in v11.0.0.")]
        public string CacheDirectory
        {
            get
            {
                return PortalController.Instance.GetCurrentPortalSettings().HomeDirectoryMapPath + "Cache";
            }
        }

        /// <summary>
        ///   Gets the CacheFileName property is used to store the FileName for this Module's
        ///   Cache.
        /// </summary>
        [Obsolete("Deprecated in DotNetNuke 7.0.0. Please use ModuleController.CacheFileName(TabModuleID). Scheduled removal in v11.0.0.")]
        public string CacheFileName
        {
            get
            {
                string strCacheKey = "TabModule:";
                strCacheKey += this.TabModuleId + ":";
                strCacheKey += Thread.CurrentThread.CurrentUICulture.ToString();
                return PortalController.Instance.GetCurrentPortalSettings().HomeDirectoryMapPath + "Cache" + "\\" + Globals.CleanFileName(strCacheKey) + ".resources";
            }
        }

        [Obsolete("Deprecated in DotNetNuke 7.0.0. Please use ModuleController.CacheKey(TabModuleID). Scheduled removal in v11.0.0.")]
        public string CacheKey
        {
            get
            {
                string strCacheKey = "TabModule:";
                strCacheKey += this.TabModuleId + ":";
                strCacheKey += Thread.CurrentThread.CurrentUICulture.ToString();
                return strCacheKey;
            }
        }
        */

        public ModuleActionCollection Actions
        {
            get
            {
                return this.ModuleContext.Actions;
            }

            set
            {
                this.ModuleContext.Actions = value;
            }
        }

        public string HelpURL
        {
            get
            {
                return this.ModuleContext.HelpURL;
            }

            set
            {
                this.ModuleContext.HelpURL = value;
            }
        }

        public ModuleInfo ModuleConfiguration
        {
            get
            {
                return this.ModuleContext.Configuration;
            }

            set
            {
                this.ModuleContext.Configuration = value;
            }
        }

        public int TabModuleId
        {
            get
            {
                return this.ModuleContext.TabModuleId;
            }

            set
            {
                this.ModuleContext.TabModuleId = value;
            }
        }

        public int ModuleId
        {
            get
            {
                return this.ModuleContext.ModuleId;
            }

            set
            {
                this.ModuleContext.ModuleId = value;
            }
        }

        /// <summary>Gets or sets the local resource file for this control.</summary>
        /// <returns>A String.</returns>
        public string LocalResourceFile
        {
            get
            {
                string fileRoot;
                if (string.IsNullOrEmpty(this.localResourceFile))
                {
                    fileRoot = Path.Combine(this.ControlPath, Localization.LocalResourceDirectory + "/" + this.ID);
                }
                else
                {
                    fileRoot = this.localResourceFile;
                }

                return fileRoot;
            }

            set
            {
                this.localResourceFile = value;
            }
        }

        /// <summary>
        /// Gets the Dependency Provider to resolve registered
        /// services with the container.
        /// </summary>
        /// <value>
        /// The Dependency Service.
        /// </value>
        protected IServiceProvider DependencyProvider => this.serviceScopeContainer.Value.ServiceScope.ServiceProvider;

        public string EditUrl()
        {
            return this.ModuleContext.EditUrl();
        }

        public string EditUrl(string controlKey)
        {
            return this.ModuleContext.EditUrl(controlKey);
        }

        public string EditUrl(string keyName, string keyValue)
        {
            return this.ModuleContext.EditUrl(keyName, keyValue);
        }

        public string EditUrl(string keyName, string keyValue, string controlKey)
        {
            return this.ModuleContext.EditUrl(keyName, keyValue, controlKey);
        }

        public string EditUrl(string keyName, string keyValue, string controlKey, params string[] additionalParameters)
        {
            return this.ModuleContext.EditUrl(keyName, keyValue, controlKey, additionalParameters);
        }

        public string EditUrl(int tabID, string controlKey, bool pageRedirect, params string[] additionalParameters)
        {
            return this.ModuleContext.NavigateUrl(tabID, controlKey, pageRedirect, additionalParameters);
        }

        public int GetNextActionID()
        {
            return this.ModuleContext.GetNextActionID();
        }

        /// <inheritdoc />
        public void Dispose()
        {
            // base.Dispose();
            if (this.serviceScopeContainer.IsValueCreated)
            {
                this.serviceScopeContainer.Value.Dispose();
            }
        }

        /*
        [DnnDeprecated(7, 0, 0, "Please use ModuleController.CacheFileName(TabModuleID)", RemovalVersion = 11)]
        public partial string GetCacheFileName(int tabModuleId)
        {
            string strCacheKey = "TabModule:";
            strCacheKey += tabModuleId + ":";
            strCacheKey += Thread.CurrentThread.CurrentUICulture.ToString();
            return PortalController.Instance.GetCurrentPortalSettings().HomeDirectoryMapPath + "Cache" + "\\" + Globals.CleanFileName(strCacheKey) + ".resources";
        }

        [DnnDeprecated(7, 0, 0, "Please use ModuleController.CacheKey(TabModuleID)", RemovalVersion = 11)]
        public partial string GetCacheKey(int tabModuleId)
        {
            string strCacheKey = "TabModule:";
            strCacheKey += tabModuleId + ":";
            strCacheKey += Thread.CurrentThread.CurrentUICulture.ToString();
            return strCacheKey;
        }

        [DnnDeprecated(7, 0, 0, "Please use ModuleController.SynchronizeModule(ModuleId)", RemovalVersion = 11)]
        public partial void SynchronizeModule()
        {
            ModuleController.SynchronizeModule(this.ModuleId);
        }
        */
        protected void OnInit()
        {
            if (this.tracelLogger.IsDebugEnabled)
            {
                this.tracelLogger.Debug($"PortalModuleBase.OnInit Start (TabId:{this.PortalSettings.ActiveTab.TabID},ModuleId:{this.ModuleId}): {this.GetType()}");
            }

            // base.OnInit(e);
            if (this.tracelLogger.IsDebugEnabled)
            {
                this.tracelLogger.Debug($"PortalModuleBase.OnInit End (TabId:{this.PortalSettings.ActiveTab.TabID},ModuleId:{this.ModuleId}): {this.GetType()}");
            }
        }

        protected void OnLoad()
        {
            if (this.tracelLogger.IsDebugEnabled)
            {
                this.tracelLogger.Debug($"PortalModuleBase.OnLoad Start (TabId:{this.PortalSettings.ActiveTab.TabID},ModuleId:{this.ModuleId}): {this.GetType()}");
            }

            // base.OnLoad(e);
            if (this.tracelLogger.IsDebugEnabled)
            {
                this.tracelLogger.Debug($"PortalModuleBase.OnLoad End (TabId:{this.PortalSettings.ActiveTab.TabID},ModuleId:{this.ModuleId}): {this.GetType()}");
            }
        }

        /// <summary>
        /// Helper method that can be used to add an ActionEventHandler to the Skin for this
        /// Module Control.
        /// </summary>
        protected void AddActionHandler(ActionEventHandler e)
        {
            /*
            UI.Skins.Skin parentSkin = UI.Skins.Skin.GetParentSkin(this);
            if (parentSkin != null)
            {
                parentSkin.RegisterModuleActionEvent(this.ModuleId, e);
            }
            */
        }

        protected string LocalizeString(string key)
        {
            return Localization.GetString(key, this.LocalResourceFile);
        }

        protected string LocalizeSafeJsString(string key)
        {
            return Localization.GetSafeJSString(key, this.LocalResourceFile);
        }
    }
}
