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
#region Usings

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
using DotNetNuke.Web.Client.ClientResourceManagement;
using Globals = DotNetNuke.Common.Globals;

#endregion

namespace DotNetNuke.UI.Modules
{
    /// -----------------------------------------------------------------------------
    /// Project	 : DotNetNuke
    /// Namespace: DotNetNuke.UI.Modules
    /// Class	 : ModuleHost
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// ModuleHost hosts a Module Control (or its cached Content).
    /// </summary>
    public sealed class ModuleHost : Panel
    {
    	private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof (ModuleHost));

        private static readonly Regex CdfMatchRegex = new Regex(@"<\!--CDF\((JAVASCRIPT|CSS|JS-LIBRARY)\|(.+?)\)-->",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        #region Private Members

        private readonly ModuleInfo _moduleConfiguration;
        private Control _control;
        private bool _isCached;

        #endregion

        #region Constructors

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Creates a Module Host control using the ModuleConfiguration for the Module
        /// </summary>
        /// <remarks>
        /// </remarks>
        public ModuleHost(ModuleInfo moduleConfiguration, Skins.Skin skin, Containers.Container container)
        {
            ID = "ModuleContent";
            Container = container;
            _moduleConfiguration = moduleConfiguration;
            Skin = skin;
        }

        #endregion

        #region Public Properties

        public Containers.Container Container { get; private set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the attached ModuleControl
        /// </summary>
        /// <returns>An IModuleControl</returns>
        public IModuleControl ModuleControl
        {
            get
            {
				//Make sure the Control tree has been created
                EnsureChildControls();
                return _control as IModuleControl;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the current POrtal Settings
        /// </summary>
        public PortalSettings PortalSettings
        {
            get
            {
                return PortalController.Instance.GetCurrentPortalSettings();
            }
        }

        public Skins.Skin Skin { get; private set; }

        #endregion

        #region Private Methods

        private bool IsVersionRequest()
        {
            int version;
            return TabVersionUtils.TryGetUrlVersion(out version);
        }

        private void InjectVersionToTheModuleIfSupported()
        {
            if (!(_control is IVersionableControl)) return;

            var versionableControl = _control as IVersionableControl;
            if (_moduleConfiguration.ModuleVersion != Null.NullInteger)
            {
                versionableControl.SetModuleVersion(_moduleConfiguration.ModuleVersion);
            }
        }

        private void InjectModuleContent(Control content)
        {
            if (_moduleConfiguration.IsWebSlice && !Globals.IsAdminControl())
            {
				//Assign the class - hslice to the Drag-N-Drop Panel
                CssClass = "hslice";
                var titleLabel = new Label
                                     {
                                         CssClass = "entry-title Hidden",
                                         Text = !string.IsNullOrEmpty(_moduleConfiguration.WebSliceTitle) ? _moduleConfiguration.WebSliceTitle : _moduleConfiguration.ModuleTitle
                                     };
                Controls.Add(titleLabel);

                var websliceContainer = new Panel {CssClass = "entry-content"};
                websliceContainer.Controls.Add(content);

                var expiry = new HtmlGenericControl {TagName = "abbr"};
                expiry.Attributes["class"] = "endtime";
                if (!Null.IsNull(_moduleConfiguration.WebSliceExpiryDate))
                {
                    expiry.Attributes["title"] = _moduleConfiguration.WebSliceExpiryDate.ToString("o");
                    websliceContainer.Controls.Add(expiry);
                }
                else if (_moduleConfiguration.EndDate < DateTime.MaxValue)
                {
                    expiry.Attributes["title"] = _moduleConfiguration.EndDate.ToString("o");
                    websliceContainer.Controls.Add(expiry);
                }

                var ttl = new HtmlGenericControl {TagName = "abbr"};
                ttl.Attributes["class"] = "ttl";
                if (_moduleConfiguration.WebSliceTTL > 0)
                {
                    ttl.Attributes["title"] = _moduleConfiguration.WebSliceTTL.ToString();
                    websliceContainer.Controls.Add(ttl);
                }
                else if (_moduleConfiguration.CacheTime > 0)
                {
                    ttl.Attributes["title"] = (_moduleConfiguration.CacheTime/60).ToString();
                    websliceContainer.Controls.Add(ttl);
                }

                Controls.Add(websliceContainer);
            }
            else
            {
                Controls.Add(content);
            }
        }

        /// ----------------------------------------------------------------------------
        /// <summary>
        /// Gets a flag that indicates whether the Module Content should be displayed
        /// </summary>
        /// <returns>A Boolean</returns>
        private bool DisplayContent()
        {
			//module content visibility options
            var content = PortalSettings.UserMode != PortalSettings.Mode.Layout;
            if (Page.Request.QueryString["content"] != null)
            {
                switch (Page.Request.QueryString["Content"].ToLower())
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

        private static void InjectMessageControl(Control container)
        {
            //inject a message placeholder for common module messaging - UI.Skins.Skin.AddModuleMessage
            var messagePlaceholder = new PlaceHolder {ID = "MessagePlaceHolder", Visible = false};
            container.Controls.Add(messagePlaceholder);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets a flag that indicates whether the Module is in View Mode
        /// </summary>
        /// <returns>A Boolean</returns>
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
                viewMode = !(ModulePermissionController.HasModuleAccess(SecurityAccessLevel.Edit, Null.NullString,
                                                              moduleInfo)); 
            }
            
            return viewMode || settings.UserMode == PortalSettings.Mode.View;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// LoadModuleControl loads the ModuleControl (PortalModuelBase)
        /// </summary>
        private void LoadModuleControl()
        {
            try
            {
                if (DisplayContent())
                {
                    //if the module supports caching and caching is enabled for the instance and the user does not have Edit rights or is currently in View mode
                    if (SupportsCaching() && IsViewMode(_moduleConfiguration, PortalSettings) && !IsVersionRequest())
                    {
						//attempt to load the cached content
                        _isCached = TryLoadCached();
                    }
                    if (!_isCached)
                    {
                    	// load the control dynamically
                        _control = ModuleControlFactory.LoadModuleControl(Page, _moduleConfiguration);
                    }
                }
                else //content placeholder
                {
                    _control = ModuleControlFactory.CreateModuleControl(_moduleConfiguration);
                }
                if (Skin != null)
                {
				
                	//check for IMC
                    Skin.Communicator.LoadCommunicator(_control);
                }
				
                //add module settings
                ModuleControl.ModuleContext.Configuration = _moduleConfiguration;
            }
            catch (ThreadAbortException exc)
            {
                Logger.Debug(exc);

                Thread.ResetAbort();
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
				
				//add module settings
                _control = ModuleControlFactory.CreateModuleControl(_moduleConfiguration);
                ModuleControl.ModuleContext.Configuration = _moduleConfiguration;
                if (TabPermissionController.CanAdminPage())
                {
					//only display the error to page administrators
                    Exceptions.ProcessModuleLoadException(_control, exc);
                }
                else
                {
                    // Otherwise just log the fact that an exception occurred
                    new ExceptionLogController().AddLog(exc);
                }
            }
            
            //Enable ViewState
            _control.ViewStateMode = ViewStateMode.Enabled;
        }

        private void LoadAjaxPanel()
        {
            // Reference dnn.js to add attachEvent/detachEvent functions in IE11 to fix Telerik (see DNN-6167)
            JavaScript.RegisterClientReference(Page, ClientAPI.ClientNamespaceReferences.dnn);

            //DNN-9145 TODO
            //var loadingPanel = new RadAjaxLoadingPanel { ID = _control.ID + "_Prog", Skin = "Default" };
            //
            //Controls.Add(loadingPanel);
            //
            //var ajaxPanel = new RadAjaxPanel
            //{
            //    ID = _control.ID + "_UP",
            //    LoadingPanelID = loadingPanel.ID,
            //    RestoreOriginalRenderDelegate = false
            //};
            //InjectMessageControl(ajaxPanel);
            //ajaxPanel.Controls.Add(_control);
            //
            //Controls.Add(ajaxPanel);
        }

        /// <summary>
        /// LoadUpdatePanel optionally loads an AJAX Update Panel
        /// </summary>
        private void LoadUpdatePanel()
        {
			//register AJAX
            AJAX.RegisterScriptManager();

            //enable Partial Rendering
            var scriptManager = AJAX.GetScriptManager(Page);
            if (scriptManager != null)
            {
                scriptManager.EnablePartialRendering = true;
            }
			
            //create update panel
            var updatePanel = new UpdatePanel
                                  {
                                      UpdateMode = UpdatePanelUpdateMode.Conditional, 
                                      ID = _control.ID + "_UP"
                                  };

            //get update panel content template
            var templateContainer = updatePanel.ContentTemplateContainer;

            //inject a message placeholder for common module messaging - UI.Skins.Skin.AddModuleMessage
            InjectMessageControl(templateContainer);

            //inject module into update panel content template
            templateContainer.Controls.Add(_control);

            //inject the update panel into the panel
            InjectModuleContent(updatePanel);

            //create image for update progress control
            var progressTemplate = "<div class=\"dnnLoading dnnPanelLoading\"></div>";

            //inject updateprogress into the panel
            var updateProgress = new UpdateProgress
                                     {
                                         AssociatedUpdatePanelID = updatePanel.ID, 
                                         ID = updatePanel.ID + "_Prog",

                                         ProgressTemplate = new LiteralTemplate(progressTemplate)
                                     };
            Controls.Add(updateProgress);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets a flag that indicates whether the Module Instance supports Caching
        /// </summary>
        /// <returns>A Boolean</returns>
        private bool SupportsCaching()
        {
            return _moduleConfiguration.CacheTime > 0;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Trys to load previously cached Module Content
        /// </summary>
        /// <returns>A Boolean that indicates whether the cahed content was loaded</returns>
        private bool TryLoadCached()
        {
            bool success = false;
            string cachedContent = string.Empty;
            try
            {
                var cache = ModuleCachingProvider.Instance(_moduleConfiguration.GetEffectiveCacheMethod());
                var varyBy = new SortedDictionary<string, string> {{"locale", Thread.CurrentThread.CurrentUICulture.ToString()}};

                string cacheKey = cache.GenerateCacheKey(_moduleConfiguration.TabModuleID, varyBy);
                byte[] cachedBytes = ModuleCachingProvider.Instance(_moduleConfiguration.GetEffectiveCacheMethod()).GetModule(_moduleConfiguration.TabModuleID, cacheKey);

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
                _control = ModuleControlFactory.CreateCachedControl(cachedContent, _moduleConfiguration);
                Controls.Add(_control);
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
                switch (match.Groups[1].Value.ToUpperInvariant())
                {
                    case "JAVASCRIPT":
                        ClientResourceManager.RegisterScript(this.Page, match.Groups[2].Value);
                        break;
                    case "CSS":
                        ClientResourceManager.RegisterStyleSheet(this.Page, match.Groups[2].Value);
                        break;
                    case "JS-LIBRARY":
                        var args = match.Groups[2].Value.Split(new[] { ',', }, StringSplitOptions.None);
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

        #endregion

        #region Protected Methods

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// CreateChildControls builds the control tree
        /// </summary>
        protected override void CreateChildControls()
        {
            Controls.Clear();

            //Load Module Control (or cached control)
            LoadModuleControl();
			
			//Optionally Inject AJAX Update Panel
            if (ModuleControl != null)
            {
                //if module is dynamically loaded and AJAX is installed and the control supports partial rendering (defined in ModuleControls table )
                if (!_isCached && _moduleConfiguration.ModuleControl.SupportsPartialRendering && AJAX.IsInstalled())
                {
                    LoadAjaxPanel();
                }
                else
                {
                    //inject a message placeholder for common module messaging - UI.Skins.Skin.AddModuleMessage
                    InjectMessageControl(this);

                    InjectVersionToTheModuleIfSupported();

                    //inject the module into the panel
                    InjectModuleContent(_control);
                }
            }
        }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            if (Host.EnableCustomModuleCssClass)
            {
                string moduleName = ModuleControl.ModuleContext.Configuration.DesktopModule.ModuleName;
                if (moduleName != null)
                {
                    moduleName = Globals.CleanName(moduleName);
                }
                Attributes.Add("class", string.Format("DNNModuleContent Mod{0}C", moduleName));
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// RenderContents renders the contents of the control to the output stream
        /// </summary>
        protected override void RenderContents(HtmlTextWriter writer)
        {
            if (_isCached)
            {
				//Render the cached control to the output stream
                base.RenderContents(writer);
            }
            else
            {
                if (SupportsCaching() && IsViewMode(_moduleConfiguration, PortalSettings) && !Globals.IsAdminControl() && !IsVersionRequest())
                {
					//Render to cache
                    var tempWriter = new StringWriter();

                    _control.RenderControl(new HtmlTextWriter(tempWriter));
                    string cachedOutput = tempWriter.ToString();

                    if (!string.IsNullOrEmpty(cachedOutput) && (!HttpContext.Current.Request.Browser.Crawler))
                    {
						//Save content to cache
                        var moduleContent = Encoding.UTF8.GetBytes(cachedOutput);
                        var cache = ModuleCachingProvider.Instance(_moduleConfiguration.GetEffectiveCacheMethod());

                        var varyBy = new SortedDictionary<string, string> {{"locale", Thread.CurrentThread.CurrentUICulture.ToString()}};

                        var cacheKey = cache.GenerateCacheKey(_moduleConfiguration.TabModuleID, varyBy);
                        cache.SetModule(_moduleConfiguration.TabModuleID, cacheKey, new TimeSpan(0, 0, _moduleConfiguration.CacheTime), moduleContent);
                    }
					
                    //Render the cached content to Response
                    writer.Write(cachedOutput);
                }
                else
                {
					//Render the control to Response
                    base.RenderContents(writer);
                }
            }
        }

        #endregion

    }
}
