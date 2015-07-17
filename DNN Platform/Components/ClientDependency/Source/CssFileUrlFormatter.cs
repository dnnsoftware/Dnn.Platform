using System;
using System.Text.RegularExpressions;

namespace ClientDependency.Core
{
	public class CssFileUrlFormatter
	{
        // DNN-6660 Fix for CDF breaking dateUris
        private static readonly Regex TransformCssRegex = new Regex(@"url\(((?![""']?data:).+?)\)", RegexOptions.Compiled);

		/// <summary>
		/// Returns the CSS file with all of the url's formatted to be absolute locations
		/// </summary>
		/// <param name="fileContent">content of the css file</param>
		/// <param name="cssLocation">the uri location of the css file</param>
		/// <returns></returns>
		public static string TransformCssFile(string fileContent, Uri cssLocation)
		{
		    var evaluator = new MatchEvaluator(
		        delegate(Match m)
		        {
		            if (m.Groups.Count == 2)
		            {
		                var match = m.Groups[1].Value.Trim('\'', '"');
		                var hashSplit = match.Split(new[] {'#'}, StringSplitOptions.RemoveEmptyEntries);

		                return string.Format(@"url(""{0}{1}"")",
		                    match.StartsWith("http") ? match : new Uri(cssLocation, match).PathAndQuery,
		                    hashSplit.Length > 1 ? ("#" + hashSplit[1]) : "");
		            }
		            return m.Value;
		        });

            var str = TransformCssRegex.Replace(fileContent, evaluator);
			return str;
		}
	}
}
