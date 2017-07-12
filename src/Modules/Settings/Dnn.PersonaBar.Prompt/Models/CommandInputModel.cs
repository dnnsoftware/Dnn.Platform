using System.Linq;
using System.Text.RegularExpressions;

namespace Dnn.PersonaBar.Prompt.Models
{
    public class CommandInputModel
    {
        public string CmdLine;

        public int CurrentPage;
        public string[] GetArgs()
        {
            var tokenEx = new Regex("[^\\s\"]+|\"[^\"]*\"");
            // Matches (1 or more chars that are NOT space or ") or (" any # of chars not a " followed by a ")
            return tokenEx.Matches(CmdLine).Cast<Match>().Select(m => m.Value.Replace("\"", "")).ToArray();
        }

        public static string Flag(string[] args, string flagName)
        {
            // strip leading dashes
            if (flagName.StartsWith("--"))
            {
                flagName = flagName.Substring(2);
            }
            flagName = "--" + flagName;

            // loop through arguments, skipping the first one (the command)
            for (var i = 1; i <= args.Length - 1; i++)
            {
                if (string.Compare(args[i], flagName, true, System.Globalization.CultureInfo.InvariantCulture) == 0)
                {
                    // found get the next argument, which will be its value
                    if (args.Length >= (i + 1))
                    {
                        return args[i + 1];
                    }
                    else
                    {
                        return string.Empty;
                        // indicates flag found but no value
                    }
                }
            }

            return null;
            // not found
        }

    }
}