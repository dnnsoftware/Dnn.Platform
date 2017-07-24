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

        public static Dictionary<string, dynamic> CIInstall(List<KeyValuePair<string, Stream>> streams)
        {
            string endpoint = "CI/Install";

            JavaScriptSerializer jsonSer = new JavaScriptSerializer();

            string json = "{}";

            using (HttpClient client = BuildClient())
            {
                try
                {
                    MultipartFormDataContent form = new MultipartFormDataContent();

                    foreach (KeyValuePair<string, Stream> keyValuePair in streams)
                    {
                        form.Add(new StreamContent(keyValuePair.Value), "none", keyValuePair.Key);
                    }

                    HttpResponseMessage response = client.PostAsync(endpoint, form).Result;

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
