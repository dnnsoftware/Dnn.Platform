#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2017
// by DotNetNuke Corporation
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation 
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and 
// to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions 
// of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
// TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.
#endregion

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

        public string PersonaBarSettings => JsonConvert.SerializeObject(_personaBarContainer.GetConfiguration());

        public string AppPath => Globals.ApplicationPath;

        public string BuildNumber => Host.CrmVersion.ToString(CultureInfo.InvariantCulture);

        #endregion

        #region Event Handlers

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            if (_personaBarContainer.Visible)
            {
                _personaBarContainer.Initialize(this);
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            PersonaBarPanel.Visible = InjectPersonaBar();

            if (!PersonaBarPanel.Visible)
            {
                RemovedAdminStyleSheet();
            }
        }

        protected override void OnPreRender(EventArgs e)
        {
            Visible = Response.StatusCode == (int)HttpStatusCode.OK;

            base.OnPreRender(e);
        }

        #endregion

        #region Private Methods

        private bool InjectPersonaBar()
        {
            if (!_personaBarContainer.Visible)
            {
                return false;
            }

            //copied this logic from DotNetNuke.UI.Skins.Skin.InjectControlPanel
            if (Request.QueryString["dnnprintmode"] == "true" || Request.QueryString["popUp"] == "true")
                return false;

            var menuStructure = PersonaBarController.Instance.GetMenu(PortalSettings, UserController.Instance.GetCurrentUserInfo());
            if (menuStructure.MenuItems == null || !menuStructure.MenuItems.Any())
            {
                return false;
            }

            RegisterPersonaBarStyleSheet();

            JavaScript.RegisterClientReference(Page, ClientAPI.ClientNamespaceReferences.dnn);
            JavaScript.RequestRegistration(CommonJs.DnnPlugins); //We need to add the Dnn JQuery plugins because the Edit Bar removes the Control Panel from the page
            JavaScript.RequestRegistration(CommonJs.KnockoutMapping);

            ServicesFramework.Instance.RequestAjaxAntiForgerySupport();

            ClientResourceManager.RegisterScript(Page, "~/Resources/Shared/Components/Tokeninput/jquery.tokeninput.js");
            ClientResourceManager.RegisterStyleSheet(Page, "~/Resources/Shared/Components/Tokeninput/Themes/token-input-facebook.css");

            return true;
        }

        private void RegisterPersonaBarStyleSheet()
        {
            ClientResourceManager.RegisterStyleSheet(Page, "~/DesktopModules/admin/Dnn.PersonaBar/css/personaBarContainer.css");
        }

        private void RemovedAdminStyleSheet()
        {
            var loader = Page.FindControl("ClientResourceIncludes");
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
