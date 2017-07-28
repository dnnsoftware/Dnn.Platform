using System;
using System.Collections;
using System.ComponentModel;
using Dnn.PersonaBar.Library.Prompt.Models;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Services.Localization;

namespace Dnn.PersonaBar.Library.Prompt
{
    public abstract class ConsoleCommandBase : IConsoleCommand
    {
        public abstract string LocalResourceFile { get; }
        protected PortalSettings PortalSettings { get; private set; }
        protected UserInfo User { get; private set; }
        protected int PortalId { get; private set; }
        protected int TabId { get; private set; }
        protected string[] Args { get; private set; }
        protected Hashtable Flags { get; private set; }
        #region Protected Methods
        protected bool HasFlag(string flagName)
        {
            flagName = NormalizeFlagName(flagName);
            return Flags.ContainsKey(flagName);
        }

        protected bool IsFlag(object input)
        {
            var inputVal = Convert.ToString(input);
            return !string.IsNullOrEmpty(inputVal) && inputVal.StartsWith("--");
        }

        protected string LocalizeString(string key)
        {
            var localizedText = Localization.GetString(key, LocalResourceFile);
            return string.IsNullOrEmpty(localizedText) ? key : localizedText;
        }
        protected void AddMessage(string message)
        {
            ValidationMessage += message;
        }

        /// <summary>
        /// Get the flag value
        /// </summary>
        /// <typeparam name="T">Type of the output expected</typeparam>
        /// <param name="flag">Flag name to look</param>
        /// <param name="fieldName">Filed name to show in message</param>
        /// <param name="defaultVal">Default value of the flag, if any.</param>
        /// <param name="required">Is this a required flag or not.</param>
        /// <param name="checkmain">Try to find the flag value in first args or not.</param>
        /// <param name="checkpositive">This would be applicable only if the output is of type int or double and value should be positive.</param>
        /// <returns></returns>
        protected virtual T GetFlagValue<T>(string flag, string fieldName, T defaultVal, bool required = false,
            bool checkmain = false, bool checkpositive = false)
        {
            const string resourceFile = "~/DesktopModules/admin/Dnn.PersonaBar/App_LocalResources/SharedResources.resx";
            dynamic value = null;
            try
            {
                if (HasFlag(flag))
                {
                    value = Flag<T>(flag, defaultVal);
                }
                else
                {
                    if (checkmain && Args.Length >= 2 && !IsFlag(Args[1]))
                    {
                        var tc = TypeDescriptor.GetConverter(typeof(T));
                        value = tc.ConvertFrom(Args[1]);
                    }
                    else if (!required)
                    {
                        value = defaultVal;
                    }
                    else
                    {
                        ValidationMessage += Localization.GetString(
                            checkmain ? "Promp_MainFlagIsRequired" : "Prompt_FlagIsRequired",
                            resourceFile)?.Replace("[0]", fieldName).Replace("[1]", flag);
                    }
                }
            }
            catch (Exception)
            {
                ValidationMessage +=
                    Localization.GetString("Prompt_InvalidType", resourceFile)?
                        .Replace("[0]", fieldName)
                        .Replace("[1]", GetTypeName(typeof(T)));
            }
            if (checkpositive &&
                (typeof(T) == typeof(int) || typeof(T) == typeof(long) || typeof(T) == typeof(int?) ||
                 typeof(T) == typeof(long?)) && value != null && Convert.ToInt32(value) <= 0)
            {
                ValidationMessage += Localization.GetString("Promp_PositiveValueRequired", resourceFile)?.Replace("[0]", fieldName);
            }
            return value ?? defaultVal;
        }
        #endregion

        #region Public Methods
        public virtual void Init(string[] args, PortalSettings portalSettings, UserInfo userInfo, int activeTabId)
        {
            Args = args;
            PortalSettings = portalSettings;
            User = userInfo;
            PortalId = portalSettings.PortalId;
            TabId = activeTabId;
            ValidationMessage = "";
            ParseFlags();
        }

        public abstract ConsoleResultModel Run();

        public virtual bool IsValid()
        {
            return string.IsNullOrEmpty(ValidationMessage);
        }
        #endregion

        #region Private Methods
        private void ParseFlags()
        {
            Flags = new Hashtable();
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

        private object Flag<T>(string flagName, T defValue)
        {
            flagName = NormalizeFlagName(flagName);
            if (!Flags.ContainsKey(flagName)) return defValue;
            var retVal = Flags[flagName];
            if (retVal == null || (string)retVal == "")
                return defValue;
            var tc = TypeDescriptor.GetConverter(typeof(T));
            return tc.ConvertFrom(retVal);
        }


        #endregion

        #region Helper Methods
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
            return "";
        }
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
        public string ResultHtml => LocalizeString($"Prompt_{GetType().Name}_ResultHtml");
    }
}