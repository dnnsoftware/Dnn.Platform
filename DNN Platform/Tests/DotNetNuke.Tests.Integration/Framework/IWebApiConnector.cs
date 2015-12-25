﻿using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;

namespace DotNetNuke.Tests.Integration.Framework
{
    public interface IWebApiConnector
    {
        int UserId { get; }
        string UserName { get; }
        bool IsLoggedIn { get; }
        CookieContainer SessionCookies { get; }
        DateTime LoggedInAtTime { get;  }
        TimeSpan Timeout { get; set; }
        string UserAgentValue { get; set; }

        void Logout();
        bool Login(string password);

        HttpResponseMessage UploadUserFile(string fileName, bool waitHttpResponse = true);
        HttpResponseMessage ActivityStreamUploadUserFile(IDictionary<string, string> headers, string fileName);

        bool UploadCmsFile(string fileName, string portalFolder);

        HttpResponseMessage PostJson(string relativeUrl,
            object content, IDictionary<string, string> contentHeaders = null, bool waitHttpResponse = true, bool ignoreLoggedIn = false);

        HttpWebResponse PostUserForm(string relativeUrl, IDictionary<string, object> formFields,
            List<string> excludedInputPrefixes, bool checkUserLoggedIn = true, bool followRedirect = false);

        HttpWebResponse MultipartFormDataPost(
            string relativeUrl, IDictionary<string, object> postParameters, IDictionary<string, string> headers = null, bool followRedirect = false);

        HttpResponseMessage GetContent(
            string relativeUrl, object parameters, Dictionary<string, string> contentHeaders = null, bool waitHttpResponse = true, bool autoRedirect = true);

        HttpResponseMessage GetContent(string relativeUrl,
            Dictionary<string, string> contentHeaders = null, bool waitHttpResponse = true, bool autoRedirect = true);

        bool AvoidCaching { get; set; }
    }
}
