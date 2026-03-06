// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Modules.Html.Components
{
    using System;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Common;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Modules.Settings;
    using DotNetNuke.Entities.Portals;

    using Microsoft.Extensions.DependencyInjection;

    /// <summary>The <see cref="SettingsRepository{T}"/> used for storing and retrieving <see cref="HtmlModuleSettings"/>.</summary>
    /// <param name="moduleController">The module controller.</param>
    /// <param name="hostSettings">The host settings.</param>
    /// <param name="hostSettingsService">The host settings service.</param>
    /// <param name="portalController">The portal controller.</param>
    public class HtmlModuleSettingsRepository(IModuleController moduleController, IHostSettings hostSettings, IHostSettingsService hostSettingsService, IPortalController portalController)
        : SettingsRepository<HtmlModuleSettings>(
            moduleController ?? Globals.GetCurrentServiceProvider().GetRequiredService<IModuleController>(),
            hostSettings ?? Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettings>(),
            hostSettingsService ?? Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettingsService>(),
            portalController ?? Globals.GetCurrentServiceProvider().GetRequiredService<IPortalController>())
    {
        /// <summary>Initializes a new instance of the <see cref="HtmlModuleSettingsRepository"/> class.</summary>
        [Obsolete("Deprecated in DotNetNuke 10.2.4. Please use overload with IHostSettings. Scheduled removal in v12.0.0.")]
        public HtmlModuleSettingsRepository()
            : this(null, null, null, null)
        {
        }
    }
}
