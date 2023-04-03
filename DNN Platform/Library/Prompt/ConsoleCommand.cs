// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Prompt
{
    using System.Collections.Generic;
    using System.Reflection;

    using DotNetNuke.Abstractions;
    using DotNetNuke.Abstractions.Portals;
    using DotNetNuke.Abstractions.Prompt;
    using DotNetNuke.Abstractions.Users;
    using DotNetNuke.Collections;
    using DotNetNuke.Services.Localization;
    using Microsoft.Extensions.DependencyInjection;

    public abstract class ConsoleCommand : IConsoleCommand
    {
        /// <inheritdoc/>
        public abstract string LocalResourceFile { get; }

        /// <inheritdoc/>
        public string ValidationMessage { get; private set; }

        /// <summary>Gets resource key for the result html.</summary>
        public virtual string ResultHtml => this.LocalizeString($"Prompt_{this.GetType().Name}_ResultHtml");

        protected IPortalSettings PortalSettings { get; private set; }

        protected IUserInfo User { get; private set; }

        protected int PortalId { get; private set; }

        protected int TabId { get; private set; }

        protected string[] Args { get; private set; }

        protected IDictionary<string, string> Flags { get; private set; }

        private static ISerializationManager SerializationManager =>
            Common.Globals.DependencyProvider.GetRequiredService<ISerializationManager>();

        /// <inheritdoc/>
        public virtual void Initialize(string[] args, IPortalSettings portalSettings, IUserInfo userInfo, int activeTabId)
        {
            this.Args = args;
            this.PortalSettings = portalSettings;
            this.User = userInfo;
            this.PortalId = portalSettings.PortalId;
            this.TabId = activeTabId;
            this.ValidationMessage = string.Empty;
            this.ParseFlags();
        }

        /// <inheritdoc/>
        public abstract IConsoleResultModel Run();

        /// <inheritdoc/>
        public virtual bool IsValid()
        {
            return string.IsNullOrEmpty(this.ValidationMessage);
        }

        protected string LocalizeString(string key)
        {
            var localizedText = Localization.GetString(key, this.LocalResourceFile);
            return string.IsNullOrEmpty(localizedText) ? key : localizedText;
        }

        protected void AddMessage(string message)
        {
            this.ValidationMessage += message;
        }

        protected void ParseParameters<T>(T myCommand)
            where T : class, new()
        {
            // LoadMapping();
            var mpg = this.CreateMapping();
            mpg.ForEach(mapping =>
            {
                var attribute = mapping.Attribute;
                var property = mapping.Property;
                var settingValue = this.Flags.ContainsKey(attribute.Name) ? this.Flags[attribute.Name] : null;
                if (settingValue != null && property.CanWrite)
                {
                    var tp = property.PropertyType;
                    SerializationManager.DeserializeProperty(myCommand, property, settingValue);
                }
            });
        }

        protected virtual IList<ParameterMapping> CreateMapping()
        {
            var mapping = new List<ParameterMapping>();
            this.GetType().GetProperties().ForEach(property =>
            {
                var attributes = property.GetCustomAttributes<ConsoleCommandParameterAttribute>(true);
                attributes.ForEach(attribute => mapping.Add(new ParameterMapping() { Attribute = attribute, Property = property }));
            });
            return mapping;
        }

        private static string NormalizeFlagName(string flagName)
        {
            if (flagName == null)
            {
                return string.Empty;
            }

            if (flagName.StartsWith("--"))
            {
                flagName = flagName.Substring(2);
            }

            return flagName.ToLower().Trim();
        }

        private void ParseFlags()
        {
            this.Flags = new Dictionary<string, string>();

            // loop through arguments, skipping the first one (the command)
            for (var i = 1; i <= this.Args.Length - 1; i++)
            {
                if (!this.Args[i].StartsWith("--"))
                {
                    continue;
                }

                // found a flag
                var flagName = NormalizeFlagName(this.Args[i]);
                var flagValue = string.Empty;
                if (i < this.Args.Length - 1)
                {
                    if (!string.IsNullOrEmpty(this.Args[i + 1]))
                    {
                        if (this.Args[i + 1].StartsWith("--"))
                        {
                            // next value is another flag, so this flag has no value
                            flagValue = string.Empty;
                        }
                        else
                        {
                            flagValue = this.Args[i + 1];
                        }
                    }
                    else
                    {
                        flagValue = string.Empty;
                    }
                }

                this.Flags.Add(flagName.ToLower(), flagValue);
            }
        }

        public struct ParameterMapping
        {
            public ConsoleCommandParameterAttribute Attribute { get; set; }

            public PropertyInfo Property { get; set; }
        }
    }
}
