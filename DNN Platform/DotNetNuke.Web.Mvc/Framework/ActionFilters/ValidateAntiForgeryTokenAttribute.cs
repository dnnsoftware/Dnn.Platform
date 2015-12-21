using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Web.Mvc.Common;

namespace DotNetNuke.Web.Mvc.Framework.ActionFilters
{
  public  class ValidateAntiForgeryTokenAttribute : AuthorizeAttributeBase
    {
        public virtual bool IsAuthenticated(HttpContextBase httpContext)
        {
            try
            {
                if (Thread.CurrentPrincipal.Identity.IsAuthenticated)
                {
                    var headers = httpContext.Request.Headers;
                    var token = httpContext.Request.Params.AllKeys.Contains("__RequestVerificationToken")
                        ? httpContext.Request.Params.GetValues("__RequestVerificationToken").FirstOrDefault()
                        : null;
                    var cookieValue = GetAntiForgeryCookieValue(httpContext);
                    if (token != null)
                    {
                        AntiForgery.Instance.Validate(cookieValue,
                            token.Split(new char[] {','}, StringSplitOptions.RemoveEmptyEntries)[0]);
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
            catch (Exception e)
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

            return "";
        }

        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            if (!IsAuthenticated(httpContext))
            {
                return false;
            }
            return true;
        }
    }
}
