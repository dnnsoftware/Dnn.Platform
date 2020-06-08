// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
#region Usings

using System;
using System.Web;
using System.Xml;

#endregion

namespace DotNetNuke.Services.Syndication
{
    public delegate void InitEventHandler(object source, EventArgs e);

    public delegate void PreRenderEventHandler(object source, EventArgs e);

    /// <summary>
    ///   Base class for RssHttpHandler - Generic handler and strongly typed ones are derived from it
    /// </summary>
    /// <typeparam name = "RssChannelType"></typeparam>
    /// <typeparam name = "RssItemType"></typeparam>
    /// <typeparam name = "RssImageType"></typeparam>
    public abstract class RssHttpHandlerBase<RssChannelType, RssItemType, RssImageType> : IHttpHandler where RssChannelType : RssChannelBase<RssItemType, RssImageType>, new()
                                                                                                       where RssItemType : RssElementBase, new()
                                                                                                       where RssImageType : RssElementBase, new()
    {
        private RssChannelType _channel;
        private HttpContext _context;

        protected RssChannelType Channel
        {
            get
            {
                return _channel;
            }
        }

        protected HttpContext Context
        {
            get
            {
                return _context;
            }
        }

        #region IHttpHandler Members

        void IHttpHandler.ProcessRequest(HttpContext context)
        {
            InternalInit(context);

            // let inherited handlers setup any special handling
            OnInit(EventArgs.Empty);

            // parse the channel name and the user name from the query string
            string userName;
            string channelName;
            RssHttpHandlerHelper.ParseChannelQueryString(context.Request, out channelName, out userName);

            // populate items (call the derived class)
            PopulateChannel(channelName, userName);

            OnPreRender(EventArgs.Empty);

            Render(new XmlTextWriter(Context.Response.OutputStream, null));
        }

        bool IHttpHandler.IsReusable
        {
            get
            {
                return false;
            }
        }

        #endregion

        public event InitEventHandler Init;
        public event PreRenderEventHandler PreRender;

        /// <summary>
        ///   Triggers the Init event.
        /// </summary>
        protected virtual void OnInit(EventArgs ea)
        {
            if (Init != null)
            {
                Init(this, ea);
            }
        }

        /// <summary>
        ///   Triggers the PreRender event.
        /// </summary>
        protected virtual void OnPreRender(EventArgs ea)
        {
            if (PreRender != null)
            {
                PreRender(this, ea);
            }
        }

        private void InternalInit(HttpContext context)
        {
            _context = context;

            // create the channel
            _channel = new RssChannelType();
            _channel.SetDefaults();

            Context.Response.ContentType = "text/xml";
        }

        protected virtual void PopulateChannel(string channelName, string userName)
        {
        }

        protected virtual void Render(XmlTextWriter writer)
        {
            XmlDocument doc = _channel.SaveAsXml();
            doc.Save(writer);
        }
    }
}
