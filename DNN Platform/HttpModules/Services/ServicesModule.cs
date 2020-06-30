// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.HttpModules.Services
{
    using System;
    using System.Text.RegularExpressions;
    using System.Web;

    using DotNetNuke.Common;

    public class ServicesModule : IHttpModule
    {
        public static readonly Regex ServiceApi = Globals.ServicesFrameworkRegex;

        public void Init(HttpApplication context)
        {
            context.BeginRequest += InitDnn;

            context.PreSendRequestHeaders += this.OnPreSendRequestHeaders;
        }

        public void Dispose()
        {
        }

        private static void InitDnn(object sender, EventArgs e)
        {
            var app = sender as HttpApplication;
            if (app != null && ServiceApi.IsMatch(app.Context.Request.RawUrl.ToLowerInvariant()))
            {
                Initialize.Init(app);
            }
        }

        private void OnPreSendRequestHeaders(object sender, EventArgs e)
        {
            var app = sender as HttpApplication;
            if (app != null)
            {
                // WEB API should not send cookies and other specific headers in repsone;
                // they reveal too much info and are security risk
                var headers = app.Response.Headers;
                headers.Remove("Server");

                // DNN-8325
                // if (ServiceApi.IsMatch(app.Context.Request.RawUrl.ToLowerInvariant()))
                // {
                //    headers.Remove("Set-Cookie");
                // }
            }
        }
    }
}
