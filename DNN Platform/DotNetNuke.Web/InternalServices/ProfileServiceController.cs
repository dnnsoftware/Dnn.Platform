// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
#region Usings

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
using DotNetNuke.Web.Api;
using DotNetNuke.Services.Registration;

#endregion

namespace DotNetNuke.Web.InternalServices
{
    [DnnAuthorize]
    public class ProfileServiceController : DnnApiController
    {
        [HttpGet]
        public HttpResponseMessage Search(string q)
        {
            var results = RegistrationProfileController.Instance.Search(PortalController.GetEffectivePortalId(PortalSettings.PortalId), q);
            return Request.CreateResponse(HttpStatusCode.OK,
                        results.OrderBy(sr => sr)
                        .Select(field => new { id = field, name = field })
                    );
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage UpdateVanityUrl(VanityUrlDTO vanityUrl)
        {
            bool modified;

            //Clean Url
            var options = UrlRewriterUtils.GetOptionsFromSettings(new FriendlyUrlSettings(PortalSettings.PortalId));
            var cleanUrl = FriendlyUrlController.CleanNameForUrl(vanityUrl.Url, options, out modified);


            if (modified)
            {
                return Request.CreateResponse(HttpStatusCode.OK, 
                    new {
                            Result = "warning",
                            Title = Localization.GetString("CleanWarningTitle", Localization.SharedResourceFile),
                            Message = Localization.GetString("ProfileUrlCleaned", Localization.SharedResourceFile),
                            SuggestedUrl = cleanUrl
                        });
            }

            //Validate for uniqueness
            var uniqueUrl = FriendlyUrlController.ValidateUrl(cleanUrl, -1, PortalSettings, out modified);


            if (modified)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                                              new
                                                  {
                                                      Result = "warning",
                                                      Title = Localization.GetString("DuplicateUrlWarningTitle", Localization.SharedResourceFile),
                                                      Message = Localization.GetString("ProfileUrlNotUnique", Localization.SharedResourceFile),
                                                      SuggestedUrl = uniqueUrl
                                                  });
            }

            var user = PortalSettings.UserInfo;
            user.VanityUrl = uniqueUrl;
            UserController.UpdateUser(PortalSettings.PortalId, user);

            DataCache.RemoveCache(string.Format(CacheController.VanityUrlLookupKey, PortalSettings.PortalId));

            //Url is clean and validated so we can update the User
            return Request.CreateResponse(HttpStatusCode.OK, new { Result = "success" });
        }
        
        public class VanityUrlDTO
        {
            public string Url { get; set; }
        }

        [DnnAuthorize]
        [HttpGet]
        public HttpResponseMessage ProfilePropertyValues()
        {
            string searchString = HttpContext.Current.Request.Params["SearchString"].NormalizeString();
            string propertyName = HttpContext.Current.Request.Params["PropName"].NormalizeString();
            int portalId = int.Parse(HttpContext.Current.Request.Params["PortalId"]);
            return Request.CreateResponse(HttpStatusCode.OK, Entities.Profile.ProfileController.SearchProfilePropertyValues(portalId, propertyName, searchString));
        }

    }
}
