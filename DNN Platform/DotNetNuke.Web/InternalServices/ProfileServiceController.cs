// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Web.InternalServices;

using System;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;

using DotNetNuke.Abstractions.Application;
using DotNetNuke.Abstractions.Logging;
using DotNetNuke.Common;
using DotNetNuke.Common.Lists;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Urls;
using DotNetNuke.Entities.Users;
using DotNetNuke.Services.Localization;
using DotNetNuke.Services.Registration;
using DotNetNuke.Web.Api;

using Microsoft.Extensions.DependencyInjection;

/// <summary>A web API controller for a user's profile.</summary>
/// <param name="portalController">The portal controller.</param>
/// <param name="appStatus">The application status.</param>
/// <param name="portalGroupController">The portal group controller.</param>
/// <param name="hostSettings">The host settings.</param>
/// <param name="hostSettingsService">The host settings service.</param>
/// <param name="eventLogger">The event logger.</param>
/// <param name="listController">The list controller.</param>
[DnnAuthorize]
public class ProfileServiceController(IPortalController portalController, IApplicationStatusInfo appStatus, IPortalGroupController portalGroupController, IHostSettings hostSettings, IHostSettingsService hostSettingsService, IEventLogger eventLogger, ListController listController)
    : DnnApiController
{
    private readonly IPortalController portalController = portalController ?? Globals.GetCurrentServiceProvider().GetRequiredService<IPortalController>();
    private readonly IApplicationStatusInfo appStatus = appStatus ?? Globals.GetCurrentServiceProvider().GetRequiredService<IApplicationStatusInfo>();
    private readonly IPortalGroupController portalGroupController = portalGroupController ?? Globals.GetCurrentServiceProvider().GetRequiredService<IPortalGroupController>();
    private readonly IHostSettings hostSettings = hostSettings ?? Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettings>();
    private readonly IHostSettingsService hostSettingsService = hostSettingsService ?? Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettingsService>();
    private readonly IEventLogger eventLogger = eventLogger ?? Globals.GetCurrentServiceProvider().GetRequiredService<IEventLogger>();
    private readonly ListController listController = listController ?? Globals.GetCurrentServiceProvider().GetRequiredService<ListController>();

    /// <summary>Initializes a new instance of the <see cref="ProfileServiceController"/> class.</summary>
    [Obsolete("Deprecated in DotNetNuke 10.2.2. Please use overload with IPortalController. Scheduled removal in v12.0.0.")]
    public ProfileServiceController()
        : this(null, null, null, null, null, null)
    {
    }

    /// <summary>Initializes a new instance of the <see cref="ProfileServiceController"/> class.</summary>
    /// <param name="portalController">The portal controller.</param>
    /// <param name="appStatus">The application status.</param>
    /// <param name="portalGroupController">The portal group controller.</param>
    [Obsolete("Deprecated in DotNetNuke 10.2.2. Please use overload with IHostSettings. Scheduled removal in v12.0.0.")]
    public ProfileServiceController(IPortalController portalController, IApplicationStatusInfo appStatus, IPortalGroupController portalGroupController)
        : this(portalController, appStatus, portalGroupController, null, null, null)
    {
    }

    /// <summary>Initializes a new instance of the <see cref="ProfileServiceController"/> class.</summary>
    /// <param name="portalController">The portal controller.</param>
    /// <param name="appStatus">The application status.</param>
    /// <param name="portalGroupController">The portal group controller.</param>
    /// <param name="hostSettings">The host settings.</param>
    /// <param name="hostSettingsService">The host settings service.</param>
    /// <param name="eventLogger">The event logger.</param>
    [Obsolete("Deprecated in DotNetNuke 10.2.4. Please use overload with ListController. Scheduled removal in v12.0.0.")]
    public ProfileServiceController(IPortalController portalController, IApplicationStatusInfo appStatus, IPortalGroupController portalGroupController, IHostSettings hostSettings, IHostSettingsService hostSettingsService, IEventLogger eventLogger)
        : this(portalController, appStatus, portalGroupController, hostSettings, hostSettingsService, eventLogger, null)
    {
    }

    /// <summary>Searches a registration profile.</summary>
    /// <param name="q">The search criteria.</param>
    /// <returns>A response with a list of objects containing <c>id</c> and <c>name</c> fields.</returns>
    [HttpGet]
    public HttpResponseMessage Search(string q)
    {
        var results = RegistrationProfileController.Instance.Search(PortalController.GetEffectivePortalId(this.portalController, this.appStatus, this.portalGroupController, this.PortalSettings.PortalId), q);
        return this.Request.CreateResponse(
            HttpStatusCode.OK,
            results.OrderBy(sr => sr)
                .Select(field => new { id = field, name = field }));
    }

    /// <summary>Updates a user's vanity URL.</summary>
    /// <param name="vanityUrl">The URL.</param>
    /// <returns>A response indicating success.</returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public HttpResponseMessage UpdateVanityUrl(VanityUrlDTO vanityUrl)
    {
        // Clean Url
        var options = UrlRewriterUtils.GetOptionsFromSettings(new FriendlyUrlSettings(this.portalController, this.hostSettings, this.hostSettingsService, this.PortalSettings.PortalId));
        var cleanUrl = FriendlyUrlController.CleanNameForUrl(vanityUrl.Url, options, out var modified);

        if (modified)
        {
            return this.Request.CreateResponse(
                HttpStatusCode.OK,
                new
                {
                    Result = "warning",
                    Title = Localization.GetString("CleanWarningTitle", Localization.SharedResourceFile),
                    Message = Localization.GetString("ProfileUrlCleaned", Localization.SharedResourceFile),
                    SuggestedUrl = cleanUrl,
                });
        }

        // Validate for uniqueness
        var uniqueUrl = FriendlyUrlController.ValidateUrl(cleanUrl, -1, this.PortalSettings, out modified);

        if (modified)
        {
            return this.Request.CreateResponse(
                HttpStatusCode.OK,
                new
                {
                    Result = "warning",
                    Title = Localization.GetString("DuplicateUrlWarningTitle", Localization.SharedResourceFile),
                    Message = Localization.GetString("ProfileUrlNotUnique", Localization.SharedResourceFile),
                    SuggestedUrl = uniqueUrl,
                });
        }

        var user = this.PortalSettings.UserInfo;
        user.VanityUrl = uniqueUrl;
        UserController.UpdateUser(this.eventLogger, this.PortalSettings.PortalId, user);

        DataCache.RemoveCache(string.Format(CultureInfo.InvariantCulture, CacheController.VanityUrlLookupKey, this.PortalSettings.PortalId));

        // Url is clean and validated so we can update the User
        return this.Request.CreateResponse(HttpStatusCode.OK, new { Result = "success", });
    }

    /// <summary>Gets the profile property values.</summary>
    /// <returns>A response with a list of values.</returns>
    [DnnAuthorize]
    [HttpGet]
    public HttpResponseMessage ProfilePropertyValues()
    {
        string searchString = HttpContext.Current.Request.Params["SearchString"].NormalizeString();
        string propertyName = HttpContext.Current.Request.Params["PropName"].NormalizeString();
        int portalId = int.Parse(HttpContext.Current.Request.Params["PortalId"], CultureInfo.InvariantCulture);
        return this.Request.CreateResponse(HttpStatusCode.OK, Entities.Profile.ProfileController.SearchProfilePropertyValues(this.listController, this.hostSettings, this.portalController, this.appStatus, this.portalGroupController, portalId, propertyName, searchString));
    }

    /// <summary>A data transfer object with information about a vanity URL.</summary>
    public class VanityUrlDTO
    {
        /// <summary>Gets or sets the URL.</summary>
        public string Url { get; set; }
    }
}
