using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace ClientDependency.Core
{

    /// <summary>
    /// Extension methods for the HttpContext object
    /// </summary>
    public static class HttpContextBaseExtensions
    {

        public static void AddCompressionResponseHeader(this HttpContextBase context, CompressionType cType)
        {
            if (cType == CompressionType.deflate)
            {
                context.Response.AddHeader("Content-encoding", "deflate");
            }
            else if (cType == CompressionType.gzip)
            {
                context.Response.AddHeader("Content-encoding", "gzip");
            }            
        }

        /// <summary>
        /// Check what kind of compression to use. Need to select the first available compression 
        /// from the header value as this is how .Net performs caching by compression so we need to follow
        /// this process.
        /// If IE 6 is detected, we will ignore compression as it's known that some versions of IE 6
        /// have issues with it.
        /// </summary>
        public static CompressionType GetClientCompression(this HttpContextBase context)
        {
            CompressionType type = CompressionType.none;

            if (context.Request.UserAgent.Contains("MSIE 6"))
            {
                return type;
            }

            string acceptEncoding = context.Request.Headers["Accept-Encoding"];

            if (!string.IsNullOrEmpty(acceptEncoding))
            {
                string[] supported = acceptEncoding.Split(',');
                //get the first type that we support
                for (var i = 0; i < supported.Length; i++)
                {
                    if (supported[i].Contains("deflate"))
                    {
                        type = CompressionType.deflate;
                        break;
                    }
                    else if (supported[i].Contains("gzip")) //sometimes it could be x-gzip!
                    {
                        type = CompressionType.gzip;
                        break;
                    }
                }
            }

            return type;
        }


        /// <summary>
        /// Checks for absolute path to root of the website.
        /// </summary>
        /// <remarks>
        /// This was taken from the mono source so should be accurate.
        /// The reason we're not using the VirtualPathUtility one is because it has bugs in 3.5 whereas
        /// if the path has query strings, it throws exceptions.
        /// </remarks>
        /// <param name="context"></param>
        /// <param name="virtualPath"></param>
        /// <returns></returns>
        public static bool IsAbsolute(this HttpContextBase context, string virtualPath)
        {
            if (IsAbsolutePath(context, virtualPath))
            {
                throw new InvalidOperationException("IsAbsolute method will check if a Virtual path is absolute, it is not supported for full URLs");
            }

            if (string.IsNullOrEmpty(virtualPath))
                throw new ArgumentNullException("virtualPath");

            return (virtualPath[0] == '/' || virtualPath[0] == '\\');
        }

        /// <summary>
        /// Returns a site relative HTTP path from a partial path starting out with a ~.
        /// Same syntax that ASP.Net internally supports but this method can be used
        /// outside of the Page framework.
        /// 
        /// Works like Control.ResolveUrl including support for ~ syntax
        /// but returns an absolute URL.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="originalUrl">Any Url including those starting with ~</param>
        /// <returns>relative url</returns>
        public static string ResolveUrl(this HttpContextBase context, string originalUrl)
        {
            if (string.IsNullOrEmpty(originalUrl))
                return originalUrl;

            // *** Absolute path - just return
            if (context.IsAbsolutePath(originalUrl))
                return originalUrl;

            // *** We don't start with the '~' -> we don't process the Url
            if (!originalUrl.StartsWith("~/"))
                return originalUrl;

            // *** Fix up path for ~ root app dir directory
            // VirtualPathUtility blows up if there is a 
            // query string, so we have to account for this.
            int queryStringStartIndex = originalUrl.IndexOf('?');
            if (queryStringStartIndex != -1)
            {
                string queryString = originalUrl.Substring(queryStringStartIndex);
                string baseUrl = originalUrl.Substring(0, queryStringStartIndex);

                return string.Concat(
                    VirtualPathUtility.ToAbsolute(baseUrl, context.Request.ApplicationPath),
                    queryString);
            }
            else
            {
                return VirtualPathUtility.ToAbsolute(originalUrl, context.Request.ApplicationPath);
            }

        }

        /// <summary>
        /// Checks for an absolute http path
        /// </summary>
        /// <remarks>
        /// Takes into account this type of url:
        /// ~/pathtoresolve/page.aspx?returnurl=http://servertoredirect/resource.aspx
        /// which is not an absolute path but contains the characters to describe it as one.
        /// </remarks>
        /// <param name="context"></param>
        /// <param name="originalUrl"></param>
        /// <returns></returns>
        public static bool IsAbsolutePath(this HttpContextBase context, string originalUrl)
        {
            // *** Absolute path - just return
            var indexOfSlashes = originalUrl.IndexOf("://");
            var indexOfQuestionMarks = originalUrl.IndexOf("?");

            if (indexOfSlashes > -1 &&
                 (indexOfQuestionMarks < 0 ||
                  (indexOfQuestionMarks > -1 && indexOfQuestionMarks > indexOfSlashes)
                  )
                )
                return true;

            return false;
        }
    }
}
