using System.Collections;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;

namespace Dnn.PersonaBar.Library.Prompt
{
    public class ConsoleCommandBase
    {
        protected PortalSettings PortalSettings { get; private set; }
        protected UserInfo User { get; private set; }
        protected int PortalId { get; private set; }
        protected int TabId { get; private set; }
        protected string[] Args { get; private set; }
        protected Hashtable Flags { get; private set; }

        protected void Initialize(string[] args, PortalSettings portalSettings, UserInfo userInfo, int activeTabId)
        {
            Args = args;
            PortalSettings = portalSettings;
            User = userInfo;
            PortalId = portalSettings.PortalId;
            TabId = activeTabId;

            ParseFlags();
        }

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

        protected string Flag(string flagName, string defValue = "")
        {
            flagName = NormalizeFlagName(flagName);
            if (!Flags.ContainsKey(flagName)) return defValue;
            var retVal = Flags[flagName];
            if (retVal == null || (string)retVal == "")
                return defValue;
            return (string)retVal;
        }

        protected bool HasFlag(string flagName)
        {
            flagName = NormalizeFlagName(flagName);
            return Flags.ContainsKey(flagName);
        }

        protected bool IsFlag(string input)
        {
            return !string.IsNullOrEmpty(input) && input.StartsWith("--");
        }

        private static string NormalizeFlagName(string flagName)
        {
            if (flagName == null)
                return string.Empty;
            if (flagName.StartsWith("--"))
                flagName = flagName.Substring(2);
            return flagName.ToLower().Trim();
        }

    }
}