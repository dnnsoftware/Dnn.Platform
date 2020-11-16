// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.EditBar.UI.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Web.Caching;
    using System.Xml;
    using System.Xml.XPath;

    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Framework;
    using DotNetNuke.Services.Cache;

    internal class LocalizationController : ServiceLocator<ILocalizationController, LocalizationController>, ILocalizationController
    {
        public static readonly TimeSpan FiveMinutes = TimeSpan.FromMinutes(5);
        public static readonly TimeSpan OneHour = TimeSpan.FromHours(1);

        public string CultureName
        {
            get { return Thread.CurrentThread.CurrentUICulture.Name; }
        }

        public Dictionary<string, string> GetLocalizedDictionary(string resourceFile, string culture)
        {
            Requires.NotNullOrEmpty("resourceFile", resourceFile);
            Requires.NotNullOrEmpty("culture", culture);

            var dictionary = new Dictionary<string, string>();
            foreach (var kvp in GetLocalizationValues(resourceFile, culture).Where(kvp => !dictionary.ContainsKey(kvp.Key)))
            {
                dictionary[kvp.Key] = kvp.Value;
            }

            return dictionary;
        }

        protected override Func<ILocalizationController> GetFactory()
        {
            return () => new LocalizationController();
        }

        private static string GetNameAttribute(XmlNode node)
        {
            if (node.Attributes != null)
            {
                var attribute = node.Attributes.GetNamedItem("name");
                if (attribute != null)
                {
                    return attribute.Value;
                }
            }

            return null;
        }

        private static string GetNameAttribute(XPathNavigator navigator)
        {
            return navigator.GetAttribute("name", string.Empty);
        }

        private static void AssertHeaderValue(IEnumerable<XmlNode> headers, string key, string value)
        {
            var header = headers.FirstOrDefault(x => GetNameAttribute(x).Equals(key, StringComparison.InvariantCultureIgnoreCase));
            if (header != null)
            {
                if (!header.InnerText.Equals(value, StringComparison.InvariantCultureIgnoreCase))
                {
                    throw new ApplicationException(string.Format("Resource header '{0}' != '{1}'", key, value));
                }
            }
            else
            {
                throw new ApplicationException(string.Format("Resource header '{0}' is missing", key));
            }
        }

        private static IEnumerable<KeyValuePair<string, string>> GetLocalizationValues(string fullPath, string culture)
        {
            using (var stream = new FileStream(System.Web.HttpContext.Current.Server.MapPath(fullPath), FileMode.Open, FileAccess.Read))
            {
                var document = new XmlDocument();
                document.Load(stream);

                // ReSharper disable AssignNullToNotNullAttribute
                var headers = document.SelectNodes(@"/root/resheader").Cast<XmlNode>().ToArray();

                // ReSharper restore AssignNullToNotNullAttribute
                AssertHeaderValue(headers, "resmimetype", "text/microsoft-resx");

                // ReSharper disable AssignNullToNotNullAttribute
                foreach (XPathNavigator navigator in document.CreateNavigator().Select("/root/data"))

                // ReSharper restore AssignNullToNotNullAttribute
                {
                    if (navigator.NodeType == XPathNodeType.Comment)
                    {
                        continue;
                    }

                    var name = GetNameAttribute(navigator);

                    const string textPostFix = ".Text";
                    if (name.EndsWith(textPostFix))
                    {
                        name = name.Substring(0, name.Length - textPostFix.Length);
                    }

                    if (string.IsNullOrEmpty(name))
                    {
                        continue;
                    }

                    var valueNode = navigator.SelectSingleNode("value");
                    if (valueNode != null)
                    {
                        yield return new KeyValuePair<string, string>(name, valueNode.Value);
                    }
                }
            }
        }
    }
}
