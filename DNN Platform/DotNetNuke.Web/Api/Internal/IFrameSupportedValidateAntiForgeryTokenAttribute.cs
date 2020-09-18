// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.Api.Internal
{
    using System;
    using System.Net.Http;
    using System.Web.Http.Controllers;

    public class IFrameSupportedValidateAntiForgeryTokenAttribute : ValidateAntiForgeryTokenAttribute
    {
        protected override Tuple<bool, string> IsAuthorized(HttpActionContext actionContext)
        {
            var result = base.IsAuthorized(actionContext);
            if (result.Item1)
            {
                return SuccessResult;
            }

            try
            {
                var queryString = actionContext.Request.GetQueryNameValuePairs();
                var token = string.Empty;
                foreach (var kvp in queryString)
                {
                    if (kvp.Key == "__RequestVerificationToken")
                    {
                        token = kvp.Value;
                        break;
                    }
                }

                var cookieValue = GetAntiForgeryCookieValue(actionContext);

                AntiForgery.Instance.Validate(cookieValue, token);
            }
            catch (Exception e)
            {
                return new Tuple<bool, string>(false, e.Message);
            }

            return SuccessResult;
        }
    }
}
