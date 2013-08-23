using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

namespace DotNetNukeSetup
{
    public class UserManager : IDisposable
    {
        public const string RqVerifTokenName = "__RequestVerificationToken";
        private readonly Uri _domain;
        private readonly string _userName;
        private readonly string _password;
        private CookieContainer _sessionCookiesContainer;
        private readonly Cookie _cookieVerificationToken;
        private string _requestVerificationToken;

        public UserManager(string baseUrl, string name, string pass)
        {
            _domain = new Uri(baseUrl);
            _userName = name;
            _password = pass;
            _sessionCookiesContainer = new CookieContainer();
            _cookieVerificationToken = new Cookie(RqVerifTokenName, string.Empty, "/", _domain.Host);
        }

        public void Dispose()
        {
            Logout();
        }

        public bool Login()
        {
            var isLoggedIn = false;
            string requestUriString;
            HttpWebRequest httpWebRequest1;
            HttpWebResponse httpResponse;
            string input;

            requestUriString = string.Format("{0}/Login.aspx", _domain);
            httpWebRequest1 = (HttpWebRequest)WebRequest.Create(requestUriString);
            httpWebRequest1.Method = "GET";
            httpWebRequest1.KeepAlive = false;
            httpResponse = httpWebRequest1.GetResponse() as HttpWebResponse;
            using (var s = httpResponse.GetResponseStream())
            {
                input = new StreamReader(s, Encoding.UTF8).ReadToEnd();
            }

            if (httpResponse.StatusCode == HttpStatusCode.OK)
            {
                ExtractVerificationCookie(httpResponse.Headers["Set-Cookie"] ?? string.Empty);

                string str1 = "id=\"__VIEWSTATE\" value=\"";
                int startIndex1 = input.IndexOf(str1) + str1.Length;
                int num1 = input.IndexOf("\"", startIndex1);
                string str2 = input.Substring(startIndex1, num1 - startIndex1);
                string str3 = "id=\"__EVENTVALIDATION\" value=\"";
                int startIndex2 = input.IndexOf(str3) + str3.Length;
                int num2 = input.IndexOf("\"", startIndex2);
                string str4 = input.Substring(startIndex2, num2 - startIndex2);
                string str5 = Regex.Match(input, "name=\"(.+?\\$txtUsername)\"").Groups[1].Value;
                string str6 = Regex.Match(input, "name=\"(.+?\\$txtPassword)\"").Groups[1].Value;
                string str7 = Regex.Match(input, "'ScriptManager', 'Form', \\['(.+?)'").Groups[1].Value;
                string str8 =
                    Regex.Match(input, "WebForm_PostBackOptions\\(&quot;(.+?\\$cmdLogin)&quot;").Groups[1].Value;
                string str9 = Regex.Match(input, "id=\"(.+?_Login_UP)\"").Groups[1].Value;
                string str10 = string.Format(
                    @"ScriptManager={6}%7C{7}&__EVENTTARGET={7}&__EVENTARGUMENT=&__VIEWSTATE={2}&" +
                    @"__VIEWSTATEENCRYPTED=&__EVENTVALIDATION={3}&{4}={0}&{5}={1}&__ASYNCPOST=true&RadAJAXControlID={8}",
                    _userName, _password, HttpUtility.UrlEncode(str2), HttpUtility.UrlEncode(str4), HttpUtility.UrlEncode(str5),
                    HttpUtility.UrlEncode(str6), HttpUtility.UrlEncode(str7), HttpUtility.UrlEncode(str8), HttpUtility.UrlEncode(str9));
                byte[] bytes = Encoding.UTF8.GetBytes(str10);

                var httpWebRequest2 = (HttpWebRequest)WebRequest.Create(requestUriString) as HttpWebRequest;
                httpWebRequest2.Method = "POST";
                httpWebRequest2.KeepAlive = false;
                httpWebRequest2.ContentType = "application/x-www-form-urlencoded";
                httpWebRequest2.CookieContainer = _sessionCookiesContainer;
                httpWebRequest2.ContentLength = (long)bytes.Length;
                using (Stream requestStream = ((WebRequest)httpWebRequest2).GetRequestStream())
                {
                    requestStream.Write(bytes, 0, bytes.Length);
                }

                httpResponse = httpWebRequest2.GetResponse() as HttpWebResponse;
                using (var s = httpResponse.GetResponseStream())
                {
                    var resp = new StreamReader(s /*, Encoding.GetEncoding("GB2312")*/).ReadToEnd();
                    //_sessionCookiesContainer.Add(httpResponse.Cookies);
                    isLoggedIn =
                        (httpResponse.StatusCode == HttpStatusCode.OK) &&
                        (httpResponse.Cookies[".DOTNETNUKE"] != null);
                    //if (IsLoggedIn) GetDefaultPageToken();
                }
            }
            return isLoggedIn;
        }

        public void Logout()
        {
            try
            {
                string requestUriString = string.Format("{0}/Home/tabid/55/ctl/Logoff/Default.aspx", _domain);
                var wc = new WebClient();
                wc.DownloadString(requestUriString);
            }
            finally
            {
                _sessionCookiesContainer = new CookieContainer();
            }
        }

        private void ExtractVerificationCookie(string cookiesString)
        {
            string[] parts1 = cookiesString.Split(',');
            foreach (var part1 in parts1)
            {
                if (part1.Contains(RqVerifTokenName))
                {

                    var parts2 = part1.Split(';');
                    foreach (var part2 in parts2)
                    {
                        if (part2.Contains(RqVerifTokenName))
                        {
                            _cookieVerificationToken.Value = part2.Split('=')[1];
                        }
                        else if (part2.Contains("path"))
                        {
                            _cookieVerificationToken.Path = part2.Split('=')[1];
                        }
                    }
                    break;
                }
            }
        }

        public string GetDefaultPageToken()
        {
            if (string.IsNullOrEmpty(_requestVerificationToken))
            {
                HttpWebRequest httpWebRequest1;
                HttpWebResponse httpResponse;
                string data;

                httpWebRequest1 = (HttpWebRequest)WebRequest.Create(_domain); // get from root
                httpWebRequest1.Method = "GET";
                httpWebRequest1.KeepAlive = false;
                httpWebRequest1.CookieContainer = _sessionCookiesContainer;
                httpResponse = httpWebRequest1.GetResponse() as HttpWebResponse;
                using (var s = httpResponse.GetResponseStream())
                {
                    data = new StreamReader(s, Encoding.UTF8).ReadToEnd();
                }

                if (httpResponse.StatusCode == HttpStatusCode.OK)
                {
                    var str1 = "input name=\"__RequestVerificationToken\" type=\"hidden\" value=\"";
                    var startIndex1 = data.IndexOf(str1) + str1.Length;
                    var num1 = data.IndexOf("\"", startIndex1);
                    _requestVerificationToken = data.Substring(startIndex1, num1 - startIndex1);
                }
            }

            return _requestVerificationToken;
        }

        public HttpResponseMessage UploadUserContentAsJson(string relativeUrl, object content, Dictionary<string, string> contentHeaders)
        {
            using (var client = GetUploadContentClient(relativeUrl))
            {
                var rqHeaders = client.DefaultRequestHeaders;
                rqHeaders.Add("RequestVerificationToken", _requestVerificationToken);
                if (contentHeaders != null)
                {
                    foreach (var hdr in contentHeaders)
                    {
                        rqHeaders.Add(hdr.Key, hdr.Value);
                    }
                }
                string requestUriString = string.Format("{0}/{1}", _domain, relativeUrl);
                var uri = new Uri(requestUriString);
                var result = client.PostAsJsonAsync(uri.ToString(), content).Result;
                result.EnsureSuccessStatusCode();
                return result;
            }
        }

        private HttpClient GetUploadContentClient(string relativeUrl)
        {
            var clientHandler = new HttpClientHandler()
                {
                    CookieContainer = _sessionCookiesContainer
                };

            var client = new HttpClient(clientHandler);
            client.BaseAddress = _domain;
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
            var resultGet = client.GetAsync("/").Result;
            var data = resultGet.Content.ReadAsStringAsync().Result;
            var str1 = "input name=\"__RequestVerificationToken\" type=\"hidden\" value=\"";
            var startIndex1 = data.IndexOf(str1) + str1.Length;
            var num1 = data.IndexOf("\"", startIndex1);
            _requestVerificationToken = data.Substring(startIndex1, num1 - startIndex1);
            client.DefaultRequestHeaders.Add("__RequestVerificationToken", _requestVerificationToken);
            return client;
        }

    }
}
