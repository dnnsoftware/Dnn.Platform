using DotNetNuke.BulkInstall.Components;
using DotNetNuke.Framework;
using DotNetNuke.Web.Client.ClientResourceManagement;
using System;

namespace DotNetNuke.BulkInstall
{
    using DotNetNuke.BulkInstall.Components;

    public partial class Manage : PolyDeployModuleBase
    {
        protected override void OnInit(EventArgs e)
        {
            ClientResourceManager.RegisterStyleSheet(Page, string.Format("{0}/dist/Manage.styles.css", TemplateSourceDirectory));

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
