// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.DDRMenu.TemplateEngine
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Web;
    using System.Web.UI;
    using System.Xml;
    using System.Xml.Xsl;

    using DotNetNuke.Web.DDRMenu.DNNCommon;

    public class TokenTemplateProcessor : ITemplateProcessor
    {
        private static readonly Dictionary<string, string> aliases = new Dictionary<string, string>
                                                                     { { "page", "node" }, { "name", "text" } };

        private static readonly Regex TemplatesRegex =
                new Regex(
                    @"(\[(?<directive>(\*|\*\>|\/\*|\>|\/\>|\?|\?!|\/\?|\=))(?<nodename>[A-Z]*)(-(?<modename>[0-9A-Z]*))?\])",
                    RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private XslCompiledTransform xsl;

        public bool LoadDefinition(TemplateDefinition baseDefinition)
        {
            if (!baseDefinition.TemplateVirtualPath.EndsWith(".txt", StringComparison.InvariantCultureIgnoreCase))
            {
                return false;
            }

            var templateText = Utilities.CachedFileContent(baseDefinition.TemplatePath);
            var xml = new XmlDocument { XmlResolver = null };
            const string xmlNs = "http://www.w3.org/1999/XSL/Transform";
            xml.LoadXml(
                @"
				<xsl:stylesheet version='1.0' xmlns:xsl='" + xmlNs + @"' xmlns:ddr='urn:ddrmenu'>
					<xsl:output method='html'/>
					<xsl:template match='/*'>
						<xsl:apply-templates select='root' />
					</xsl:template>
					<xsl:template match='root'>
					</xsl:template>
				</xsl:stylesheet>");

            var validParams = baseDefinition.DefaultTemplateArguments.ConvertAll(a => a.Name.ToLowerInvariant());
            validParams.AddRange(new[] { "controlid", "options", "dnnpath", "manifestpath", "portalpath", "skinpath" });

            var docElt = xml.DocumentElement;
            var outputElt = (XmlElement)docElt.GetElementsByTagName("output", xmlNs)[0];
            foreach (var param in validParams)
            {
                var elt = xml.CreateElement("param", xmlNs);
                elt.SetAttribute("name", param);
                docElt.InsertAfter(elt, outputElt);
            }

            var current = (XmlElement)docElt.GetElementsByTagName("template", xmlNs)[1];
            var stack = new Stack<XmlElement>();

            var index = 0;
            foreach (Match match in TemplatesRegex.Matches(templateText))
            {
                current.AppendChild(xml.CreateTextNode(templateText.Substring(index, match.Index - index)));

                var directive = match.Groups["directive"].Value;
                var nodeName = match.Groups["nodename"].Value.ToLowerInvariant();
                var modeName = match.Groups["modename"].Value.ToLowerInvariant();

                string alias;
                if (aliases.TryGetValue(nodeName, out alias))
                {
                    nodeName = alias;
                }

                if (directive == "=")
                {
                    var elt = xml.CreateElement("value-of", xmlNs);
                    if (validParams.Contains(nodeName))
                    {
                        elt.SetAttribute("select", "ddr:HtmlEncode($" + nodeName + ")");
                    }
                    else
                    {
                        elt.SetAttribute("select", "ddr:HtmlEncode(concat(" + nodeName + ", @" + nodeName + "))");
                    }

                    current.AppendChild(elt);
                }
                else if (directive == "*")
                {
                    var elt = xml.CreateElement("for-each", xmlNs);
                    elt.SetAttribute("select", nodeName);
                    current.AppendChild(elt);
                    stack.Push(current);
                    current = elt;
                }
                else if (directive == "*>")
                {
                    var elt = xml.CreateElement("apply-templates", xmlNs);
                    elt.SetAttribute("select", nodeName);
                    elt.SetAttribute("mode", "M" + modeName);
                    current.AppendChild(elt);
                }
                else if (directive == ">")
                {
                    var elt = xml.CreateElement("template", xmlNs);
                    elt.SetAttribute("match", nodeName);
                    elt.SetAttribute("mode", "M" + modeName);
                    xml.DocumentElement.AppendChild(elt);
                    stack.Push(current);
                    current = elt;
                }
                else if (directive[0] == '?')
                {
                    XmlElement elt;
                    if (nodeName != "else")
                    {
                        elt = xml.CreateElement("when", xmlNs);
                        var test = string.Format("{0} or (@{0}=1) or (@{0}!=0 and @{0}!=1 and @{0}!='')", nodeName);
                        if (directive == "?!")
                        {
                            test = string.Format("not({0})", test);
                        }

                        elt.SetAttribute("test", test);

                        var choose = xml.CreateElement("choose", xmlNs);
                        current.AppendChild(choose);
                        choose.AppendChild(elt);
                        stack.Push(current);
                    }
                    else
                    {
                        elt = xml.CreateElement("otherwise", xmlNs);
                        current.ParentNode.AppendChild(elt);
                    }

                    current = elt;
                }
                else if (directive[0] == '/')
                {
                    current = stack.Pop();
                }

                index = match.Index + match.Length;
            }

            current.AppendChild(xml.CreateTextNode(templateText.Substring(index)));

            this.xsl = new XslCompiledTransform();
            this.xsl.Load(xml);
            return true;
        }

        public void Render(object source, HtmlTextWriter htmlWriter, TemplateDefinition liveDefinition)
        {
            var resolver = new PathResolver(liveDefinition.Folder);
            var args = new XsltArgumentList();
            args.AddExtensionObject("urn:ddrmenu", new XsltFunctions());
            args.AddExtensionObject("urn:dnngarden", new XsltFunctions());
            args.AddParam("controlid", string.Empty, DNNContext.Current.HostControl.ClientID);
            args.AddParam("options", string.Empty, ConvertToJson(liveDefinition.ClientOptions));
            args.AddParam("dnnpath", string.Empty, resolver.Resolve("/", PathResolver.RelativeTo.Dnn));
            args.AddParam("manifestpath", string.Empty, resolver.Resolve("/", PathResolver.RelativeTo.Manifest));
            args.AddParam("portalpath", string.Empty, resolver.Resolve("/", PathResolver.RelativeTo.Portal));
            args.AddParam("skinpath", string.Empty, resolver.Resolve("/", PathResolver.RelativeTo.Skin));
            liveDefinition.TemplateArguments.ForEach(a => args.AddParam(a.Name.ToLowerInvariant(), string.Empty, a.Value));

            var sb = new StringBuilder();

            using (var xmlStream = new MemoryStream())
            using (var outputWriter = new StringWriter(sb))
            {
                Utilities.SerialiserFor(source.GetType()).Serialize(xmlStream, source);
                xmlStream.Seek(0, SeekOrigin.Begin);
                this.xsl.Transform(XmlReader.Create(xmlStream), args, outputWriter);
            }

            htmlWriter.Write(HttpUtility.HtmlDecode(sb.ToString()));
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
