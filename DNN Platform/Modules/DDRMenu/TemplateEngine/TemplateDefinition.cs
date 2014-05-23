using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Caching;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Xml;
using DotNetNuke.Common;
using DotNetNuke.Web.DDRMenu.DNNCommon;

namespace DotNetNuke.Web.DDRMenu.TemplateEngine
{
	public class TemplateDefinition
	{
		internal string Folder;
		internal string TemplatePath;
		internal string TemplateVirtualPath;
		internal string TemplateHeadPath;
		internal readonly List<string> ScriptKeys = new List<string>();
		internal readonly Dictionary<string, string> Scripts = new Dictionary<string, string>();
		internal readonly List<string> StyleSheets = new List<string>();
		internal readonly List<ClientOption> DefaultClientOptions = new List<ClientOption>();
		internal readonly List<TemplateArgument> DefaultTemplateArguments = new List<TemplateArgument>();
		internal ITemplateProcessor Processor;

		public List<ClientOption> ClientOptions = new List<ClientOption>();
		public List<TemplateArgument> TemplateArguments = new List<TemplateArgument>();

		private readonly Regex regexLinks =
			new Regex(
				"( (href|src)=['\"]?)(?!http:|ftp:|mailto:|file:|javascript:|/)([^'\">]+['\">])",
				RegexOptions.Singleline | RegexOptions.IgnoreCase | RegexOptions.Compiled);

		internal static TemplateDefinition FromName(string templateName, string manifestName)
		{
			var manifestUrl = new PathResolver(null).Resolve(
				templateName + "/" + manifestName,
				PathResolver.RelativeTo.Container,
				PathResolver.RelativeTo.Skin,
				PathResolver.RelativeTo.Portal,
				PathResolver.RelativeTo.Module,
				PathResolver.RelativeTo.Dnn);
			return FromManifest(manifestUrl);
		}

		internal static TemplateDefinition FromManifest(string manifestUrl)
		{
			var httpContext = HttpContext.Current;
			var cache = httpContext.Cache;
			var manifestPath = httpContext.Server.MapPath(manifestUrl);

			var baseDef = cache[manifestPath] as TemplateDefinition;
			if (baseDef == null)
			{
				baseDef = new TemplateDefinition {Folder = Path.GetDirectoryName(manifestUrl)};

				var xml = new XmlDocument();
				xml.Load(manifestPath);

				var resolver = new PathResolver(baseDef.Folder);

				// ReSharper disable PossibleNullReferenceException
				foreach (XmlNode node in xml.DocumentElement.ChildNodes)
					// ReSharper restore PossibleNullReferenceException
				{
					if (node.NodeType == XmlNodeType.Element)
					{
						var elt = (XmlElement)node;
						switch (elt.LocalName)
						{
							case "template":
								baseDef.TemplateVirtualPath = GetResolvedPath(elt, resolver);
								baseDef.TemplatePath = httpContext.Server.MapPath(baseDef.TemplateVirtualPath);
								break;
							case "templateHead":
								baseDef.TemplateHeadPath = httpContext.Server.MapPath(GetResolvedPath(elt, resolver));
								break;
							case "scripts":
								foreach (XmlElement scriptElt in elt.GetElementsByTagName("script"))
								{
									var jsObject = scriptElt.GetAttribute("jsObject");
									var scriptPath = String.IsNullOrEmpty(scriptElt.InnerText.Trim())
									                 	? ""
									                 	: Globals.ResolveUrl(GetResolvedPath(scriptElt, resolver));
									var key = String.IsNullOrEmpty(jsObject) ? scriptPath : jsObject;
									var script = CreateScript(jsObject, scriptPath);
									if (!String.IsNullOrEmpty(script))
									{
										baseDef.ScriptKeys.Add(key);
										baseDef.Scripts.Add(key, script);
									}
								}
								break;
							case "stylesheets":
								foreach (XmlElement cssElt in elt.GetElementsByTagName("stylesheet"))
								{
									var cssPath = Globals.ResolveUrl(GetResolvedPath(cssElt, resolver));
									baseDef.StyleSheets.Add(cssPath);
								}
								break;
							case "defaultClientOptions":
								foreach (XmlElement optionElt in elt.GetElementsByTagName("clientOption"))
								{
									var optionName = optionElt.GetAttribute("name");
									var optionType = optionElt.GetAttribute("type");
									var optionValue = optionElt.GetAttribute("value");
									if (String.IsNullOrEmpty(optionType))
									{
										optionType = "passthrough";
									}
									switch (optionType)
									{
										case "number":
											baseDef.DefaultClientOptions.Add(new ClientNumber(optionName, optionValue));
											break;
										case "boolean":
											baseDef.DefaultClientOptions.Add(new ClientBoolean(optionName, optionValue));
											break;
										case "string":
											baseDef.DefaultClientOptions.Add(new ClientString(optionName, optionValue));
											break;
										default:
											baseDef.DefaultClientOptions.Add(new ClientOption(optionName, optionValue));
											break;
									}
								}
								break;
							case "defaultTemplateArguments":
								foreach (XmlElement argElt in elt.GetElementsByTagName("templateArgument"))
								{
									var argName = argElt.GetAttribute("name");
									var argValue = argElt.GetAttribute("value");
									baseDef.DefaultTemplateArguments.Add(new TemplateArgument(argName, argValue));
								}
								break;
						}
					}
				}

				foreach (var processor in DNNAbstract.SupportedTemplateProcessors())
				{
					if (processor.LoadDefinition(baseDef))
					{
						baseDef.Processor = processor;
						break;
					}
				}

				if (baseDef.Processor == null)
				{
					throw new ApplicationException(String.Format("Can't find processor for manifest {0}", manifestPath));
				}

				cache.Insert(manifestPath, baseDef, new CacheDependency(new[] {manifestPath, baseDef.TemplatePath}));
			}

			var result = baseDef.Clone();
			result.Reset();
			return result;
		}

		private static string GetResolvedPath(XmlNode scriptElt, PathResolver pathResolver)
		{
			return pathResolver.Resolve(
				scriptElt.InnerText.Trim(),
				PathResolver.RelativeTo.Manifest,
				PathResolver.RelativeTo.Skin,
				PathResolver.RelativeTo.Module,
				PathResolver.RelativeTo.Portal,
				PathResolver.RelativeTo.Dnn);
		}

		private static string CreateScript(string jsObject, string scriptPath)
		{
			string result;

			jsObject = jsObject ?? "";

			if (String.IsNullOrEmpty(scriptPath))
			{
				var scheme = HttpContext.Current.Request.Url.Scheme;
				switch (jsObject)
				{
					case "jQuery":
					case "DDRjQuery":
						scriptPath = DNNAbstract.RequestJQuery()
						             	? ""
						             	: (scheme + "://ajax.googleapis.com/ajax/libs/jquery/1.4.2/jquery.min.js");
						break;
					case "jQuery.ui":
						scriptPath = scheme + "://ajax.googleapis.com/ajax/libs/jqueryui/1.8.1/jquery-ui.min.js";
						break;
					default:
						throw new ApplicationException(String.Format("Can't deduce script path for JavaScript object '{0}'", jsObject));
				}
			}

			if (string.IsNullOrEmpty(jsObject))
			{
				result = String.IsNullOrEmpty(scriptPath)
				         	? ""
				         	: String.Format(@"<script type=""text/javascript"" src=""{0}""></script>", scriptPath);
			}
			else
			{
				if (jsObject == "DDRjQuery")
				{
					result = String.IsNullOrEmpty(scriptPath)
					         	? @"<script type=""text/javascript"">DDRjQuery=window.DDRjQuery||jQuery;</script>"
					         	: String.Format(
					         		@"<script type=""text/javascript"">if (!window.DDRjQuery) {{if (window.jQuery && (jQuery.fn.jquery>=""1.3"")) DDRjQuery=jQuery; else document.write(unescape('%3Cscript src=""{0}"" type=""text/javascript""%3E%3C/script%3E'));}}</script><script type=""text/javascript"">if (!window.DDRjQuery) DDRjQuery=jQuery.noConflict(true);</script>",
					         		scriptPath);
				}
				else
				{
					result = String.IsNullOrEmpty(scriptPath)
					         	? ""
					         	: String.Format(
					         		@"<script type=""text/javascript"">if (!({0})) document.write(unescape('%3Cscript src=""{1}"" type=""text/javascript""%3E%3C/script%3E'));</script>",
					         		GetObjectCheckScript(jsObject),
					         		scriptPath);
				}
			}

			return result;
		}

		private static string GetObjectCheckScript(string jsObject)
		{
			var objectParts = jsObject.Split('.');
			var objectToCheck = new StringBuilder("window");
			var objectsToCheck = new List<String>();
			foreach (var part in objectParts)
			{
				objectToCheck.AppendFormat(".{0}", part);
				objectsToCheck.Add(objectToCheck.ToString());
			}
			return String.Join(" && ", objectsToCheck.ToArray());
		}

		public TemplateDefinition Clone()
		{
			return (TemplateDefinition)MemberwiseClone();
		}

		public void Reset()
		{
			ClientOptions = new List<ClientOption>(DefaultClientOptions);
			TemplateArguments = new List<TemplateArgument>(DefaultTemplateArguments);
		}

		public void AddClientOptions(List<ClientOption> options, bool replace)
		{
			if (options != null)
			{
				foreach (var option in options)
				{
					var option1 = option;
					if (replace)
					{
						ClientOptions.RemoveAll(o => o.Name == option1.Name);
					}
					if (!ClientOptions.Exists(o => o.Name == option1.Name))
					{
						ClientOptions.Add(option);
					}
				}
			}
		}

		public void AddTemplateArguments(List<TemplateArgument> args, bool replace)
		{
			if (args != null)
			{
				foreach (var arg in args)
				{
					var arg1 = arg;
					if (replace)
					{
						TemplateArguments.RemoveAll(a => a.Name == arg1.Name);
					}
					if (!TemplateArguments.Exists(a => a.Name == arg1.Name))
					{
						TemplateArguments.Add(arg);
					}
				}
			}
		}

		internal void PreRender()
		{
			var page = DNNContext.Current.Page;
			var headControls = page.Header.Controls;

			var contextItems = HttpContext.Current.Items;
			foreach (var stylesheet in StyleSheets)
			{
				if (!contextItems.Contains(stylesheet))
				{
					var cssControl = new HtmlGenericControl("link");
					cssControl.Attributes.Add("rel", "stylesheet");
					cssControl.Attributes.Add("type", "text/css");
					cssControl.Attributes.Add("href", stylesheet);
					headControls.Add(cssControl);

					contextItems.Add(stylesheet, true);
				}
			}

			foreach (var scriptKey in ScriptKeys)
			{
				var clientScript = page.ClientScript;
				if (!clientScript.IsClientScriptBlockRegistered(typeof(TemplateDefinition), scriptKey))
				{
					clientScript.RegisterClientScriptBlock(typeof(TemplateDefinition), scriptKey, Scripts[scriptKey], false);
				}
			}

			var headContent = String.IsNullOrEmpty(TemplateHeadPath) ? "" : Utilities.CachedFileContent(TemplateHeadPath);
			var expandedHead = regexLinks.Replace(headContent, "$1" + DNNContext.Current.ActiveTab.SkinPath + "$3");
			page.Header.Controls.Add(new LiteralControl(expandedHead));
		}

		internal void Render(object source, HtmlTextWriter htmlWriter)
		{
			Processor.Render(source, htmlWriter, this);
		}
	}
}