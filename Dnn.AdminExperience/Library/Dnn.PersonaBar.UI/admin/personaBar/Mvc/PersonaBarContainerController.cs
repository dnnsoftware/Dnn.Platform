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
    using DotNetNuke.Abstractions.ClientResources;
    using DotNetNuke.Abstractions.Logging;
    using DotNetNuke.Abstractions.Security.Permissions;
    using DotNetNuke.ContentSecurityPolicy;
    using DotNetNuke.Entities.Host;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Framework.JavaScriptLibraries;
    using DotNetNuke.Services.ClientDependency;
    using DotNetNuke.UI.Utilities;
    using DotNetNuke.Web.MvcPipeline.Framework.JavascriptLibraries;
    using Newtonsoft.Json;

    using Globals = DotNetNuke.Common.Globals;

    /// <summary>
    /// Controller for managing the Persona Bar container in the DNN platform.
    /// </summary>
    public class PersonaBarContainerController : Controller
    {
        private readonly IPersonaBarContainer personaBarContainer;
        private readonly IHostSettings hostSettings;
        private readonly IJavaScriptLibraryHelper javaScript;
        private readonly IClientResourceController clientResourceController;
        private readonly IContentSecurityPolicy contentSecurityPolicy;

        /// <summary>
        /// Initializes a new instance of the <see cref="PersonaBarContainerController"/> class.
        /// </summary>
        /// <param name="personaBarContainer">The Persona Bar container.</param>
        /// <param name="hostSettings">The host settings.</param>
        /// <param name="javaScript">The JavaScript library helper.</param>
        /// <param name="clientResourceController">The client resources controller.</param>
        /// <param name="contentSecurityPolicy">The content security policy.</param>
        public PersonaBarContainerController(IPersonaBarContainer personaBarContainer, IHostSettings hostSettings, IJavaScriptLibraryHelper javaScript, IClientResourceController clientResourceController, IContentSecurityPolicy contentSecurityPolicy)
        {
            this.personaBarContainer = personaBarContainer;
            this.hostSettings = hostSettings;
            this.javaScript = javaScript;
            this.clientResourceController = clientResourceController;
            this.contentSecurityPolicy = contentSecurityPolicy;
        }

        /// <summary>
        /// Gets the application path.
        /// </summary>
        public static string AppPath => Globals.ApplicationPath;

        /// <summary>
        /// Gets the build number of the application.
        /// </summary>
        public string BuildNumber => this.hostSettings.CrmVersion.ToString(CultureInfo.InvariantCulture);

        /// <summary>
        /// Gets the Persona Bar settings as a JSON string.
        /// </summary>
        public string PersonaBarSettings => JsonConvert.SerializeObject(this.personaBarContainer.GetConfiguration());

        /// <summary>
        /// Returns the default view for the Persona Bar container.
        /// </summary>
        /// <returns>An <see cref="ActionResult"/> representing the view.</returns>
        [HttpGet]
        public ActionResult Index()
        {
            return this.View(new PersonaBarContainerModel()
            {
                PersonaBarSettings = this.PersonaBarSettings,
                AppPath = AppPath,
                BuildNumber = this.BuildNumber,
                Visible = this.InjectPersonaBar(),
                Nonce = this.contentSecurityPolicy.Nonce,
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

            var menuStructure = PersonaBarController.Instance.GetMenu(PortalSettings.Current, UserController.Instance.GetCurrentUserInfo());
            if (menuStructure.MenuItems == null || !menuStructure.MenuItems.Any())
            {
                return false;
            }

            this.RegisterPersonaBarStyleSheet();

            MvcJavaScript.RegisterClientReference(ClientAPI.ClientNamespaceReferences.dnn);
            this.javaScript.RequestRegistration(CommonJs.DnnPlugins); // We need to add the Dnn JQuery plugins because the Edit Bar removes the Control Panel from the page
            this.javaScript.RequestRegistration(CommonJs.KnockoutMapping);

            // ServicesFramework.Instance.RequestAjaxAntiForgerySupport(); // to later add this line
            this.clientResourceController.RegisterScript("~/Resources/Shared/Components/Tokeninput/jquery.tokeninput.js");
            this.clientResourceController.RegisterStylesheet("~/Resources/Shared/Components/Tokeninput/Themes/token-input-facebook.css");

            return true;
        }

        private void RegisterPersonaBarStyleSheet()
        {
            this.clientResourceController.RegisterStylesheet("~/DesktopModules/admin/Dnn.PersonaBar/css/personaBarContainer.css");
        }
    }
}
