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
    using DotNetNuke.Abstractions.ClientResources;
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
#pragma warning disable CS0618 // Type or member is obsolete
        private readonly IPersonaBarContainer personaBarContainer = Dnn.PersonaBar.Library.Containers.PersonaBarContainer.Instance;
#pragma warning restore CS0618 // Type or member is obsolete
        private readonly IClientResourceController clientResourceController;
        private readonly IJavaScriptLibraryHelper javaScript;

        /// <summary>
        /// Initializes a new instance of the <see cref="PersonaBarContainerController"/> class.
        /// </summary>
        /// <param name="clientResourceController">The client resource controller used to manage client-side resources.</param>
        /// <param name="javaScript">JavaScript Library Helper.</param>
        public PersonaBarContainerController(IClientResourceController clientResourceController, IJavaScriptLibraryHelper javaScript)
        {
            this.clientResourceController = clientResourceController;
            this.javaScript = javaScript;
        }

        /// <summary>
        /// Gets the application path.
        /// </summary>
        public static string AppPath => Globals.ApplicationPath;

        /// <summary>
        /// Gets the build number of the application.
        /// </summary>
#pragma warning disable CS0618 // Type or member is obsolete
        public static string BuildNumber => Host.CrmVersion.ToString(CultureInfo.InvariantCulture);
#pragma warning restore CS0618 // Type or member is obsolete

        /// <summary>
        /// Gets the Persona Bar settings as a JSON string.
        /// </summary>
        public string PersonaBarSettings => JsonConvert.SerializeObject(this.personaBarContainer.GetConfiguration());

        /// <summary>
        /// Gets the current portal settings.
        /// </summary>
        protected static PortalSettings PortalSettings
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
#pragma warning disable CA3147 // Mark Verb Handlers With Validate Antiforgery Token
        public ActionResult Index()
#pragma warning restore CA3147 // Mark Verb Handlers With Validate Antiforgery Token
        {
            return this.View(new PersonaBarContainerModel()
            {
                PersonaBarSettings = this.PersonaBarSettings,
                AppPath = AppPath,
                BuildNumber = BuildNumber,
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

            var menuStructure = PersonaBarController.Instance.GetMenu(PortalSettings, UserController.Instance.GetCurrentUserInfo());
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
