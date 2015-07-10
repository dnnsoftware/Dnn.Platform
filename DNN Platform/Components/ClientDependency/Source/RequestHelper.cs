using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using ClientDependency.Core.Config;

namespace ClientDependency.Core
{
    /// <summary>
    /// A utility class for getting the string result from an URL resource
    /// </summary>
    internal class RequestHelper
    {
        private static readonly string ByteOrderMarkUtf8 = Encoding.UTF8.GetString(Encoding.UTF8.GetPreamble());

        /// <summary>
        /// Tries to convert the url to a uri, then read the request into a string and return it.
        /// This takes into account relative vs absolute URI's
        /// </summary>
        /// <param name="url"></param>
        /// <param name="approvedDomains">a list of domains approved to make requests to in order to get a response</param>
        /// <param name="requestContents"></param>
        /// <param name="http"></param>
        /// <param name="resultUri">
        /// The Uri that was used to get the result. Depending on the extension this may be absolute and might not be. 
        /// If it is an aspx request, then it will be relative.
        /// </param>
        /// <returns>true if successful, false if not successful</returns>
        /// <remarks>
        /// if the path is a relative local path, the we use Server.Execute to get the request output, otherwise
        /// if it is an absolute path, a WebClient request is made to fetch the contents.
        /// </remarks>
        internal static bool TryReadUri(
            string url, 
            HttpContextBase http, 
            IEnumerable<string> approvedDomains, 
            out string requestContents,
            out Uri resultUri)
        {
            Uri uri;
            if (Uri.TryCreate(url, UriKind.RelativeOrAbsolute, out uri))
            {
                //flag of whether or not to make a request to get the external resource (used below)
                var bundleExternalUri = false;

                //if its a relative path, then check if we should execute/retreive contents,
                //otherwise change it to an absolute path and try to request it.
                if (!uri.IsAbsoluteUri)
                {
                    //if this is an ASPX page, we should execute it instead of http getting it.
                    if (uri.ToString().EndsWith(".aspx", StringComparison.InvariantCultureIgnoreCase))
                    {
                        var sw = new StringWriter();
                        try
                        {
                            http.Server.Execute(url, sw);
                            requestContents = sw.ToString();
                            sw.Close();
                            resultUri = uri;
                            return true;
                        }
                        catch (Exception ex)
                        {
                            ClientDependencySettings.Instance.Logger.Error(string.Format("Could not load file contents from {0}. EXCEPTION: {1}", url, ex.Message), ex);
                            requestContents = "";
                            resultUri = null;
                            return false;
                        }
                    }

                    //if this is a call for a web resource, we should http get it
                    if (url.StartsWith(http.Request.ApplicationPath.TrimEnd('/') + "/webresource.axd", StringComparison.InvariantCultureIgnoreCase))
                    {
                        bundleExternalUri = true;
                    }
                }

                try
                {
                    //we've gotten this far, make the URI absolute and try to load it
                    uri = uri.MakeAbsoluteUri(http);

                    if (uri.IsWebUri())
                    {
                        //if this isn't a web resource, we need to check if its approved
                        if (!bundleExternalUri)
                        {
                            // get the domain to test, with starting dot and trailing port, then compare with
                            // declared (authorized) domains. the starting dot is here to allow for subdomain
                            // approval, eg '.maps.google.com:80' will be approved by rule '.google.com:80', yet
                            // '.roguegoogle.com:80' will not.
                            var domain = string.Format(".{0}:{1}", uri.Host, uri.Port);

                            if (approvedDomains.Any(bundleDomain => domain.EndsWith(bundleDomain)))
                            {
                                bundleExternalUri = true;
                            }
                        }

                        if (bundleExternalUri)
                        {
                            requestContents = GetXmlResponse(uri);
                            resultUri = uri;
                            return true;
                        }

                        ClientDependencySettings.Instance.Logger.Error(string.Format("Could not load file contents from {0}. Domain is not white-listed.", url), null);
                    }
                    
                }
                catch (Exception ex)
                {
                    ClientDependencySettings.Instance.Logger.Error(string.Format("Could not load file contents from {0}. EXCEPTION: {1}", url, ex.Message), ex);
                }   
            }
            requestContents = "";
            resultUri = null;
            return false;
        }

        /// <summary>
        /// Gets the web response and ensures that the BOM is not present not matter what encoding is specified.
        /// </summary>
        /// <param name="resource"></param>
        /// <returns></returns>
        internal static string GetXmlResponse(Uri resource)
        {
            string xml;

            using (var client = new WebClient())
            {
                client.Credentials = CredentialCache.DefaultNetworkCredentials;
                client.Encoding = Encoding.UTF8;
                xml = client.DownloadString(resource);
            }

            if (xml.StartsWith(ByteOrderMarkUtf8))
            {
                xml = xml.Remove(0, ByteOrderMarkUtf8.Length - 1);
            }

            return xml;
        }

    }
}
