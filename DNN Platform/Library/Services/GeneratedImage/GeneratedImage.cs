using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace DotNetNuke.Services.GeneratedImage
{
    public class GeneratedImage : Image
    {
        private const string TimestampField = "__timestamp";
        private string _timestamp;
        private readonly Control _bindingContainer;
        private readonly HttpContextBase _context;
        private string _imageHandlerUrl;

        public string ImageHandlerUrl
        {
            get
            {
                return _imageHandlerUrl ?? string.Empty;
            }
            set
            {
                _imageHandlerUrl = value;
            }
        }

        public string Timestamp
        {
            get
            {
                return _timestamp ?? string.Empty;
            }
            set
            {
                _timestamp = value;
            }
        }

        public List<ImageParameter> Parameters { get; }

        private new HttpContextBase Context => _context ?? new HttpContextWrapper(HttpContext.Current);

        private new Control BindingContainer => _bindingContainer ?? base.BindingContainer;

        public GeneratedImage()
        {
            Parameters = new List<ImageParameter>();
        }

        internal GeneratedImage(HttpContextBase context, Control bindingContainer) : this()
        {
            _context = context;
            _bindingContainer = bindingContainer;
        }

        protected override void OnDataBinding(EventArgs e)
        {
            base.OnDataBinding(e);

            Control bindingContainer = BindingContainer;
            foreach (var parameter in Parameters)
            {
                parameter.BindingContainer = bindingContainer;
                parameter.DataBind();
            }
        }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            if (DesignMode)
            {
                return;
            }

            ImageUrl = BuildImageUrl();
        }

        private string BuildImageUrl()
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.Append(ImageHandlerUrl);

            var paramAlreadyAdded = false;

            foreach (var parameter in Parameters)
            {
                AddQueryStringParameter(stringBuilder, paramAlreadyAdded, parameter.Name, parameter.Value);
                paramAlreadyAdded = true;
            }

            string timeStamp = Timestamp?.Trim();
            if (!string.IsNullOrEmpty(timeStamp))
            {
                AddQueryStringParameter(stringBuilder, paramAlreadyAdded, TimestampField, timeStamp);
            }

            return stringBuilder.ToString();
        }

        private static void AddQueryStringParameter(StringBuilder stringBuilder, bool paramAlreadyAdded, string name, string value)
        {
            stringBuilder.Append(paramAlreadyAdded ? '&' : '?');
            stringBuilder.Append(HttpUtility.UrlEncode(name));
            stringBuilder.Append('=');
            stringBuilder.Append(HttpUtility.UrlEncode(value));
        }
    }
}
