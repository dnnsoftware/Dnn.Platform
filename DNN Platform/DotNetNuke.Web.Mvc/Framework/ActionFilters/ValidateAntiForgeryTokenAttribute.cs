// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.Mvc.Framework.ActionFilters
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Web;
    using System.Web.Mvc;

    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Web.Mvc.Common;

    public class ValidateAntiForgeryTokenAttribute : AuthorizeAttributeBase
    {
        public virtual bool IsAuthenticated(HttpContextBase httpContext)
        {
            try
            {
                if (Thread.CurrentPrincipal.Identity.IsAuthenticated)
                {
                    var headers = httpContext.Request.Headers;
                    var form = httpContext.Request.Form;

                    // Try to fetch the token from Headers. (Used with Dnn service framework.).
                    // If not found then fetch it from form fields. (Would be used with standard MVC call).
                    var token = headers.AllKeys.Contains("RequestVerificationToken") ? headers.GetValues("RequestVerificationToken").FirstOrDefault()
                        : (
                        form.AllKeys.Contains("__RequestVerificationToken") ? form.GetValues("__RequestVerificationToken").FirstOrDefault() : null);

                    var cookieValue = this.GetAntiForgeryCookieValue(httpContext);
                    if (token != null)
                    {
                        AntiForgery.Instance.Validate(cookieValue, token);
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        protected string GetAntiForgeryCookieValue(HttpContextBase context)
        {
            foreach (var cookieValue in context.Request.Headers.GetValues("Cookie"))
            {
                var nameIndex = cookieValue.IndexOf(AntiForgery.Instance.CookieName, StringComparison.InvariantCultureIgnoreCase);

                if (nameIndex > -1)
                {
                    var valueIndex = nameIndex + AntiForgery.Instance.CookieName.Length + 1;
                    var valueEndIndex = cookieValue.Substring(valueIndex).IndexOf(';');
                    return valueEndIndex > -1 ? cookieValue.Substring(valueIndex, valueEndIndex) : cookieValue.Substring(valueIndex);
                }
            }

            return string.Empty;
        }

        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            if (!this.IsAuthenticated(httpContext))
            {
                return false;
            }

            return true;
        }
    }
}
