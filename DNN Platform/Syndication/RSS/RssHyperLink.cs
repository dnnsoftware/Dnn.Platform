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
#region Usings

using System;
using System.Web.UI;
using System.Web.UI.WebControls;

#endregion

namespace DotNetNuke.Services.Syndication
{
    /// <summary>
    ///   RssHyperLink control - works with RssHttpHandler
    /// </summary>
    public class RssHyperLink : HyperLink
    {
        private string _channelName;
        private bool _includeUserName;

        public RssHyperLink()
        {
            Text = "RSS";
        }

        // passed to RssHttpHandler
        public string ChannelName
        {
            get
            {
                return _channelName;
            }
            set
            {
                _channelName = value;
            }
        }

        // when flag is set, the current user'd name is passed to RssHttpHandler
        public bool IncludeUserName
        {
            get
            {
                return _includeUserName;
            }
            set
            {
                _includeUserName = value;
            }
        }

        protected override void OnPreRender(EventArgs e)
        {
            // modify the NavigateUrl to include optional user name and channel name
            string channel = _channelName != null ? _channelName : string.Empty;
            string user = _includeUserName ? Context.User.Identity.Name : string.Empty;
            NavigateUrl = RssHttpHandlerHelper.GenerateChannelLink(NavigateUrl, channel, user);

            // add <link> to <head> tag (if <head runat=server> is present)
            if (Page.Header != null)
            {
                string title = string.IsNullOrEmpty(channel) ? Text : channel;

                Page.Header.Controls.Add(new LiteralControl(string.Format("\r\n<link rel=\"alternate\" type=\"application/rss+xml\" title=\"{0}\" href=\"{1}\" />", title, NavigateUrl)));
            }

            base.OnPreRender(e);
        }
    }
}