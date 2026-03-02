// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.Api.Auth.ApiTokens
{
    using System;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Common;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Modules.Settings;
    using DotNetNuke.Entities.Portals;

    using Microsoft.Extensions.DependencyInjection;

    /// <summary>A repository for retrieving API Token settings.</summary>
    /// <param name="moduleController">The module controller.</param>
    /// <param name="hostSettings">The host settings.</param>
    /// <param name="hostSettingsService">The host settings service.</param>
    /// <param name="portalController">The portal controller.</param>
    public class ApiTokenSettingsRepository(IModuleController moduleController, IHostSettings hostSettings, IHostSettingsService hostSettingsService, IPortalController portalController)
        : SettingsRepository<ApiTokenSettings>(
            moduleController ?? Globals.GetCurrentServiceProvider().GetRequiredService<IModuleController>(),
            hostSettings ?? Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettings>(),
            hostSettingsService ?? Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettingsService>(),
            portalController ?? Globals.GetCurrentServiceProvider().GetRequiredService<IPortalController>())
    {
        /// <summary>Initializes a new instance of the <see cref="ApiTokenSettingsRepository"/> class.</summary>
        [Obsolete("Deprecated in DotNetNuke 10.2.4. Please use overload with IHostSettings. Scheduled removal in v12.0.0.")]
        public ApiTokenSettingsRepository()
            : this(null, null, null, null)
        {
        }
    }
}
