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

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Services.Cache;

    using Microsoft.Extensions.DependencyInjection;

    /// <summary>Settings related to request filters.</summary>
    [Serializable]
    [XmlRoot("RewriterConfig")]
    public class RequestFilterSettings
    {
        private const string RequestFilterConfig = "RequestFilter.Config";
        private readonly IHostSettings hostSettings;

        /// <summary>Initializes a new instance of the <see cref="RequestFilterSettings"/> class.</summary>
        [Obsolete(
            "Deprecated in DotNetNuke 10.0.2. Please use overload with IHostSettings. Scheduled removal in v12.0.0.")]
        public RequestFilterSettings()
            : this(null)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="RequestFilterSettings"/> class.</summary>
        /// <param name="hostSettings">The host settings.</param>
        public RequestFilterSettings(IHostSettings hostSettings)
        {
            this.hostSettings = hostSettings ?? Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettings>();
        }

        /// <summary>Gets a value indicating whether request filters are enabled.</summary>
        public bool Enabled => this.hostSettings.EnableRequestFilters;

        /// <summary>Gets or sets the collection of rules.</summary>
        public List<RequestFilterRule> Rules { get; set; } = new();

        /// <summary>Get the current settings from the XML config file.</summary>
        /// <returns>A <see cref="RequestFilterSettings"/> instance.</returns>
        [Obsolete("Deprecated in DotNetNuke 10.0.2. Please use overload with IHostSettings. Scheduled removal in v12.0.0.")]
        public static RequestFilterSettings GetSettings()
            => GetSettings(Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettings>(), Globals.GetCurrentServiceProvider().GetRequiredService<IApplicationStatusInfo>());

        /// <summary>Get the current settings from the XML config file.</summary>
        /// <param name="hostSettings">The host settings.</param>
        /// <param name="appStatus">The application status.</param>
        /// <returns>A <see cref="RequestFilterSettings"/> instance.</returns>
        public static RequestFilterSettings GetSettings(IHostSettings hostSettings, IApplicationStatusInfo appStatus)
        {
            var settings = (RequestFilterSettings)DataCache.GetCache(RequestFilterConfig);
            if (settings == null)
            {
                settings = new RequestFilterSettings(hostSettings);
                string filePath = Config.GetPathToFile(appStatus, Config.ConfigFileType.DotNetNuke);

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

        /// <summary>Saves the rules into the <c>DotNetNuke.config</c> file.</summary>
        /// <param name="rules">The rules.</param>
        [Obsolete("Deprecated in DotNetNuke 10.0.2. Please use overload with IApplicationStatusInfo. Scheduled removal in v12.0.0.")]
        public static void Save(List<RequestFilterRule> rules)
            => Save(Globals.GetCurrentServiceProvider().GetRequiredService<IApplicationStatusInfo>(), rules);

        /// <summary>Saves the rules into the <c>DotNetNuke.config</c> file.</summary>
        /// <param name="appStatus">The application status.</param>
        /// <param name="rules">The rules.</param>
        public static void Save(IApplicationStatusInfo appStatus, List<RequestFilterRule> rules)
        {
            string filePath = Config.GetPathToFile(appStatus, Config.ConfigFileType.DotNetNuke);
            if (!File.Exists(filePath))
            {
                string defaultConfigFile = appStatus.ApplicationMapPath + Globals.glbConfigFolder + Globals.glbDotNetNukeConfig;
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
