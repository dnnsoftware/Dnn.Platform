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
#region Usings

using System;
using System.Web;

using DotNetNuke.Common;
using DotNetNuke.Common.Utils;
using DotNetNuke.Entities.Urls;
using DotNetNuke.HttpModules.UrlRewrite;

#endregion

namespace DotNetNuke.HttpModules.RequestFilter
{
    public class RequestFilterModule : IHttpModule
    {
        private const string InstalledKey = "httprequestfilter.attemptedinstall";

        #region IHttpModule Members

        /// <summary>
        /// Implementation of <see cref="IHttpModule"/>
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

        #endregion

        private static void FilterRequest(object sender, EventArgs e)
        {
            var app = (HttpApplication) sender;
            if ((app == null) || (app.Context == null) || (app.Context.Items == null))
            {
                return;
            }
            var request = app.Context.Request;

            if (!Initialize.ProcessHttpModule(request, true, true))
            {
                return;
            }

            //Carry out first time initialization tasks
            Initialize.Init(app);
			
            //only do this if we havn't already attempted an install.  This prevents PreSendRequestHeaders from
            //trying to add this item way to late.  We only want the first run through to do anything.
            //also, we use the context to store whether or not we've attempted an add, as it's thread-safe and
            //scoped to the request.  An instance of this module can service multiple requests at the same time,
            //so we cannot use a member variable.
            if (!app.Context.Items.Contains(InstalledKey))
            {
                //log the install attempt in the HttpContext
                //must do this first as several IF statements
                //below skip full processing of this method
                app.Context.Items.Add(InstalledKey, true);
                var settings = RequestFilterSettings.GetSettings();
                if ((settings == null || settings.Rules.Count == 0 || !settings.Enabled))
                {
                    return;
                }
                foreach (var rule in settings.Rules)
                {
                    //Added ability to determine the specific value types for addresses
                    //this check was necessary so that your rule could deal with IPv4 or IPv6
                    //To use this mode, add ":IPv4" or ":IPv6" to your servervariable name.
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
                    if ((!string.IsNullOrEmpty(varVal)))
                    {
                        if ((rule.Matches(varVal)))
                        {
                            rule.Execute();
                        }
                    }
                }
            }
        }
    }
}
