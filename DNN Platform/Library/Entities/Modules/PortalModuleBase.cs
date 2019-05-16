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
#region Usings

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

#endregion

namespace DotNetNuke.Entities.Modules
{
    /// -----------------------------------------------------------------------------
    /// Project	 : DotNetNuke
    /// Class	 : PortalModuleBase
    ///
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The PortalModuleBase class defines a custom base class inherited by all
    /// desktop portal modules within the Portal.
    ///
    /// The PortalModuleBase class defines portal specific properties
    /// that are used by the portal framework to correctly display portal modules
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

        /// <summary>
        /// Gets or sets the Dependency Provider to resolve registered 
        /// services with the container.
        /// </summary>
        /// <value>
        /// The Dependency Service.
        /// </value>
        protected IServiceProvider DependencyProvider { get; }
        
        public PortalModuleBase()
        {
            DependencyProvider = Globals.DependencyProvider;
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ModuleActionCollection Actions
        {
            get
            {
                return ModuleContext.Actions;
            }
            set
            {
                ModuleContext.Actions = value;
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Control ContainerControl
        {
            get
            {
                return Globals.FindControlRecursive(this, "ctr" + ModuleId);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// The EditMode property is used to determine whether the user is in the 
        /// Administrator role
        /// Cache
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// -----------------------------------------------------------------------------
        public bool EditMode
        {
            get
            {
                return ModuleContext.EditMode;
            }
        }

        public string HelpURL
        {
            get
            {
                return ModuleContext.HelpURL;
            }
            set
            {
                ModuleContext.HelpURL = value;
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool IsEditable
        {
            get
            {
                return ModuleContext.IsEditable;
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ModuleInfo ModuleConfiguration
        {
            get
            {
                return ModuleContext.Configuration;
            }
            set
            {
                ModuleContext.Configuration = value;
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int PortalId
        {
            get
            {
                return ModuleContext.PortalId;
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int TabId
        {
            get
            {
                return ModuleContext.TabId;
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int TabModuleId
        {
            get
            {
                return ModuleContext.TabModuleId;
            }
            set
            {
                ModuleContext.TabModuleId = value;
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int ModuleId
        {
            get
            {
                return ModuleContext.ModuleId;
            }
            set
            {
                ModuleContext.ModuleId = value;
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public UserInfo UserInfo
        {
            get
            {
                return PortalSettings.UserInfo;
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int UserId
        {
            get
            {
                return PortalSettings.UserId;
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public PortalAliasInfo PortalAlias
        {
            get
            {
                return PortalSettings.PortalAlias;
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Hashtable Settings
        {
            get
            {
                return ModuleContext.Settings;
            }
        }

        #region IModuleControl Members

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the underlying base control for this ModuleControl
        /// </summary>
        /// <returns>A String</returns>
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
        /// Gets the Path for this control (used primarily for UserControls)
        /// </summary>
        /// <returns>A String</returns>
        /// -----------------------------------------------------------------------------
        public string ControlPath
        {
            get
            {
                return TemplateSourceDirectory + "/";
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the Name for this control
        /// </summary>
        /// <returns>A String</returns>
        /// -----------------------------------------------------------------------------
        public string ControlName
        {
            get
            {
                return GetType().Name.Replace("_", ".");
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets and sets the local resource file for this control
        /// </summary>
        /// <returns>A String</returns>
        /// -----------------------------------------------------------------------------
        public string LocalResourceFile
        {
            get
            {
                string fileRoot;
                if (string.IsNullOrEmpty(_localResourceFile))
                {
                    fileRoot = Path.Combine(ControlPath, Localization.LocalResourceDirectory + "/" + ID);
                }
                else
                {
                    fileRoot = _localResourceFile;
                }
                return fileRoot;
            }
            set
            {
                _localResourceFile = value;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the Module Context for this control
        /// </summary>
        /// <returns>A ModuleInstanceContext</returns>
        /// -----------------------------------------------------------------------------
        public ModuleInstanceContext ModuleContext
        {
            get
            {
                if (_moduleContext == null)
                {
                    _moduleContext = new ModuleInstanceContext(this);
                }
                return _moduleContext;
            }
        }

        #endregion

        protected override void OnInit(EventArgs e)
        {
            if (_tracelLogger.IsDebugEnabled)
                _tracelLogger.Debug($"PortalModuleBase.OnInit Start (TabId:{PortalSettings.ActiveTab.TabID},ModuleId:{ModuleId}): {GetType()}");
            base.OnInit(e);
            if (_tracelLogger.IsDebugEnabled)
                _tracelLogger.Debug($"PortalModuleBase.OnInit End (TabId:{PortalSettings.ActiveTab.TabID},ModuleId:{ModuleId}): {GetType()}");
        }
        protected override void OnLoad(EventArgs e)
        {
            if (_tracelLogger.IsDebugEnabled)
                _tracelLogger.Debug($"PortalModuleBase.OnLoad Start (TabId:{PortalSettings.ActiveTab.TabID},ModuleId:{ModuleId}): {GetType()}");
            base.OnLoad(e);
            if (_tracelLogger.IsDebugEnabled)
                _tracelLogger.Debug($"PortalModuleBase.OnLoad End (TabId:{PortalSettings.ActiveTab.TabID},ModuleId:{ModuleId}): {GetType()}");
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string EditUrl()
        {
            return ModuleContext.EditUrl();
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string EditUrl(string ControlKey)
        {
            return ModuleContext.EditUrl(ControlKey);
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string EditUrl(string KeyName, string KeyValue)
        {
            return ModuleContext.EditUrl(KeyName, KeyValue);
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string EditUrl(string KeyName, string KeyValue, string ControlKey)
        {
            return ModuleContext.EditUrl(KeyName, KeyValue, ControlKey);
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string EditUrl(string KeyName, string KeyValue, string ControlKey, params string[] AdditionalParameters)
        {
            return ModuleContext.EditUrl(KeyName, KeyValue, ControlKey, AdditionalParameters);
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string EditUrl(int TabID, string ControlKey, bool PageRedirect, params string[] AdditionalParameters)
        {
            return ModuleContext.NavigateUrl(TabID, ControlKey, PageRedirect, AdditionalParameters);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Helper method that can be used to add an ActionEventHandler to the Skin for this 
        /// Module Control
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// -----------------------------------------------------------------------------
        protected void AddActionHandler(ActionEventHandler e)
        {
            UI.Skins.Skin ParentSkin = UI.Skins.Skin.GetParentSkin(this);
            if (ParentSkin != null)
            {
                ParentSkin.RegisterModuleActionEvent(ModuleId, e);
            }
        }

        protected string LocalizeString(string key)
        {
            return Localization.GetString(key, LocalResourceFile);
        }

        protected string LocalizeSafeJsString(string key)
        {
            return Localization.GetSafeJSString(key, LocalResourceFile);
        }


        public int GetNextActionID()
        {
            return ModuleContext.GetNextActionID();
        }

        #region "Obsolete methods"

        // CONVERSION: Remove obsoleted methods (FYI some core modules use these, such as Links)
        /// -----------------------------------------------------------------------------
        /// <summary>
        ///   The CacheDirectory property is used to return the location of the "Cache"
        ///   Directory for the Module
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
        ///   The CacheFileName property is used to store the FileName for this Module's
        ///   Cache
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
                strCacheKey += TabModuleId + ":";
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
                strCacheKey += TabModuleId + ":";
                strCacheKey += Thread.CurrentThread.CurrentUICulture.ToString();
                return strCacheKey;
            }
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
            ModuleController.SynchronizeModule(ModuleId);
        }

        #endregion
    }
}
