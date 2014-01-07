#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2014
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
using System.Linq;
using DotNetNuke.Web.Api.Internal;

namespace DotNetNuke.Web.Api
{
    public class ValidateAntiForgeryTokenAttribute : AuthorizeAttributeBase
    {
        public override bool IsAuthorized(AuthFilterContext context)
        {
            try
            {
                string cookieValue = GetAntiForgeryCookieValue(context);
                var token = context.ActionContext.Request.Headers.GetValues("RequestVerificationToken").FirstOrDefault();

                AntiForgery.Instance.Validate(cookieValue, token);
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

                    if (valueEndIndex > -1)
                    {
                        return cookieValue.Substring(valueIndex, valueEndIndex);
                    }
                    else
                    {
                        return cookieValue.Substring(valueIndex);
                    }
                }
            }
            
            return "";
        }

        public override bool AllowMultiple
        {
            get { return false; }
        }
    }
}