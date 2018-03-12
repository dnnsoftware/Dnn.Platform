using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace DeployClient
{
    class API
    {
        private static string APIKey = Program.Options.APIKey;

        private static HttpClient BuildClient()
        {
            HttpClient client = new HttpClient();

            client.BaseAddress = new Uri(new Uri(Program.Options.TargetUri), "DesktopModules/PolyDeploy/API/");
            client.DefaultRequestHeaders.Add("x-api-key", APIKey);
            client.Timeout = TimeSpan.FromSeconds(25);

            return client;
        }

        public static string CreateSession()
        {
            string endpoint = "Remote/CreateSession";

            using (HttpClient client = BuildClient())
            {
                string json = client.GetStringAsync(endpoint).Result;

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

        public static Dictionary<string, dynamic> GetSession(string sessionGuid)
        {
            string endpoint = string.Format("Remote/GetSession?sessionGuid={0}", sessionGuid);

            JavaScriptSerializer jsonSer = new JavaScriptSerializer();

            using (HttpClient client = BuildClient())
            {
                string json = client.GetStringAsync(endpoint).Result;

                return jsonSer.Deserialize<Dictionary<string, dynamic>>(json);
            }
        }

        public static void AddPackages(string sessionGuid, List<KeyValuePair<string, Stream>> streams)
        {
            string endpoint = string.Format("Remote/AddPackages?sessionGuid={0}", sessionGuid);

            using (HttpClient client = BuildClient())
            {
                MultipartFormDataContent form = new MultipartFormDataContent();

                foreach (KeyValuePair<string, Stream> keyValuePair in streams)
                {
                    form.Add(new StreamContent(keyValuePair.Value), "none", keyValuePair.Key);
                }

                HttpResponseMessage response = client.PostAsync(endpoint, form).Result;
            }
        }

        public static void AddPackageAsync(string sessionGuid, Stream stream, string filename)
        {
            string endpoint = string.Format("Remote/AddPackages?sessionGuid={0}", sessionGuid);

            using (HttpClient client = BuildClient())
            {
                MultipartFormDataContent form = new MultipartFormDataContent();

                form.Add(new StreamContent(stream), "none", filename);

                HttpResponseMessage response = client.PostAsync(endpoint, form).Result;
            }
        }

        public static bool Install(string sessionGuid, out SortedList<string, dynamic> response)
        {
            string endpoint = string.Format("Remote/Install?sessionGuid={0}", sessionGuid);

            bool success = false;

            JavaScriptSerializer jsonSer = new JavaScriptSerializer();

            response = null;

            using (HttpClient client = BuildClient())
            {
                try
                {
                    HttpResponseMessage httpResponse = client.GetAsync(endpoint).Result;

                    if (httpResponse.StatusCode.Equals(HttpStatusCode.OK))
                    {
                        success = true;
                        string json = httpResponse.Content.ReadAsStringAsync().Result;
                        response = jsonSer.Deserialize<SortedList<string, dynamic>>(json);
                    }
                }
                catch (Exception ex)
                {
                    // Nothing to do.
                }

                // Always fail.
                return false;
            }
        }
    }
}
