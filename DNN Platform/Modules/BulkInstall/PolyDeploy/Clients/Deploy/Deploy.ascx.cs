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
        protected override void OnInit(EventArgs e)
        {
            ClientResourceManager.RegisterStyleSheet(Page, string.Format("{0}/dist/Deploy.styles.css", TemplateSourceDirectory));

            ClientResourceManager.RegisterScript(Page, string.Format("{0}/dist/Deploy.bundle.js", TemplateSourceDirectory), 500);
            base.OnInit(e);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            // Register for Services Framework.
            ServicesFramework.Instance.RequestAjaxAntiForgerySupport();
        }

        public ModuleActionCollection ModuleActions
        {
            get
            {
                ModuleActionCollection Actions = new ModuleActionCollection();

                Actions.Add(GetNextActionID(), Localization.GetString("Manage", this.LocalResourceFile), "", "", "", EditUrl("Manage"), false, SecurityAccessLevel.Edit, true, false);

                return Actions;
            }
        }
    }
}
