using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using System.Collections;

namespace Dnn.PersonaBar.Prompt.Common
{
    public class ConsoleCommandBase
    {

        protected PortalSettings PortalSettings { get; private set; }
        protected UserInfo User { get; private set; }
        protected int PortalId { get; private set; }
        protected int TabId { get; private set; }
        protected string[] args { get; private set; }
        protected Hashtable flags { get; private set; }

        protected void Initialize(string[] args, PortalSettings portalSettings, UserInfo userInfo, int activeTabId)
        {
            this.args = args;
            this.PortalSettings = portalSettings;
            this.User = userInfo;
            this.PortalId = portalSettings.PortalId;
            this.TabId = activeTabId;

            ParseFlags();
        }

        private void ParseFlags()
        {
            this.flags = new Hashtable();
            // loop through arguments, skipping the first one (the command)
            for (int i = 1; i <= args.Length - 1; i++)
            {
                if (args[i].StartsWith("--"))
                {
                    // found a flag
                    var flagName = NormalizeFlagName(args[i]);
                    var flagValue = string.Empty;
                    if (i < args.Length - 1)
                    {
                        if (!string.IsNullOrEmpty(args[i + 1]))
                        {
                            if (args[i + 1].StartsWith("--"))
                            {
                                // next value is another flag, so this flag has no value
                                flagValue = string.Empty;
                            }
                            else
                            {
                                flagValue = args[i + 1];
                            }
                        }
                        else
                        {
                            flagValue = string.Empty;
                        }
                    }
                    flags.Add(flagName.ToLower(), flagValue);
                }
            }
        }

        protected string Flag(string flagName, string defValue = "")
        {
            flagName = NormalizeFlagName(flagName);
            if (flags.ContainsKey(flagName))
            {
                var retVal = flags[flagName];
                if (retVal == null || (string)retVal == "")
                    return defValue;
                return (string)retVal;
            }
            else
            {
                return defValue;
            }
        }

        protected bool HasFlag(string flagName)
        {
            flagName = NormalizeFlagName(flagName);
            return flags.ContainsKey(flagName);
        }

        protected bool IsFlag(string input)
        {
            if (string.IsNullOrEmpty(input))
                return false;
            if (input.StartsWith("--"))
                return true;
            return false;
        }

        private string NormalizeFlagName(string flagName)
        {
            if (flagName == null)
                return string.Empty;
            if (flagName.StartsWith("--"))
                flagName = flagName.Substring(2);
            return flagName.ToLower().Trim();
        }

    }
}