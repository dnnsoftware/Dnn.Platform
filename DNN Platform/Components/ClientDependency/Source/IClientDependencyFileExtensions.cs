using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace ClientDependency.Core
{
    public static class ClientDependencyFileExtensions
    {
        /// <summary>
        /// Resolves an absolute web path for the file path
        /// </summary>
        /// <param name="file"></param>
        /// <param name="http"></param>
        /// <returns></returns>
        public static string ResolveFilePath(this IClientDependencyFile file, HttpContextBase http)
        {
            var trimmedPath = file.FilePath.Trim();
            if (string.IsNullOrEmpty(trimmedPath))
            {
                throw new ArgumentException("The Path specified is null", "Path");
            }
            if (trimmedPath.StartsWith("~/"))
            {
                return http.ResolveUrl(file.FilePath);
            }
            if (trimmedPath.StartsWith("http://", StringComparison.InvariantCultureIgnoreCase) 
                || trimmedPath.StartsWith("https://", StringComparison.InvariantCultureIgnoreCase))
            {
                return file.FilePath; 
            }
            if (!http.IsAbsolute(file.FilePath))
            {
                //get the relative path
                var path = http.Request.AppRelativeCurrentExecutionFilePath.Substring(0, http.Request.AppRelativeCurrentExecutionFilePath.LastIndexOf('/') + 1);
                return http.ResolveUrl(path + file.FilePath);
            }
            return file.FilePath;
        }

        

    }
}
