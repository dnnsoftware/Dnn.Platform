// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Syndication
{
    using System;
    using System.Web;
    using System.Xml;

    public delegate void InitEventHandler(object source, EventArgs e);

    public delegate void PreRenderEventHandler(object source, EventArgs e);

    /// <summary>Base class for RssHttpHandler - Generic handler and strongly typed ones are derived from it.</summary>
    /// <typeparam name="TRssChannelType">The channel type.</typeparam>
    /// <typeparam name="TRssItemType">The item type.</typeparam>
    /// <typeparam name="TRssImageType">The image type.</typeparam>
    public abstract class RssHttpHandlerBase<TRssChannelType, TRssItemType, TRssImageType> : IHttpHandler
        where TRssChannelType : RssChannelBase<TRssItemType, TRssImageType>, new()
        where TRssItemType : RssElementBase, new()
        where TRssImageType : RssElementBase, new()
    {
        private TRssChannelType channel;
        private HttpContext context;

        public event InitEventHandler Init;

        public event PreRenderEventHandler PreRender;

        /// <inheritdoc/>
        bool IHttpHandler.IsReusable
        {
            get
            {
                return false;
            }
        }

        protected TRssChannelType Channel
        {
            get
            {
                return this.channel;
            }
        }

        protected HttpContext Context
        {
            get
            {
                return this.context;
            }
        }

        /// <inheritdoc/>
        void IHttpHandler.ProcessRequest(HttpContext context)
        {
            this.InternalInit(context);

            // let inherited handlers setup any special handling
            this.OnInit(EventArgs.Empty);

            // parse the channel name and the user name from the query string
            string userName;
            string channelName;
            RssHttpHandlerHelper.ParseChannelQueryString(context.Request, out channelName, out userName);

            // populate items (call the derived class)
            this.PopulateChannel(channelName, userName);

            this.OnPreRender(EventArgs.Empty);

            this.Render(new XmlTextWriter(this.Context.Response.OutputStream, null));
        }

        /// <summary>  Triggers the Init event.</summary>
        protected virtual void OnInit(EventArgs ea)
        {
            if (this.Init != null)
            {
                this.Init(this, ea);
            }
        }

        /// <summary>  Triggers the PreRender event.</summary>
        protected virtual void OnPreRender(EventArgs ea)
        {
            if (this.PreRender != null)
            {
                this.PreRender(this, ea);
            }
        }

        protected virtual void PopulateChannel(string channelName, string userName)
        {
        }

        protected virtual void Render(XmlTextWriter writer)
        {
            XmlDocument doc = this.channel.SaveAsXml();
            doc.Save(writer);
        }

        private void InternalInit(HttpContext context)
        {
            this.context = context;

            // create the channel
            this.channel = new TRssChannelType();
            this.channel.SetDefaults();

            this.Context.Response.ContentType = "text/xml";
        }
    }
}
