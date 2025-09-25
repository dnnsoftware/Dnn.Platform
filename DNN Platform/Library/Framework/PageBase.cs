// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Framework
{
    using System;
    using System.Collections;
    using System.Collections.Specialized;
    using System.Globalization;
    using System.Net;
    using System.Text.RegularExpressions;
    using System.Web;
    using System.Web.UI;
    using System.Web.UI.HtmlControls;
    using System.Web.UI.WebControls;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Icons;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Services.Exceptions;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.UI.Modules;
    using DotNetNuke.Web.Client.ClientResourceManagement;

    using Microsoft.Extensions.DependencyInjection;

    /// <summary>PageBase provides a custom DotNetNuke base class for pages.</summary>
    public abstract class PageBase : Page
    {
        private const string LinkItemPattern = "<(a|link|img|script|input|form|object).[^>]*(href|src|action)=(\\\"|'|)(.[^\\\"']*)(\\\"|'|)[^>]*>";

        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(PageBase));
        private static readonly Regex LinkItemMatchRegex = new Regex(LinkItemPattern, RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private readonly ILog traceLogger = LoggerSource.Instance.GetLogger("DNN.Trace");
        private readonly ArrayList localizedControls = [];
        private readonly IPortalController portalController;
        private readonly IApplicationStatusInfo appStatus;
        private readonly IHostSettings hostSettings;

        private PageStatePersister persister;
        private CultureInfo pageCulture;
        private string localResourceFile;

        /// <summary>Initializes a new instance of the <see cref="PageBase"/> class.</summary>
        [Obsolete("Deprecated in DotNetNuke 10.0.2. Please use overload with IPortalController. Scheduled removal in v12.0.0.")]
        protected PageBase()
            : this(null, null, null)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="PageBase"/> class.</summary>
        /// <param name="portalController">The portal controller.</param>
        /// <param name="appStatus">The application status.</param>
        /// <param name="hostSettings">The host settings.</param>
        protected PageBase(IPortalController portalController, IApplicationStatusInfo appStatus, IHostSettings hostSettings)
        {
            this.portalController = portalController ?? Globals.GetCurrentServiceProvider().GetRequiredService<IPortalController>();
            this.appStatus = appStatus ?? Globals.GetCurrentServiceProvider().GetRequiredService<IApplicationStatusInfo>();
            this.hostSettings = hostSettings ?? Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettings>();
        }

        public PortalSettings PortalSettings => this.portalController.GetCurrentPortalSettings();

        public NameValueCollection HtmlAttributes { get; } = [];

        public CultureInfo PageCulture => this.pageCulture ?? (this.pageCulture = Localization.GetPageLocale(this.PortalSettings));

        public string LocalResourceFile
        {
            get
            {
                string fileRoot;
                var page = this.Request.ServerVariables["SCRIPT_NAME"].Split('/');
                if (string.IsNullOrEmpty(this.localResourceFile))
                {
                    fileRoot = string.Concat(this.TemplateSourceDirectory, "/", Localization.LocalResourceDirectory, "/", page[page.GetUpperBound(0)], ".resx");
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

        public string CanonicalLinkUrl { get; set; }

        /// <summary>Gets a value indicating whether HTTP headers has been sent to client.</summary>
        public bool HeaderIsWritten { get; internal set; }

        /// <summary>Gets pageStatePersister returns an instance of the class that will be used to persist the Page State.</summary>
        /// <returns>A System.Web.UI.PageStatePersister.</returns>
        protected override PageStatePersister PageStatePersister
        {
            get
            {
                // Set ViewState Persister to default (as defined in Base Class)
                if (this.persister == null)
                {
                    this.persister = base.PageStatePersister;

                    if (this.appStatus.Status == UpgradeStatus.None)
                    {
                        switch (this.hostSettings.PageStatePersister)
                        {
                            case "M":
                                this.persister = new CachePageStatePersister(this);
                                break;
                            case "D":
                                this.persister = new DiskPageStatePersister(this);
                                break;
                        }
                    }
                }

                return this.persister;
            }
        }

        /// <inheritdoc cref="HtmlUtils.JavaScriptStringEncode(string)"/>
        public static IHtmlString JavaScriptStringEncode(string value)
            => HtmlUtils.JavaScriptStringEncode(value);

        /// <inheritdoc cref="HtmlUtils.JavaScriptStringEncode(string,bool)"/>
        public static IHtmlString JavaScriptStringEncode(string value, bool addDoubleQuotes)
            => HtmlUtils.JavaScriptStringEncode(value, addDoubleQuotes);

        /// <summary>
        /// <para>RemoveKeyAttribute remove the key attribute from the control. If this isn't done, then the HTML output will have
        /// a bad attribute on it which could cause some older browsers problems.</para>
        /// </summary>
        /// <param name="affectedControls">ArrayList that hold the controls that have been localized. This is later used for the removal of the key attribute.</param>
        public static void RemoveKeyAttribute(ArrayList affectedControls)
        {
            if (affectedControls == null)
            {
                return;
            }

            int i;
            for (i = 0; i <= affectedControls.Count - 1; i++)
            {
                var ac = (AttributeCollection)affectedControls[i];
                ac.Remove(Localization.KeyName);
                ac.Remove(IconController.IconKeyName);
                ac.Remove(IconController.IconSizeName);
                ac.Remove(IconController.IconStyleName);
            }
        }

        /// <summary><para>GetControlAttribute looks a the type of control and does it's best to find an AttributeCollection.</para></summary>
        /// <param name="control">Control to find the AttributeCollection on.</param>
        /// <param name="affectedControls">ArrayList that hold the controls that have been localized. This is later used for the removal of the key attribute.</param>
        /// <param name="attributeName">Name of key to search for.</param>
        /// <returns>A string containing the key for the specified control or null if a key attribute wasn't found.</returns>
        internal static string GetControlAttribute(Control control, ArrayList affectedControls, string attributeName)
        {
            AttributeCollection attributeCollection = null;
            string key = null;
            if (!(control is LiteralControl))
            {
                if (control is WebControl webControl)
                {
                    attributeCollection = webControl.Attributes;
                    key = attributeCollection[attributeName];
                }
                else
                {
                    if (control is HtmlControl htmlControl)
                    {
                        attributeCollection = htmlControl.Attributes;
                        key = attributeCollection[attributeName];
                    }
                    else
                    {
                        if (control is UserControl userControl)
                        {
                            attributeCollection = userControl.Attributes;
                            key = attributeCollection[attributeName];
                        }
                        else
                        {
                            var controlType = control.GetType();
                            var attributeProperty = controlType.GetProperty("Attributes", typeof(AttributeCollection));
                            if (attributeProperty != null)
                            {
                                attributeCollection = (AttributeCollection)attributeProperty.GetValue(control, null);
                                key = attributeCollection[attributeName];
                            }
                        }
                    }
                }
            }

            if (key != null && affectedControls != null)
            {
                affectedControls.Add(attributeCollection);
            }

            return key;
        }

        /// <summary><para>ProcessControl performs the high level localization for a single control and optionally it's children.</para></summary>
        /// <param name="control">Control to find the AttributeCollection on.</param>
        /// <param name="affectedControls">ArrayList that hold the controls that have been localized. This is later used for the removal of the key attribute.</param>
        /// <param name="includeChildren">If true, causes this method to process children of this controls.</param>
        /// <param name="resourceFileRoot">Root Resource File.</param>
        internal void ProcessControl(Control control, ArrayList affectedControls, bool includeChildren, string resourceFileRoot)
        {
            if (!control.Visible)
            {
                return;
            }

            // Perform the substitution if a key was found
            var key = GetControlAttribute(control, affectedControls, Localization.KeyName);
            if (!string.IsNullOrEmpty(key))
            {
                // Translation starts here ....
                var value = Localization.GetString(key, resourceFileRoot);
                this.LocalizeControl(control, value);
            }

            // Translate ListControl items here
            if (control is ListControl listControl)
            {
                for (var i = 0; i <= listControl.Items.Count - 1; i++)
                {
                    var attributeCollection = listControl.Items[i].Attributes;
                    key = attributeCollection[Localization.KeyName];
                    if (key != null)
                    {
                        var value = Localization.GetString(key, resourceFileRoot);
                        if (!string.IsNullOrEmpty(value))
                        {
                            listControl.Items[i].Text = value;
                        }
                    }

                    if (key != null && affectedControls != null)
                    {
                        affectedControls.Add(attributeCollection);
                    }
                }
            }

            // UrlRewriting Issue - ResolveClientUrl gets called instead of ResolveUrl
            // Manual Override to ResolveUrl
            if (control is Image image)
            {
                if (image.ImageUrl.IndexOf("~", StringComparison.Ordinal) != -1)
                {
                    image.ImageUrl = this.Page.ResolveUrl(image.ImageUrl);
                }

                // Check for IconKey
                if (string.IsNullOrEmpty(image.ImageUrl))
                {
                    var iconKey = GetControlAttribute(control, affectedControls, IconController.IconKeyName);
                    var iconSize = GetControlAttribute(control, affectedControls, IconController.IconSizeName);
                    var iconStyle = GetControlAttribute(control, affectedControls, IconController.IconStyleName);
                    image.ImageUrl = IconController.IconURL(iconKey, iconSize, iconStyle);
                }
            }

            // UrlRewriting Issue - ResolveClientUrl gets called instead of ResolveUrl
            // Manual Override to ResolveUrl
            if (control is HtmlImage htmlImage)
            {
                if (htmlImage.Src.IndexOf("~", StringComparison.Ordinal) != -1)
                {
                    htmlImage.Src = this.Page.ResolveUrl(htmlImage.Src);
                }

                // Check for IconKey
                if (string.IsNullOrEmpty(htmlImage.Src))
                {
                    var iconKey = GetControlAttribute(control, affectedControls, IconController.IconKeyName);
                    var iconSize = GetControlAttribute(control, affectedControls, IconController.IconSizeName);
                    var iconStyle = GetControlAttribute(control, affectedControls, IconController.IconStyleName);
                    htmlImage.Src = IconController.IconURL(iconKey, iconSize, iconStyle);
                }
            }

            // UrlRewriting Issue - ResolveClientUrl gets called instead of ResolveUrl
            // Manual Override to ResolveUrl
            if (control is HyperLink ctrl)
            {
                if (ctrl.NavigateUrl.IndexOf("~", StringComparison.Ordinal) != -1)
                {
                    ctrl.NavigateUrl = this.Page.ResolveUrl(ctrl.NavigateUrl);
                }

                if (ctrl.ImageUrl.IndexOf("~", StringComparison.Ordinal) != -1)
                {
                    ctrl.ImageUrl = this.Page.ResolveUrl(ctrl.ImageUrl);
                }

                // Check for IconKey
                if (string.IsNullOrEmpty(ctrl.ImageUrl))
                {
                    var iconKey = GetControlAttribute(control, affectedControls, IconController.IconKeyName);
                    var iconSize = GetControlAttribute(control, affectedControls, IconController.IconSizeName);
                    var iconStyle = GetControlAttribute(control, affectedControls, IconController.IconStyleName);
                    ctrl.ImageUrl = IconController.IconURL(iconKey, iconSize, iconStyle);
                }
            }

            // Process child controls
            if (!includeChildren || !control.HasControls())
            {
                return;
            }

            if (control is not IModuleControl objModuleControl)
            {
                // Cache results from reflection calls for performance
                var pi = control.GetType().GetProperty("LocalResourceFile");
                if (pi != null)
                {
                    // Attempt to get property value
                    var pv = pi.GetValue(control, null);

                    // If controls has a LocalResourceFile property use this, otherwise pass the resource file root
                    this.IterateControls(control.Controls, affectedControls, pv == null ? resourceFileRoot : pv.ToString());
                }
                else
                {
                    // Pass Resource File Root through
                    this.IterateControls(control.Controls, affectedControls, resourceFileRoot);
                }
            }
            else
            {
                // Get Resource File Root from Controls LocalResourceFile Property
                this.IterateControls(control.Controls, affectedControls, objModuleControl.LocalResourceFile);
            }
        }

        protected virtual void RegisterAjaxScript()
        {
            if (ServicesFrameworkInternal.Instance.IsAjaxScriptSupportRequired)
            {
                ServicesFrameworkInternal.Instance.RegisterAjaxScript(this.Page);
            }
        }

        /// <inheritdoc/>
        protected override void OnError(EventArgs e)
        {
            base.OnError(e);
            Exception exc = this.Server.GetLastError();
            Logger.Fatal("An error has occurred while loading page.", exc);

            string strURL = Globals.ApplicationURL();
            if (exc is HttpException exception && !this.IsViewStateFailure(exception))
            {
                try
                {
                    // if the exception's status code set to 404, we need display 404 page if defined or show no found info.
                    var statusCode = exception.GetHttpCode();
                    if (statusCode == 404)
                    {
                        UrlUtils.Handle404Exception(this.Response, this.PortalSettings);
                    }

                    if (this.PortalSettings?.ErrorPage500 != -1)
                    {
                        var url = this.GetErrorUrl($"~/Default.aspx?tabid={this.PortalSettings.ErrorPage500}", exception, false);
                        HttpContext.Current.Response.Redirect(url);
                    }
                    else
                    {
                        HttpContext.Current.Response.Clear();
                        HttpContext.Current.Server.Transfer("~/ErrorPage.aspx");
                    }
                }
                catch (Exception)
                {
                    HttpContext.Current.Response.Clear();
                    var errorMessage = Localization.GetString("NoSitesForThisInstallation.Error", Localization.GlobalResourceFile);
                    HttpContext.Current.Server.Transfer($"~/ErrorPage.aspx?status=503&error={HttpUtility.UrlEncode(errorMessage)}");
                }
            }

            strURL = this.GetErrorUrl(strURL, exc);
            Exceptions.ProcessPageLoadException(exc, strURL);
        }

        /// <inheritdoc/>
        protected override void OnInit(EventArgs e)
        {
            var isInstallPage = HttpContext.Current.Request.Url.LocalPath.ToLowerInvariant().Contains("installwizard.aspx");
            if (ScriptManager.GetCurrent(this) == null)
            {
                AJAX.AddScriptManager(this, !isInstallPage);
            }

            var dnncoreFilePath = HttpContext.Current.IsDebuggingEnabled
                   ? "~/js/Debug/dnncore.js"
                   : "~/js/dnncore.js";

            ClientResourceManager.RegisterScript(this, dnncoreFilePath);

            base.OnInit(e);
        }

        /// <inheritdoc/>
        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            // Because we have delayed registration of the jQuery script,
            // Modules can override the standard behavior by including their own script on the page.
            // The module must register the script with the "jQuery" key and should notify user
            // of potential version conflicts with core jQuery support.
            // if (jQuery.IsRequested)
            // {
            //    jQuery.RegisterJQuery(Page);
            // }
            // if (jQuery.IsUIRequested)
            // {
            //    jQuery.RegisterJQueryUI(Page);
            // }
            // if (jQuery.AreDnnPluginsRequested)
            // {
            //    jQuery.RegisterDnnJQueryPlugins(Page);
            // }
            // if (jQuery.IsHoverIntentRequested)
            // {
            //    jQuery.RegisterHoverIntent(Page);
            // }
            if (ServicesFrameworkInternal.Instance.IsAjaxAntiForgerySupportRequired)
            {
                ServicesFrameworkInternal.Instance.RegisterAjaxAntiForgery(this.Page);
            }

            this.RegisterAjaxScript();
        }

        /// <inheritdoc/>
        protected override void Render(HtmlTextWriter writer)
        {
            this.LogDnnTrace("PageBase.Render", "Start", $"{this.Page.Request.Url.AbsoluteUri}");

            this.IterateControls(this.Controls, this.localizedControls, this.LocalResourceFile);
            RemoveKeyAttribute(this.localizedControls);
            AJAX.RemoveScriptManager(this);
            base.Render(writer);

            this.LogDnnTrace("PageBase.Render", "End", $"{this.Page.Request.Url.AbsoluteUri}");
        }

        private string GetErrorUrl(string url, Exception exc, bool hideContent = true)
        {
            var separator = url.IndexOf('?') == -1 ? '?' : '&';
            if (this.Request.QueryString["error"] != null)
            {
                return $"{url}{separator}error=terminate";
            }

            var user = UserController.Instance.GetCurrentUserInfo();
            var errorMessage = exc == null || user is not { IsSuperUser: true }
                    ? "An unexpected error has occurred"
                    : exc.Message;
            url = $"{url}{separator}error={WebUtility.UrlEncode(errorMessage)}";
            if (!Globals.IsAdminControl() && hideContent)
            {
                return $"{url}&content=0";
            }

            return url;
        }

        private bool IsViewStateFailure(Exception e)
        {
            return !this.User.Identity.IsAuthenticated && e?.InnerException is ViewStateException;
        }

        private void IterateControls(ControlCollection controls, ArrayList affectedControls, string resourceFileRoot)
        {
            foreach (Control c in controls)
            {
                this.ProcessControl(c, affectedControls, true, resourceFileRoot);
                this.LogDnnTrace("PageBase.IterateControls", "Info", $"ControlId: {c.ID}");
            }
        }

        private void LogDnnTrace(string origin, string action, string message)
        {
            var tabId = -1;
            if (this.PortalSettings?.ActiveTab != null)
            {
                tabId = this.PortalSettings.ActiveTab.TabID;
            }

            if (this.traceLogger.IsDebugEnabled)
            {
                this.traceLogger.Debug($"{origin} {action} (TabId:{tabId},{message})");
            }
        }

        private void LocalizeControl(Control control, string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return;
            }

            if (control is BaseValidator validator)
            {
                validator.ErrorMessage = value;
                validator.Text = value;
                return;
            }

            if (control is Label label)
            {
                label.Text = value;
                return;
            }

            if (control is LinkButton linkButton)
            {
                var imgMatches = LinkItemMatchRegex.Matches(value);
                foreach (Match match in imgMatches)
                {
                    if (match.Groups[match.Groups.Count - 2].Value.IndexOf("~", StringComparison.Ordinal) == -1)
                    {
                        continue;
                    }

                    var resolvedUrl = this.Page.ResolveUrl(match.Groups[match.Groups.Count - 2].Value);
                    value = value.Replace(match.Groups[match.Groups.Count - 2].Value, resolvedUrl);
                }

                linkButton.Text = value;
                if (string.IsNullOrEmpty(linkButton.ToolTip))
                {
                    linkButton.ToolTip = value;
                }

                return;
            }

            if (control is HyperLink hyperLink)
            {
                hyperLink.Text = value;
                return;
            }

            if (control is Button button)
            {
                button.Text = value;
                return;
            }

            if (control is HtmlButton htmlButton)
            {
                htmlButton.Attributes["Title"] = value;
                return;
            }

            if (control is HtmlImage htmlImage)
            {
                htmlImage.Alt = value;
                return;
            }

            if (control is CheckBox checkBox)
            {
                checkBox.Text = value;
                return;
            }

            if (control is Image image)
            {
                image.AlternateText = value;
                image.ToolTip = value;
                return;
            }

            if (control is TextBox textBox)
            {
                textBox.ToolTip = value;
            }
        }
    }
}
