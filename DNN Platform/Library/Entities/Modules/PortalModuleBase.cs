// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Entities.Modules;

using System;
using System.Collections;
using System.ComponentModel;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;
using System.Web.UI;

using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Modules.Actions;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Framework;
using DotNetNuke.Instrumentation;
using DotNetNuke.Internal.SourceGenerators;
using DotNetNuke.Services.Localization;
using DotNetNuke.UI.Modules;

/// Project  : DotNetNuke
/// Class    : PortalModuleBase
///
/// <summary>
/// The PortalModuleBase class defines a custom base class inherited by all
/// desktop portal modules within the Portal.
///
/// The PortalModuleBase class defines portal specific properties
/// that are used by the portal framework to correctly display portal modules.
/// </summary>
public partial class PortalModuleBase : UserControlBase, IModuleControl
{
    protected static readonly Regex FileInfoRegex = new Regex(
        @"\.([a-z]{2,3}\-[0-9A-Z]{2,4}(-[A-Z]{2})?)(\.(Host|Portal-\d+))?\.resx$",
        RegexOptions.IgnoreCase | RegexOptions.Compiled,
        TimeSpan.FromSeconds(1));

    private readonly ILog tracelLogger = LoggerSource.Instance.GetLogger("DNN.Trace");
    private readonly Lazy<ServiceScopeContainer> serviceScopeContainer = new Lazy<ServiceScopeContainer>(ServiceScopeContainer.GetRequestOrCreateScope);
    private string localResourceFile;
    private ModuleInstanceContext moduleContext;

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public Control ContainerControl
    {
        get
        {
            return Globals.FindControlRecursive(this, "ctr" + this.ModuleId);
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

    /// <summary>Gets the underlying base control for this ModuleControl.</summary>
    /// <returns>A String.</returns>
    public Control Control
    {
        get
        {
            return this;
        }
    }

    /// <summary>Gets the Path for this control (used primarily for UserControls).</summary>
    /// <returns>A String.</returns>
    public string ControlPath
    {
        get
        {
            return this.TemplateSourceDirectory + "/";
        }
    }

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

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public string EditUrl()
    {
        return this.ModuleContext.EditUrl();
    }

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public string EditUrl(string controlKey)
    {
        return this.ModuleContext.EditUrl(controlKey);
    }

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public string EditUrl(string keyName, string keyValue)
    {
        return this.ModuleContext.EditUrl(keyName, keyValue);
    }

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public string EditUrl(string keyName, string keyValue, string controlKey)
    {
        return this.ModuleContext.EditUrl(keyName, keyValue, controlKey);
    }

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public string EditUrl(string keyName, string keyValue, string controlKey, params string[] additionalParameters)
    {
        return this.ModuleContext.EditUrl(keyName, keyValue, controlKey, additionalParameters);
    }

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public string EditUrl(int tabID, string controlKey, bool pageRedirect, params string[] additionalParameters)
    {
        return this.ModuleContext.NavigateUrl(tabID, controlKey, pageRedirect, additionalParameters);
    }

    public int GetNextActionID()
    {
        return this.ModuleContext.GetNextActionID();
    }

    /// <inheritdoc />
    public override void Dispose()
    {
        base.Dispose();
        if (this.serviceScopeContainer.IsValueCreated)
        {
            this.serviceScopeContainer.Value.Dispose();
        }
    }

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

    /// <inheritdoc/>
    protected override void OnInit(EventArgs e)
    {
        if (this.tracelLogger.IsDebugEnabled)
        {
            this.tracelLogger.Debug($"PortalModuleBase.OnInit Start (TabId:{this.PortalSettings.ActiveTab.TabID},ModuleId:{this.ModuleId}): {this.GetType()}");
        }

        base.OnInit(e);
        if (this.tracelLogger.IsDebugEnabled)
        {
            this.tracelLogger.Debug($"PortalModuleBase.OnInit End (TabId:{this.PortalSettings.ActiveTab.TabID},ModuleId:{this.ModuleId}): {this.GetType()}");
        }
    }

    /// <inheritdoc/>
    protected override void OnLoad(EventArgs e)
    {
        if (this.tracelLogger.IsDebugEnabled)
        {
            this.tracelLogger.Debug($"PortalModuleBase.OnLoad Start (TabId:{this.PortalSettings.ActiveTab.TabID},ModuleId:{this.ModuleId}): {this.GetType()}");
        }

        base.OnLoad(e);
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
        UI.Skins.Skin parentSkin = UI.Skins.Skin.GetParentSkin(this);
        if (parentSkin != null)
        {
            parentSkin.RegisterModuleActionEvent(this.ModuleId, e);
        }
    }

    /// <inheritdoc cref="Localization.GetString(string,string)"/>
    [DnnDeprecated(10, 0, 2, "Use LocalizeText or LocalizeHtml")]
    protected partial string LocalizeString(string key)
    {
        return Localization.GetString(key, this.LocalResourceFile);
    }

    /// <inheritdoc cref="Localization.GetSafeJSString(string,string)"/>
    [DnnDeprecated(10, 0, 2, "Use LocalizeJsString")]
    protected partial string LocalizeSafeJsString(string key)
    {
        return Localization.GetSafeJSString(key, this.LocalResourceFile);
    }

    /// <summary>Gets the text associated with the <paramref name="key"/> in this control's <see cref="LocalResourceFile"/>.</summary>
    /// <param name="key">The resource key.</param>
    /// <returns>The localized text.</returns>
    protected string LocalizeText(string key)
        => Localization.GetString(key, this.LocalResourceFile);

    /// <summary>Gets the HTML associated with the <paramref name="key"/> in this control's <see cref="LocalResourceFile"/>.</summary>
    /// <param name="key">The resource key.</param>
    /// <returns>The localized text as HTML.</returns>
    protected IHtmlString LocalizeHtml(string key)
        => new HtmlString(Localization.GetString(key, this.LocalResourceFile));

    /// <summary>Gets the text associated with the <paramref name="key"/> in this control's <see cref="LocalResourceFile"/>.</summary>
    /// <param name="key">The resource key.</param>
    /// <param name="addDoubleQuotes">A value that indicates whether double quotation marks will be included around the encoded string.</param>
    /// <returns>The localized text encoded as a JavaScript string.</returns>
    protected IHtmlString LocalizeJsString(string key, bool addDoubleQuotes = false)
        => HtmlUtils.JavaScriptStringEncode(Localization.GetString(key, this.LocalResourceFile), addDoubleQuotes);
}
