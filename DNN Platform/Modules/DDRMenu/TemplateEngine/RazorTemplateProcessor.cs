using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Text;
using System.Web.UI;

using DotNetNuke.Web.DDRMenu.DNNCommon;
using DotNetNuke.Web.Razor;

namespace DotNetNuke.Web.DDRMenu.TemplateEngine
{
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
            if (!(string.IsNullOrEmpty(liveDefinition.TemplateVirtualPath)))
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

                var razorEngine = new RazorEngine(liveDefinition.TemplateVirtualPath, null, null);
                var writer = new StringWriter();
                razorEngine.Render<dynamic>(writer, model);

                htmlWriter.Write(writer.ToString());
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
    }
}