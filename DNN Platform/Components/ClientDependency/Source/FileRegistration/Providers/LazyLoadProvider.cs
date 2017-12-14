using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using ClientDependency.Core.Config;

namespace ClientDependency.Core.FileRegistration.Providers
{
    public class LazyLoadProvider : WebFormsFileRegistrationProvider
	{

		public const string DefaultName = "LazyLoadProvider";

		public override void Initialize(string name, System.Collections.Specialized.NameValueCollection config)
		{
			// Assign the provider a default name if it doesn't have one
			if (string.IsNullOrEmpty(name))
				name = DefaultName;

			base.Initialize(name, config);
		}

		/// <summary>Path to the dependency loader we need for adding control dependencies.</summary>
        protected const string DependencyLoaderResourceName = "ClientDependency.Core.Resources.LazyLoader.js";

        protected override string RenderJsDependencies(IEnumerable<IClientDependencyFile> jsDependencies, HttpContextBase http, IDictionary<string, string> htmlAttributes)
        {
            var asArray = jsDependencies.ToArray();

            if (!asArray.Any())
				return string.Empty;

            var sb = new StringBuilder();

            if (http.IsDebuggingEnabled || !EnableCompositeFiles)
			{
                foreach (var dependency in asArray)
				{
                    sb.Append(RenderSingleJsFile(string.Format("'{0}','{1}'", dependency.FilePath, string.Empty), htmlAttributes));
				}
			}
            else if (DisableCompositeBundling)
            {
                foreach (var dependency in asArray)
                {
                    RenderJsComposites(http, htmlAttributes, sb, Enumerable.Repeat(dependency, 1));
                }
            }
            else
            {
                RenderJsComposites(http, htmlAttributes, sb, asArray);
            }

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
                sb.Append(RenderSingleJsFile(string.Format("'{0}','{1}'", s, string.Empty), htmlAttributes));
            }
        }

	    protected override string RenderSingleJsFile(string js, IDictionary<string, string> htmlAttributes)
		{
            if(!js.StartsWith("'"))
                js = string.Format("'{0}'", js);

            var strClientLoader = new StringBuilder("CDLazyLoader");
			strClientLoader.AppendFormat(".AddJs({0})", js);
			strClientLoader.Append(';');
            return strClientLoader.ToString();
		}

        protected override string RenderCssDependencies(IEnumerable<IClientDependencyFile> cssDependencies, HttpContextBase http, IDictionary<string, string> htmlAttributes)
        {
            var asArray = cssDependencies.ToArray();

            if (!asArray.Any())
                return string.Empty;

            var sb = new StringBuilder();

            if (http.IsDebuggingEnabled || !EnableCompositeFiles)
			{
                foreach (var dependency in asArray)
				{
                    sb.Append(RenderSingleCssFile(dependency.FilePath, htmlAttributes));
				}
			}
            else if (DisableCompositeBundling)
            {
                foreach (var dependency in asArray)
                {
                    RenderCssComposites(http, htmlAttributes, sb, Enumerable.Repeat(dependency, 1));
                }
            }
            else
			{
                RenderCssComposites(http, htmlAttributes, sb, asArray);
			}

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
                sb.Append(RenderSingleCssFile(s, htmlAttributes));
            }
        }

	    protected override string RenderSingleCssFile(string css, IDictionary<string, string> htmlAttributes)
		{
            var strClientLoader = new StringBuilder("CDLazyLoader");
			strClientLoader.AppendFormat(".AddCss('{0}')", css);
			strClientLoader.Append(';');
            return strClientLoader.ToString();
		}

        protected override void RegisterDependencies(HttpContextBase http, string js, string css)
        {
            if (!(http.CurrentHandler is Page))
            {
                throw new InvalidOperationException("The current HttpHandler in a WebFormsFileRegistrationProvider must be of type Page");
            }
            var page = (Page)http.CurrentHandler;

            page.ClientScript.RegisterClientScriptResource(typeof(LazyLoadProvider), DependencyLoaderResourceName);

            RegisterScript(js, page);
            RegisterScript(css, page);
        }		

        private void RegisterScript(string strScript, Page page)
		{
            var mgr = ScriptManager.GetCurrent(page);

			if (mgr == null)
			{
                if (page.Form == null)
                    throw new InvalidOperationException("A form tag must be present on the page with a runat='server' attribute specified");
                page.ClientScript.RegisterStartupScript(GetType(), strScript.GetHashCode().ToString(), strScript, true);
			}
			else
			{
                ScriptManager.RegisterStartupScript(page, GetType(), strScript.GetHashCode().ToString(), strScript, true);
			}
		}

	}

}
