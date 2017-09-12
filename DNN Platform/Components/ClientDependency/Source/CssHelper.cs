using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using ClientDependency.Core.CompositeFiles;

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
                        continue;
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
        /// Searches from the beginning of the stream to detect @import statements. Any relative @import statement found will be 
        /// added to the importedPaths collection, any absolute/external @import statement will be appended to the externalImportedPaths
        /// which will need to be pre-fixed to the resulting css file after processing.        
        /// </summary>
        /// <param name="stream">The css stream</param>
        /// <param name="importedPaths"></param>
        /// <param name="externalImportedPaths">
        /// The absolute/external @import statement that will need to be prefixed to the resultant css
        /// </param>
        /// <returns>
        /// The Streams position starting at the first char after all @import statements
        /// </returns>
        /// <remarks>
        /// The Stream's position will be at the position returned from this method
        /// </remarks>
        public static long ParseImportStatements(Stream stream, out IEnumerable<string> importedPaths, out string externalImportedPaths)
        {
            if (!stream.CanRead) throw new InvalidOperationException("Cannot read Stream object");

            //read the content until we know we are no longer on an import statement
            if (stream.CanSeek)
            {
                stream.Position = 0;
            }

            const string searchStatement = "@import ";

            var imports = new StringBuilder();
            var tempImports = new StringBuilder();

            //new reader (but don't dispose since we don't want to dispose the stream)
            TextReader reader = new StreamReader(stream);
            var currIndex = -1;
            
            while (true)
            {
                var next = reader.Read();
                if (next == -1)
                {
                    break;
                }

                var c = (char) next;

                //still searching for the '@import' block at the top
                if (currIndex == -1 && char.IsWhiteSpace(c))
                {
                    //maintain whitespace with the output
                    tempImports.Append(c);
                }
                else if (currIndex == -2)
                {
                    //we've found the entire searchStatement, keep processing until we hit the semicolon

                    tempImports.Append(c);

                    if (c == ';')
                    {
                        //we're at the end, reset the index so that it looks for the searchStatement again
                        currIndex = -1;
                        //write to the main imports and reset the temp one
                        imports.Append(tempImports);
                        tempImports.Clear();
                    }
                }
                else if (searchStatement[currIndex + 1] == c)
                {
                    //we've found the next char in the search statement
                    tempImports.Append(c);
                    currIndex++;
                    if (currIndex == (searchStatement.Length - 1))
                    {
                        //we've found the whole statement, set the flag that we are processing
                        currIndex = -2;
                    }
                }
                else
                {
                    //reset and start again
                    currIndex = -1;
                    tempImports.Clear();
                }             
            }

            externalImportedPaths = ParseImportStatements(imports.ToString(), out importedPaths);

            return stream.Position;
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
        /// Minifies Css from a string input
        /// </summary>
        /// <param name="body"></param>
        /// <returns></returns>
        public static string MinifyCss(string body)
        {
            using (var ms = new MemoryStream())
            using (var writer = new StreamWriter(ms))
            {
                writer.Write(body);
                writer.Flush();
                return MinifyCss(ms);
            }
        }

        /// <summary>
        /// Minifies CSS from a stream input
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        public static string MinifyCss(Stream stream)
        {
            var cssMinify = new CssMinifier();
            if (!stream.CanRead) throw new InvalidOperationException("Cannot read input stream");
            if (stream.CanSeek)
            {
                stream.Position = 0;
            }
            return cssMinify.Minify(new StreamReader(stream));
        }
    }
}