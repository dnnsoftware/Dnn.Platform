// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.Api
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Web.Http.Controllers;
    using System.Web.Http.Filters;

    using DotNetNuke.Web.Api.Internal;

    public class ValidateAntiForgeryTokenAttribute : ActionFilterAttribute
    {
        protected static Tuple<bool, string> SuccessResult = new Tuple<bool, string>(true, null);

        private static readonly List<string> BypassedAuthTypes = new List<string>();

        public override bool AllowMultiple => false;

        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            if (!BypassTokenCheck())
            {
                var result = this.IsAuthorized(actionContext);
                if (!result.Item1)
                {
                    throw new UnauthorizedAccessException(result.Item2);
                }
            }
        }

        internal static void AppendToBypassAuthTypes(string authType)
        {
            var text = (authType ?? string.Empty).Trim();
            if (text.Length > 0)
            {
                BypassedAuthTypes.Add(text);
            }
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

            return string.Empty;
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
                    {
                        return new Tuple<bool, string>(false, "RequestVerificationToken not present");
                    }

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

        private static bool BypassTokenCheck()
        {
            // bypass anti-forgery for those handllers that request so.
            var authType = Thread.CurrentPrincipal?.Identity?.AuthenticationType;
            return !string.IsNullOrEmpty(authType) && BypassedAuthTypes.Contains(authType);
        }
    }
}
