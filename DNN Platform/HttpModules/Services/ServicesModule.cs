// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.HttpModules.Services
{
    using System;
    using System.Text.RegularExpressions;
    using System.Web;

    using DotNetNuke.Common;

    /// <summary>An <see cref="IHttpModule"/> for DNN's ASP.NET Web API Services Framework.</summary>
    public class ServicesModule : IHttpModule
    {
        /// <inheritdoc cref="Globals.ServicesFrameworkRegex"/>
        public static readonly Regex ServiceApi = Globals.ServicesFrameworkRegex;

        /// <inheritdoc/>
        public void Init(HttpApplication context)
        {
            context.BeginRequest += InitDnn;

            context.PreSendRequestHeaders += OnPreSendRequestHeaders;
        }

        /// <inheritdoc/>
        public void Dispose()
        {
        }

        private static void InitDnn(object sender, EventArgs e)
        {
            if (sender is HttpApplication app && ServiceApi.IsMatch(app.Context.Request.RawUrl.ToLowerInvariant()))
            {
                Initialize.Init(app);
            }
        }

        private static void OnPreSendRequestHeaders(object sender, EventArgs e)
        {
            if (sender is HttpApplication app)
            {
                // WEB API should not send cookies and other specific headers in response;
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
