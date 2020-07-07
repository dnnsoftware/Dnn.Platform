// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Syndication
{
    using System;
    using System.Web.UI;
    using System.Web.UI.WebControls;

    /// <summary>
    ///   RssHyperLink control - works with RssHttpHandler.
    /// </summary>
    public class RssHyperLink : HyperLink
    {
        private string _channelName;
        private bool _includeUserName;

        public RssHyperLink()
        {
            this.Text = "RSS";
        }

        // passed to RssHttpHandler
        public string ChannelName
        {
            get
            {
                return this._channelName;
            }

            set
            {
                this._channelName = value;
            }
        }

        // when flag is set, the current user'd name is passed to RssHttpHandler
        public bool IncludeUserName
        {
            get
            {
                return this._includeUserName;
            }

            set
            {
                this._includeUserName = value;
            }
        }

        protected override void OnPreRender(EventArgs e)
        {
            // modify the NavigateUrl to include optional user name and channel name
            string channel = this._channelName != null ? this._channelName : string.Empty;
            string user = this._includeUserName ? this.Context.User.Identity.Name : string.Empty;
            this.NavigateUrl = RssHttpHandlerHelper.GenerateChannelLink(this.NavigateUrl, channel, user);

            // add <link> to <head> tag (if <head runat=server> is present)
            if (this.Page.Header != null)
            {
                string title = string.IsNullOrEmpty(channel) ? this.Text : channel;

                this.Page.Header.Controls.Add(new LiteralControl(string.Format("\r\n<link rel=\"alternate\" type=\"application/rss+xml\" title=\"{0}\" href=\"{1}\" />", title, this.NavigateUrl)));
            }

            base.OnPreRender(e);
        }
    }
}
