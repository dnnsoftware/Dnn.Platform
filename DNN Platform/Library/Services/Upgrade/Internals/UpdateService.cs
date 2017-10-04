#region Copyright

// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2017
// by DotNetNuke Corporation
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation 
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and 
// to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions 
// of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
// TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.

#endregion

using System;
using System.IO;
using System.Net;

using DotNetNuke.Application;
using DotNetNuke.Common;

namespace DotNetNuke.Services.Upgrade.Internals
{
    public class UpdateService
    {
        private static String ApplicationVersion
        {
            get
            {
                var version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
                return Globals.FormatVersion(version, "00", 3, "");
            }
        }

        private static String ApplicationName
        {
            get
            {
                return DotNetNukeContext.Current.Application.Name;
            }
        }


        public static StreamReader GetLanguageList()
        {
            String url = DotNetNukeContext.Current.Application.UpgradeUrl + "/languages.aspx";
            url += "?core=" + ApplicationVersion;
            url += "&type=Framework";
            url += "&name=" + ApplicationName;

            StreamReader myResponseReader = GetResponseAsStream(url);
            return myResponseReader;
        }

        public static String GetLanguageDownloadUrl(String cultureCode)
        {
            String url = DotNetNukeContext.Current.Application.UpgradeUrl + "/languages.aspx";
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
            //creating the proxy for the service call using the HttpWebRequest class
            var webReq = (HttpWebRequest) WebRequest.Create(url);

            //Set the method/action type
            webReq.Method = "GET";

            //We use form contentType
            webReq.ContentType = "text/xml; charset=utf-8";

            //Get the response handle, we have no true response yet!
            var webResp = (HttpWebResponse) webReq.GetResponse();

            //Now, we read the response (the string), and output it.
            Stream myResponse = webResp.GetResponseStream();

            //read the stream into streamreader
            var myResponseReader = new StreamReader(myResponse);
            return myResponseReader;
        }
    }
}