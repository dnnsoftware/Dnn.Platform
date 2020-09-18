// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.DDRMenu.TemplateEngine
{
    using System;
    using System.Collections.Generic;
    using System.Dynamic;
    using System.IO;
    using System.Text;
    using System.Web;
    using System.Web.UI;
    using System.Web.WebPages;

    using DotNetNuke.Web.DDRMenu.DNNCommon;
    using DotNetNuke.Web.Razor;

    public class RazorTemplateProcessor : ITemplateProcessor
    {
        public bool LoadDefinition(TemplateDefinition baseDefinition)
        {
            var virtualPath = baseDefinition.TemplateVirtualPath;

            if (!virtualPath.EndsWith(".cshtml", StringComparison.InvariantCultureIgnoreCase) &&
                !virtualPath.EndsWith(".vbhtml", StringComparison.InvariantCultureIgnoreCase))
            {
                return false;
            }

            return true;
        }

        public void Render(object source, HtmlTextWriter htmlWriter, TemplateDefinition liveDefinition)
        {
            if (!string.IsNullOrEmpty(liveDefinition.TemplateVirtualPath))
            {
                var resolver = new PathResolver(liveDefinition.Folder);
                dynamic model = new ExpandoObject();
                model.Source = source;
                model.ControlID = DNNContext.Current.HostControl.ClientID;
                model.Options = ConvertToJson(liveDefinition.ClientOptions);
                model.DNNPath = resolver.Resolve("/", PathResolver.RelativeTo.Dnn);
                model.ManifestPath = resolver.Resolve("/", PathResolver.RelativeTo.Manifest);
                model.PortalPath = resolver.Resolve("/", PathResolver.RelativeTo.Portal);
                model.SkinPath = resolver.Resolve("/", PathResolver.RelativeTo.Skin);
                var modelDictionary = model as IDictionary<string, object>;
                liveDefinition.TemplateArguments.ForEach(a => modelDictionary.Add(a.Name, a.Value));
                htmlWriter.Write(this.RenderTemplate(liveDefinition.TemplateVirtualPath, model));
            }
        }

        protected static string ConvertToJson(List<ClientOption> options)
        {
            var result = new StringBuilder();
            result.Append("{");

            if (options != null)
            {
                foreach (var option in options)
                {
                    if (option is ClientNumber)
                    {
                        result.AppendFormat("{0}:{1},", option.Name, Utilities.ConvertToJs(Convert.ToDecimal(option.Value)));
                    }
                    else if (option is ClientString)
                    {
                        result.AppendFormat("{0}:{1},", option.Name, Utilities.ConvertToJs(option.Value));
                    }
                    else if (option is ClientBoolean)
                    {
                        result.AppendFormat(
                            "{0}:{1},", option.Name, Utilities.ConvertToJs(Convert.ToBoolean(option.Value.ToLowerInvariant())));
                    }
                    else
                    {
                        result.AppendFormat("{0}:{1},", option.Name, option.Value);
                    }
                }

                if (options.Count > 0)
                {
                    result.Remove(result.Length - 1, 1);
                }
            }

            result.Append("}");
            return result.ToString();
        }

        private StringWriter RenderTemplate(string virtualPath, dynamic model)
        {
            var page = WebPageBase.CreateInstanceFromVirtualPath(virtualPath);
            var httpContext = new HttpContextWrapper(HttpContext.Current);
            var pageContext = new WebPageContext(httpContext, page, model);

            var writer = new StringWriter();

            if (page is WebPage)
            {
                page.ExecutePageHierarchy(pageContext, writer);
            }
            else
            {
                var razorEngine = new RazorEngine(virtualPath, null, null);
                razorEngine.Render<dynamic>(writer, model);
            }

            return writer;
        }
    }
}
