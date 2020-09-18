// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Providers.Caching.SimpleWebFarmCachingProvider
{
    using System;
    using System.Linq;
    using System.Net;
    using System.Threading;

    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Controllers;
    using DotNetNuke.Entities.Host;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Services.Cache;
    using DotNetNuke.Services.Exceptions;

    using HttpWebRequest = System.Net.HttpWebRequest;

    public class SimpleWebFarmCachingProvider : CachingProvider
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(SimpleWebFarmCachingProvider));

        private readonly int executionTimeout = 5000; // Limit timeout to 5 seconds as cache operations should be quick

        public override void Clear(string type, string data)
        {
            // Clear the local cache
            this.ClearCacheInternal(type, data, true);

            // Per API implementation standards only notify others if expiration has not been desabled
            if (CacheExpirationDisable)
            {
                return;
            }

            // Notify other servers
            this.NotifyOtherServers("Clear~" + type, data);
        }

        public override void Remove(string key)
        {
            // Remove from local cache
            this.RemoveInternal(key);

            // Per API implementation standards only notify others if expiration has not been disabled
            if (CacheExpirationDisable)
            {
                return;
            }

            // Notify Other Servers
            this.NotifyOtherServers("Remove", key);
        }

        /// <summary>
        /// This method responds to an incoming request to process synchronization from an additional server.
        /// </summary>
        /// <remarks>
        /// This is internal as it should only be called from <see cref="SimpleWebFarmSynchronizationHandler"/>.
        /// </remarks>
        /// <param name="command">The command to process, currently supported Remove and Clear~{Type}.</param>
        /// <param name="detail">Additional detail to pass to the caching sub-system.</param>
        internal void ProcessSynchronizationRequest(string command, string detail)
        {
            // Handle basic removal
            if (command.StartsWith("remove", StringComparison.OrdinalIgnoreCase))
            {
                this.RemoveInternal(detail);
                return;
            }

            // A clear method will have additional type information included, split using the ~ character
            if (command.StartsWith("clear~", StringComparison.InvariantCultureIgnoreCase))
            {
                var commandParts = command.Split('~');
                this.ClearCacheInternal(commandParts[1], detail, true);
            }
        }

        private static void HandleNotificationTimeout(object state, bool timedOut)
        {
            if (!timedOut)
            {
                return;
            }

            // Abort if possible
            var request = (HttpWebRequest)state;
            request?.Abort();
        }

        private void NotifyOtherServers(string command, string detail)
        {
            // Do not send notifications to other servers if currently upgrading
            if (Globals.Status != Globals.UpgradeStatus.None)
            {
                return;
            }

            // Get all servers currently in the database that could be used for synchronization, excluding this one
            // But focus on only servers that could be used for this application and notifications
            // including activity within 60 minutes
            var lastActivityDate = DateTime.Now.AddHours(-1);
            var additionalServers = ServerController.GetEnabledServers()
                .Where(s => !string.IsNullOrWhiteSpace(s.Url)
                            && s.LastActivityDate >= lastActivityDate
                            && s.ServerName != Globals.ServerName)
                .ToList();

            // If we have no additional servers do nothing
            if (additionalServers.Count == 0)
            {
                return;
            }

            // Otherwise notify each server
            foreach (var server in additionalServers)
            {
                // Setup parameters for sending
                var commandParameter = Host.DebugMode ? command : UrlUtils.EncryptParameter(command, Host.GUID);
                var detailParameter = Host.DebugMode ? detail : UrlUtils.EncryptParameter(detail, Host.GUID);
                var protocol = HostController.Instance.GetBoolean("UseSSLForCacheSync", false) ? "https://" : "http://";
                var notificationUrl =
                    $"{protocol}{server.Url}/SimpleWebFarmSync.aspx?command={commandParameter}&detail={detailParameter}";

                // Build a webrequest
                var notificationRequest = WebRequest.CreateHttp(notificationUrl);

                // Create a cookie container so we can get cookies and use default credentials
                notificationRequest.CookieContainer = new CookieContainer();
                notificationRequest.UseDefaultCredentials = true;

                // Start the asynchronous request
                var result = notificationRequest.BeginGetResponse(this.OnServerNotificationCompleteCallback, notificationRequest);

                // Register timeout
                // TODO: Review possible use of async/await C# 7 implementation
                ThreadPool.RegisterWaitForSingleObject(result.AsyncWaitHandle, HandleNotificationTimeout, notificationRequest, this.executionTimeout, true);
            }
        }

        private void OnServerNotificationCompleteCallback(IAsyncResult asynchronousResult)
        {
            // Get the request from the state object
            var request = (HttpWebRequest)asynchronousResult.AsyncState;
            try
            {
                // Get the response
                using (var response = (HttpWebResponse)request.EndGetResponse(asynchronousResult))
                {
                    // If status code is ok do nothing
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        return;
                    }

                    // Otherwise log the failure
                    Exceptions.LogException(new ApplicationException(
                        $"Error sending cache server notification.  Url: {request.RequestUri.AbsoluteUri} with a status code {response.StatusCode}"));
                }
            }
            catch (WebException e)
            {
                if (e.Status != WebExceptionStatus.RequestCanceled)
                {
                    Exceptions.LogException(new Exception("Synchronization Error in Request: " + request.RequestUri.AbsoluteUri, e));
                }
            }
        }
    }
}
