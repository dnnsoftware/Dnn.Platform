using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Web;
using ClientDependency.Core.CompositeFiles.Providers;
using ClientDependency.Core.Config;

namespace ClientDependency.Core.CompositeFiles
{
    /// <summary>
    /// The default file local file writer in CDF
    /// </summary>
    public class DefaultFileWriter : IFileWriter
    {
        public bool WriteToStream(BaseCompositeFileProcessingProvider provider, StreamWriter sw, FileInfo fi, ClientDependencyType type, string origUrl, HttpContextBase http)
        {
            try
            {
                //if it is a file based dependency then read it
                using (var fileStream = fi.OpenRead())
                {
                    WriteContentToStream(provider, sw, fileStream, type, http, origUrl);
                }
                return true;
            }
            catch (Exception ex)
            {
                ClientDependencySettings.Instance.Logger.Error($"Could not write file {fi.FullName} contents to stream. EXCEPTION: {ex.Message}", ex);
                return false;
            }
        }

        /// <summary>
        /// Writes the actual contents of a file or request result to the stream and ensures the contents are minified if necessary
        /// </summary>
        /// <param name="provider"></param>
        /// <param name="sw"></param>
        /// <param name="content"></param>
        /// <param name="type"></param>
        /// <param name="context"></param>
        /// <param name="originalUrl">The original Url that the content is related to</param>
        public static void WriteContentToStream(BaseCompositeFileProcessingProvider provider, StreamWriter sw, string content, ClientDependencyType type, HttpContextBase context, string originalUrl)
        {
            if (type == ClientDependencyType.Css)
            {
                IEnumerable<string> importedPaths;
                var removedImports = CssHelper.ParseImportStatements(content, out importedPaths);

                //need to write the imported sheets first since these theoretically should *always* be at the top for browser to support them
                foreach (var importPath in importedPaths)
                {
                    var uri = new Uri(originalUrl, UriKind.RelativeOrAbsolute)
                        .MakeAbsoluteUri(context);
                    var absolute = uri.ToAbsolutePath(importPath);
                    provider.WritePathToStream(ClientDependencyType.Css, absolute, context, sw);
                }

                //ensure the Urls in the css are changed to absolute
                var parsedUrls = CssHelper.ReplaceUrlsWithAbsolutePaths(removedImports, originalUrl, context);

                //then we write the css with the removed import statements
                sw.WriteLine(provider.MinifyFile(parsedUrls, ClientDependencyType.Css));
            }
            else
            {
                sw.WriteLine(provider.MinifyFile(content, type));
            }
        }

        /// <summary>
        /// Writes the input stream to the output stream and ensures the contents are minified if necessary
        /// </summary>
        /// <param name="provider"></param>
        /// <param name="sw"></param>
        /// <param name="stream"></param>
        /// <param name="type"></param>
        /// <param name="context"></param>
        /// <param name="originalUrl">The original Url that the content is related to</param>
        public static void WriteContentToStream(BaseCompositeFileProcessingProvider provider, StreamWriter sw, Stream stream, ClientDependencyType type, HttpContextBase context, string originalUrl)
        {
            if (type == ClientDependencyType.Css)
            {
                IEnumerable<string> importedPaths;
                string externalImports;
                CssHelper.ParseImportStatements(stream, out importedPaths, out externalImports);

                //we can write the external imports found at the top
                sw.WriteLine(externalImports);

                //need to write the imported sheets first since these theoretically should *always* be at the top for browser to support them
                foreach (var importPath in importedPaths)
                {
                    var uri = new Uri(originalUrl, UriKind.RelativeOrAbsolute)
                        .MakeAbsoluteUri(context);
                    var absolute = uri.ToAbsolutePath(importPath);
                    provider.WritePathToStream(ClientDependencyType.Css, absolute, context, sw);
                }
                
                var minified = provider.MinifyFile(stream, type);

                //ensure the Urls in the css are changed to absolute
                var parsedUrls = CssHelper.ReplaceUrlsWithAbsolutePaths(minified, originalUrl, context);

                //then we write the css with the removed import statements
                sw.WriteLine(parsedUrls);
            }
            else
            {
                sw.WriteLine(provider.MinifyFile(stream, type));
            }
        }
    }
}