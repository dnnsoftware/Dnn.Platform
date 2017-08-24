using System;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Entities.Modules.Actions;
using DotNetNuke.Services.Localization;
using DotNetNuke.Security;
using DotNetNuke.Web.Client.ClientResourceManagement;

namespace Cantarus.Modules.PolyDeploy
{

    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The View class displays the content
    /// </summary>
    /// -----------------------------------------------------------------------------
    public partial class View : PolyDeployModuleBase //, IActionable
    {

        #region Event Handlers

        override protected void OnInit(EventArgs e)
        {

#if RELEASE
            // Minified resources for Release.
            ClientResourceManager.RegisterStyleSheet(Page, string.Format("{0}/css/poly-deploy.min.css", TemplateSourceDirectory));

            ClientResourceManager.RegisterScript(Page, string.Format("{0}/Angular/dist/poly-deploy.bundle.min.js", TemplateSourceDirectory), 3);
            ClientResourceManager.RegisterScript(Page, string.Format("{0}/Angular/dist/poly-deploy.min.js", TemplateSourceDirectory), 4);
#else
            // Non-minified resources for everything else.
            ClientResourceManager.RegisterStyleSheet(Page, string.Format("{0}/css/poly-deploy.css", TemplateSourceDirectory));

            ClientResourceManager.RegisterScript(Page, string.Format("{0}/Angular/dist/poly-deploy.bundle.js", TemplateSourceDirectory), 500);
            ClientResourceManager.RegisterScript(Page, string.Format("{0}/Angular/dist/poly-deploy.js", TemplateSourceDirectory), 501);
#endif

            InitializeComponent();
            base.OnInit(e);
        }

        private void InitializeComponent()
        {
            this.Load += new System.EventHandler(this.Page_Load);
        }


        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Page_Load runs when the control is loaded
        /// </summary>
        /// -----------------------------------------------------------------------------
        private void Page_Load(object sender, System.EventArgs e)
        {
            try
            {

            }
            catch (Exception exc) //Module failed to load
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        #endregion

        #region Optional Interfaces

        public ModuleActionCollection ModuleActions
        {
            get
            {
                ModuleActionCollection Actions = new ModuleActionCollection();
                Actions.Add(GetNextActionID(), Localization.GetString("EditModule", this.LocalResourceFile), "", "", "", EditUrl(), false, SecurityAccessLevel.Edit, true, false);
                return Actions;
            }
        }

        #endregion

    }

}
