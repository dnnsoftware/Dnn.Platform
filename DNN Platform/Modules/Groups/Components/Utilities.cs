using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text.RegularExpressions;

namespace DotNetNuke.Modules.Groups {
    public class Utilities {
        static internal string ParseTokenWrapper(string Template, string Token, bool Condition) {

            string pattern = "(\\[" + Token + "\\](.*?)\\[\\/" + Token + "\\])";
            Regex regExp = new Regex(pattern, RegexOptions.Singleline | RegexOptions.IgnoreCase | RegexOptions.Multiline);
            MatchCollection matches = default(MatchCollection);
            matches = regExp.Matches(Template);
            foreach (Match match in matches) {
                if (Condition) {
                    Template = Template.Replace(match.Value, match.Groups[2].Value);
                } else {
                    Template = Template.Replace(match.Value, string.Empty);
                }
            }
            return Template;
        }
        public static string NavigateUrl(int TabId, string[] @params)
        {
            return DotNetNuke.Common.Globals.NavigateURL(TabId, "", @params);
        }
        public static string[] AddParams(string param, string[] currParams)
        {
            string[] tmpParams = new string[] { param };
            int intLength = tmpParams.Length;
            int currLength = currParams.Length;
            Array.Resize(ref tmpParams, (intLength + currLength));
            currParams.CopyTo(tmpParams, intLength);
            return tmpParams;
        }
    }
}