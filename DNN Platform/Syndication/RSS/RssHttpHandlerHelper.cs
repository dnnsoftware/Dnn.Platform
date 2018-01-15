#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2018
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
using System.Web;
using System.Web.Security;

#endregion

namespace DotNetNuke.Services.Syndication
{
    /// <summary>
    ///   Helper class (for RssHtppHandler) to pack and unpack user name and channel to from/to query string
    /// </summary>
    public class RssHttpHandlerHelper
    {
        private RssHttpHandlerHelper()
        {
        }

        // helper to generate link [to the .ashx] containing channel name and (encoded) userName
        public static string GenerateChannelLink(string handlerPath, string channelName, string userName)
        {
            string link = VirtualPathUtility.ToAbsolute(handlerPath);

            if (string.IsNullOrEmpty(userName))
            {
                if (!string.IsNullOrEmpty(channelName))
                {
                    link += "?c=" + HttpUtility.UrlEncode(channelName);
                }
            }
            else
            {
                if (channelName == null)
                {
                    channelName = string.Empty;
                }

                userName = "." + userName; // not to confuse the encrypted string with real auth ticket for real user
                DateTime ticketDate = DateTime.Now.AddDays(-100); // already expried

                var t = new FormsAuthenticationTicket(2, userName, ticketDate, ticketDate.AddDays(2), false, channelName, "/");

                link += "?t=" + FormsAuthentication.Encrypt(t);
            }

            return link;
        }

        internal static void ParseChannelQueryString(HttpRequest request, out string channelName, out string userName)
        {
            string ticket = request.QueryString["t"];

            if (string.IsNullOrEmpty(ticket))
            {
                userName = string.Empty;
                // optional unencrypted channel name
                channelName = request.QueryString["c"];
            }
            else
            {
                // encrypted user name and channel name
                FormsAuthenticationTicket t = FormsAuthentication.Decrypt(ticket);
                userName = t.Name.Substring(1); // remove extra prepended '.'
                channelName = t.UserData;
            }
        }
    }
}