#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2013
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
using System.Collections.Specialized;
using System.Globalization;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

using DotNetNuke.Common;
using DotNetNuke.Entities.Host;
using DotNetNuke.Entities.Icons;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Framework.JavaScriptLibraries;
using DotNetNuke.Instrumentation;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Services.Localization;
using DotNetNuke.UI.Modules;
using DotNetNuke.Web.Client.ClientResourceManagement;

#endregion

namespace DotNetNuke.Framework
{
    /// -----------------------------------------------------------------------------
    /// Namespace:  DotNetNuke.Framework
    /// Project:    DotNetNuke
    /// Class:      PageBase
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// PageBase provides a custom DotNetNuke base class for pages
    /// </summary>
    /// <history>
    ///		[cnurse]	11/30/2006	documented
    /// </history>
    /// -----------------------------------------------------------------------------
    public abstract class PageBase : Page
    {
    	private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof (PageBase));

	    private PageStatePersister _persister;
        #region Private Members

        private readonly NameValueCollection _htmlAttributes = new NameValueCollection();
        private readonly ArrayList _localizedControls;
        private CultureInfo _pageCulture;
        private string _localResourceFile;

        #endregion

        #region Constructors

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Creates the Page
        /// </summary>
        /// <history>
        /// 	[cnurse]	    11/30/2006	Documented
        /// </history>
        /// -----------------------------------------------------------------------------
        protected PageBase()
        {
            _localizedControls = new ArrayList();
        }

        #endregion

        #region Protected Properties

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// PageStatePersister returns an instance of the class that will be used to persist the Page State
        /// </summary>
        /// <returns>A System.Web.UI.PageStatePersister</returns>
        /// <history>
        /// 	[cnurse]	    11/30/2005	Created
        /// </history>
        /// -----------------------------------------------------------------------------
        protected override PageStatePersister PageStatePersister
        {
            get
            {
                //Set ViewState Persister to default (as defined in Base Class)
	            if (_persister == null)
	            {
		            _persister = base.PageStatePersister;

		            if (Globals.Status == Globals.UpgradeStatus.None)
		            {
			            switch (Host.PageStatePersister)
			            {
				            case "M":
								_persister = new CachePageStatePersister(this);
					            break;
				            case "D":
								_persister = new DiskPageStatePersister(this);
					            break;
			            }
		            }
	            }

				return _persister;
            }
        }

        #endregion

        #region Public Properties

        public PortalSettings PortalSettings
        {
            get
            {
                return PortalController.GetCurrentPortalSettings();
            }
        }

        public NameValueCollection HtmlAttributes
        {
            get
            {
                return _htmlAttributes;
            }
        }

        public CultureInfo PageCulture
        {
            get
            {
                return _pageCulture ?? (_pageCulture = Localization.GetPageLocale(PortalSettings));
            }
        }

        public string LocalResourceFile
        {
            get
            {
                string fileRoot;
                string[] page = Request.ServerVariables["SCRIPT_NAME"].Split('/');
                if (String.IsNullOrEmpty(_localResourceFile))
                {
                    fileRoot = TemplateSourceDirectory + "/" + Localization.LocalResourceDirectory + "/" + page[page.GetUpperBound(0)] + ".resx";
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

        #endregion

        #region Private Methods

        private string GetErrorUrl(string url, Exception exc, bool hideContent = true)
        {
            if (Request.QueryString["error"] != null)
            {
                url += (url.IndexOf("?", StringComparison.Ordinal) == -1 ? "?" : "&") + "error=terminate";
            }
            else
            {
                url += (url.IndexOf("?", StringComparison.Ordinal) == -1 ? "?" : "&") + "error=" + (exc == null || UserController.GetCurrentUserInfo() == null || !UserController.GetCurrentUserInfo().IsSuperUser ? "An unexpected error has occurred" : Server.UrlEncode(exc.Message));
                if (!Globals.IsAdminControl() && hideContent)
                {
                    url += "&content=0";
                }
            }

            return url;
        }

        private bool IsViewStateFailure(Exception e)
        {
            return !User.Identity.IsAuthenticated && e != null && e.InnerException is ViewStateException;
        }

        private void IterateControls(ControlCollection controls, ArrayList affectedControls, string resourceFileRoot)
        {
            foreach (Control c in controls)
            {
                ProcessControl(c, affectedControls, true, resourceFileRoot);
            }
        }

        #endregion

        #region Protected Methods

        protected virtual void RegisterAjaxScript()
        {
            if (ServicesFrameworkInternal.Instance.IsAjaxScriptSupportRequired)
            {
                ServicesFrameworkInternal.Instance.RegisterAjaxScript(Page);
            }
        }

        protected override void OnError(EventArgs e)
        {
            base.OnError(e);
            Exception exc = Server.GetLastError();
            Logger.Fatal("An error has occurred while loading page.", exc);

            string strURL = Globals.ApplicationURL();
            if (exc is HttpException && !IsViewStateFailure(exc))
            {
                if (PortalSettings.ErrorPage500 != -1)
                {
                    string url = GetErrorUrl("~/Default.aspx?tabid=" + PortalSettings.ErrorPage500, exc, false);
                    HttpContext.Current.Response.Redirect(url);
                }
                else
                {
                    HttpContext.Current.Response.Clear();
                    HttpContext.Current.Server.Transfer("~/ErrorPage.aspx");
                }
            }

            strURL = GetErrorUrl(strURL, exc);
            Exceptions.ProcessPageLoadException(exc, strURL);
        }

        protected override void OnInit(EventArgs e)
        {
	        var isInstallPage = HttpContext.Current.Request.Url.LocalPath.ToLower().Contains("installwizard.aspx");
			if (!isInstallPage)
            {
                Localization.SetThreadCultures(PageCulture, PortalSettings);
            }

			AJAX.AddScriptManager(this, !isInstallPage);

            var dnncoreFilePath = HttpContext.Current.IsDebuggingEnabled
                   ? "~/js/Debug/dnncore.js"
                   : "~/js/dnncore.js";

            ClientResourceManager.RegisterScript(this, dnncoreFilePath);

            base.OnInit(e);
        }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            //Because we have delayed registration of the jQuery script,
            //Modules can override the standard behavior by including their own script on the page.
            //The module must register the script with the "jQuery" key and should notify user
            //of potential version conflicts with core jQuery support.
            //if (jQuery.IsRequested)
            //{
            //    jQuery.RegisterJQuery(Page);
            //}
            //if (jQuery.IsUIRequested)
            //{
            //    jQuery.RegisterJQueryUI(Page);
            //}
            //if (jQuery.AreDnnPluginsRequested)
            //{
            //    jQuery.RegisterDnnJQueryPlugins(Page);
            //}
            //if (jQuery.IsHoverIntentRequested)
            //{
            //    jQuery.RegisterHoverIntent(Page);
            //}

            JavaScript.Register(Page);

            if(ServicesFrameworkInternal.Instance.IsAjaxAntiForgerySupportRequired)
            {
                ServicesFrameworkInternal.Instance.RegisterAjaxAntiForgery(Page);
            }

            RegisterAjaxScript();
        }

        protected override void Render(HtmlTextWriter writer)
        {
            IterateControls(Controls, _localizedControls, LocalResourceFile);
            RemoveKeyAttribute(_localizedControls);
            AJAX.RemoveScriptManager(this);
            base.Render(writer);
        }


        #endregion

        #region Public Methods

        /// <summary>
        /// <para>GetControlAttribute looks a the type of control and does it's best to find an AttributeCollection.</para>
        /// </summary>
        /// <param name="control">Control to find the AttributeCollection on</param>
        /// <param name="affectedControls">ArrayList that hold the controls that have been localized. This is later used for the removal of the key attribute.</param>				
        /// <param name="attributeName">Name of key to search for.</param>				
        /// <returns>A string containing the key for the specified control or null if a key attribute wasn't found</returns>
        internal static string GetControlAttribute(Control control, ArrayList affectedControls, string attributeName)
        {
            AttributeCollection attributeCollection = null;
            string key = null;
            if (!(control is LiteralControl))
            {
                if (control is WebControl)
                {
                    var webControl = (WebControl)control;
                    attributeCollection = webControl.Attributes;
                    key = attributeCollection[attributeName];
                }
                else
                {
                    if (control is HtmlControl)
                    {
                        var htmlControl = (HtmlControl)control;
                        attributeCollection = htmlControl.Attributes;
                        key = attributeCollection[attributeName];
                    }
                    else
                    {
                        if (control is UserControl)
                        {
                            var userControl = (UserControl)control;
                            attributeCollection = userControl.Attributes;
                            key = attributeCollection[attributeName];
                        }
                        else
                        {
                            Type controlType = control.GetType();
                            PropertyInfo attributeProperty = controlType.GetProperty("Attributes", typeof(AttributeCollection));
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

        /// <summary>
        /// <para>ProcessControl peforms the high level localization for a single control and optionally it's children.</para>
        /// </summary>
        /// <param name="control">Control to find the AttributeCollection on</param>
        /// <param name="affectedControls">ArrayList that hold the controls that have been localized. This is later used for the removal of the key attribute.</param>				
        /// <param name="includeChildren">If true, causes this method to process children of this controls.</param>
        /// <param name="resourceFileRoot">Root Resource File.</param>
        internal void ProcessControl(Control control, ArrayList affectedControls, bool includeChildren, string resourceFileRoot)
        {
            if (!control.Visible) return;

            //Perform the substitution if a key was found
            string key = GetControlAttribute(control, affectedControls, Localization.KeyName);
            if (!string.IsNullOrEmpty(key))
            {
                //Translation starts here ....
                string value = Localization.GetString(key, resourceFileRoot);
                if (control is Label)
                {
                    var label = (Label)control;
                    if (!String.IsNullOrEmpty(value))
                    {
                        label.Text = value;
                    }
                }
                if (control is LinkButton)
                {
                    var linkButton = (LinkButton)control;
                    if (!String.IsNullOrEmpty(value))
                    {
                        MatchCollection imgMatches = Regex.Matches(value, "<(a|link|img|script|input|form).[^>]*(href|src|action)=(\\\"|'|)(.[^\\\"']*)(\\\"|'|)[^>]*>", RegexOptions.IgnoreCase);
                        foreach (Match match in imgMatches)
                        {
                            if ((match.Groups[match.Groups.Count - 2].Value.IndexOf("~", StringComparison.Ordinal) != -1))
                            {
                                string resolvedUrl = Page.ResolveUrl(match.Groups[match.Groups.Count - 2].Value);
                                value = value.Replace(match.Groups[match.Groups.Count - 2].Value, resolvedUrl);
                            }
                        }
                        linkButton.Text = value;
                        if (string.IsNullOrEmpty(linkButton.ToolTip))
                        {
                            linkButton.ToolTip = value;
                        }
                    }
                }
                if (control is HyperLink)
                {
                    var hyperLink = (HyperLink)control;
                    if (!String.IsNullOrEmpty(value))
                    {
                        hyperLink.Text = value;
                    }
                }
                if (control is ImageButton)
                {
                    var imageButton = (ImageButton)control;
                    if (!String.IsNullOrEmpty(value))
                    {
                        imageButton.AlternateText = value;
                    }
                }
                if (control is Button)
                {
                    var button = (Button)control;
                    if (!String.IsNullOrEmpty(value))
                    {
                        button.Text = value;
                    }
                }
                if (control is HtmlButton)
                {
                    var button = (HtmlButton)control;
                    if (!String.IsNullOrEmpty(value))
                    {
                        button.Attributes["Title"] = value;
                    }
                }
                if (control is HtmlImage)
                {
                    var htmlImage = (HtmlImage)control;
                    if (!String.IsNullOrEmpty(value))
                    {
                        htmlImage.Alt = value;
                    }
                }
                if (control is CheckBox)
                {
                    var checkBox = (CheckBox)control;
                    if (!String.IsNullOrEmpty(value))
                    {
                        checkBox.Text = value;
                    }
                }
                if (control is BaseValidator)
                {
                    var baseValidator = (BaseValidator)control;
                    if (!String.IsNullOrEmpty(value))
                    {
                        baseValidator.ErrorMessage = value;
                    }
                }
                if (control is Image)
                {
                    var image = (Image)control;
                    if (!String.IsNullOrEmpty(value))
                    {
                        image.AlternateText = value;
                        image.ToolTip = value;
                    }
                }
            }

            //Translate listcontrol items here 
            if (control is ListControl)
            {
                var listControl = (ListControl)control;
                for (int i = 0; i <= listControl.Items.Count - 1; i++)
                {
                    AttributeCollection attributeCollection = listControl.Items[i].Attributes;
                    key = attributeCollection[Localization.KeyName];
                    if (key != null)
                    {
                        string value = Localization.GetString(key, resourceFileRoot);
                        if (!String.IsNullOrEmpty(value))
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


            //UrlRewriting Issue - ResolveClientUrl gets called instead of ResolveUrl
            //Manual Override to ResolveUrl
            if (control is Image)
            {
                var image = (Image)control;
                if (image.ImageUrl.IndexOf("~", StringComparison.Ordinal) != -1)
                {
                    image.ImageUrl = Page.ResolveUrl(image.ImageUrl);
                }

                //Check for IconKey
                if (string.IsNullOrEmpty(image.ImageUrl))
                {
                    string iconKey = GetControlAttribute(control, affectedControls, IconController.IconKeyName);
                    string iconSize = GetControlAttribute(control, affectedControls, IconController.IconSizeName);
                    string iconStyle = GetControlAttribute(control, affectedControls, IconController.IconStyleName);
                    image.ImageUrl = IconController.IconURL(iconKey, iconSize, iconStyle);
                }
            }

            //UrlRewriting Issue - ResolveClientUrl gets called instead of ResolveUrl
            //Manual Override to ResolveUrl
            if (control is HtmlImage)
            {
                var htmlImage = (HtmlImage)control;
                if (htmlImage.Src.IndexOf("~", StringComparison.Ordinal) != -1)
                {
                    htmlImage.Src = Page.ResolveUrl(htmlImage.Src);
                }

                //Check for IconKey
                if (string.IsNullOrEmpty(htmlImage.Src))
                {
                    string iconKey = GetControlAttribute(control, affectedControls, IconController.IconKeyName);
                    string iconSize = GetControlAttribute(control, affectedControls, IconController.IconSizeName);
                    string iconStyle = GetControlAttribute(control, affectedControls, IconController.IconStyleName);
                    htmlImage.Src = IconController.IconURL(iconKey, iconSize, iconStyle);
                }
            }

            //UrlRewriting Issue - ResolveClientUrl gets called instead of ResolveUrl
            //Manual Override to ResolveUrl
            if (control is HyperLink)
            {
                HyperLink ctrl;
                ctrl = (HyperLink)control;
                if ((ctrl.NavigateUrl.IndexOf("~", StringComparison.Ordinal) != -1))
                {
                    ctrl.NavigateUrl = Page.ResolveUrl(ctrl.NavigateUrl);
                }
                if ((ctrl.ImageUrl.IndexOf("~", StringComparison.Ordinal) != -1))
                {
                    ctrl.ImageUrl = Page.ResolveUrl(ctrl.ImageUrl);
                }

                //Check for IconKey
                if (string.IsNullOrEmpty(ctrl.ImageUrl))
                {
                    string iconKey = GetControlAttribute(control, affectedControls, IconController.IconKeyName);
                    string iconSize = GetControlAttribute(control, affectedControls, IconController.IconSizeName);
                    string iconStyle = GetControlAttribute(control, affectedControls, IconController.IconStyleName);
                    ctrl.ImageUrl = IconController.IconURL(iconKey, iconSize, iconStyle);
                }
            }

            //Process child controls
            if (includeChildren && control.HasControls())
            {
                var objModuleControl = control as IModuleControl;
                if (objModuleControl == null)
                {
                    PropertyInfo pi = control.GetType().GetProperty("LocalResourceFile");
                    if (pi != null && pi.GetValue(control, null) != null)
                    {
                        //If controls has a LocalResourceFile property use this
                        IterateControls(control.Controls, affectedControls, pi.GetValue(control, null).ToString());
                    }
                    else
                    {
                        //Pass Resource File Root through
                        IterateControls(control.Controls, affectedControls, resourceFileRoot);
                    }
                }
                else
                {
                    //Get Resource File Root from Controls LocalResourceFile Property
                    IterateControls(control.Controls, affectedControls, objModuleControl.LocalResourceFile);
                }
            }
        }

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

        #endregion

        #region Obsolete Methods

        [Obsolete("Deprecated in DNN 6.1. Replaced by ClientResourceManager.RegisterStyleSheet.")]
        protected static void AddStyleSheetInternal(Page page, string id, string styleSheet, bool isFirst)
        {
            Control objCSS = page.FindControl("CSS");
            if (objCSS != null)
            {
                Control objCtrl = page.Header.FindControl(id);
                if (objCtrl == null)
                {
                    var objLink = new HtmlLink { ID = id };
                    objLink.Attributes["rel"] = "stylesheet";
                    objLink.Attributes["type"] = "text/css";
                    objLink.Href = styleSheet;
                    if (isFirst)
                    {
                        int iLink;
                        for (iLink = 0; iLink <= objCSS.Controls.Count - 1; iLink++)
                        {
                            if (objCSS.Controls[iLink] is HtmlLink)
                            {
                                break;
                            }
                        }
                        objCSS.Controls.AddAt(iLink, objLink);
                    }
                    else
                    {
                        objCSS.Controls.Add(objLink);
                    }
                }
            }
        }

        [Obsolete("Deprecated in DNN 6.1. Replaced by ClientResourceManager.RegisterStyleSheet.")]
        public static void RegisterStyleSheet(Page page, string styleSheet)
        {
            RegisterStyleSheet(page, styleSheet, false);
        }

        [Obsolete("Deprecated in DNN 6.1. Replaced by ClientResourceManager.RegisterStyleSheet.")]
        public static void RegisterStyleSheet(Page page, string styleSheet, bool isFirst)
        {
            if (isFirst)
            {
                ClientResourceManager.RegisterStyleSheet(page, styleSheet, 0);
            }
            else
            {
                ClientResourceManager.RegisterStyleSheet(page, styleSheet);
            }
        }

        #endregion

    }
}