using System;
using System.Text.RegularExpressions;
using DotNetNuke.Common.Utilities;

namespace DotNetNuke.Modules.Groups {
    public class Utilities {
        internal static string ParseTokenWrapper(string Template, string Token, bool Condition) {
            var pattern = "(\\[" + Token + "\\](.*?)\\[\\/" + Token + "\\])";
            var regExp = RegexUtils.GetCachedRegex(pattern, RegexOptions.Singleline | RegexOptions.IgnoreCase | RegexOptions.Multiline);
            var matches = regExp.Matches(Template);
            foreach (Match match in matches)
            {
                Template = Template.Replace(match.Value, Condition ? match.Groups[2].Value : string.Empty);
            }
            return Template;
        }
        public static string NavigateUrl(int TabId, string[] @params)
        {
            return Common.Globals.NavigateURL(TabId, "", @params);
        }
        public static string[] AddParams(string param, string[] currParams)
        {
            var tmpParams = new string[] { param };
            var intLength = tmpParams.Length;
            var currLength = currParams.Length;
            Array.Resize(ref tmpParams, (intLength + currLength));
            currParams.CopyTo(tmpParams, intLength);
            return tmpParams;
        }
    }
}