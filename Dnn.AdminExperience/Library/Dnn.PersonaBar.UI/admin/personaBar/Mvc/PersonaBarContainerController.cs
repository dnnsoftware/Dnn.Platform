// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Framework.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Net.NetworkInformation;
    using System.Security.Policy;
    using System.Web;
    using System.Web.Mvc;

    using Dnn.PersonaBar.Library.Containers;
    using Dnn.PersonaBar.Library.Controllers;
    using Dnn.PersonaBar.UI.Controllers;
    using DotNetNuke.Entities.Host;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Framework;
    using DotNetNuke.Framework.JavaScriptLibraries;
    using DotNetNuke.UI.ControlPanels;
    using DotNetNuke.UI.Utilities;
    using DotNetNuke.Web.Client.ClientResourceManagement;
    using DotNetNuke.Web.MvcPipeline.Framework.JavascriptLibraries;
    using Newtonsoft.Json;

    using Globals = DotNetNuke.Common.Globals;

    /// <summary>
    /// Controller for managing the Persona Bar container in the DNN platform.
    /// </summary>
    public class PersonaBarContainerController : Controller
    {
#pragma warning disable CS0618 // Type or member is obsolete
        private readonly IPersonaBarContainer personaBarContainer = Dnn.PersonaBar.Library.Containers.PersonaBarContainer.Instance;
#pragma warning restore CS0618 // Type or member is obsolete

        /// <summary>
        /// Gets the Persona Bar settings as a JSON string.
        /// </summary>
        public string PersonaBarSettings => JsonConvert.SerializeObject(this.personaBarContainer.GetConfiguration());

        /// <summary>
        /// Gets the application path.
        /// </summary>
        public string AppPath => Globals.ApplicationPath;

        /// <summary>
        /// Gets the build number of the application.
        /// </summary>
#pragma warning disable CS0618 // Type or member is obsolete
        public string BuildNumber => Host.CrmVersion.ToString(CultureInfo.InvariantCulture);
#pragma warning restore CS0618 // Type or member is obsolete

        /// <summary>
        /// Gets the current portal settings.
        /// </summary>
        protected PortalSettings PortalSettings
        {
            get
            {
#pragma warning disable CS0618 // Type or member is obsolete
                return PortalController.Instance.GetCurrentPortalSettings();
#pragma warning restore CS0618 // Type or member is obsolete
            }
        }

        /// <summary>
        /// Returns the default view for the Persona Bar container.
        /// </summary>
        /// <returns>An <see cref="ActionResult"/> representing the view.</returns>
        public ActionResult Index()
        {
            return this.View(new PersonaBarContainerModel()
            {
                PersonaBarSettings = this.PersonaBarSettings,
                AppPath = this.AppPath,
                BuildNumber = this.BuildNumber,
                Visible = this.InjectPersonaBar(),
            });
        }

        private bool InjectPersonaBar()
        {
            if (!this.personaBarContainer.Visible)
            {
                return false;
            }

            // copied this logic from DotNetNuke.UI.Skins.Skin.InjectControlPanel
            if (this.Request.QueryString["dnnprintmode"] == "true" || this.Request.QueryString["popUp"] == "true")
            {
                return false;
            }

            var menuStructure = PersonaBarController.Instance.GetMenu(this.PortalSettings, UserController.Instance.GetCurrentUserInfo());
            if (menuStructure.MenuItems == null || !menuStructure.MenuItems.Any())
            {
                return false;
            }

            this.RegisterPersonaBarStyleSheet();

            MvcJavaScript.RegisterClientReference(this.ControllerContext, ClientAPI.ClientNamespaceReferences.dnn);
            MvcJavaScript.RequestRegistration(CommonJs.DnnPlugins); // We need to add the Dnn JQuery plugins because the Edit Bar removes the Control Panel from the page
            MvcJavaScript.RequestRegistration(CommonJs.KnockoutMapping);

            // ServicesFramework.Instance.RequestAjaxAntiForgerySupport(); // to later add this line
            MvcClientResourceManager.RegisterScript(this.ControllerContext, "~/Resources/Shared/Components/Tokeninput/jquery.tokeninput.js");
            MvcClientResourceManager.RegisterStyleSheet(this.ControllerContext, "~/Resources/Shared/Components/Tokeninput/Themes/token-input-facebook.css");

            return true;
        }

        private void RegisterPersonaBarStyleSheet()
        {
            MvcClientResourceManager.RegisterStyleSheet(this.ControllerContext, "~/DesktopModules/admin/Dnn.PersonaBar/css/personaBarContainer.css");
        }
    }
}
