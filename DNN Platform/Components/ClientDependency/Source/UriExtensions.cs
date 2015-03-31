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
            return uri.IsAbsoluteUri ? uri.Host.Equals(http.Request.Url.Host, StringComparison.OrdinalIgnoreCase) : true;
        }

    }
}
