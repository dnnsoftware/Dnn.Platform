// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.UI.Modules
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Web;
    using System.Web.UI;
    using System.Web.UI.HtmlControls;
    using System.Web.UI.WebControls;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Host;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Tabs.TabVersions;
    using DotNetNuke.Framework;
    using DotNetNuke.Framework.JavaScriptLibraries;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Security;
    using DotNetNuke.Security.Permissions;
    using DotNetNuke.Services.Exceptions;
    using DotNetNuke.Services.Log.EventLog;
    using DotNetNuke.Services.ModuleCache;
    using DotNetNuke.UI.Utilities;
    using DotNetNuke.UI.WebControls;
    using DotNetNuke.Web.Client;
    using DotNetNuke.Web.Client.ClientResourceManagement;
    using Microsoft.Extensions.DependencyInjection;

    using Globals = DotNetNuke.Common.Globals;

    /// -----------------------------------------------------------------------------
    /// Project  : DotNetNuke
    /// Namespace: DotNetNuke.UI.Modules
    /// Class    : ModuleHost
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// ModuleHost hosts a Module Control (or its cached Content).
    /// </summary>
    public sealed class ModuleHost : Panel
    {
        private const string DefaultCssProvider = "DnnPageHeaderProvider";
        private const string DefaultJsProvider = "DnnBodyProvider";

        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(ModuleHost));

        private static readonly Regex CdfMatchRegex = new Regex(
            @"<\!--CDF\((?<type>JAVASCRIPT|CSS|JS-LIBRARY)\|(?<path>.+?)(\|(?<provider>.+?)\|(?<priority>\d+?))?\)-->",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private readonly ModuleInfo _moduleConfiguration;
        private readonly IModuleControlPipeline _moduleControlPipeline = Globals.DependencyProvider.GetRequiredService<IModuleControlPipeline>();
        private Control _control;
        private bool _isCached;

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="ModuleHost"/> class.
        /// Creates a Module Host control using the ModuleConfiguration for the Module.
        /// </summary>
        /// <remarks>
        /// </remarks>
        public ModuleHost(ModuleInfo moduleConfiguration, Skins.Skin skin, Containers.Container container)
        {
            this.ID = "ModuleContent";
            this.Container = container;
            this._moduleConfiguration = moduleConfiguration;
            this.Skin = skin;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the attached ModuleControl.
        /// </summary>
        /// <returns>An IModuleControl.</returns>
        public IModuleControl ModuleControl
        {
            get
            {
                // Make sure the Control tree has been created
                this.EnsureChildControls();
                return this._control as IModuleControl;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the current POrtal Settings.
        /// </summary>
        public PortalSettings PortalSettings
        {
            get
            {
                return PortalController.Instance.GetCurrentPortalSettings();
            }
        }

        public Containers.Container Container { get; private set; }

        public Skins.Skin Skin { get; private set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets a flag that indicates whether the Module is in View Mode.
        /// </summary>
        /// <returns>A Boolean.</returns>
        internal static bool IsViewMode(ModuleInfo moduleInfo, PortalSettings settings)
        {
            bool viewMode;

            if (ModulePermissionController.HasModuleAccess(SecurityAccessLevel.ViewPermissions, Null.NullString,
                                                              moduleInfo))
            {
                viewMode = false;
            }
            else
            {
                viewMode = !ModulePermissionController.HasModuleAccess(SecurityAccessLevel.Edit, Null.NullString,
                                                              moduleInfo);
            }

            return viewMode || settings.UserMode == PortalSettings.Mode.View;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// CreateChildControls builds the control tree.
        /// </summary>
        protected override void CreateChildControls()
        {
            this.Controls.Clear();

            // Load Module Control (or cached control)
            this.LoadModuleControl();

            // Optionally Inject AJAX Update Panel
            if (this.ModuleControl != null)
            {
                // if module is dynamically loaded and AJAX is installed and the control supports partial rendering (defined in ModuleControls table )
                if (!this._isCached && this._moduleConfiguration.ModuleControl.SupportsPartialRendering && AJAX.IsInstalled())
                {
                    this.LoadUpdatePanel();
                }
                else
                {
                    // inject a message placeholder for common module messaging - UI.Skins.Skin.AddModuleMessage
                    InjectMessageControl(this);

                    this.InjectVersionToTheModuleIfSupported();

                    // inject the module into the panel
                    this.InjectModuleContent(this._control);
                }
            }
        }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            if (Host.EnableCustomModuleCssClass)
            {
                string moduleName = this.ModuleControl.ModuleContext.Configuration.DesktopModule.ModuleName;
                if (moduleName != null)
                {
                    moduleName = Globals.CleanName(moduleName);
                }

                this.Attributes.Add("class", string.Format("DNNModuleContent Mod{0}C", moduleName));
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// RenderContents renders the contents of the control to the output stream.
        /// </summary>
        protected override void RenderContents(HtmlTextWriter writer)
        {
            if (this._isCached)
            {
                // Render the cached control to the output stream
                base.RenderContents(writer);
            }
            else
            {
                if (this.SupportsCaching() && IsViewMode(this._moduleConfiguration, this.PortalSettings) && !Globals.IsAdminControl() && !this.IsVersionRequest())
                {
                    // Render to cache
                    var tempWriter = new StringWriter();

                    this._control.RenderControl(new HtmlTextWriter(tempWriter));
                    string cachedOutput = tempWriter.ToString();

                    if (!string.IsNullOrEmpty(cachedOutput) && (!HttpContext.Current.Request.Browser.Crawler))
                    {
                        // Save content to cache
                        var moduleContent = Encoding.UTF8.GetBytes(cachedOutput);
                        var cache = ModuleCachingProvider.Instance(this._moduleConfiguration.GetEffectiveCacheMethod());

                        var varyBy = new SortedDictionary<string, string> { { "locale", Thread.CurrentThread.CurrentUICulture.ToString() } };

                        var cacheKey = cache.GenerateCacheKey(this._moduleConfiguration.TabModuleID, varyBy);
                        cache.SetModule(this._moduleConfiguration.TabModuleID, cacheKey, new TimeSpan(0, 0, this._moduleConfiguration.CacheTime), moduleContent);
                    }

                    // Render the cached content to Response
                    writer.Write(cachedOutput);
                }
                else
                {
                    // Render the control to Response
                    base.RenderContents(writer);
                }
            }
        }

        private static void InjectMessageControl(Control container)
        {
            // inject a message placeholder for common module messaging - UI.Skins.Skin.AddModuleMessage
            var messagePlaceholder = new PlaceHolder { ID = "MessagePlaceHolder", Visible = false };
            container.Controls.Add(messagePlaceholder);
        }

        private bool IsVersionRequest()
        {
            int version;
            return TabVersionUtils.TryGetUrlVersion(out version);
        }

        private void InjectVersionToTheModuleIfSupported()
        {
            if (!(this._control is IVersionableControl))
            {
                return;
            }

            var versionableControl = this._control as IVersionableControl;
            if (this._moduleConfiguration.ModuleVersion != Null.NullInteger)
            {
                versionableControl.SetModuleVersion(this._moduleConfiguration.ModuleVersion);
            }
        }

        private void InjectModuleContent(Control content)
        {
            if (this._moduleConfiguration.IsWebSlice && !Globals.IsAdminControl())
            {
                // Assign the class - hslice to the Drag-N-Drop Panel
                this.CssClass = "hslice";
                var titleLabel = new Label
                {
                    CssClass = "entry-title Hidden",
                    Text = !string.IsNullOrEmpty(this._moduleConfiguration.WebSliceTitle) ? this._moduleConfiguration.WebSliceTitle : this._moduleConfiguration.ModuleTitle,
                };
                this.Controls.Add(titleLabel);

                var websliceContainer = new Panel { CssClass = "entry-content" };
                websliceContainer.Controls.Add(content);

                var expiry = new HtmlGenericControl { TagName = "abbr" };
                expiry.Attributes["class"] = "endtime";
                if (!Null.IsNull(this._moduleConfiguration.WebSliceExpiryDate))
                {
                    expiry.Attributes["title"] = this._moduleConfiguration.WebSliceExpiryDate.ToString("o");
                    websliceContainer.Controls.Add(expiry);
                }
                else if (this._moduleConfiguration.EndDate < DateTime.MaxValue)
                {
                    expiry.Attributes["title"] = this._moduleConfiguration.EndDate.ToString("o");
                    websliceContainer.Controls.Add(expiry);
                }

                var ttl = new HtmlGenericControl { TagName = "abbr" };
                ttl.Attributes["class"] = "ttl";
                if (this._moduleConfiguration.WebSliceTTL > 0)
                {
                    ttl.Attributes["title"] = this._moduleConfiguration.WebSliceTTL.ToString();
                    websliceContainer.Controls.Add(ttl);
                }
                else if (this._moduleConfiguration.CacheTime > 0)
                {
                    ttl.Attributes["title"] = (this._moduleConfiguration.CacheTime / 60).ToString();
                    websliceContainer.Controls.Add(ttl);
                }

                this.Controls.Add(websliceContainer);
            }
            else
            {
                this.Controls.Add(content);
            }
        }

        /// ----------------------------------------------------------------------------
        /// <summary>
        /// Gets a flag that indicates whether the Module Content should be displayed.
        /// </summary>
        /// <returns>A Boolean.</returns>
        private bool DisplayContent()
        {
            // module content visibility options
            var content = this.PortalSettings.UserMode != PortalSettings.Mode.Layout;
            if (this.Page.Request.QueryString["content"] != null)
            {
                switch (this.Page.Request.QueryString["Content"].ToLowerInvariant())
                {
                    case "1":
                    case "true":
                        content = true;
                        break;
                    case "0":
                    case "false":
                        content = false;
                        break;
                }
            }

            if (Globals.IsAdminControl())
            {
                content = true;
            }

            return content;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// LoadModuleControl loads the ModuleControl (PortalModuelBase).
        /// </summary>
        private void LoadModuleControl()
        {
            try
            {
                if (this.DisplayContent())
                {
                    // if the module supports caching and caching is enabled for the instance and the user does not have Edit rights or is currently in View mode
                    if (this.SupportsCaching() && IsViewMode(this._moduleConfiguration, this.PortalSettings) && !this.IsVersionRequest())
                    {
                        // attempt to load the cached content
                        this._isCached = this.TryLoadCached();
                    }

                    if (!this._isCached)
                    {
                        // load the control dynamically
                        this._control = this._moduleControlPipeline.LoadModuleControl(this.Page, this._moduleConfiguration);
                    }
                }
                else // content placeholder
                {
                    this._control = this._moduleControlPipeline.CreateModuleControl(this._moduleConfiguration);
                }

                if (this.Skin != null)
                {
                    // check for IMC
                    this.Skin.Communicator.LoadCommunicator(this._control);
                }

                // add module settings
                this.ModuleControl.ModuleContext.Configuration = this._moduleConfiguration;
            }
            catch (ThreadAbortException exc)
            {
                Logger.Debug(exc);

                Thread.ResetAbort();
            }
            catch (Exception exc)
            {
                Logger.Error(exc);

                // add module settings
                this._control = this._moduleControlPipeline.CreateModuleControl(this._moduleConfiguration);
                this.ModuleControl.ModuleContext.Configuration = this._moduleConfiguration;
                if (TabPermissionController.CanAdminPage())
                {
                    // only display the error to page administrators
                    Exceptions.ProcessModuleLoadException(this._control, exc);
                }
                else
                {
                    // Otherwise just log the fact that an exception occurred
                    new ExceptionLogController().AddLog(exc);
                }
            }

            // Enable ViewState
            this._control.ViewStateMode = ViewStateMode.Enabled;
        }

        /// <summary>
        /// LoadUpdatePanel optionally loads an AJAX Update Panel.
        /// </summary>
        private void LoadUpdatePanel()
        {
            // register AJAX
            AJAX.RegisterScriptManager();

            // enable Partial Rendering
            var scriptManager = AJAX.GetScriptManager(this.Page);
            if (scriptManager != null)
            {
                scriptManager.EnablePartialRendering = true;
            }

            // create update panel
            var updatePanel = new UpdatePanel
            {
                UpdateMode = UpdatePanelUpdateMode.Conditional,
                ID = this._control.ID + "_UP",
            };

            // get update panel content template
            var templateContainer = updatePanel.ContentTemplateContainer;

            // inject a message placeholder for common module messaging - UI.Skins.Skin.AddModuleMessage
            InjectMessageControl(templateContainer);

            // inject module into update panel content template
            templateContainer.Controls.Add(this._control);

            // inject the update panel into the panel
            this.InjectModuleContent(updatePanel);

            // create image for update progress control
            var progressTemplate = "<div class=\"dnnLoading dnnPanelLoading\"></div>";

            // inject updateprogress into the panel
            var updateProgress = new UpdateProgress
            {
                AssociatedUpdatePanelID = updatePanel.ID,
                ID = updatePanel.ID + "_Prog",

                ProgressTemplate = new LiteralTemplate(progressTemplate),
            };
            this.Controls.Add(updateProgress);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets a flag that indicates whether the Module Instance supports Caching.
        /// </summary>
        /// <returns>A Boolean.</returns>
        private bool SupportsCaching()
        {
            return this._moduleConfiguration.CacheTime > 0;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Trys to load previously cached Module Content.
        /// </summary>
        /// <returns>A Boolean that indicates whether the cahed content was loaded.</returns>
        private bool TryLoadCached()
        {
            bool success = false;
            string cachedContent = string.Empty;
            try
            {
                var cache = ModuleCachingProvider.Instance(this._moduleConfiguration.GetEffectiveCacheMethod());
                var varyBy = new SortedDictionary<string, string> { { "locale", Thread.CurrentThread.CurrentUICulture.ToString() } };

                string cacheKey = cache.GenerateCacheKey(this._moduleConfiguration.TabModuleID, varyBy);
                byte[] cachedBytes = ModuleCachingProvider.Instance(this._moduleConfiguration.GetEffectiveCacheMethod()).GetModule(this._moduleConfiguration.TabModuleID, cacheKey);

                if (cachedBytes != null && cachedBytes.Length > 0)
                {
                    cachedContent = Encoding.UTF8.GetString(cachedBytes);
                    success = true;
                }
            }
            catch (Exception ex)
            {
                cachedContent = string.Empty;
                Exceptions.LogException(ex);
                success = false;
            }

            if (success)
            {
                this.RestoreCachedClientResourceRegistrations(cachedContent);
                this._control = this._moduleControlPipeline.CreateCachedControl(cachedContent, this._moduleConfiguration);
                this.Controls.Add(this._control);
            }

            return success;
        }

        /// <summary>
        /// Restores client resource registrations from the <paramref name="cachedContent"/>.
        /// These are registrations that originated from <c>DnnJsInclude</c>, <c>DnnCssInclude</c>, and <c>JavaScriptLibraryInclude</c> controls.
        /// </summary>
        /// <param name="cachedContent">The HTML text of the cached module.</param>
        private void RestoreCachedClientResourceRegistrations(string cachedContent)
        {
            // parse the registered CDF from content
            var matches = CdfMatchRegex.Matches(cachedContent);
            if (matches.Count == 0)
            {
                return;
            }

            foreach (Match match in matches)
            {
                cachedContent = cachedContent.Replace(match.Value, string.Empty);
                var dependencyType = match.Groups["type"].Value.ToUpperInvariant();
                var filePath = match.Groups["path"].Value;
                var forceProvider = string.Empty;
                var priority = Null.NullInteger;

                if (match.Groups["provider"].Success)
                {
                    forceProvider = match.Groups["provider"].Value;
                }

                if (match.Groups["priority"].Success)
                {
                    priority = Convert.ToInt32(match.Groups["priority"].Value);
                }

                switch (dependencyType)
                {
                    case "JAVASCRIPT":
                        if (string.IsNullOrEmpty(forceProvider))
                        {
                            forceProvider = DefaultJsProvider;
                        }

                        if (priority == Null.NullInteger)
                        {
                            priority = (int)FileOrder.Js.DefaultPriority;
                        }

                        ClientResourceManager.RegisterScript(this.Page, filePath, priority, forceProvider);
                        break;
                    case "CSS":
                        if (string.IsNullOrEmpty(forceProvider))
                        {
                            forceProvider = DefaultCssProvider;
                        }

                        if (priority == Null.NullInteger)
                        {
                            priority = (int)FileOrder.Css.DefaultPriority;
                        }

                        ClientResourceManager.RegisterStyleSheet(this.Page, filePath, priority, forceProvider);
                        break;
                    case "JS-LIBRARY":
                        var args = filePath.Split(new[] { ',', }, StringSplitOptions.None);
                        if (string.IsNullOrEmpty(args[1]))
                        {
                            JavaScript.RequestRegistration(args[0]);
                        }
                        else if (string.IsNullOrEmpty(args[2]))
                        {
                            JavaScript.RequestRegistration(args[0], new Version(args[1]));
                        }
                        else
                        {
                            JavaScript.RequestRegistration(args[0], new Version(args[1]), (SpecificVersion)Enum.Parse(typeof(SpecificVersion), args[2]));
                        }

                        break;
                }
            }
        }
    }
}
