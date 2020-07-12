// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.DDRMenu
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using System.Xml.Serialization;

    using DotNetNuke.Web.DDRMenu.TemplateEngine;

    public class Settings
    {
        private List<ClientOption> clientOptions;

        private List<TemplateArgument> templateArguments;

        public string MenuStyle { get; set; }

        public string NodeXmlPath { get; set; }

        public string NodeSelector { get; set; }

        public string IncludeNodes { get; set; }

        public string ExcludeNodes { get; set; }

        public string NodeManipulator { get; set; }

        public bool IncludeContext { get; set; }

        public bool IncludeHidden { get; set; }

        public List<ClientOption> ClientOptions
        {
            get { return this.clientOptions ?? (this.clientOptions = new List<ClientOption>()); }
            set { this.clientOptions = value; }
        }

        public List<TemplateArgument> TemplateArguments
        {
            get { return this.templateArguments ?? (this.templateArguments = new List<TemplateArgument>()); }
            set { this.templateArguments = value; }
        }

        public static Settings FromXml(string xml)
        {
            var ser = new XmlSerializer(typeof(Settings));
            using (var reader = new StringReader(xml))
            {
                return (Settings)ser.Deserialize(reader);
            }
        }

        public static List<ClientOption> ClientOptionsFromSettingString(string s)
        {
            var result = new List<ClientOption>();
            if (!string.IsNullOrEmpty(s))
            {
                foreach (var clientOption in SplitIntoStrings(s))
                {
                    var n = clientOption.IndexOf('=');
                    result.Add(new ClientOption(clientOption.Substring(0, n), clientOption.Substring(n + 1)));
                }
            }

            return result;
        }

        public static List<TemplateArgument> TemplateArgumentsFromSettingString(string s)
        {
            var result = new List<TemplateArgument>();
            if (!string.IsNullOrEmpty(s))
            {
                foreach (var templateArgument in SplitIntoStrings(s))
                {
                    var n = templateArgument.IndexOf('=');
                    result.Add(new TemplateArgument(templateArgument.Substring(0, n), templateArgument.Substring(n + 1)));
                }
            }

            return result;
        }

        public static string ToSettingString(List<ClientOption> clientOptions)
        {
            return string.Join("\r\n", clientOptions.ConvertAll(o => o.Name + "=" + o.Value).ToArray());
        }

        public static string ToSettingString(List<TemplateArgument> templateArguments)
        {
            return string.Join("\r\n", templateArguments.ConvertAll(o => o.Name + "=" + o.Value).ToArray());
        }

        public string ToXml()
        {
            var sb = new StringBuilder();
            var ser = new XmlSerializer(typeof(Settings));
            using (var writer = new StringWriter(sb))
            {
                ser.Serialize(writer, this);
            }

            return sb.ToString();
        }

        public override string ToString()
        {
            try
            {
                return this.ToXml();
            }
            catch (Exception exc)
            {
                return exc.ToString();
            }
        }

        private static IEnumerable<string> SplitIntoStrings(string fullString)
        {
            var strings = new List<string>(fullString.Split('\r', '\n'));
            strings.RemoveAll(string.IsNullOrEmpty);
            return strings;
        }
    }
}
