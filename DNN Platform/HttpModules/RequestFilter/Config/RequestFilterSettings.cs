// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.HttpModules.RequestFilter
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Xml;
    using System.Xml.Serialization;
    using System.Xml.XPath;

    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Host;
    using DotNetNuke.Services.Cache;

    [Serializable]
    [XmlRoot("RewriterConfig")]
    public class RequestFilterSettings
    {
        private const string RequestFilterConfig = "RequestFilter.Config";

        private List<RequestFilterRule> _rules = new List<RequestFilterRule>();

        public bool Enabled
        {
            get
            {
                return Host.EnableRequestFilters;
            }
        }

        public List<RequestFilterRule> Rules
        {
            get
            {
                return this._rules;
            }

            set
            {
                this._rules = value;
            }
        }

        /// <summary>
        /// Get the current settings from the xml config file.
        /// </summary>
        /// <returns></returns>
        public static RequestFilterSettings GetSettings()
        {
            var settings = (RequestFilterSettings)DataCache.GetCache(RequestFilterConfig);
            if (settings == null)
            {
                settings = new RequestFilterSettings();
                string filePath = Common.Utilities.Config.GetPathToFile(Common.Utilities.Config.ConfigFileType.DotNetNuke);

                // Create a FileStream for the Config file
                using (var fileReader = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    var doc = new XPathDocument(fileReader);
                    XPathNodeIterator ruleList = doc.CreateNavigator().Select("/configuration/blockrequests/rule");
                    while (ruleList.MoveNext())
                    {
                        try
                        {
                            string serverVar = ruleList.Current.GetAttribute("servervar", string.Empty);
                            string values = ruleList.Current.GetAttribute("values", string.Empty);
                            var ac = (RequestFilterRuleType)Enum.Parse(typeof(RequestFilterRuleType), ruleList.Current.GetAttribute("action", string.Empty));
                            var op = (RequestFilterOperatorType)Enum.Parse(typeof(RequestFilterOperatorType), ruleList.Current.GetAttribute("operator", string.Empty));
                            string location = ruleList.Current.GetAttribute("location", string.Empty);
                            var rule = new RequestFilterRule(serverVar, values, op, ac, location);
                            settings.Rules.Add(rule);
                        }
                        catch (Exception ex)
                        {
                            DotNetNuke.Services.Exceptions.Exceptions.LogException(new Exception(string.Format("Unable to read RequestFilter Rule: {0}:", ruleList.Current.OuterXml), ex));
                        }
                    }
                }

                if (File.Exists(filePath))
                {
                    // Set back into Cache
                    DataCache.SetCache(RequestFilterConfig, settings, new DNNCacheDependency(filePath));
                }
            }

            return settings;
        }

        public static void Save(List<RequestFilterRule> rules)
        {
            string filePath = Common.Utilities.Config.GetPathToFile(Common.Utilities.Config.ConfigFileType.DotNetNuke);
            if (!File.Exists(filePath))
            {
                string defaultConfigFile = Globals.ApplicationMapPath + Globals.glbConfigFolder + Globals.glbDotNetNukeConfig;
                if (File.Exists(defaultConfigFile))
                {
                    File.Copy(defaultConfigFile, filePath, true);
                }
            }

            var doc = new XmlDocument { XmlResolver = null };
            doc.Load(filePath);
            XmlNode ruleRoot = doc.SelectSingleNode("/configuration/blockrequests");
            ruleRoot.RemoveAll();
            foreach (RequestFilterRule rule in rules)
            {
                XmlElement xmlRule = doc.CreateElement("rule");
                XmlAttribute var = doc.CreateAttribute("servervar");
                var.Value = rule.ServerVariable;
                xmlRule.Attributes.Append(var);
                XmlAttribute val = doc.CreateAttribute("values");
                val.Value = rule.RawValue;
                xmlRule.Attributes.Append(val);
                XmlAttribute op = doc.CreateAttribute("operator");
                op.Value = rule.Operator.ToString();
                xmlRule.Attributes.Append(op);
                XmlAttribute action = doc.CreateAttribute("action");
                action.Value = rule.Action.ToString();
                xmlRule.Attributes.Append(action);
                XmlAttribute location = doc.CreateAttribute("location");
                location.Value = rule.Location;
                xmlRule.Attributes.Append(location);
                ruleRoot.AppendChild(xmlRule);
            }

            var settings = new XmlWriterSettings();
            settings.Indent = true;
            using (XmlWriter writer = XmlWriter.Create(filePath, settings))
            {
                doc.WriteContentTo(writer);
            }
        }
    }
}
