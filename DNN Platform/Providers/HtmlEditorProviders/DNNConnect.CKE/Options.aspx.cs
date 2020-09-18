using System;
using System.Globalization;
using System.IO;
using System.Web;
using System.Web.UI.HtmlControls;
using DNNConnect.CKEditorProvider.Controls;
using DotNetNuke.Common;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Framework;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Services.Localization;

namespace DNNConnect.CKEditorProvider
{
    /// <summary>
    /// The options page.
    /// </summary>
    public partial class Options : PageBase
    {
        #region Constants and Fields

        /// <summary>
        /// The request.
        /// </summary>
        private readonly HttpRequest request = HttpContext.Current.Request;

        /// <summary>
        /// The _portal settings.
        /// </summary>
        private PortalSettings curPortalSettings;

        /// <summary>
        ///   Gets Current Language from Url
        /// </summary>
        protected string LangCode
        {
            get
            {
                return request.QueryString["langCode"];
            }
        }

        /// <summary>
        ///   Gets the Name for the Current Resource file name
        /// </summary>
        protected string ResXFile
        {
            get
            {
                string[] page = Request.ServerVariables["SCRIPT_NAME"].Split('/');

                string fileRoot = string.Format(
                    "{0}/{1}/{2}.resx",
                    TemplateSourceDirectory,
                    Localization.LocalResourceDirectory,
                    page[page.GetUpperBound(0)]);

                return fileRoot;
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Register the java scripts and CSS
        /// </summary>
        /// <param name="e">
        /// The Event Args.
        /// </param>
        protected override void OnPreRender(EventArgs e)
        {
            var jqueryScriptLink = new HtmlGenericControl("script");

            jqueryScriptLink.Attributes["type"] = "text/javascript";
            jqueryScriptLink.Attributes["src"] = "//ajax.googleapis.com/ajax/libs/jquery/1/jquery.min.js";

            favicon.Controls.Add(jqueryScriptLink);

            var jqueryUiScriptLink = new HtmlGenericControl("script");

            jqueryUiScriptLink.Attributes["type"] = "text/javascript";
            jqueryUiScriptLink.Attributes["src"] = "//ajax.googleapis.com/ajax/libs/jqueryui/1/jquery-ui.min.js";

            favicon.Controls.Add(jqueryUiScriptLink);

            var notificationScriptLink = new HtmlGenericControl("script");

            notificationScriptLink.Attributes["type"] = "text/javascript";
            notificationScriptLink.Attributes["src"] = ResolveUrl("js/jquery.notification.js");

            favicon.Controls.Add(notificationScriptLink);

            var optionsScriptLink = new HtmlGenericControl("script");

            optionsScriptLink.Attributes["type"] = "text/javascript";
            optionsScriptLink.Attributes["src"] = ResolveUrl("js/Options.js");

            favicon.Controls.Add(optionsScriptLink);

            var objCssLink = new HtmlGenericSelfClosing("link");

            objCssLink.Attributes["rel"] = "stylesheet";
            objCssLink.Attributes["type"] = "text/css";
            objCssLink.Attributes["href"] = "//ajax.googleapis.com/ajax/libs/jqueryui/1/themes/blitzer/jquery-ui.css";

            favicon.Controls.Add(objCssLink);

            var notificationCssLink = new HtmlGenericSelfClosing("link");

            notificationCssLink.Attributes["rel"] = "stylesheet";
            notificationCssLink.Attributes["type"] = "text/css";
            notificationCssLink.Attributes["href"] = ResolveUrl("css/jquery.notification.css");

            favicon.Controls.Add(notificationCssLink);

            var optionsCssLink = new HtmlGenericSelfClosing("link");

            optionsCssLink.Attributes["rel"] = "stylesheet";
            optionsCssLink.Attributes["type"] = "text/css";
            optionsCssLink.Attributes["href"] = ResolveUrl("css/Options.css");

            favicon.Controls.Add(optionsCssLink);

            base.OnPreRender(e);
        }

        /// <summary>
        /// Raises the <see cref="E:Init" /> event.
        /// </summary>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected override void OnInit(EventArgs e)
        {
            // CODEGEN: This call is required by the ASP.NET Web Form Designer.
            InitializeComponent();
            base.OnInit(e);

            // Favicon
            LoadFavIcon();
        }

        /// <summary>
        /// Handles the Load event of the Page control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void Page_Load(object sender, EventArgs e)
        {
            ModuleInfo modInfo = null;
            ModuleController db = new ModuleController();

            try
            {
                // Get ModuleID from Url
                int moduleId;
                if (!int.TryParse(request.QueryString["mid"], NumberStyles.Integer, CultureInfo.InvariantCulture, out moduleId))
                {
                    moduleId = -1;
                }

                // Get TabId from Url
                int tabId;
                if (!int.TryParse(request.QueryString["tid"], NumberStyles.Integer, CultureInfo.InvariantCulture, out tabId))
                {
                    tabId = -1;
                }

                if (moduleId != -1 && tabId != -1)
                {
                    modInfo = db.GetModule(moduleId, tabId, false);
                }
                else
                {
                    ClosePage();
                }
            }
            catch (Exception exception)
            {
                Exceptions.ProcessPageLoadException(exception);

                ClosePage();
            }

            try
            {
                // Get ModuleID from Url
                var oEditorOptions = (CKEditorOptions)Page.LoadControl("CKEditorOptions.ascx");

                oEditorOptions.ID = "CKEditor_Options";
                oEditorOptions.ModuleConfiguration = modInfo;

                phControls.Controls.Add(oEditorOptions);
            }
            catch (Exception exception)
            {
                Exceptions.ProcessPageLoadException(exception);

                ClosePage();
            }
        }

        /// <summary>
        /// Closes the page.
        /// </summary>
        private void ClosePage()
        {
            Page.ClientScript.RegisterStartupScript(
                GetType(), "closeScript", "javascript:self.close();", true);
        }

        /// <summary>
        /// Gets the portal settings.
        /// </summary>
        /// <returns>
        /// The Portal Settings
        /// </returns>
        private PortalSettings GetPortalSettings()
        {
            int iTabId = 0, iPortalId = 0;

            PortalSettings portalSettings;

            try
            {
                if (request.QueryString["tabid"] != null)
                {
                    iTabId = int.Parse(request.QueryString["tabid"]);
                }

                if (request.QueryString["PortalID"] != null)
                {
                    iPortalId = int.Parse(request.QueryString["PortalID"]);
                }

                string sDomainName = Globals.GetDomainName(Request, true);

                string sPortalAlias = PortalAliasController.GetPortalAliasByPortal(iPortalId, sDomainName);

                PortalAliasInfo objPortalAliasInfo = PortalAliasController.Instance.GetPortalAlias(sPortalAlias);

                portalSettings = new PortalSettings(iTabId, objPortalAliasInfo);
            }
            catch (Exception)
            {
                portalSettings = (PortalSettings)HttpContext.Current.Items["PortalSettings"];
            }

            return portalSettings;
        }

        /// <summary>
        /// Required method for Designer support - do not modify
        ///   the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            curPortalSettings = GetPortalSettings();
        }

        /// <summary>
        /// Load Favicon from Current Portal Home Directory
        /// </summary>
        private void LoadFavIcon()
        {
            if (!File.Exists(Path.Combine(curPortalSettings.HomeDirectoryMapPath, "favicon.ico")))
            {
                return;
            }

            var faviconUrl = Path.Combine(curPortalSettings.HomeDirectory, "favicon.ico");

            var objLink = new HtmlGenericSelfClosing("link");

            objLink.Attributes["rel"] = "shortcut icon";
            objLink.Attributes["href"] = faviconUrl;

            favicon.Controls.Add(objLink);
        }

        #endregion
    }
}