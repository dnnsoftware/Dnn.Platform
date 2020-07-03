// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Entities.Modules
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.IO;
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Web.UI;

    using DotNetNuke.Common;
    using DotNetNuke.Entities.Modules.Actions;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Framework;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Security.Permissions;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.UI.Modules;

    /// -----------------------------------------------------------------------------
    /// Project  : DotNetNuke
    /// Class    : PortalModuleBase
    ///
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The PortalModuleBase class defines a custom base class inherited by all
    /// desktop portal modules within the Portal.
    ///
    /// The PortalModuleBase class defines portal specific properties
    /// that are used by the portal framework to correctly display portal modules.
    /// </summary>
    /// <remarks>
    /// </remarks>
    public class PortalModuleBase : UserControlBase, IModuleControl
    {
        protected static readonly Regex FileInfoRegex = new Regex(
            @"\.([a-z]{2,3}\-[0-9A-Z]{2,4}(-[A-Z]{2})?)(\.(Host|Portal-\d+))?\.resx$",
            RegexOptions.IgnoreCase | RegexOptions.Compiled, TimeSpan.FromSeconds(1));

        private readonly ILog _tracelLogger = LoggerSource.Instance.GetLogger("DNN.Trace");
        private string _localResourceFile;
        private ModuleInstanceContext _moduleContext;

        public PortalModuleBase()
        {
            this.DependencyProvider = Globals.DependencyProvider;
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Control ContainerControl
        {
            get
            {
                return Globals.FindControlRecursive(this, "ctr" + this.ModuleId);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets a value indicating whether the EditMode property is used to determine whether the user is in the
        /// Administrator role
        /// Cache.
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// -----------------------------------------------------------------------------
        public bool EditMode
        {
            get
            {
                return this.ModuleContext.EditMode;
            }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool IsEditable
        {
            get
            {
                return this.ModuleContext.IsEditable;
            }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int PortalId
        {
            get
            {
                return this.ModuleContext.PortalId;
            }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int TabId
        {
            get
            {
                return this.ModuleContext.TabId;
            }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public UserInfo UserInfo
        {
            get
            {
                return this.PortalSettings.UserInfo;
            }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int UserId
        {
            get
            {
                return this.PortalSettings.UserId;
            }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public PortalAliasInfo PortalAlias
        {
            get
            {
                return this.PortalSettings.PortalAlias;
            }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Hashtable Settings
        {
            get
            {
                return this.ModuleContext.Settings;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the underlying base control for this ModuleControl.
        /// </summary>
        /// <returns>A String.</returns>
        /// -----------------------------------------------------------------------------
        public Control Control
        {
            get
            {
                return this;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the Path for this control (used primarily for UserControls).
        /// </summary>
        /// <returns>A String.</returns>
        /// -----------------------------------------------------------------------------
        public string ControlPath
        {
            get
            {
                return this.TemplateSourceDirectory + "/";
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the Name for this control.
        /// </summary>
        /// <returns>A String.</returns>
        /// -----------------------------------------------------------------------------
        public string ControlName
        {
            get
            {
                return this.GetType().Name.Replace("_", ".");
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the Module Context for this control.
        /// </summary>
        /// <returns>A ModuleInstanceContext.</returns>
        /// -----------------------------------------------------------------------------
        public ModuleInstanceContext ModuleContext
        {
            get
            {
                if (this._moduleContext == null)
                {
                    this._moduleContext = new ModuleInstanceContext(this);
                }

                return this._moduleContext;
            }
        }

        // CONVERSION: Remove obsoleted methods (FYI some core modules use these, such as Links)

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///   Gets the CacheDirectory property is used to return the location of the "Cache"
        ///   Directory for the Module.
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// -----------------------------------------------------------------------------
        [Obsolete("This property is deprecated.  Plaese use ModuleController.CacheDirectory(). Scheduled removal in v11.0.0.")]
        public string CacheDirectory
        {
            get
            {
                return PortalController.Instance.GetCurrentPortalSettings().HomeDirectoryMapPath + "Cache";
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///   Gets the CacheFileName property is used to store the FileName for this Module's
        ///   Cache.
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// -----------------------------------------------------------------------------
        [Obsolete("This property is deprecated.  Please use ModuleController.CacheFileName(TabModuleID). Scheduled removal in v11.0.0.")]
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

        [Obsolete("This property is deprecated.  Please use ModuleController.CacheKey(TabModuleID). Scheduled removal in v11.0.0.")]
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

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
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

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
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

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
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

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
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

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets and sets the local resource file for this control.
        /// </summary>
        /// <returns>A String.</returns>
        /// -----------------------------------------------------------------------------
        public string LocalResourceFile
        {
            get
            {
                string fileRoot;
                if (string.IsNullOrEmpty(this._localResourceFile))
                {
                    fileRoot = Path.Combine(this.ControlPath, Localization.LocalResourceDirectory + "/" + this.ID);
                }
                else
                {
                    fileRoot = this._localResourceFile;
                }

                return fileRoot;
            }

            set
            {
                this._localResourceFile = value;
            }
        }

        /// <summary>
        /// Gets the Dependency Provider to resolve registered
        /// services with the container.
        /// </summary>
        /// <value>
        /// The Dependency Service.
        /// </value>
        protected IServiceProvider DependencyProvider { get; }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string EditUrl()
        {
            return this.ModuleContext.EditUrl();
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string EditUrl(string ControlKey)
        {
            return this.ModuleContext.EditUrl(ControlKey);
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string EditUrl(string KeyName, string KeyValue)
        {
            return this.ModuleContext.EditUrl(KeyName, KeyValue);
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string EditUrl(string KeyName, string KeyValue, string ControlKey)
        {
            return this.ModuleContext.EditUrl(KeyName, KeyValue, ControlKey);
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string EditUrl(string KeyName, string KeyValue, string ControlKey, params string[] AdditionalParameters)
        {
            return this.ModuleContext.EditUrl(KeyName, KeyValue, ControlKey, AdditionalParameters);
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string EditUrl(int TabID, string ControlKey, bool PageRedirect, params string[] AdditionalParameters)
        {
            return this.ModuleContext.NavigateUrl(TabID, ControlKey, PageRedirect, AdditionalParameters);
        }

        public int GetNextActionID()
        {
            return this.ModuleContext.GetNextActionID();
        }

        [Obsolete("This property is deprecated.  Please use ModuleController.CacheFileName(TabModuleID). Scheduled removal in v11.0.0.")]
        public string GetCacheFileName(int tabModuleId)
        {
            string strCacheKey = "TabModule:";
            strCacheKey += tabModuleId + ":";
            strCacheKey += Thread.CurrentThread.CurrentUICulture.ToString();
            return PortalController.Instance.GetCurrentPortalSettings().HomeDirectoryMapPath + "Cache" + "\\" + Globals.CleanFileName(strCacheKey) + ".resources";
        }

        [Obsolete("This property is deprecated.  Please use ModuleController.CacheKey(TabModuleID). Scheduled removal in v11.0.0.")]
        public string GetCacheKey(int tabModuleId)
        {
            string strCacheKey = "TabModule:";
            strCacheKey += tabModuleId + ":";
            strCacheKey += Thread.CurrentThread.CurrentUICulture.ToString();
            return strCacheKey;
        }

        [Obsolete("This method is deprecated.  Plaese use ModuleController.SynchronizeModule(ModuleId). Scheduled removal in v11.0.0.")]
        public void SynchronizeModule()
        {
            ModuleController.SynchronizeModule(this.ModuleId);
        }

        protected override void OnInit(EventArgs e)
        {
            if (this._tracelLogger.IsDebugEnabled)
            {
                this._tracelLogger.Debug($"PortalModuleBase.OnInit Start (TabId:{this.PortalSettings.ActiveTab.TabID},ModuleId:{this.ModuleId}): {this.GetType()}");
            }

            base.OnInit(e);
            if (this._tracelLogger.IsDebugEnabled)
            {
                this._tracelLogger.Debug($"PortalModuleBase.OnInit End (TabId:{this.PortalSettings.ActiveTab.TabID},ModuleId:{this.ModuleId}): {this.GetType()}");
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            if (this._tracelLogger.IsDebugEnabled)
            {
                this._tracelLogger.Debug($"PortalModuleBase.OnLoad Start (TabId:{this.PortalSettings.ActiveTab.TabID},ModuleId:{this.ModuleId}): {this.GetType()}");
            }

            base.OnLoad(e);
            if (this._tracelLogger.IsDebugEnabled)
            {
                this._tracelLogger.Debug($"PortalModuleBase.OnLoad End (TabId:{this.PortalSettings.ActiveTab.TabID},ModuleId:{this.ModuleId}): {this.GetType()}");
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Helper method that can be used to add an ActionEventHandler to the Skin for this
        /// Module Control.
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// -----------------------------------------------------------------------------
        protected void AddActionHandler(ActionEventHandler e)
        {
            UI.Skins.Skin ParentSkin = UI.Skins.Skin.GetParentSkin(this);
            if (ParentSkin != null)
            {
                ParentSkin.RegisterModuleActionEvent(this.ModuleId, e);
            }
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
