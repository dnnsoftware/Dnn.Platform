using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using ClientDependency.Core.Config;

namespace ClientDependency.Core
{
    public static class CssHelper
    {
        private static readonly Regex ImportCssRegex = new Regex(@"@import url\((.+?)\);?", RegexOptions.Compiled);
        private static readonly Regex CssUrlRegex = new Regex(@"url\(((?![""']?data:|[""']?#).+?)\)", RegexOptions.Compiled);

        /// <summary>
        /// Returns the paths for the import statements and the resultant original css without the import statements
        /// </summary>
        /// <param name="content">The original css contents</param>
        /// <param name="importedPaths"></param>
        /// <returns></returns>
        public static string ParseImportStatements(string content, out IEnumerable<string> importedPaths)
        {
            var pathsFound = new List<string>();
            var matches = ImportCssRegex.Matches(content);
            foreach (Match match in matches)
            {
                //Ignore external imports
                var urlMatch = CssUrlRegex.Match(match.Value);
                if (urlMatch.Success && urlMatch.Groups.Count >= 2)
                {
                    var path = urlMatch.Groups[1].Value.Trim('\'', '"');
                    if ((path.StartsWith("http://", StringComparison.InvariantCultureIgnoreCase)
                         || path.StartsWith("https://", StringComparison.InvariantCultureIgnoreCase)
                         || path.StartsWith("//", StringComparison.InvariantCultureIgnoreCase)))
                    {
                        Uri uri;
                        if (IsAbsoluteUrl(path, out uri))
                        {
                            continue;
                        }
                        var domain = $".{uri.Host}:{uri.Port}";
                        var approvedDomains =
                            ClientDependencySettings.Instance.DefaultCompositeFileProcessingProvider.BundleDomains;
                        if (!approvedDomains.Any(bundleDomain => domain.EndsWith(bundleDomain)))
                        {
                            continue;
                        }
                    }
                }

                //Strip the import statement                
                content = content.ReplaceFirst(match.Value, "");

                //write import css content
                var filePath = match.Groups[1].Value.Trim('\'', '"');
                pathsFound.Add(filePath);
            }

            importedPaths = pathsFound;
            return content.Trim();
        }

        /// <summary>
        /// Returns the CSS file with all of the url's formatted to be absolute locations
        /// </summary>
        /// <param name="fileContents"></param>
        /// <param name="url"></param>
        /// <param name="http"></param>
        /// <returns></returns>
        public static string ReplaceUrlsWithAbsolutePaths(string fileContents, string url, HttpContextBase http)
        {
            var uri = new Uri(url, UriKind.RelativeOrAbsolute);
            fileContents = CssHelper.ReplaceUrlsWithAbsolutePaths(fileContents, uri.MakeAbsoluteUri(http));
            return fileContents;
        }

        /// <summary>
        /// Returns the CSS file with all of the url's formatted to be absolute locations
        /// </summary>
        /// <param name="fileContent">content of the css file</param>
        /// <param name="cssLocation">the uri location of the css file</param>
        /// <returns></returns>
        public static string ReplaceUrlsWithAbsolutePaths(string fileContent, Uri cssLocation)
        {
            var str = CssUrlRegex.Replace(fileContent, m =>
                {
                    if (m.Groups.Count == 2)
                    {
                        var match = m.Groups[1].Value.Trim('\'', '"');
                        var hashSplit = match.Split(new[] {'#'}, StringSplitOptions.RemoveEmptyEntries);

                        return string.Format(@"url(""{0}{1}"")",
                                             (match.StartsWith("http://", StringComparison.InvariantCultureIgnoreCase)
                                             || match.StartsWith("https://", StringComparison.InvariantCultureIgnoreCase)
                                             || match.StartsWith("//", StringComparison.InvariantCultureIgnoreCase)) ? match : new Uri(cssLocation, match).PathAndQuery,
                                             hashSplit.Length > 1 ? ("#" + hashSplit[1]) : "");
                    }
                    return m.Value;
                });            

            return str;
        }

        /// <summary>
        /// Minifies Css
        /// </summary>
        /// <param name="body"></param>
        /// <returns></returns>
        public static string MinifyCss(string body)
        {
            body = Regex.Replace(body, @"[\n\r]+\s*", string.Empty);
            body = Regex.Replace(body, @"\s+", " ");
            body = Regex.Replace(body, @"\s?([:,;{}])\s?", "$1");
            body = Regex.Replace(body, @"([\s:]0)(px|pt|%|em)", "$1");
            body = Regex.Replace(body, @"/\*[\d\D]*?\*/", string.Empty);
            return body;

        }

        private static bool IsAbsoluteUrl(string url, out Uri uri)
        {
            return Uri.TryCreate(url, UriKind.Absolute, out uri);
        }
    }
}