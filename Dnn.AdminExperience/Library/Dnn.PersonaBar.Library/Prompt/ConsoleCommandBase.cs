// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Library.Prompt
{
    using System;
    using System.Collections;
    using System.ComponentModel;

    using Dnn.PersonaBar.Library.Prompt.Models;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Services.Localization;

    public abstract class ConsoleCommandBase : IConsoleCommand
    {
        public abstract string LocalResourceFile { get; }

        /// <summary>
        /// Gets resource key for the result html.
        /// </summary>
        public virtual string ResultHtml => this.LocalizeString($"Prompt_{this.GetType().Name}_ResultHtml");

        public string ValidationMessage { get; private set; }

        protected PortalSettings PortalSettings { get; private set; }

        protected UserInfo User { get; private set; }

        protected int PortalId { get; private set; }

        protected int TabId { get; private set; }

        protected string[] Args { get; private set; }

        protected Hashtable Flags { get; private set; }

        /// <summary>
        /// Get the flag value.
        /// </summary>
        /// <typeparam name="T">Type of the output expected.</typeparam>
        /// <param name="flag">Flag name to look.</param>
        /// <param name="fieldName">Filed name to show in message.</param>
        /// <param name="defaultVal">Default value of the flag, if any.</param>
        /// <param name="required">Is this a required flag or not.</param>
        /// <param name="checkmain">Try to find the flag value in first args or not.</param>
        /// <param name="checkpositive">This would be applicable only if the output is of type int or double and value should be positive.</param>
        /// <returns></returns>
        public virtual T GetFlagValue<T>(string flag, string fieldName, T defaultVal, bool required = false,
            bool checkmain = false, bool checkpositive = false)
        {
            const string resourceFile = "~/DesktopModules/admin/Dnn.PersonaBar/App_LocalResources/SharedResources.resx";
            dynamic value = null;
            try
            {
                if (this.HasFlag(flag))
                {
                    if (this.IsBoolean<T>())
                    {
                        value = this.Flag<bool>(flag, true);
                    }
                    else
                    {
                        value = this.Flag<T>(flag, defaultVal);
                    }
                }
                else
                {
                    if (checkmain && this.Args.Length >= 2 && !this.IsFlag(this.Args[1]))
                    {
                        var tc = TypeDescriptor.GetConverter(typeof(T));
                        value = tc.ConvertFrom(this.Args[1]);
                    }
                    else if (!required)
                    {
                        value = defaultVal;
                    }
                    else
                    {
                        this.ValidationMessage += Localization.GetString(
                            checkmain ? "Promp_MainFlagIsRequired" : "Prompt_FlagIsRequired",
                            resourceFile)?.Replace("[0]", fieldName).Replace("[1]", flag);
                    }
                }
            }
            catch (Exception)
            {
                this.ValidationMessage +=
                    Localization.GetString("Prompt_InvalidType", resourceFile)?
                        .Replace("[0]", fieldName)
                        .Replace("[1]", GetTypeName(typeof(T)));
            }

            if (checkpositive &&
                (typeof(T) == typeof(int) || typeof(T) == typeof(long) || typeof(T) == typeof(int?) ||
                 typeof(T) == typeof(long?)) && value != null && Convert.ToInt32(value) <= 0)
            {
                this.ValidationMessage += Localization.GetString("Promp_PositiveValueRequired", resourceFile)?.Replace("[0]", fieldName);
            }

            return value ?? defaultVal;
        }

        public virtual void Init(string[] args, PortalSettings portalSettings, UserInfo userInfo, int activeTabId)
        {
        }

        public void Initialize(string[] args, PortalSettings portalSettings, UserInfo userInfo, int activeTabId)
        {
            this.Args = args;
            this.PortalSettings = portalSettings;
            this.User = userInfo;
            this.PortalId = portalSettings.PortalId;
            this.TabId = activeTabId;
            this.ValidationMessage = string.Empty;
            this.ParseFlags();
            this.Init(args, portalSettings, userInfo, activeTabId);
        }

        public abstract ConsoleResultModel Run();

        public virtual bool IsValid()
        {
            return string.IsNullOrEmpty(this.ValidationMessage);
        }

        protected bool HasFlag(string flagName)
        {
            flagName = NormalizeFlagName(flagName);
            return this.Flags.ContainsKey(flagName);
        }

        protected bool IsFlag(object input)
        {
            var inputVal = Convert.ToString(input);
            return !string.IsNullOrEmpty(inputVal) && inputVal.StartsWith("--");
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

        private static string GetTypeName(Type type)
        {
            if (type.FullName.ToLowerInvariant().Contains("int"))
            {
                return "Integer";
            }

            if (type.FullName.ToLowerInvariant().Contains("double"))
            {
                return "Double";
            }

            if (type.FullName.ToLowerInvariant().Contains("bool"))
            {
                return "Boolean";
            }

            if (type.FullName.ToLowerInvariant().Contains("datetime"))
            {
                return "DateTime";
            }

            return string.Empty;
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
            this.Flags = new Hashtable();

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

        private object Flag<T>(string flagName, T defValue)
        {
            flagName = NormalizeFlagName(flagName);
            if (!this.Flags.ContainsKey(flagName))
            {
                return defValue;
            }

            var retVal = this.Flags[flagName];
            if (retVal == null || (string)retVal == string.Empty)
            {
                return defValue;
            }

            var tc = TypeDescriptor.GetConverter(typeof(T));
            return tc.ConvertFrom(retVal);
        }

        private bool IsBoolean<T>()
        {
            return typeof(T) == typeof(bool?);
        }
    }
}
