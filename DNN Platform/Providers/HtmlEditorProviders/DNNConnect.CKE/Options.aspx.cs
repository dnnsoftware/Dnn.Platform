// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DNNConnect.CKEditorProvider
{
    using System;
    using System.Globalization;
    using System.Web;
    using System.Web.UI;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Extensions;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Framework;
    using DotNetNuke.Framework.JavaScriptLibraries;
    using DotNetNuke.Services.Exceptions;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.Web.Client.ClientResourceManagement;

    using Microsoft.Extensions.DependencyInjection;

    /// <summary>The options page.</summary>
    public partial class Options : PageBase
    {
        private readonly IHostSettings hostSettings;

        /// <summary>The request.</summary>
        private readonly HttpRequest request = HttpContext.Current.Request;

        /// <summary>The _portal settings.</summary>
        private PortalSettings curPortalSettings;

        /// <summary>Initializes a new instance of the <see cref="Options"/> class.</summary>
        public Options()
            : this(null, null, null)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="Options"/> class.</summary>
        /// <param name="portalController">The portal controller.</param>
        /// <param name="appStatus">The application status.</param>
        /// <param name="hostSettings">The host settings.</param>
        public Options(IPortalController portalController, IApplicationStatusInfo appStatus, IHostSettings hostSettings)
            : base(portalController, appStatus, hostSettings)
        {
            this.hostSettings = hostSettings ?? HttpContextSource.Current.GetScope().ServiceProvider.GetRequiredService<IHostSettings>();
        }

        /// <summary>  Gets Current Language from Url.</summary>
        protected string LangCode => this.request.QueryString["langCode"];

        /// <summary>  Gets the Name for the Current Resource file name.</summary>
        protected string ResXFile
        {
            get
            {
                string[] page = this.Request.ServerVariables["SCRIPT_NAME"].Split('/');

                string fileRoot = string.Format(
                    "{0}/{1}/{2}.resx",
                    this.TemplateSourceDirectory,
                    Localization.LocalResourceDirectory,
                    page[page.GetUpperBound(0)]);

                return fileRoot;
            }
        }

        /// <summary>Register the java scripts and CSS.</summary>
        /// <param name="e">
        /// The Event Args.
        /// </param>
        protected override void OnPreRender(EventArgs e)
        {
            JavaScript.RequestRegistration(CommonJs.jQuery);
            JavaScript.RequestRegistration(CommonJs.jQueryUI);
            ClientResourceManager.RegisterScript(this.Page, this.ResolveUrl("js/jquery.notification.js"));
            ClientResourceManager.RegisterScript(this.Page, this.ResolveUrl("js/Options.js"));
            ClientResourceManager.RegisterStyleSheet(this.Page, "https://ajax.googleapis.com/ajax/libs/jqueryui/1/themes/blitzer/jquery-ui.css");
            ClientResourceManager.RegisterStyleSheet(this.Page, this.ResolveUrl("css/jquery.notification.css"));
            ClientResourceManager.RegisterStyleSheet(this.Page, this.ResolveUrl("css/Options.css"));

            base.OnPreRender(e);
        }

        /// <summary>Raises the <see cref="E:Init" /> event.</summary>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected override void OnInit(EventArgs e)
        {
            // CODEGEN: This call is required by the ASP.NET Web Form Designer.
            this.InitializeComponent();
            base.OnInit(e);

            // Favicon
            this.LoadFavIcon();
        }

        /// <summary>Handles the Load event of the Page control.</summary>
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
                if (!int.TryParse(this.request.QueryString["mid"], NumberStyles.Integer, CultureInfo.InvariantCulture, out moduleId))
                {
                    moduleId = -1;
                }

                // Get TabId from Url
                int tabId;
                if (!int.TryParse(this.request.QueryString["tid"], NumberStyles.Integer, CultureInfo.InvariantCulture, out tabId))
                {
                    tabId = -1;
                }

                if (moduleId != -1 && tabId != -1)
                {
                    modInfo = db.GetModule(moduleId, tabId, false);
                }
                else
                {
                    this.ClosePage();
                }
            }
            catch (Exception exception)
            {
                Exceptions.ProcessPageLoadException(exception);

                this.ClosePage();
            }

            try
            {
                // Get ModuleID from Url
                var oEditorOptions = (CKEditorOptions)this.Page.LoadControl("CKEditorOptions.ascx");

                oEditorOptions.ID = "CKEditor_Options";
                oEditorOptions.ModuleConfiguration = modInfo;

                this.phControls.Controls.Add(oEditorOptions);
            }
            catch (Exception exception)
            {
                Exceptions.ProcessPageLoadException(exception);

                this.ClosePage();
            }
        }

        /// <summary>Closes the page.</summary>
        private void ClosePage()
        {
            this.Page.ClientScript.RegisterStartupScript(
                this.GetType(), "closeScript", "javascript:self.close();", true);
        }

        /// <summary>Gets the portal settings.</summary>
        /// <returns>
        /// The Portal Settings.
        /// </returns>
        private PortalSettings GetPortalSettings()
        {
            int iTabId = 0, iPortalId = 0;

            PortalSettings portalSettings;

            try
            {
                if (this.request.QueryString["tabid"] != null)
                {
                    iTabId = int.Parse(this.request.QueryString["tabid"]);
                }

                if (this.request.QueryString["PortalID"] != null)
                {
                    iPortalId = int.Parse(this.request.QueryString["PortalID"]);
                }

                string sDomainName = Globals.GetDomainName(this.Request, true);

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
            this.curPortalSettings = this.GetPortalSettings();

            this.ClientResourceLoader.DataBind();
            this.ClientResourceLoader.PreRender += (sender, args) => JavaScript.Register(this.Page);
        }

        /// <summary>Load Favicon from Current Portal Home Directory.</summary>
        private void LoadFavIcon()
        {
            this.favicon.Controls.Add(new LiteralControl(DotNetNuke.UI.Internals.FavIcon.GetHeaderLink(this.hostSettings, this.curPortalSettings.PortalId)));
        }
    }
}
