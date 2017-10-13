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
    public partial class Manage : PolyDeployModuleBase //, IActionable
    {
        protected override void OnInit(EventArgs e)
        {
            ClientResourceManager.RegisterStyleSheet(Page, string.Format("{0}/../../css/poly-deploy.css", TemplateSourceDirectory));

            ClientResourceManager.RegisterScript(Page, string.Format("{0}/dist/Manage.bundle.js", TemplateSourceDirectory), 500);

            base.OnInit(e);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            // Register for Services Framework.
            ServicesFramework.Instance.RequestAjaxAntiForgerySupport();
        }
    }
}
