using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Script.Serialization;

namespace DeployClient
{
    class API
    {
        private static string APIKey = Program.Options.APIKey;

        private static HttpClient BuildClient()
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            HttpClient client = new HttpClient()
            {
                BaseAddress = new Uri(new Uri(Program.Options.TargetUri), "DesktopModules/PolyDeploy/API/")
            };

            client.DefaultRequestHeaders.Add("x-api-key", APIKey);

            return client;
        }

        public static async Task<string> CreateSessionAsync()
        {
            string endpoint = "Remote/CreateSession";

            using (HttpClient client = BuildClient())
            {
                string json = await client.GetStringAsync(endpoint);

                JavaScriptSerializer jsonSer = new JavaScriptSerializer();

                Dictionary<string, dynamic> session = jsonSer.Deserialize<Dictionary<string, dynamic>>(json);

                string sessionGuid = null;

                if (session.ContainsKey("Guid"))
                {
                    sessionGuid = session["Guid"];
                }

                return sessionGuid;
            }
        }

        public static async Task<(bool success, Dictionary<string, dynamic> results)> GetSessionAsync(string sessionGuid)
        {
            string endpoint = string.Format("Remote/GetSession?sessionGuid={0}", sessionGuid);

            JavaScriptSerializer jsonSer = new JavaScriptSerializer();

            using (HttpClient client = BuildClient())
            {
                var httpResponse = await client.GetAsync(endpoint);
                if (httpResponse.StatusCode == HttpStatusCode.OK)
                {
                    string json = await httpResponse.Content.ReadAsStringAsync();
                    return (true, jsonSer.Deserialize<Dictionary<string, dynamic>>(json));
                }

                if (httpResponse.StatusCode == HttpStatusCode.NotFound)
                {
                    return (false, new Dictionary<string, dynamic>(0));
                }

                throw new HttpException($"Invalid status code returned from remote api: {httpResponse.StatusCode}");
            }
        }

        public static async Task AddPackagesAsync(string sessionGuid, List<KeyValuePair<string, Stream>> streams)
        {
            string endpoint = string.Format("Remote/AddPackages?sessionGuid={0}", sessionGuid);

            using (HttpClient client = BuildClient())
            {
                MultipartFormDataContent form = new MultipartFormDataContent();

                foreach (KeyValuePair<string, Stream> keyValuePair in streams)
                {
                    form.Add(new StreamContent(keyValuePair.Value), "none", keyValuePair.Key);
                }

                await client.PostAsync(endpoint, form);
            }
        }

        public static async Task AddPackageAsync(string sessionGuid, Stream stream, string filename)
        {
            string endpoint = string.Format("Remote/AddPackages?sessionGuid={0}", sessionGuid);

            using (HttpClient client = BuildClient())
            {
                MultipartFormDataContent form = new MultipartFormDataContent();

                form.Add(new StreamContent(stream), "none", filename);

                await client.PostAsync(endpoint, form);
            }
        }

        public static async Task<(bool success, SortedList<string, dynamic> response)> InstallAsync(string sessionGuid)
        {
            string endpoint = string.Format("Remote/Install?sessionGuid={0}", sessionGuid);

            JavaScriptSerializer jsonSer = new JavaScriptSerializer();

            using (HttpClient client = BuildClient())
            {
                try
                {
                    HttpResponseMessage httpResponse = await client.GetAsync(endpoint);

                    if (httpResponse.StatusCode.Equals(HttpStatusCode.OK))
                    {
                        string json = await httpResponse.Content.ReadAsStringAsync();
                        return (true, jsonSer.Deserialize<SortedList<string, dynamic>>(json));
                    }
                }
                catch (Exception ex)
                {
                    // Nothing to do.
                }

                // Always fail.
                return (false, null);
            }
        }
    }
}
