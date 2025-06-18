// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Framework.Controllers
{
    using System.Globalization;
    using System.Linq;
    using System.Web.Mvc;

    using Dnn.PersonaBar.Library.Containers;
    using Dnn.PersonaBar.Library.Controllers;
    using Dnn.PersonaBar.UI.Controllers;
    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Framework.JavaScriptLibraries;
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
        private readonly IHostSettings hostSettings;
        private readonly IPersonaBarContainer personaBarContainer;

        /// <summary>
        /// Initializes a new instance of the <see cref="PersonaBarContainerController"/> class.
        /// </summary>
        /// <param name="hostSettings">The host settings used to configure the Persona Bar container controller.</param>
        /// <param name="personaBarContainer">The personalbar container used to configure the Persona Bar container controller.</param>
        public PersonaBarContainerController(IHostSettings hostSettings, IPersonaBarContainer personaBarContainer)
        {
            this.personaBarContainer = personaBarContainer;
            this.hostSettings = hostSettings;
        }

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
        public string BuildNumber => this.hostSettings.CrmVersion.ToString(CultureInfo.InvariantCulture);

        /// <summary>
        /// Gets the current portal settings.
        /// </summary>
        protected PortalSettings PortalSettings => PortalSettings.Current;

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

        /// <summary>
        /// Determines whether the Persona Bar should be injected into the page.
        /// </summary>
        /// <returns><c>true</c> if the Persona Bar should be injected; otherwise, <c>false</c>.</returns>
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

        /// <summary>
        /// Registers the Persona Bar stylesheet.
        /// </summary>
        private void RegisterPersonaBarStyleSheet()
        {
            MvcClientResourceManager.RegisterStyleSheet(this.ControllerContext, "~/DesktopModules/admin/Dnn.PersonaBar/css/personaBarContainer.css");
        }
    }
}
