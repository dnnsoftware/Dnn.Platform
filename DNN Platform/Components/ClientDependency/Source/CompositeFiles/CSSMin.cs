using System;
using System.Text.RegularExpressions;

namespace ClientDependency.Core.CompositeFiles
{
    public class CssMin
    {
        private static readonly Regex Step1Regex = new Regex(@"[\n\r]+\s*", RegexOptions.Compiled);
        private static readonly Regex Step2Regex = new Regex(@"\s+", RegexOptions.Compiled);
        private static readonly Regex Step3Regex = new Regex(@"\s?([:,;{}])\s?", RegexOptions.Compiled);
        private static readonly Regex Step4Regex = new Regex(@"([\s:]0)(px|pt|%|em)", RegexOptions.Compiled);
        private static readonly Regex Step5Regex = new Regex(@"/\*[\d\D]*?\*/", RegexOptions.Compiled);

        public static string CompressCSS(string body)
        {            
            body = Step1Regex.Replace(body, string.Empty);
            body = Step2Regex.Replace(body, " ");
            body = Step3Regex.Replace(body, "$1");
            body = Step4Regex.Replace(body, "$1");
            body = Step5Regex.Replace(body, string.Empty);
            return body;

        }
    }
}
