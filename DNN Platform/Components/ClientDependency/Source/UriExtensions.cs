using System;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Web;
using System.Net;
using ClientDependency.Core.Config;

namespace ClientDependency.Core
{
    public static class UriExtensions
    {

        public static bool IsWebUri(this Uri uri)
        {
            return uri.Scheme.Equals("http", StringComparison.InvariantCultureIgnoreCase)
                   || uri.Scheme.Equals("https", StringComparison.InvariantCultureIgnoreCase);
        }

        internal static string ToAbsolutePath(this Uri originalUri, string path)
        {
            var hashSplit = path.Split(new[] { '#' }, StringSplitOptions.RemoveEmptyEntries);

            return string.Format(@"{0}{1}",
                                 (path.StartsWith("http://", StringComparison.InvariantCultureIgnoreCase)
                                 || path.StartsWith("https://", StringComparison.InvariantCultureIgnoreCase)
                                 || path.StartsWith("//", StringComparison.InvariantCultureIgnoreCase)) ? path : new Uri(originalUri, path).PathAndQuery,
                                 hashSplit.Length > 1 ? ("#" + hashSplit[1]) : "");
        }

        /// <summary>
        /// Checks if the url is a local/relative uri, if it is, it makes it absolute based on the 
        /// current request uri.
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="http"></param>
        /// <returns></returns>
        public static Uri MakeAbsoluteUri(this Uri uri, HttpContextBase http)
        {
            if (!uri.IsAbsoluteUri)
            {
                if (http.Request.Url != null)
                {
                    var left = http.Request.Url.GetLeftPart(UriPartial.Authority);
                    var absoluteUrl = new Uri(new Uri(left), uri);
                    return absoluteUrl;
                }
            }
            return uri;
        }

        /// <summary>
        /// Determines if the uri is a locally based web file
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="http"></param>
        /// <returns></returns>
        public static bool IsLocalUri(this Uri uri, HttpContextBase http)
        {
            if (http.Request == null)
            {
                throw new InvalidOperationException("The Request must be assigned to the context");
            }
            if (http.Request.Url == null)
            {
                throw new InvalidOperationException("The Url must be assigned to the Request");
            }           
            if (!uri.IsAbsoluteUri)
            {
                uri = uri.MakeAbsoluteUri(http);
            }
            return uri.IsWebUri() && string.Equals(http.Request.Url.Host, uri.Host, StringComparison.OrdinalIgnoreCase);
        }
    }
}
