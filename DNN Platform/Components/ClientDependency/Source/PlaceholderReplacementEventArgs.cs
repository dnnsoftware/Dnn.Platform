using System.Text.RegularExpressions;
using System.Web;

namespace ClientDependency.Core
{
    internal class PlaceholderReplacementEventArgs : PlaceholdersReplacedEventArgs
    {
        public ClientDependencyType Type { get; private set; }
        public Match RegexMatch { get; private set; }

        public PlaceholderReplacementEventArgs(
            HttpContextBase httpContext, 
            ClientDependencyType type, 
            string replacedText,
            Match regexMatch) 
            : base(httpContext, replacedText)
        {
            Type = type;
            RegexMatch = regexMatch;
        }
    }
}