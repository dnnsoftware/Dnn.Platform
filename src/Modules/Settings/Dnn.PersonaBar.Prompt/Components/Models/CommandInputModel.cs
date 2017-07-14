using System.Linq;
using System.Text.RegularExpressions;

namespace Dnn.PersonaBar.Prompt.Components.Models
{
    public class CommandInputModel
    {
        public string CmdLine { get; set; }
        public int CurrentPage { get; set; }
        public string[] Args
        {
            get
            {
                var tokenEx = new Regex("[^\\s\"]+|\"[^\"]*\"");
                // Matches (1 or more chars that are NOT space or ") or (" any # of chars not a " followed by a ")
                return tokenEx.Matches(CmdLine).Cast<Match>().Select(m => m.Value.Replace("\"", "")).ToArray();
            }
        }
    }
}