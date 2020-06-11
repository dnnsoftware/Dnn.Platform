﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

using System;
using System.Globalization;
using System.Linq;
using System.Net;
using Dnn.PersonaBar.Library.Containers;
using Dnn.PersonaBar.Library.Controllers;
using DotNetNuke.Entities.Host;
using DotNetNuke.Entities.Users;
using DotNetNuke.Framework;
using DotNetNuke.Framework.JavaScriptLibraries;
using DotNetNuke.UI.ControlPanels;
using DotNetNuke.UI.Utilities;
using DotNetNuke.Web.Client.ClientResourceManagement;
using Newtonsoft.Json;
using Globals = DotNetNuke.Common.Globals;

// ReSharper disable once CheckNamespace
namespace Dnn.PersonaBar.UI.UserControls
{
    public partial class PersonaBarContainer : ControlPanelBase
    {
        #region Fields

        private readonly IPersonaBarContainer _personaBarContainer = Library.Containers.PersonaBarContainer.Instance;

        #endregion

        #region Properties

        public string PersonaBarSettings => JsonConvert.SerializeObject(this._personaBarContainer.GetConfiguration());

        public string AppPath => Globals.ApplicationPath;

        public string BuildNumber => Host.CrmVersion.ToString(CultureInfo.InvariantCulture);

        #endregion

        #region Event Handlers

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            if (this._personaBarContainer.Visible)
            {
                this._personaBarContainer.Initialize(this);
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            this.PersonaBarPanel.Visible = this.InjectPersonaBar();

            if (!this.PersonaBarPanel.Visible)
            {
                this.RemovedAdminStyleSheet();
            }
        }

        protected override void OnPreRender(EventArgs e)
        {
            this.Visible = this.Response.StatusCode == (int)HttpStatusCode.OK;

            base.OnPreRender(e);
        }

        #endregion

        #region Private Methods

        private bool InjectPersonaBar()
        {
            if (!this._personaBarContainer.Visible)
            {
                return false;
            }

            //copied this logic from DotNetNuke.UI.Skins.Skin.InjectControlPanel
            if (this.Request.QueryString["dnnprintmode"] == "true" || this.Request.QueryString["popUp"] == "true")
                return false;

            var menuStructure = PersonaBarController.Instance.GetMenu(this.PortalSettings, UserController.Instance.GetCurrentUserInfo());
            if (menuStructure.MenuItems == null || !menuStructure.MenuItems.Any())
            {
                return false;
            }

            this.RegisterPersonaBarStyleSheet();

            JavaScript.RegisterClientReference(this.Page, ClientAPI.ClientNamespaceReferences.dnn);
            JavaScript.RequestRegistration(CommonJs.DnnPlugins); //We need to add the Dnn JQuery plugins because the Edit Bar removes the Control Panel from the page
            JavaScript.RequestRegistration(CommonJs.KnockoutMapping);

            ServicesFramework.Instance.RequestAjaxAntiForgerySupport();

            ClientResourceManager.RegisterScript(this.Page, "~/Resources/Shared/Components/Tokeninput/jquery.tokeninput.js");
            ClientResourceManager.RegisterStyleSheet(this.Page, "~/Resources/Shared/Components/Tokeninput/Themes/token-input-facebook.css");

            return true;
        }

        private void RegisterPersonaBarStyleSheet()
        {
            ClientResourceManager.RegisterStyleSheet(this.Page, "~/DesktopModules/admin/Dnn.PersonaBar/css/personaBarContainer.css");
        }

        private void RemovedAdminStyleSheet()
        {
            var loader = this.Page.FindControl("ClientResourceIncludes");
            if (loader != null)
            {
                for (var i = 0; i < loader.Controls.Count; i++)
                {
                    var cssInclude = loader.Controls[i] as DnnCssInclude;
                    if (cssInclude != null)
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

        #endregion
    }
}
