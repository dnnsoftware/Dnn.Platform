// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

// ReSharper disable once CheckNamespace
namespace Dnn.PersonaBar.UI.UserControls
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Linq;
    using System.Net;

    using Dnn.PersonaBar.Library.Containers;
    using Dnn.PersonaBar.Library.Controllers;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Abstractions.ClientResources;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Framework;
    using DotNetNuke.Framework.JavaScriptLibraries;
    using DotNetNuke.Services.ClientDependency;
    using DotNetNuke.UI.ControlPanels;
    using DotNetNuke.UI.Utilities;
    using DotNetNuke.Web.Client.ClientResourceManagement;

    using Microsoft.Extensions.DependencyInjection;

    using Newtonsoft.Json;

    using Globals = DotNetNuke.Common.Globals;

    /// <summary>The control containing the Persona Bar.</summary>
    public partial class PersonaBarContainer : ControlPanelBase
    {
        private readonly IPersonaBarContainer personaBarContainer;
        private readonly IPersonaBarController personaBarController;
        private readonly IHostSettings hostSettings;
        private readonly IJavaScriptLibraryHelper javaScript;
        private readonly IClientResourceController clientResourceController;

        /// <summary>Initializes a new instance of the <see cref="PersonaBarContainer"/> class.</summary>
        public PersonaBarContainer()
            : this(null, null, null, null, null)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="PersonaBarContainer"/> class.</summary>
        /// <param name="personaBarContainer">The Persona Bar container.</param>
        /// <param name="personaBarController">The Persona Bar controller.</param>
        /// <param name="hostSettings">The host settings.</param>
        /// <param name="javaScript">The JavaScript library helper.</param>
        /// <param name="clientResourceController">The client resources controller.</param>
        public PersonaBarContainer(IPersonaBarContainer personaBarContainer, IPersonaBarController personaBarController, IHostSettings hostSettings, IJavaScriptLibraryHelper javaScript, IClientResourceController clientResourceController)
        {
            this.personaBarContainer = personaBarContainer ?? Globals.GetCurrentServiceProvider().GetRequiredService<IPersonaBarContainer>();
            this.personaBarController = personaBarController ?? Globals.GetCurrentServiceProvider().GetRequiredService<IPersonaBarController>();
            this.hostSettings = hostSettings ?? Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettings>();
            this.javaScript = javaScript ?? Globals.GetCurrentServiceProvider().GetRequiredService<IJavaScriptLibraryHelper>();
            this.clientResourceController = clientResourceController ?? Globals.GetCurrentServiceProvider().GetRequiredService<IClientResourceController>();
        }

        /// <summary>Gets a JSON representation of <see cref="IPersonaBarContainer.GetConfiguration"/>.</summary>
        public string PersonaBarSettings => JsonConvert.SerializeObject(this.personaBarContainer.GetConfiguration());

        /// <summary>Gets the site's virtual application root path.</summary>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Breaking change")]
        public string AppPath => Globals.ApplicationPath;

        /// <summary>Gets the client resource version number.</summary>
        public string BuildNumber => this.hostSettings.CrmVersion.ToString(CultureInfo.InvariantCulture);

        /// <inheritdoc/>
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            if (this.personaBarContainer.Visible)
            {
                this.personaBarContainer.Initialize(this);
            }
        }

        /// <inheritdoc/>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            this.PersonaBarPanel.Visible = this.InjectPersonaBar();

            if (!this.PersonaBarPanel.Visible)
            {
                this.RemovedAdminStyleSheet();
            }
        }

        /// <inheritdoc/>
        protected override void OnPreRender(EventArgs e)
        {
            this.Visible = this.Response.StatusCode == (int)HttpStatusCode.OK;

            base.OnPreRender(e);
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

            var menuStructure = this.personaBarController.GetMenu(this.PortalSettings, UserController.Instance.GetCurrentUserInfo());
            if (menuStructure.MenuItems == null || !menuStructure.MenuItems.Any())
            {
                return false;
            }

            this.RegisterPersonaBarStyleSheet();

            JavaScript.RegisterClientReference(this.Page, ClientAPI.ClientNamespaceReferences.dnn);
            this.javaScript.RequestRegistration(CommonJs.DnnPlugins); // We need to add the Dnn JQuery plugins because the Edit Bar removes the Control Panel from the page
            this.javaScript.RequestRegistration(CommonJs.KnockoutMapping);

            ServicesFramework.Instance.RequestAjaxAntiForgerySupport();

            this.clientResourceController.RegisterScript("~/Resources/Shared/Components/Tokeninput/jquery.tokeninput.js");
            this.clientResourceController.RegisterStylesheet("~/Resources/Shared/Components/Tokeninput/Themes/token-input-facebook.css");

            return true;
        }

        private void RegisterPersonaBarStyleSheet()
        {
            this.clientResourceController.RegisterStylesheet("~/DesktopModules/admin/Dnn.PersonaBar/css/personaBarContainer.css");
        }

        private void RemovedAdminStyleSheet()
        {
            var loader = this.Page.FindControl("ClientResourceIncludes");
            if (loader != null)
            {
                for (var i = 0; i < loader.Controls.Count; i++)
                {
                    if (loader.Controls[i] is DnnCssInclude cssInclude)
                    {
                        if (cssInclude.FilePath == (Globals.HostPath + "admin.css"))
                        {
                            loader.Controls.Remove(cssInclude);
                            break;
                        }
                    }
                }
            }
        }
    }
}
