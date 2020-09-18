// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Web.InternalServices
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Web;
    using System.Web.Http;

    using DotNetNuke.Common.Lists;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Profile;
    using DotNetNuke.Entities.Urls;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.Services.Registration;
    using DotNetNuke.Web.Api;

    [DnnAuthorize]
    public class ProfileServiceController : DnnApiController
    {
        [HttpGet]
        public HttpResponseMessage Search(string q)
        {
            var results = RegistrationProfileController.Instance.Search(PortalController.GetEffectivePortalId(this.PortalSettings.PortalId), q);
            return this.Request.CreateResponse(
                HttpStatusCode.OK,
                results.OrderBy(sr => sr)
                        .Select(field => new { id = field, name = field }));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage UpdateVanityUrl(VanityUrlDTO vanityUrl)
        {
            bool modified;

            // Clean Url
            var options = UrlRewriterUtils.GetOptionsFromSettings(new FriendlyUrlSettings(this.PortalSettings.PortalId));
            var cleanUrl = FriendlyUrlController.CleanNameForUrl(vanityUrl.Url, options, out modified);

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
            UserController.UpdateUser(this.PortalSettings.PortalId, user);

            DataCache.RemoveCache(string.Format(CacheController.VanityUrlLookupKey, this.PortalSettings.PortalId));

            // Url is clean and validated so we can update the User
            return this.Request.CreateResponse(HttpStatusCode.OK, new { Result = "success" });
        }

        [DnnAuthorize]
        [HttpGet]
        public HttpResponseMessage ProfilePropertyValues()
        {
            string searchString = HttpContext.Current.Request.Params["SearchString"].NormalizeString();
            string propertyName = HttpContext.Current.Request.Params["PropName"].NormalizeString();
            int portalId = int.Parse(HttpContext.Current.Request.Params["PortalId"]);
            return this.Request.CreateResponse(HttpStatusCode.OK, Entities.Profile.ProfileController.SearchProfilePropertyValues(portalId, propertyName, searchString));
        }

        public class VanityUrlDTO
        {
            public string Url { get; set; }
        }
    }
}
