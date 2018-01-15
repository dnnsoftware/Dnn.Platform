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