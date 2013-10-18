using System;
using System.Web.UI;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Framework;
using DotNetNuke.Web.Client.ClientResourceManagement;

namespace DotNetNuke.Web.UI.WebControls
{
    public class DnnGettingStarted : Control, INamingContainer
    {

        private readonly Lazy<DnnGettingStartedOptions> _options =
            new Lazy<DnnGettingStartedOptions>(() => new DnnGettingStartedOptions());

        private DnnGettingStartedOptions Options
        {
            get
            {
                return _options.Value;
            }
        }

        public bool ShowOnStarup
        {
            set { Options.ShowOnStartup = value; }
        }

        public string ContentUrl
        {
            set { Options.ContentUrl = value; }
        }

        public string UserManualLinkUrl
        {
            set { Options.UserManualLinkUrl = value; }
        }

        public string Skin
        {
            get;
            set;
        }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            RegisterClientScript(Skin);
            RegisterStartupScript();
        }

        private void RegisterClientScript(string skin)
        {
            ServicesFramework.Instance.RequestAjaxScriptSupport();
            ServicesFramework.Instance.RequestAjaxAntiForgerySupport();
            jQuery.RequestDnnPluginsRegistration();
            jQuery.RegisterFileUpload(Page);

            ClientResourceManager.RegisterStyleSheet(Page, "~/Resources/Shared/Components/GettingStarted/dnn.GettingStarted.css");
            if (!string.IsNullOrEmpty(skin))
            {
                ClientResourceManager.RegisterStyleSheet(Page, "~/Resources/Shared/Components/GettingStarted/dnn.GettingStarted." + skin + ".css");
            }
            ClientResourceManager.RegisterScript(Page, "~/Resources/Shared/scripts/dnn.extensions.js");
            ClientResourceManager.RegisterScript(Page, "~/Resources/Shared/scripts/dnn.jquery.extensions.js");
            ClientResourceManager.RegisterScript(Page, "~/Resources/Shared/Components/GettingStarted/dnn.GettingStarted.js");
        }

        private void RegisterStartupScript()
        {
            var optionsAsJsonString = Json.Serialize(Options);

            var script = string.Format("dnn.showGettingStarted({0});{1}", optionsAsJsonString, Environment.NewLine);

            if (ScriptManager.GetCurrent(Page) != null)
            {
                // respect MS AJAX
                ScriptManager.RegisterStartupScript(Page, GetType(), ClientID + "DnnGettingStarted", script, true);
            }
            else
            {
                Page.ClientScript.RegisterStartupScript(GetType(), ClientID + "DnnGettingStarted", script, true);
            }

        }
    }
}
