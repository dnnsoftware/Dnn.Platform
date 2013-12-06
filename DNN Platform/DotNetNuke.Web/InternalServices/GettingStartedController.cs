#region Copyright

// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2013
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
using System.Net;
using System.Net.Http;
using System.Web.Http;

using DotNetNuke.Entities.Controllers;
using DotNetNuke.Web.Api;

namespace DotNetNuke.Web.InternalServices
{
    [RequireHost]
    public class GettingStartedController : DnnApiController
    {
        private const string GettingStartedHideKey = "GettingStarted_Hide_{0}";
        private const string GettingStartedDisplayKey = "GettingStarted_Display_{0}";

        public class ClosePageDto
        {
            public bool IsHidden { get; set; }
        }

        public class EmailDto
        {
            public string Email { get; set; }
        }

        [HttpGet]
        public HttpResponseMessage GetGettingStartedPageSettings()
        {
            var isHidden = HostController.Instance.GetBoolean(String.Format(GettingStartedHideKey, PortalSettings.UserId), false);
            var userEmailAddress = PortalSettings.UserInfo.Email;

            return Request.CreateResponse(HttpStatusCode.OK, new { IsHidden = isHidden, EmailAddress = userEmailAddress});
        }

        [HttpPost]
        public HttpResponseMessage CloseGettingStartedPage(ClosePageDto dto)
        {
            HostController.Instance.Update(String.Format(GettingStartedHideKey, PortalSettings.UserId), dto.IsHidden.ToString());
            HostController.Instance.Update(String.Format(GettingStartedDisplayKey, PortalSettings.UserId), "false");

            return Request.CreateResponse(HttpStatusCode.OK, "Success");
        }

        [HttpPost]
        public HttpResponseMessage SubscribeToNewsletter(EmailDto dto)
        {
            HostController.Instance.Update("NewsletterSubscribeEmail", dto.Email);

            return Request.CreateResponse(HttpStatusCode.OK, "Success");
        }

        [HttpGet]
        public HttpResponseMessage IsValidUrl(string url)
        {
            var isValid = IsValidUrlInternal(url);

            return Request.CreateResponse(HttpStatusCode.OK, new { IsValid = isValid });
        }

        /// <summary>
        /// Checks if url does not return server or protocol errors
        /// </summary>
        /// <param name="url">Url to check</param>
        /// <returns></returns>
        private static bool IsValidUrlInternal(string url)
        {
            HttpWebResponse response = null;
            try
            {
                var request = WebRequest.Create(url);
                request.Timeout = 5000; // set the timeout to 5 seconds to keep the user from waiting too long for the page to load
                request.Method = "HEAD"; // get only the header information - no need to download any content

                response = request.GetResponse() as HttpWebResponse;

                if (response == null)
                {
                    return false;
                }
                var statusCode = (int)response.StatusCode;
                if (statusCode >= 500 && statusCode <= 510) // server errors
                {
                    return false;
                }
            }
            catch (Exception)
            {
                return false;
            }
            finally
            {
                if (response != null)
                {
                    response.Close();
                }
            }
            return true;
        }

    }
}
