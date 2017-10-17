using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ClientDependency.Core.FileRegistration.Providers;
using ClientDependency.Core.Config;
using System.Web.UI;
using System.Web;

namespace ClientDependency.Core.FileRegistration.Providers
{
    public class LazyLoadRenderer : BaseRenderer
    {
        public LazyLoadRenderer()
        {
            PlaceholderParser.PlaceholderReplaced += PlaceholderParserPlaceholderReplaced;
            PlaceholderParser.AllPlaceholdersReplaced += PlaceholdersReplaced;
        }

        /// <summary>
        /// This is used to ensure we render the lazy load script once before either the lazy js or css is rendered. 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <remarks>
        /// See comments in PlaceholdersReplaced for more details
        /// </remarks>
        static void PlaceholderParserPlaceholderReplaced(object sender, PlaceholderReplacementEventArgs e)
        {
            //if the replacement was for this renderer
            if (e.RegexMatch.Groups.Count > 1 && e.RegexMatch.Groups[1].ToString() == DefaultName)
            {
                //we will pre-pend a special token above this output so we can detect it in the PlaceholdersReplaced event
                // and we'll tag the current http context with a key so we can detect that this reques is for us
                e.HttpContext.Items[PlaceholderReplacementProcessing] = true;
                e.ReplacedText = LazyLoadScriptPlaceholder + e.ReplacedText;
            }   
        }

        /// <summary>
        /// This allows us to ensure that the lazy load script is placed higher in the rendered html than either the css or the js rendered to the page.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <remarks>
        /// TO accomplish this we detect if the http context has a special key (PlaceholderReplacementProcessing), if so we'll replace the first encountered
        /// token with the lazy load script and replace any other ones with empty text (so it is only loaded once). We need to do this because we have no 
        /// idea if the developer has rendered the lazy css or lazy js first.
        /// </remarks>
        static void PlaceholdersReplaced(object sender, PlaceholdersReplacedEventArgs e)
        {

            if (e.HttpContext.Items[PlaceholderReplacementProcessing] is bool 
                && (bool)e.HttpContext.Items[PlaceholderReplacementProcessing])
            {
                var url = GetWebResourceUrl(typeof(LazyLoadProvider), DependencyLoaderResourceName);
                var lazyScriptTag = string.Format(HtmlEmbedContants.ScriptEmbedWithSource, url, "");

                //replace the first occurance
                e.ReplacedText = e.ReplacedText.ReplaceFirst(LazyLoadScriptPlaceholder, lazyScriptTag);
                //replace the rest with nothing
                e.ReplacedText = e.ReplacedText.Replace(LazyLoadScriptPlaceholder, "");

                //set the flag to null so we don't process again
                e.HttpContext.Items[PlaceholderReplacementProcessing] = null;
            }
        }

        private const string LazyLoadScriptPlaceholder = "<!--[Javascript:Name=\"LazyLoadRenderer_LazyLoadScript\"]//-->";
        private const string PlaceholderReplacementProcessing = "LazyLoadRenderer_Processing";

        public const string DefaultName = "LazyLoadRenderer";

        public override void Initialize(string name, System.Collections.Specialized.NameValueCollection config)
        {
            // Assign the provider a default name if it doesn't have one
            if (string.IsNullOrEmpty(name))
                name = DefaultName;

            base.Initialize(name, config);
        }

        /// <summary>Path to the dependency loader we need for adding control dependencies.</summary>
        protected const string DependencyLoaderResourceName = "ClientDependency.Core.Resources.LazyLoader.js";
        
        private static readonly object Locker = new object();
        
        /// <summary>
        /// This is silly to have to do this but MS don't give you a way in MVC to do this
        /// </summary>
        /// <param name="type"></param>
        /// <param name="resourceId"></param>
        /// <returns></returns>
        private static string GetWebResourceUrl(Type type, string resourceId)
        {
            if (type == null) throw new ArgumentNullException("type");
            var page = new Page();
            return page.ClientScript.GetWebResourceUrl(type, resourceId);
        }

        /// <summary>
        /// Renders many Js dependencies.
        /// </summary>
        /// <param name="jsDependencies"></param>
        /// <param name="http"></param>
        /// <param name="htmlAttributes"></param>
        /// <returns></returns>
        protected override string RenderJsDependencies(IEnumerable<IClientDependencyFile> jsDependencies, HttpContextBase http, IDictionary<string, string> htmlAttributes)
        {
            var asArray = jsDependencies.ToArray();

            if (!asArray.Any())
				return string.Empty;

            var sb = new StringBuilder();   
            var strClientLoader = new StringBuilder();


            if (http.IsDebuggingEnabled || !EnableCompositeFiles)
			{
                foreach (var dependency in asArray)
				{
                    strClientLoader.Append("CDLazyLoader");
                    strClientLoader.AppendFormat(".AddJs('{0}')", dependency.FilePath);
                    strClientLoader.AppendLine(";");
				}
			}
            else if (DisableCompositeBundling)
            {
                foreach (var dependency in asArray)
                {
                    RenderJsComposites(http, htmlAttributes, strClientLoader, Enumerable.Repeat(dependency, 1));
                }
            }
            else
			{
                RenderJsComposites(http, htmlAttributes, strClientLoader, asArray);
            }

            sb.Append(string.Format(HtmlEmbedContants.ScriptEmbedWithCode, strClientLoader));

            return sb.ToString();
        }

        protected override void RenderJsComposites(HttpContextBase http, IDictionary<string, string> htmlAttributes, StringBuilder sb, IEnumerable<IClientDependencyFile> dependencies)
        {
            var comp = ClientDependencySettings.Instance.DefaultCompositeFileProcessingProvider.ProcessCompositeList(
                    dependencies,
                    ClientDependencyType.Javascript, 
                    http,
                    ClientDependencySettings.Instance.CompositeFileHandlerPath);

            foreach (var s in comp)
            {
                sb.Append("CDLazyLoader");
                sb.AppendFormat(".AddJs('{0}')", s);
                sb.AppendLine(";");
            }
        }

        
        /// <summary>
        /// Renders many Css dependencies. 
        /// </summary>
        /// <param name="cssDependencies"></param>
        /// <param name="http"></param>
        /// <param name="htmlAttributes"></param>
        /// <returns></returns>
        protected override string RenderCssDependencies(IEnumerable<IClientDependencyFile> cssDependencies, HttpContextBase http, IDictionary<string, string> htmlAttributes)
        {
            var asArray = cssDependencies.ToArray();

            if (!asArray.Any())
                return string.Empty;

            var sb = new StringBuilder();
            var strClientLoader = new StringBuilder();

            if (http.IsDebuggingEnabled || !EnableCompositeFiles)
            {
                foreach (var dependency in asArray)
                {
                    strClientLoader.Append("CDLazyLoader");
                    strClientLoader.AppendFormat(".AddCss('{0}')", dependency.FilePath);
                    strClientLoader.AppendLine(";");
                }
            }
            else if (DisableCompositeBundling)
            {
                foreach (var dependency in asArray)
                {
                    RenderCssComposites(http, htmlAttributes, strClientLoader, Enumerable.Repeat(dependency, 1));
                }
            }
            else
            {
                RenderCssComposites(http, htmlAttributes, strClientLoader, asArray);
            }

            sb.Append(string.Format(HtmlEmbedContants.ScriptEmbedWithCode, strClientLoader));

            return sb.ToString();
        }

        protected override void RenderCssComposites(HttpContextBase http, IDictionary<string, string> htmlAttributes, StringBuilder sb, IEnumerable<IClientDependencyFile> dependencies)
        {
            var comp = ClientDependencySettings.Instance.DefaultCompositeFileProcessingProvider.ProcessCompositeList(
                    dependencies,
                    ClientDependencyType.Css,
                    http,
                    ClientDependencySettings.Instance.CompositeFileHandlerPath);

            foreach (var s in comp)
            {
                sb.Append("CDLazyLoader");
                sb.AppendFormat(".AddCss('{0}')", s);
                sb.AppendLine(";");
            }
        }

        protected override string RenderSingleJsFile(string js, IDictionary<string, string> htmlAttributes)
        {
            if (!js.StartsWith("'"))
                js = string.Format("'{0}'", js);

            var strClientLoader = new StringBuilder("CDLazyLoader");
            strClientLoader.AppendFormat(".AddJs({0})", js);
            strClientLoader.Append(';');

            return string.Format(HtmlEmbedContants.ScriptEmbedWithCode, strClientLoader.ToString());
        }

        protected override string RenderSingleCssFile(string css, IDictionary<string, string> htmlAttributes)
        {
            var strClientLoader = new StringBuilder("CDLazyLoader");
            strClientLoader.AppendFormat(".AddCss('{0}')", css);
            strClientLoader.Append(';');

            return string.Format(HtmlEmbedContants.ScriptEmbedWithCode, strClientLoader);
        }
    }
}
