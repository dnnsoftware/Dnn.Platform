// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System;
using System.Net.Http;
using System.Web.Http.Controllers;

namespace DotNetNuke.Web.Api.Internal
{
    public class IFrameSupportedValidateAntiForgeryTokenAttribute : ValidateAntiForgeryTokenAttribute
    {
        protected override Tuple<bool, string> IsAuthorized(HttpActionContext actionContext)
        {
            var result = base.IsAuthorized(actionContext);
            if (result.Item1) return SuccessResult;

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
