// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.HttpModules.RequestFilter
{
    using System;
    using System.Web;

    using DotNetNuke.Common;
    using DotNetNuke.Common.Utils;
    using DotNetNuke.Entities.Urls;
    using DotNetNuke.HttpModules.UrlRewrite;

    public class RequestFilterModule : IHttpModule
    {
        private const string InstalledKey = "httprequestfilter.attemptedinstall";

        /// <summary>
        /// Implementation of <see cref="IHttpModule"/>.
        /// </summary>
        /// <remarks>
        /// Currently empty.  Nothing to really do, as I have no member variables.
        /// </remarks>
        public void Dispose()
        {
        }

        public void Init(HttpApplication context)
        {
            context.BeginRequest += FilterRequest;
        }

        private static void FilterRequest(object sender, EventArgs e)
        {
            var app = (HttpApplication)sender;
            if ((app == null) || (app.Context == null) || (app.Context.Items == null))
            {
                return;
            }

            var request = app.Context.Request;

            if (!Initialize.ProcessHttpModule(request, true, true))
            {
                return;
            }

            // Carry out first time initialization tasks
            Initialize.Init(app);

            // only do this if we havn't already attempted an install.  This prevents PreSendRequestHeaders from
            // trying to add this item way to late.  We only want the first run through to do anything.
            // also, we use the context to store whether or not we've attempted an add, as it's thread-safe and
            // scoped to the request.  An instance of this module can service multiple requests at the same time,
            // so we cannot use a member variable.
            if (!app.Context.Items.Contains(InstalledKey))
            {
                // log the install attempt in the HttpContext
                // must do this first as several IF statements
                // below skip full processing of this method
                app.Context.Items.Add(InstalledKey, true);
                var settings = RequestFilterSettings.GetSettings();
                if (settings == null || settings.Rules.Count == 0 || !settings.Enabled)
                {
                    return;
                }

                foreach (var rule in settings.Rules)
                {
                    // Added ability to determine the specific value types for addresses
                    // this check was necessary so that your rule could deal with IPv4 or IPv6
                    // To use this mode, add ":IPv4" or ":IPv6" to your servervariable name.
                    var varArray = rule.ServerVariable.Split(':');
                    var varVal = request.ServerVariables[varArray[0]];
                    if (varArray[0].EndsWith("_ADDR", StringComparison.InvariantCultureIgnoreCase) && varArray.Length > 1)
                    {
                        switch (varArray[1])
                        {
                            case "IPv4":
                                varVal = NetworkUtils.GetAddress(varVal, AddressType.IPv4);
                                break;
                            case "IPv6":
                                varVal = NetworkUtils.GetAddress(varVal, AddressType.IPv4);
                                break;
                        }
                    }

                    if (!string.IsNullOrEmpty(varVal))
                    {
                        if (rule.Matches(varVal))
                        {
                            rule.Execute();
                        }
                    }
                }
            }
        }
    }
}
