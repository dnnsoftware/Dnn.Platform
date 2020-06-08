// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
using System;
using System.Collections.Generic;
using System.Linq;
using DotNetNuke.Web.Api.Internal;
using System.Threading;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;

namespace DotNetNuke.Web.Api
{
    public class ValidateAntiForgeryTokenAttribute : ActionFilterAttribute
    {
        private static readonly List<string> BypassedAuthTypes = new List<string>();

        protected static Tuple<bool, string> SuccessResult = new Tuple<bool, string>(true, null);

        internal static void AppendToBypassAuthTypes(string authType)
        {
            var text = (authType ?? "").Trim();
            if (text.Length > 0)
            {
                BypassedAuthTypes.Add(text);
            }
        }

        private static bool BypassTokenCheck()
        {
            // bypass anti-forgery for those handllers that request so.
            var authType = Thread.CurrentPrincipal?.Identity?.AuthenticationType;
            return !string.IsNullOrEmpty(authType) && BypassedAuthTypes.Contains(authType);
        }

        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            if (!BypassTokenCheck())
            {
                var result = IsAuthorized(actionContext);
                if (!result.Item1)
                {
                    throw new UnauthorizedAccessException(result.Item2);
                }
            }
        }

        protected virtual Tuple<bool, string> IsAuthorized(HttpActionContext actionContext)
        {
            try
            {
                if (!BypassTokenCheck())
                {
                    string token = null;
                    IEnumerable<string> values;
                    if (actionContext?.Request != null &&
                        actionContext.Request.Headers.TryGetValues("RequestVerificationToken", out values))
                    {
                        token = values.FirstOrDefault();
                    }

                    if (string.IsNullOrEmpty(token))
                        return new Tuple<bool, string>(false, "RequestVerificationToken not present");

                    var cookieValue = GetAntiForgeryCookieValue(actionContext);
                    AntiForgery.Instance.Validate(cookieValue, token);
                }
            }
            catch (Exception e)
            {
                return new Tuple<bool, string>(false, e.Message);
            }

            return SuccessResult;
        }

        protected static string GetAntiForgeryCookieValue(HttpActionContext actionContext)
        {
            IEnumerable<string> cookies;
            if (actionContext?.Request != null && actionContext.Request.Headers.TryGetValues("Cookie", out cookies))
            {
                foreach (var cookieValue in cookies)
                {
                    var nameIndex = cookieValue.IndexOf(AntiForgery.Instance.CookieName, StringComparison.InvariantCultureIgnoreCase);
                    if (nameIndex > -1)
                    {
                        var valueIndex = nameIndex + AntiForgery.Instance.CookieName.Length + 1;
                        var valueEndIndex = cookieValue.Substring(valueIndex).IndexOf(';');
                        return valueEndIndex > -1 ? cookieValue.Substring(valueIndex, valueEndIndex) : cookieValue.Substring(valueIndex);
                    }
                }
            }

            return "";
        }

        public override bool AllowMultiple => false;
    }
}
