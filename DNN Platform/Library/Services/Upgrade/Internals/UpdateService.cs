// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Services.Upgrade.Internals
{
    using System;
    using System.IO;
    using System.Net;

    using DotNetNuke.Application;
    using DotNetNuke.Common;

    public class UpdateService
    {
        private static string ApplicationVersion
        {
            get
            {
                var version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
                return Globals.FormatVersion(version, "00", 3, string.Empty);
            }
        }

        private static string ApplicationName
        {
            get
            {
                return DotNetNukeContext.Current.Application.Name;
            }
        }

        public static StreamReader GetLanguageList()
        {
            string url = DotNetNukeContext.Current.Application.UpgradeUrl + "/languages.aspx";
            url += "?core=" + ApplicationVersion;
            url += "&type=Framework";
            url += "&name=" + ApplicationName;

            StreamReader myResponseReader = GetResponseAsStream(url);
            return myResponseReader;
        }

        public static string GetLanguageDownloadUrl(string cultureCode)
        {
            string url = DotNetNukeContext.Current.Application.UpgradeUrl + "/languages.aspx";
            url += "?core=" + ApplicationVersion;
            url += "&type=Framework";
            url += "&name=" + ApplicationName;
            url += "&culture=" + cultureCode;

            StreamReader myResponseReader = GetResponseAsStream(url);
            string downloadUrl = myResponseReader.ReadToEnd();
            return downloadUrl;
        }

        private static StreamReader GetResponseAsStream(string url)
        {
            // creating the proxy for the service call using the HttpWebRequest class
            var webReq = (HttpWebRequest)WebRequest.Create(url);

            // Set the method/action type
            webReq.Method = "GET";

            // We use form contentType
            webReq.ContentType = "text/xml; charset=utf-8";

            // Get the response handle, we have no true response yet!
            var webResp = (HttpWebResponse)webReq.GetResponse();

            // Now, we read the response (the string), and output it.
            Stream myResponse = webResp.GetResponseStream();

            // read the stream into streamreader
            var myResponseReader = new StreamReader(myResponse);
            return myResponseReader;
        }
    }
}
