using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Net;
using System.Web.Caching;

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


                var dict = http.Cache["IsLocalUri"] as IDictionary<string, string>;

                if (dict != null)
                {
                    if (dict.ContainsKey(uri.ToString()))
                    {
                        bool cachedLocal = Convert.ToBoolean(dict[uri.ToString()]);
                        return cachedLocal;
                    }
                }

                var host = Dns.GetHostAddresses(uri.Host);
                var local = Dns.GetHostAddresses(Dns.GetHostName());
                foreach (var hostAddress in host)
                {
                    if (IPAddress.IsLoopback(hostAddress))
                    {
                        AddUriCacheitem(http, uri.ToString(), true);
                        isLocal = true;
                        break;
                    }
                    if (local.Contains(hostAddress))
                    {
                        AddUriCacheitem(http, uri.ToString(), true);
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
                AddUriCacheitem(http, uri.ToString(), true);
                return true;
            }
            
            return isLocal;
        }

        /// <summary>
        /// add an item to cache to potentially save expensive dns lookups
        /// </summary>
        /// <param name="http">the current httpcontext</param>
        /// <param name="url">the url being examined</param>
        /// <param name="islocal">whether it is a local file</param>
   
        private static void AddUriCacheitem(HttpContextBase http,string url, bool islocal)
        {
              IDictionary<string, string> testDict = new Dictionary<string, string>();
                testDict.Add(url, islocal.ToString());
                http.Cache.Insert("IsLocalUri", testDict);

        }
    }
}
