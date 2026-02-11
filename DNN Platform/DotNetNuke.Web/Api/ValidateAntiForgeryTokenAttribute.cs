// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.Api
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Threading;
    using System.Web.Http.Controllers;
    using System.Web.Http.Filters;

    using DotNetNuke.Web.Api.Internal;

    /// <summary>A web API action filter which validates the anti-forgery token.</summary>
    public class ValidateAntiForgeryTokenAttribute : ActionFilterAttribute
    {
        /// <summary>The success result.</summary>
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1306:FieldNamesMustBeginWithLowerCaseLetter", Justification = "Breaking Change")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]

        // ReSharper disable once InconsistentNaming
        protected static readonly Tuple<bool, string> SuccessResult = new Tuple<bool, string>(true, null);

        private static readonly List<string> BypassedAuthTypes = new List<string>();

        /// <inheritdoc />
        public override bool AllowMultiple => false;

        /// <inheritdoc />
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

        /// <summary>Appends the given <paramref name="authType"/> to the bypassed auth types.</summary>
        /// <param name="authType">The auth type to add.</param>
        internal static void AppendToBypassAuthTypes(string authType)
        {
            var text = (authType ?? string.Empty).Trim();
            if (text.Length > 0)
            {
                BypassedAuthTypes.Add(text);
            }
        }

        /// <summary>Gets the value of the anti-forgery cookie.</summary>
        /// <param name="actionContext">The request context.</param>
        /// <returns>The cookie value or <see cref="string.Empty"/>.</returns>
        protected static string GetAntiForgeryCookieValue(HttpActionContext actionContext)
        {
            if (actionContext?.Request != null && actionContext.Request.Headers.TryGetValues("Cookie", out var cookies))
            {
                foreach (var cookieValue in cookies)
                {
                    var nameIndex = cookieValue.IndexOf(AntiForgery.Instance.CookieName, StringComparison.OrdinalIgnoreCase);
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

        /// <summary>Whether the request is authorized.</summary>
        /// <param name="actionContext">The request context.</param>
        /// <returns>A <see cref="Tuple"/> where the first value is whether the request is authorized, and the second value is a message.</returns>
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
            // bypass anti-forgery for those handlers that request so.
            var authType = Thread.CurrentPrincipal?.Identity?.AuthenticationType;
            return !string.IsNullOrEmpty(authType) && BypassedAuthTypes.Contains(authType);
        }
    }
}
