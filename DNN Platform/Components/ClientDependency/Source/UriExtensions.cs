using System;
using System.Linq;
using System.Text;
using System.Web;
using System.Net;

namespace ClientDependency.Core
{
    public static class UriExtensions
    {
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
        /// Determines if the uri is a locally based file
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="http"></param>
        /// <returns></returns>
        public static bool IsLocalUri(this Uri uri, HttpContextBase http)
        {
            var isLocal = false;
            
            try
            {
                if (!uri.IsAbsoluteUri)
                {
                    uri = uri.MakeAbsoluteUri(http);
                }

                var host = Dns.GetHostAddresses(uri.Host);
                var local = Dns.GetHostAddresses(Dns.GetHostName());
                foreach (var hostAddress in host)
                {
                    if (IPAddress.IsLoopback(hostAddress))
                    {
                        isLocal = true;
                        break;
                    }
                    if (local.Contains(hostAddress))
                    {
                        isLocal = true;
                    }

                    if (isLocal)
                    {
                        break;
                    }
                }
            }
            catch (Exception)
            {
                
                //suppress error - triggered if user has no internet connection or environment permission
                //we assume local files as we cannot support non local files without an internet connection
                return true;
            }
            
            return isLocal;
        }
    }
}
