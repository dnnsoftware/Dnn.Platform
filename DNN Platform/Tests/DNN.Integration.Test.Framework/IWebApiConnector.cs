﻿// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;

namespace DNN.Integration.Test.Framework
{
    public interface IWebApiConnector
    {
        int UserId { get; }
        string UserName { get; }
        bool IsLoggedIn { get; }
        CookieContainer SessionCookies { get; }
        DateTime LoggedInAtTime { get;  }
        TimeSpan Timeout { get; set; }
        Uri Domain { get; }
        string UserAgentValue { get; set; }

        void Logout();
        bool Login(string password);

        HttpResponseMessage UploadUserFile(string fileName, bool waitHttpResponse = true, int userId = -1);
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
