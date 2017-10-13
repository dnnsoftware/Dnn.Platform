using System;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Entities.Modules.Actions;
using DotNetNuke.Services.Localization;
using DotNetNuke.Security;
using DotNetNuke.Web.Client.ClientResourceManagement;
using DotNetNuke.Framework;
using Cantarus.Modules.PolyDeploy.Components;
using DotNetNuke.Entities.Modules;

namespace Cantarus.Modules.PolyDeploy
{
    public partial class Deploy : PolyDeployModuleBase, IActionable
    {

        #region Event Handlers

        protected override void OnInit(EventArgs e)
        {
            ClientResourceManager.RegisterStyleSheet(Page, string.Format("{0}/../../css/poly-deploy.css", TemplateSourceDirectory));

            ClientResourceManager.RegisterScript(Page, string.Format("{0}/dist/Deploy.bundle.js", TemplateSourceDirectory), 500);

            InitializeComponent();
            base.OnInit(e);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            // Register for Services Framework.
            ServicesFramework.Instance.RequestAjaxAntiForgerySupport();
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
                Actions.Add(GetNextActionID(), Localization.GetString("Manage", this.LocalResourceFile), "", "", "", EditUrl("Manage"), false, SecurityAccessLevel.Edit, true, false);
                return Actions;
            }
        }

        #endregion

    }

}
