#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2017
// by DotNetNuke Corporation
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation 
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and 
// to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions 
// of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
// TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using DotNetNuke.Web.Api.Internal;
using System.Threading;

namespace DotNetNuke.Web.Api
{
    public class ValidateAntiForgeryTokenAttribute : AuthorizeAttributeBase
    {
        private static readonly List<string> BypassedAuthTypes = new List<string>();

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

        public override bool IsAuthorized(AuthFilterContext context)
        {
            try
            {
                if (!BypassTokenCheck())
                {
                    var headers = context.ActionContext.Request.Headers;
                    var token = headers.GetValues("RequestVerificationToken").FirstOrDefault();
                    var cookieValue = GetAntiForgeryCookieValue(context);
                    AntiForgery.Instance.Validate(cookieValue, token);
                }
            }
            catch(Exception e)
            {
                context.AuthFailureMessage = e.Message;
                return false;
            }

            return true;
        }

        protected string GetAntiForgeryCookieValue(AuthFilterContext context)
        {
            foreach (var cookieValue in context.ActionContext.Request.Headers.GetValues("Cookie"))
            {
                var nameIndex = cookieValue.IndexOf(AntiForgery.Instance.CookieName, StringComparison.InvariantCultureIgnoreCase);

                if(nameIndex > -1)
                {
                    var valueIndex = nameIndex + AntiForgery.Instance.CookieName.Length + 1;
                    var valueEndIndex = cookieValue.Substring(valueIndex).IndexOf(';');
                    return valueEndIndex > -1 ? cookieValue.Substring(valueIndex, valueEndIndex) : cookieValue.Substring(valueIndex);
                }
            }
            
            return "";
        }

        public override bool AllowMultiple => false;
    }
}