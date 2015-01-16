using System;
using System.Collections.Specialized;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using ClientDependency.Core.Controls;

namespace ClientDependency.Core.FileRegistration.Providers
{
    /// <summary>
    /// Uses PlaceHolder controls to render the CSS and JavaScript
    /// </summary>
    public class PlaceHolderProvider : LoaderControlProvider
    {

        public PlaceHolderProvider()
        {
            JavaScriptPlaceHolderId = "JavaScriptPlaceHolder";
            CssPlaceHolderId = "CssPlaceHolder";
        }


        public new const string DefaultName = "PlaceHolderProvider";

        public string JavaScriptPlaceHolderId { get; set; }
        public string CssPlaceHolderId { get; set; }

        public override void Initialize(string name, NameValueCollection config)
        {
            // Assign the provider a default name if it doesn't have one
            if (string.IsNullOrEmpty(name))
                name = DefaultName;

            base.Initialize(name, config);

            //for some stupid reason r# says this will never be null, but it certainly can be!
            if (config == null) return;

            if (config["javascriptPlaceHolderId"] != null)
            {
                JavaScriptPlaceHolderId = config["javascriptPlaceHolderId"];
            }
            if (config["cssPlaceHolderId"] != null)
            {
                CssPlaceHolderId = config["cssPlaceHolderId"];
            }
        }

        /// <summary>
        /// Registers the dependencies as controls of the placeholder controls specified
        /// </summary>
        /// <param name="http"></param>
        /// <param name="js"></param>
        /// <param name="css"></param>
        /// <remarks>
        /// For some reason ampersands that aren't html escaped are not compliant to HTML standards when they exist in 'link' or 'script' tags in URLs,
        /// we need to replace the ampersands with &amp; . This is only required for this one w3c compliancy, the URL itself is a valid URL.
        /// </remarks>
        protected override void RegisterDependencies(HttpContextBase http, string js, string css)
        {
            var jsPlaceholder = ClientDependencyLoader.GetInstance(http).Page.FlattenChildren()
                .FirstOrDefault(x => x.ID == JavaScriptPlaceHolderId);
            if (jsPlaceholder == null || (!(jsPlaceholder is PlaceHolder)))
            {
                throw new NullReferenceException("Could not find the placeholder control to render the JavaScript:" + JavaScriptPlaceHolderId);
            }
            AddToControl(http, js.Replace("&", "&amp;"), jsPlaceholder);

            var cssPlaceholder = ClientDependencyLoader.GetInstance(http).Page.FlattenChildren()
                .FirstOrDefault(x => x.ID == CssPlaceHolderId);
            if (cssPlaceholder == null || (!(cssPlaceholder is PlaceHolder)))
            {
                throw new NullReferenceException("Could not find the placeholder control to render the CSS:" + CssPlaceHolderId);
            }
            AddToControl(http, css.Replace("&", "&amp;"), cssPlaceholder);
        }

        private static void AddToControl(HttpContextBase http, string literal, Control parent)
        {
            var dCtl = new LiteralControl(literal);
            parent.Controls.Add(dCtl);
        }


    }
}