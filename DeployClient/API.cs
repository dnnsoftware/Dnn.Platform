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
        private static string APIKey = Properties.Settings.Default.APIKey;

        private static HttpClient BuildClient()
        {
            HttpClient client = new HttpClient();

            client.BaseAddress = new Uri(new Uri(Properties.Settings.Default.TargetUri), "DesktopModules/PolyDeploy/API/");
            client.DefaultRequestHeaders.Add("x-api-key", Properties.Settings.Default.APIKey);
            client.Timeout = TimeSpan.FromMinutes(5);

            return client;
        }

        public static string CreateSession()
        {
            string endpoint = "CI/CreateSession";

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

        public static void AddPackages(string session, List<KeyValuePair<string, Stream>> streams)
        {
            string endpoint = string.Format("CI/AddPackages?session={0}", session);

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

        public static void AddPackageAsync(string session, Stream stream, string filename)
        {
            string endpoint = string.Format("CI/AddPackages?session={0}", session);

            using (HttpClient client = BuildClient())
            {
                MultipartFormDataContent form = new MultipartFormDataContent();

                form.Add(new StreamContent(stream), "none", filename);

                HttpResponseMessage response = client.PostAsync(endpoint, form).Result;
            }
        }

        public static Dictionary<string, dynamic> Install(string session)
        {
            string endpoint = string.Format("CI/Install?session={0}", session);

            JavaScriptSerializer jsonSer = new JavaScriptSerializer();

            string json = "{}";

            using (HttpClient client = BuildClient())
            {
                try
                {
                    HttpResponseMessage response = client.GetAsync(endpoint).Result;

                    Console.WriteLine(response.RequestMessage.RequestUri);

                    if (response.StatusCode.Equals(HttpStatusCode.OK))
                    {
                        json = response.Content.ReadAsStringAsync().Result;
                    }
                    else
                    {
                        throw new Exception(string.Format("Received status code: {0}", response.StatusCode.ToString()));
                    }

                    Console.WriteLine(json);
                }
                catch (Exception ex)
                {
                    throw new Exception("CIInstall failure", ex);
                }

                return jsonSer.Deserialize<Dictionary<string, dynamic>>(json);
            }
        }
    }
}
