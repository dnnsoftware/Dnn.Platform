using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
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

            return client;
        }

        public static async Task<Dictionary<string, dynamic>> CIInstall(List<KeyValuePair<string, Stream>> streams)
        {
            string endpoint = "CI/Install";

            using (HttpClient client = BuildClient())
            {
                MultipartFormDataContent form = new MultipartFormDataContent();

                foreach (KeyValuePair<string, Stream> keyValuePair in streams)
                {
                    form.Add(new StreamContent(keyValuePair.Value), "none", keyValuePair.Key);
                }

                HttpResponseMessage response = await client.PostAsync(endpoint, form);

                string json = await response.Content.ReadAsStringAsync();

                JavaScriptSerializer jsonSer = new JavaScriptSerializer();

                return jsonSer.Deserialize<Dictionary<string, dynamic>>(json);
            }
        }
    }
}
