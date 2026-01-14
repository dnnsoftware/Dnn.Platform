// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.EditBar.UI.HttpModules
{
    using System.Diagnostics.CodeAnalysis;
    using System.Web;

    using Dnn.EditBar.UI.Controllers;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Application;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Extensions;
    using DotNetNuke.Entities.Controllers;
    using DotNetNuke.Entities.Host;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.UI.Skins.EventListeners;

    using Microsoft.Extensions.DependencyInjection;

    public class EditBarModule : IHttpModule
    {
        private static readonly object LockAppStarted = new object();
        private static bool hasAppStarted;

        private readonly IHostSettings hostSettings;

        /// <summary>Initializes a new instance of the <see cref="EditBarModule"/> class.</summary>
        public EditBarModule()
            : this(null)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="EditBarModule"/> class.</summary>
        /// <param name="hostSettings">The host settings.</param>
        public EditBarModule(IHostSettings hostSettings)
        {
            if (hostSettings is not null)
            {
                this.hostSettings = hostSettings;
            }
            else
            {
                var scope = HttpContextSource.Current?.GetScope();
                if (scope is not null)
                {
                    this.hostSettings = scope.ServiceProvider.GetRequiredService<IHostSettings>();
                }
                else
                {
                    this.hostSettings = new HostSettings(new HostController());
                }
            }
        }

        [SuppressMessage("Microsoft.Naming", "CA1725:ParameterNamesShouldMatchBaseDeclaration", Justification = "Breaking change")]
        public void Init(HttpApplication application)
        {
            if (hasAppStarted)
            {
                return;
            }

            lock (LockAppStarted)
            {
                if (hasAppStarted)
                {
                    return;
                }

                this.ApplicationStart();
                hasAppStarted = true;
            }
        }

        public void Dispose()
        {
        }

        private void ApplicationStart()
        {
            DotNetNukeContext.Current.SkinEventListeners.Add(new SkinEventListener(SkinEventType.OnSkinInit, this.OnSkinInit));
        }

        private void OnSkinInit(object sender, SkinEventArgs e)
        {
            if (this.hostSettings.DisableEditBar)
            {
                return;
            }

            var request = e.Skin.Page.Request;
            var isSpecialPageMode = request.QueryString["dnnprintmode"] == "true" || request.QueryString["popUp"] == "true";
            if (isSpecialPageMode || Globals.IsAdminControl())
            {
                return;
            }

            if (ContentEditorManager.GetCurrent(e.Skin.Page) == null && !Globals.IsAdminControl())
            {
                if (PortalSettings.Current.UserId > 0)
                {
                    e.Skin.Page.Form.Controls.Add(new ContentEditorManager { Skin = e.Skin });
                }
            }
        }
    }
}
