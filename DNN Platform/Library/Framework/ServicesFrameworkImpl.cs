// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Framework
{
    using System;
    using System.Globalization;
    using System.Web.Helpers;
    using System.Web.UI;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Abstractions.ClientResources;
    using DotNetNuke.Abstractions.Logging;
    using DotNetNuke.Common;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Framework.JavaScriptLibraries;
    using DotNetNuke.Services.ClientDependency;
    using DotNetNuke.UI.Utilities;

    using Microsoft.Extensions.DependencyInjection;

    using Globals = DotNetNuke.Common.Globals;

    /// <summary>The default <see cref="IServicesFramework"/> implementation.</summary>
    internal class ServicesFrameworkImpl : IServicesFramework, IServiceFrameworkInternals
    {
        private const string AntiForgeryKey = "dnnAntiForgeryRequested";
        private const string ScriptKey = "dnnSFAjaxScriptRequested";
        private readonly IApplicationStatusInfo appStatus;
        private readonly IEventLogger eventLogger;

        /// <summary>Initializes a new instance of the <see cref="ServicesFrameworkImpl"/> class.</summary>
        [Obsolete("Deprecated in DotNetNuke 10.2.2. Please use overload with IApplicationStatusInfo and IEventLogger. Scheduled removal in v12.0.0.")]
        public ServicesFrameworkImpl()
            : this(null, null)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="ServicesFrameworkImpl"/> class.</summary>
        /// <param name="appStatus">The application status.</param>
        /// <param name="eventLogger">The event logger.</param>
        public ServicesFrameworkImpl(IApplicationStatusInfo appStatus, IEventLogger eventLogger)
        {
            var servicesProvider = Globals.GetCurrentServiceProvider();
            this.appStatus = appStatus ?? servicesProvider.GetRequiredService<IApplicationStatusInfo>();
            this.eventLogger = eventLogger ?? servicesProvider.GetRequiredService<IEventLogger>();
        }

        /// <inheritdoc />
        public bool IsAjaxAntiForgerySupportRequired
        {
            get { return CheckKey(AntiForgeryKey); }
        }

        /// <inheritdoc />
        public bool IsAjaxScriptSupportRequired
        {
            get { return CheckKey(ScriptKey); }
        }

        /// <inheritdoc />
        public void RequestAjaxAntiForgerySupport()
        {
            this.RequestAjaxScriptSupport();
            SetKey(AntiForgeryKey);
        }

        /// <inheritdoc />
        public void RegisterAjaxAntiForgery(Page page)
        {
            var ctl = page.FindControl("ClientResourcesFormBottom");
            ctl?.Controls.Add(new LiteralControl(AntiForgery.GetHtml().ToHtmlString()));
        }

        /// <inheritdoc />
        public void RequestAjaxScriptSupport()
        {
            JavaScript.RequestRegistration(this.appStatus, this.eventLogger, PortalSettings.Current, CommonJs.jQuery);
            SetKey(ScriptKey);
        }

        /// <inheritdoc />
        public void RegisterAjaxScript(Page page)
        {
            var path = ServicesFramework.GetServiceFrameworkRoot();
            if (string.IsNullOrEmpty(path))
            {
                return;
            }

            JavaScript.RegisterClientReference(page, ClientAPI.ClientNamespaceReferences.dnn);
            ClientAPI.RegisterClientVariable(page, "sf_siteRoot", path, /*overwrite*/ true);
            ClientAPI.RegisterClientVariable(page, "sf_tabId", PortalSettings.Current.ActiveTab.TabID.ToString(CultureInfo.InvariantCulture), /*overwrite*/ true);

            string scriptPath;
            if (HttpContextSource.Current.IsDebuggingEnabled)
            {
                scriptPath = "~/js/Debug/dnn.servicesframework.js";
            }
            else
            {
                scriptPath = "~/js/dnn.servicesframework.js";
            }

            GetClientResourcesController().RegisterScript(scriptPath);
        }

        private static void SetKey(string key)
        {
            HttpContextSource.Current.Items[key] = true;
        }

        private static bool CheckKey(string antiForgeryKey)
        {
            return HttpContextSource.Current.Items.Contains(antiForgeryKey);
        }

        private static IClientResourceController GetClientResourcesController()
        {
            var serviceProvider = Globals.GetCurrentServiceProvider();
            return serviceProvider.GetRequiredService<IClientResourceController>();
        }
    }
}
