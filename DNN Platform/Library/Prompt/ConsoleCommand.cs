// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
using DotNetNuke.Abstractions.Portals;
using DotNetNuke.Abstractions.Prompt;
using DotNetNuke.Abstractions.Users;
using DotNetNuke.Collections;
using DotNetNuke.Services.Localization;
using System.Collections.Generic;
using System.Reflection;

namespace DotNetNuke.Prompt
{
    public abstract class ConsoleCommand : IConsoleCommand
    {
        public abstract string LocalResourceFile { get; }
        protected IPortalSettings PortalSettings { get; private set; }
        protected IUserInfo User { get; private set; }
        protected int PortalId { get; private set; }
        protected int TabId { get; private set; }
        protected string[] Args { get; private set; }
        protected IDictionary<string, string> Flags { get; private set; }

        #region Protected Methods
        protected string LocalizeString(string key)
        {
            var localizedText = Localization.GetString(key, LocalResourceFile);
            return string.IsNullOrEmpty(localizedText) ? key : localizedText;
        }
        protected void AddMessage(string message)
        {
            ValidationMessage += message;
        }
        protected void ParseParameters<T>(T myCommand) where T : class, new()
        {
            //LoadMapping();
            var mpg = CreateMapping();
            mpg.ForEach(mapping =>
            {
                var attribute = mapping.Attribute;
                var property = mapping.Property;
                var settingValue = Flags.ContainsKey(attribute.Name) ? Flags[attribute.Name] : null;
                if (settingValue != null && property.CanWrite)
                {
                    var tp = property.PropertyType;
                    Entities.Modules.Settings.SerializationController.DeserializeProperty(myCommand, property, settingValue);
                }
            });
        }
        #endregion

        #region Public Methods
        public virtual void Initialize(string[] args, IPortalSettings portalSettings, IUserInfo userInfo, int activeTabId)
        {
            Args = args;
            PortalSettings = portalSettings;
            User = userInfo;
            PortalId = portalSettings.PortalId;
            TabId = activeTabId;
            ValidationMessage = "";
            ParseFlags();
        }

        public abstract IConsoleResultModel Run();

        public virtual bool IsValid()
        {
            return string.IsNullOrEmpty(ValidationMessage);
        }
        #endregion

        #region Private Methods
        private void ParseFlags()
        {
            Flags = new Dictionary<string, string>();
            // loop through arguments, skipping the first one (the command)
            for (var i = 1; i <= Args.Length - 1; i++)
            {
                if (!Args[i].StartsWith("--")) continue;
                // found a flag
                var flagName = NormalizeFlagName(Args[i]);
                var flagValue = string.Empty;
                if (i < Args.Length - 1)
                {
                    if (!string.IsNullOrEmpty(Args[i + 1]))
                    {
                        if (Args[i + 1].StartsWith("--"))
                        {
                            // next value is another flag, so this flag has no value
                            flagValue = string.Empty;
                        }
                        else
                        {
                            flagValue = Args[i + 1];
                        }
                    }
                    else
                    {
                        flagValue = string.Empty;
                    }
                }
                Flags.Add(flagName.ToLower(), flagValue);
            }
        }
        #endregion

        #region Helper Methods
        private static string NormalizeFlagName(string flagName)
        {
            if (flagName == null)
                return string.Empty;
            if (flagName.StartsWith("--"))
                flagName = flagName.Substring(2);
            return flagName.ToLower().Trim();
        }
        #endregion

        public string ValidationMessage { get; private set; }

        /// <summary>
        /// Resource key for the result html.
        /// </summary>
        public virtual string ResultHtml => LocalizeString($"Prompt_{GetType().Name}_ResultHtml");

        #region Mapping Properties
        public struct ParameterMapping
        {
            public ConsoleCommandParameterAttribute Attribute { get; set; }
            public PropertyInfo Property { get; set; }
        }

        protected virtual IList<ParameterMapping> CreateMapping()
        {
            var mapping = new List<ParameterMapping>();
            GetType().GetProperties().ForEach(property =>
            {
                var attributes = property.GetCustomAttributes<ConsoleCommandParameterAttribute>(true);
                attributes.ForEach(attribute => mapping.Add(new ParameterMapping() { Attribute = attribute, Property = property }));
            });
            return mapping;
        }
        #endregion
    }
}
