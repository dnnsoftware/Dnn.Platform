// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.Client.Providers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Web;
    using System.Web.UI;
    using System.Web.UI.HtmlControls;

    using ClientDependency.Core;
    using ClientDependency.Core.Config;
    using ClientDependency.Core.FileRegistration.Providers;

    /// <summary>
    /// Registers resources at the top of the body on default.aspx.
    /// </summary>
    public class DnnBodyProvider : DnnFileRegistrationProvider
    {
        /// <summary>
        /// The name of the provider.
        /// </summary>
        public const string DefaultName = "DnnBodyProvider";

        /// <summary>
        /// The name of the placeholder in which the controls will be rendered.
        /// </summary>
        public const string DnnBodyPlaceHolderName = "BodySCRIPTS";

        /// <summary>
        /// Initializes the provider.
        /// </summary>
        /// <param name="name">The friendly name of the provider.
        ///                 </param><param name="config">A collection of the name/value pairs representing the provider-specific attributes specified in the configuration for this provider.
        ///                 </param><exception cref="T:System.ArgumentNullException">The name of the provider is null.
        ///                 </exception><exception cref="T:System.ArgumentException">The name of the provider has a length of zero.
        ///                 </exception><exception cref="T:System.InvalidOperationException">An attempt is made to call <see cref="M:System.Configuration.Provider.ProviderBase.Initialize(System.String,System.Collections.Specialized.NameValueCollection)"/> on a provider after the provider has already been initialized.
        ///                 </exception>
        public override void Initialize(string name, System.Collections.Specialized.NameValueCollection config)
        {
            // Assign the provider a default name if it doesn't have one
            if (string.IsNullOrEmpty(name))
            {
                name = DefaultName;
            }

            base.Initialize(name, config);
        }

        protected override string RenderJsDependencies(IEnumerable<IClientDependencyFile> jsDependencies, HttpContextBase http, IDictionary<string, string> htmlAttributes)
        {
            if (!jsDependencies.Any())
            {
                return string.Empty;
            }

            var sb = new StringBuilder();

            if (http.IsDebuggingEnabled || !this.EnableCompositeFiles)
            {
                foreach (var dependency in jsDependencies)
                {
                    sb.Append(this.RenderSingleJsFile(dependency.FilePath, htmlAttributes));
                }
            }
            else
            {
                var comp = ClientDependencySettings.Instance.DefaultCompositeFileProcessingProvider.ProcessCompositeList(jsDependencies, ClientDependencyType.Javascript, http);
                foreach (var s in comp)
                {
                    sb.Append(this.RenderSingleJsFile(s, htmlAttributes));
                }
            }

            return sb.ToString();
        }

        protected override string RenderSingleJsFile(string js, IDictionary<string, string> htmlAttributes)
        {
            return string.Format(HtmlEmbedContants.ScriptEmbedWithSource, js, htmlAttributes.ToHtmlAttributes());
        }

        protected override string RenderCssDependencies(IEnumerable<IClientDependencyFile> cssDependencies, HttpContextBase http, IDictionary<string, string> htmlAttributes)
        {
            if (!cssDependencies.Any())
            {
                return string.Empty;
            }

            var sb = new StringBuilder();

            if (http.IsDebuggingEnabled || !this.EnableCompositeFiles)
            {
                foreach (var dependency in cssDependencies)
                {
                    sb.Append(this.RenderSingleCssFile(dependency.FilePath, htmlAttributes));
                }
            }
            else
            {
                var comp = ClientDependencySettings.Instance.DefaultCompositeFileProcessingProvider.ProcessCompositeList(cssDependencies, ClientDependencyType.Css, http);
                foreach (var s in comp)
                {
                    sb.Append(this.RenderSingleCssFile(s, htmlAttributes));
                }
            }

            return sb.ToString();
        }

        protected override string RenderSingleCssFile(string css, IDictionary<string, string> htmlAttributes)
        {
            return string.Format(HtmlEmbedContants.CssEmbedWithSource, css, htmlAttributes.ToHtmlAttributes());
        }

        /// <summary>
        /// Registers the dependencies in the body of default.aspx.
        /// </summary>
        /// <param name="http"></param>
        /// <param name="js"></param>
        /// <param name="css"></param>
        /// <remarks>
        /// For some reason ampersands that aren't html escaped are not compliant to HTML standards when they exist in 'link' or 'script' tags in URLs,
        /// we need to replace the ampersands with &amp; . This is only required for this one w3c compliancy, the URL itself is a valid URL.
        ///
        /// </remarks>
        protected override void RegisterDependencies(HttpContextBase http, string js, string css)
        {
            if (!(http.CurrentHandler is Page))
            {
                throw new InvalidOperationException("The current HttpHandler in a WebFormsFileRegistrationProvider must be of type Page");
            }

            var page = (Page)http.CurrentHandler;

            if (page.Header == null)
            {
                throw new NullReferenceException("DnnBodyProvider requires a runat='server' tag in the page's header tag");
            }

            var jsScriptBlock = new LiteralControl(js.Replace("&", "&amp;"));
            var cssStyleBlock = new LiteralControl(css.Replace("&", "&amp;"));

            var holderControl = page.FindControl(DnnBodyPlaceHolderName);
            holderControl.Controls.Add(jsScriptBlock);
            holderControl.Controls.Add(cssStyleBlock);

            var form = (HtmlForm)page.FindControl("Form");
            if (form != null)
            {
                form.Controls.Remove(holderControl);
                form.Controls.AddAt(0, holderControl);
            }

            var scriptManager = ScriptManager.GetCurrent(page);
            if (scriptManager != null && scriptManager.IsInAsyncPostBack)
            {
                holderControl.ID = "$crm_" + holderControl.ID;
                scriptManager.RegisterDataItem(holderControl, string.Format("{0}{1}", jsScriptBlock.Text, cssStyleBlock.Text));
            }
        }
    }
}
