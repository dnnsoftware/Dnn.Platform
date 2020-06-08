// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
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
