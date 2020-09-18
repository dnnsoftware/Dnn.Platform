// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Services.GeneratedImage
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Web;
    using System.Web.UI;
    using System.Web.UI.WebControls;

    public class GeneratedImage : Image
    {
        private const string TimestampField = "__timestamp";
        private readonly Control _bindingContainer;
        private readonly HttpContextBase _context;
        private string _timestamp;
        private string _imageHandlerUrl;

        public GeneratedImage()
        {
            this.Parameters = new List<ImageParameter>();
        }

        internal GeneratedImage(HttpContextBase context, Control bindingContainer)
            : this()
        {
            this._context = context;
            this._bindingContainer = bindingContainer;
        }

        public List<ImageParameter> Parameters { get; }

        public string ImageHandlerUrl
        {
            get
            {
                return this._imageHandlerUrl ?? string.Empty;
            }

            set
            {
                this._imageHandlerUrl = value;
            }
        }

        public string Timestamp
        {
            get
            {
                return this._timestamp ?? string.Empty;
            }

            set
            {
                this._timestamp = value;
            }
        }

        private new HttpContextBase Context => this._context ?? new HttpContextWrapper(HttpContext.Current);

        private new Control BindingContainer => this._bindingContainer ?? base.BindingContainer;

        protected override void OnDataBinding(EventArgs e)
        {
            base.OnDataBinding(e);

            Control bindingContainer = this.BindingContainer;
            foreach (var parameter in this.Parameters)
            {
                parameter.BindingContainer = bindingContainer;
                parameter.DataBind();
            }
        }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            if (this.DesignMode)
            {
                return;
            }

            this.ImageUrl = this.BuildImageUrl();
        }

        private static void AddQueryStringParameter(StringBuilder stringBuilder, bool paramAlreadyAdded, string name, string value)
        {
            stringBuilder.Append(paramAlreadyAdded ? '&' : '?');
            stringBuilder.Append(HttpUtility.UrlEncode(name));
            stringBuilder.Append('=');
            stringBuilder.Append(HttpUtility.UrlEncode(value));
        }

        private string BuildImageUrl()
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.Append(this.ImageHandlerUrl);

            var paramAlreadyAdded = false;

            foreach (var parameter in this.Parameters)
            {
                AddQueryStringParameter(stringBuilder, paramAlreadyAdded, parameter.Name, parameter.Value);
                paramAlreadyAdded = true;
            }

            string timeStamp = this.Timestamp?.Trim();
            if (!string.IsNullOrEmpty(timeStamp))
            {
                AddQueryStringParameter(stringBuilder, paramAlreadyAdded, TimestampField, timeStamp);
            }

            return stringBuilder.ToString();
        }
    }
}
