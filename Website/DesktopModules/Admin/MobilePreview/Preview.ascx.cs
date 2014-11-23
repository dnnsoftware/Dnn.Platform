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
#region Usings

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.UI.WebControls;

using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Host;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Framework;
using DotNetNuke.Services.Mobile;
using DotNetNuke.Web.Client.ClientResourceManagement;
using DotNetNuke.Web.UI.WebControls;

#endregion

namespace DotNetNuke.Modules.Admin.MobilePreview
{
	public partial class Preview : PortalModuleBase
	{
		#region "Public Properties"

		protected string PreviewUrl
		{
			get
			{
				var tabId = PortalSettings.HomeTabId;

				if (Request.QueryString["previewTab"] != null)
				{
					int.TryParse(Request.QueryString["previewTab"], out tabId);
				}

				return Globals.NavigateURL(tabId, string.Empty, string.Empty);
			}
		}

		#endregion

		#region "Event Handlers"

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			if (!string.IsNullOrEmpty(Request.QueryString["UserAgent"]))
			{
				var userAgent = UrlUtils.DecryptParameter(Request.QueryString["UserAgent"]);
				if (Request.QueryString["SendAgent"] != "true")
				{
					userAgent = Request.UserAgent;
				}

				CreateViewProxy(userAgent);
			}

			this.Page.Title = LocalizeString("PageTitle");

			ClientResourceManager.RegisterScript(this.Page, string.Format("{0}Scripts/PreviewEmulator.js", this.ControlPath));

			if (!IsPostBack)
			{
				BindProfiles();
			}
		}

		#endregion

		#region "Private Methods"

		private void BindProfiles()
		{
			var profiles = new PreviewProfileController().GetProfilesByPortal(ModuleContext.PortalId);
			ddlProfileList.Items.Clear();

			var selectedProfile = -1;
			if (Request.QueryString["profile"] != null)
			{
				selectedProfile = Convert.ToInt32(Request.QueryString["profile"]);
			}

			foreach (var previewProfile in profiles)
			{
				var value = string.Format("width : \"{0}\", height : \"{1}\", userAgent: \"{2}\"", previewProfile.Width, previewProfile.Height, UrlUtils.EncryptParameter(previewProfile.UserAgent));

				var listItem = new ListItem(previewProfile.Name, value);
				if (selectedProfile == previewProfile.Id)
				{
					listItem.Selected = true;
				}

				ddlProfileList.AddItem(listItem.Text, listItem.Value);
			}
		}

		private void CreateViewProxy(string userAgent)
		{
			Response.Clear();

            //Depending on number of redirects, GetHttpContent can be a self-recursive call.
			Response.Write(GetHttpContent(PreviewUrl, userAgent));

			Response.End();
		}

		private string GetHttpContent(string url, string userAgent)
		{
			try
			{
				//add dnnprintmode in url so that all DNN page will display in normal.
				var requestUrl = url;
				if (requestUrl.IndexOf("dnnprintmode") == -1)
				{
					requestUrl = string.Format("{0}{1}dnnprintmode=true", url, url.IndexOf("?") == -1 ? "?" : "&");
				}

				var wreq = Globals.GetExternalRequest(requestUrl);
				wreq.UserAgent = userAgent;
				wreq.Referer = Request.Url.ToString();
				wreq.Method = "GET";
				wreq.Timeout = Host.WebRequestTimeout;
				wreq.AllowAutoRedirect = false;
				wreq.ContentType = "application/x-www-form-urlencoded";
				SetCookies(wreq);

				if (requestUrl.StartsWith("https://"))
				{
					ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
				}

				var httpResponse = wreq.GetResponse() as HttpWebResponse;

				//if the request has been redirect, add PrintMode in url and re-request
				if (httpResponse.StatusCode == HttpStatusCode.Found || httpResponse.StatusCode == HttpStatusCode.MovedPermanently)
				{
					var redirectUrl = httpResponse.Headers["Location"];

                    //calling same method recursive.
					return GetHttpContent(redirectUrl, userAgent);
				}

				//get content from http response
				var responseStream = httpResponse.GetResponseStream();

				using (var reader = new StreamReader(responseStream, true))
				{
					var content = reader.ReadToEnd();

					var requestUri = wreq.Address;

					content = MakeAbsoluteUrl(content, requestUri);

					//append current url to a js variable, so that can be used in client side.
					content += string.Format("<script type=\"text/javascript\">window.dnnPreviewUrl = '{0}';</script>", url);
					return content;
				}
			}
			catch (SecurityException)
			{
				return string.Format("<span style=\"color: red;\">{0}</span>", "You must change your website's trust level to full trust if you want to preview an SSL path.");
			}
			catch (Exception ex)
			{
				return string.Format("<span style=\"color: red;\">{0}</span>", "ERROR:" + ex.Message);
			}
		}

		//append current cookies to the http request
        private void SetCookies(HttpWebRequest proxyRequest)
        {
            proxyRequest.CookieContainer = new CookieContainer();
            var sessionCookie = GetSessionCookieName();
            foreach (var key in Request.Cookies.AllKeys)
            {
                if (key == sessionCookie)
                {
                    continue;
                }

                var cookie = new Cookie(key, Request.Cookies[key].Value, "/", proxyRequest.RequestUri.Host);
                proxyRequest.CookieContainer.Add(cookie);
            }
        }

		private string MakeAbsoluteUrl(string content, Uri requestUri)
		{
			var domain = requestUri.AbsoluteUri.Replace(requestUri.PathAndQuery, string.Empty);
			var currDirectory = domain;
			for (var i = 0; i < requestUri.Segments.Length - 1; i++)
			{
				currDirectory += requestUri.Segments[i];

			}

			Regex pathReg = new Regex("(src|href)=['\"]?([^>'\"\\s]+)['\"]?");
			MatchCollection matches = pathReg.Matches(content);
			foreach (Match match in matches)
			{
                var path = match.Groups[2].Value;
                if (!string.IsNullOrEmpty(path) && path.IndexOf(":") == -1 && !path.StartsWith("#"))
                {
					var prefix = path.StartsWith("/") ? domain : currDirectory;
					content = content.Replace(match.Value, string.Format("{0}=\"{1}{2}\"", match.Groups[1].Value, prefix, path));
				}
			}

			return content;
		}

        private string GetSessionCookieName()
        {
            return "ASP.NET_SessionId";
        }

		#endregion
	}
}