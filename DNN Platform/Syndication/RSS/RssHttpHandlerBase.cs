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

    /// <summary>
    ///   Base class for RssHttpHandler - Generic handler and strongly typed ones are derived from it.
    /// </summary>
    /// <typeparam name = "RssChannelType"></typeparam>
    /// <typeparam name = "RssItemType"></typeparam>
    /// <typeparam name = "RssImageType"></typeparam>
    public abstract class RssHttpHandlerBase<RssChannelType, RssItemType, RssImageType> : IHttpHandler
        where RssChannelType : RssChannelBase<RssItemType, RssImageType>, new()
        where RssItemType : RssElementBase, new()
        where RssImageType : RssElementBase, new()
    {
        private RssChannelType _channel;
        private HttpContext _context;

        public event InitEventHandler Init;

        public event PreRenderEventHandler PreRender;

        bool IHttpHandler.IsReusable
        {
            get
            {
                return false;
            }
        }

        protected RssChannelType Channel
        {
            get
            {
                return this._channel;
            }
        }

        protected HttpContext Context
        {
            get
            {
                return this._context;
            }
        }

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

        /// <summary>
        ///   Triggers the Init event.
        /// </summary>
        protected virtual void OnInit(EventArgs ea)
        {
            if (this.Init != null)
            {
                this.Init(this, ea);
            }
        }

        /// <summary>
        ///   Triggers the PreRender event.
        /// </summary>
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
            XmlDocument doc = this._channel.SaveAsXml();
            doc.Save(writer);
        }

        private void InternalInit(HttpContext context)
        {
            this._context = context;

            // create the channel
            this._channel = new RssChannelType();
            this._channel.SetDefaults();

            this.Context.Response.ContentType = "text/xml";
        }
    }
}
