using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Web.UI;
using ClientDependency.Core.Controls;
using ClientDependency.Core.Config;
using System.Web;

namespace ClientDependency.Core.FileRegistration.Providers
{
    /// <summary>
    /// Uses the LoaderControl to render the CSS and JS specified.
    /// </summary>
    public class LoaderControlProvider : WebFormsFileRegistrationProvider
    {

        public const string DefaultName = "LoaderControlProvider";

        public override void Initialize(string name, System.Collections.Specialized.NameValueCollection config)
        {
            // Assign the provider a default name if it doesn't have one
            if (string.IsNullOrEmpty(name))
                name = DefaultName;

            base.Initialize(name, config);
        }        

        protected override string RenderSingleJsFile(string js, IDictionary<string, string> htmlAttributes)
        {
            return string.Format(HtmlEmbedContants.ScriptEmbedWithSource, js, htmlAttributes.ToHtmlAttributes());
        }

        protected override string RenderSingleCssFile(string css, IDictionary<string, string> htmlAttributes)
        {
            return string.Format(HtmlEmbedContants.CssEmbedWithSource, css, htmlAttributes.ToHtmlAttributes());
        }

        /// <summary>
        /// Registers the dependencies as controls of the LoaderControl controls collection
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
            AddToControl(http, css.Replace("&", "&amp;"));
            AddToControl(http, js.Replace("&", "&amp;"));
        }

        private static void AddToControl(HttpContextBase http, string literal)
        {
            var dCtl = new LiteralControl(literal);
            ClientDependencyLoader.GetInstance(http).Controls.Add(dCtl);
        }

       
    }
}
