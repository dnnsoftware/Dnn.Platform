// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DNN.Integration.Test.Framework
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Http;

    public interface IWebApiConnector
    {
        int UserId { get; }

        string UserName { get; }

        bool IsLoggedIn { get; }

        CookieContainer SessionCookies { get; }

        DateTime LoggedInAtTime { get; }

        Uri Domain { get; }

        TimeSpan Timeout { get; set; }

        string UserAgentValue { get; set; }

        bool AvoidCaching { get; set; }

        void Logout();

        bool Login(string password);

        HttpResponseMessage UploadUserFile(string fileName, bool waitHttpResponse = true, int userId = -1);

        HttpResponseMessage ActivityStreamUploadUserFile(IDictionary<string, string> headers, string fileName);

        bool UploadCmsFile(string fileName, string portalFolder);

        HttpResponseMessage PostJson(
            string relativeUrl,
            object content, IDictionary<string, string> contentHeaders = null, bool waitHttpResponse = true, bool ignoreLoggedIn = false);

        HttpWebResponse PostUserForm(string relativeUrl, IDictionary<string, object> formFields,
            List<string> excludedInputPrefixes, bool checkUserLoggedIn = true, bool followRedirect = false);

        HttpWebResponse MultipartFormDataPost(
            string relativeUrl, IDictionary<string, object> postParameters, IDictionary<string, string> headers = null, bool followRedirect = false);

        HttpResponseMessage GetContent(
            string relativeUrl, object parameters, Dictionary<string, string> contentHeaders = null, bool waitHttpResponse = true, bool autoRedirect = true);

        HttpResponseMessage GetContent(
            string relativeUrl,
            Dictionary<string, string> contentHeaders = null, bool waitHttpResponse = true, bool autoRedirect = true);
    }
}
