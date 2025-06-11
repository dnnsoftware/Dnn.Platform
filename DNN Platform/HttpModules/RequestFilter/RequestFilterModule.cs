// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.HttpModules.RequestFilter
{
    using System;
    using System.Web;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utils;

    using Microsoft.Extensions.DependencyInjection;

    /// <summary>An <see cref="IHttpModule"/> to implement request filtering.</summary>
    public class RequestFilterModule : IHttpModule
    {
        private const string InstalledKey = "httprequestfilter.attemptedinstall";
        private readonly IHostSettings hostSettings;
        private readonly IApplicationStatusInfo appStatus;

        /// <summary>Initializes a new instance of the <see cref="RequestFilterModule"/> class.</summary>
        [Obsolete("Deprecated in DotNetNuke 10.0.2. Please use overload with IHostSettings. Scheduled removal in v12.0.0.")]
        public RequestFilterModule()
            : this(null, null)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="RequestFilterModule"/> class.</summary>
        /// <param name="hostSettings">The host settings.</param>
        /// <param name="appStatus">The application status.</param>
        public RequestFilterModule(IHostSettings hostSettings, IApplicationStatusInfo appStatus)
        {
            this.hostSettings = hostSettings ?? Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettings>();
            this.appStatus = appStatus ?? Globals.GetCurrentServiceProvider().GetRequiredService<IApplicationStatusInfo>();
        }

        /// <summary>Implementation of <see cref="IHttpModule"/>.</summary>
        /// <remarks>
        /// Currently empty.  Nothing to really do, as I have no member variables.
        /// </remarks>
        public void Dispose()
        {
        }

        /// <inheritdoc/>
        public void Init(HttpApplication context)
        {
            context.BeginRequest += this.FilterRequest;
        }

        private void FilterRequest(object sender, EventArgs e)
        {
            var app = (HttpApplication)sender;
            if (app?.Context?.Items == null)
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

            // only do this if we haven't already attempted an installation.  This prevents PreSendRequestHeaders from
            // trying to add this item way to late.  We only want the first run through to do anything.
            // also, we use the context to store whether we've attempted an add, as it's thread-safe and
            // scoped to the request.  An instance of this module can service multiple requests at the same time,
            // so we cannot use a member variable.
            if (app.Context.Items.Contains(InstalledKey))
            {
                return;
            }

            // log the installation attempt in the HttpContext
            // must do this first as several IF statements
            // below skip full processing of this method
            app.Context.Items.Add(InstalledKey, true);
            var settings = RequestFilterSettings.GetSettings(this.hostSettings, this.appStatus);
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
                    varVal = varArray[1] switch
                    {
                        "IPv4" => NetworkUtils.GetAddress(varVal, AddressType.IPv4),
                        "IPv6" => NetworkUtils.GetAddress(varVal, AddressType.IPv4),
                        _ => varVal,
                    };
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
