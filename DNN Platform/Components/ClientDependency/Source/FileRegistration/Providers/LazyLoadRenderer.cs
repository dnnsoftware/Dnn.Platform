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
        private string GetWebResourceUrl(Type type, string resourceId)
        {
            if (type == null)
                type = GetType();

            var page = new Page();
            return page.ClientScript.GetWebResourceUrl(type, resourceId);
        }

        /// <summary>
        /// This will check if the lazy loader script has been registered yet (it does this by storing a flah in the HttpContext.Items)
        /// if it hasn't then it will add the script registration to the StringBuilder
        /// </summary>
        /// <param name="sb"></param>
        /// <param name="http"></param>
        private void RegisterLazyLoadScript(StringBuilder sb, HttpContextBase http)
        {
            if (http.Items["LazyLoaderLoaded"] == null || (bool)http.Items["LazyLoaderLoaded"] == false)
            {
                lock (Locker)
                {
                    if (http.Items["LazyLoaderLoaded"] == null || (bool)http.Items["LazyLoaderLoaded"] == false)
                    {
                        http.Items["LazyLoaderLoaded"] = true;

                        var url = GetWebResourceUrl(typeof(LazyLoadProvider), DependencyLoaderResourceName);
                        sb.Append(string.Format(HtmlEmbedContants.ScriptEmbedWithSource, url, ""));   
                    }
                }
            }
        }

        protected override string RenderJsDependencies(IEnumerable<IClientDependencyFile> jsDependencies, HttpContextBase http, IDictionary<string, string> htmlAttributes)
        {
            if (!jsDependencies.Any())
				return string.Empty;

            var sb = new StringBuilder();

            if (http.IsDebuggingEnabled || !EnableCompositeFiles)
			{
				foreach (var dependency in jsDependencies)
				{
                    sb.Append(RenderSingleJsFile(string.Format("'{0}','{1}'", dependency.FilePath, string.Empty), htmlAttributes));
				}
			}
			else
			{

                RegisterLazyLoadScript(sb, http);

                var comp = ClientDependencySettings.Instance.DefaultCompositeFileProcessingProvider.ProcessCompositeList(jsDependencies, ClientDependencyType.Javascript, http);
                foreach (var s in comp)
                {
                    sb.Append(RenderSingleJsFile(string.Format("'{0}','{1}'", s, string.Empty), htmlAttributes));
                }   
			}

            return sb.ToString();
        }

        /// <summary>
        /// Registers the Css dependencies. 
        /// </summary>
        /// <param name="cssDependencies"></param>
        /// <param name="http"></param>
        /// <param name="htmlAttributes"></param>
        /// <returns></returns>
        protected override string RenderCssDependencies(IEnumerable<IClientDependencyFile> cssDependencies, HttpContextBase http, IDictionary<string, string> htmlAttributes)
        {
            if (!cssDependencies.Any())
                return string.Empty;

            var sb = new StringBuilder();

            if (http.IsDebuggingEnabled || !EnableCompositeFiles)
            {
                foreach (var dependency in cssDependencies)
                {
                    sb.Append(RenderSingleCssFile(dependency.FilePath, htmlAttributes));
                }
            }
            else
            {

                RegisterLazyLoadScript(sb, http);

                var comp = ClientDependencySettings.Instance.DefaultCompositeFileProcessingProvider.ProcessCompositeList(cssDependencies, ClientDependencyType.Css, http);
                foreach (var s in comp)
                {
                    sb.Append(RenderSingleCssFile(s, htmlAttributes));
                }
            }

            return sb.ToString();
        }

        protected override string RenderSingleJsFile(string js, IDictionary<string, string> htmlAttributes)
        {
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
