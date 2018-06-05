using System;
using System.Linq;
using System.Net.Http;

namespace Cantarus.Modules.PolyDeploy.Components.WebAPI
{
    internal static class HttpRequestMessageExtensions
    {
        public static string GetApiKey(this HttpRequestMessage request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            // Is there an api key header present?
            if (request.Headers.Contains("x-api-key"))
            {
                // Get the api key from the header.
                return request.Headers.GetValues("x-api-key").FirstOrDefault();
            }

            return null;
        }
    }
}
