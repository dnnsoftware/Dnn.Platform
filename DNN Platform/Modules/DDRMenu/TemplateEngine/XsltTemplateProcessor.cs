// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.DDRMenu.TemplateEngine
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using System.Web;
    using System.Web.UI;
    using System.Xml;
    using System.Xml.Xsl;

    using DotNetNuke.Web.DDRMenu.DNNCommon;

    public class XsltTemplateProcessor : ITemplateProcessor
    {
        private XslCompiledTransform xsl;

        public bool LoadDefinition(TemplateDefinition baseDefinition)
        {
            try
            {
                var virtualPath = baseDefinition.TemplateVirtualPath;
                if (!virtualPath.EndsWith(".xsl", StringComparison.InvariantCultureIgnoreCase) &&
                    !virtualPath.EndsWith(".xslt", StringComparison.InvariantCultureIgnoreCase))
                {
                    return false;
                }

                this.xsl = Utilities.CachedXslt(baseDefinition.TemplatePath);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public void Render(object source, HtmlTextWriter htmlWriter, TemplateDefinition liveDefinition)
        {
            var resolver = new PathResolver(liveDefinition.Folder);
            var hostPage = DNNContext.Current.Page;

            var args = new XsltArgumentList();
            args.AddExtensionObject("urn:ddrmenu", new XsltFunctions());
            args.AddExtensionObject("urn:dnngarden", new XsltFunctions());
            args.AddParam("ControlID", string.Empty, DNNContext.Current.HostControl.ClientID);
            args.AddParam("Options", string.Empty, ConvertToJson(liveDefinition.ClientOptions));
            args.AddParam("DNNPath", string.Empty, hostPage.ResolveUrl(resolver.Resolve("/", PathResolver.RelativeTo.Dnn)));
            args.AddParam("ManifestPath", string.Empty, hostPage.ResolveUrl(resolver.Resolve("/", PathResolver.RelativeTo.Manifest)));
            args.AddParam("PortalPath", string.Empty, hostPage.ResolveUrl(resolver.Resolve("/", PathResolver.RelativeTo.Portal)));
            args.AddParam("SkinPath", string.Empty, hostPage.ResolveUrl(resolver.Resolve("/", PathResolver.RelativeTo.Skin)));
            liveDefinition.TemplateArguments.ForEach(a => args.AddParam(a.Name, string.Empty, a.Value));

            HttpContext.Current.Items["Resolver"] = resolver;

            using (var xmlStream = new MemoryStream())
            {
                Utilities.SerialiserFor(source.GetType()).Serialize(xmlStream, source);
                xmlStream.Seek(0, SeekOrigin.Begin);
                this.xsl.Transform(XmlReader.Create(xmlStream), args, htmlWriter);
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
