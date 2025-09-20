// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.MvcPipeline.Attributes
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web;
    using System.Web.Mvc;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Web.MvcPipeline.Configuration;

    /// <summary>
    /// Rate limiting attribute that can be applied to controller actions to limit requests globally.
    /// </summary>
    public class RateLimitAttribute : ActionFilterAttribute
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(RateLimitAttribute));
        private static readonly object LockObject = new object();
        private static readonly ConcurrentDictionary<string, List<DateTime>> RequestHistory = new ConcurrentDictionary<string, List<DateTime>>();

        /// <summary>
        /// Initializes a new instance of the <see cref="RateLimitAttribute"/> class.
        /// </summary>
        /// <param name="maxRequests">Maximum number of requests allowed per time window. Use -1 to use configuration settings.</param>
        /// <param name="timeWindowMinutes">Time window in minutes for the rate limit. Use -1 to use configuration settings.</param>
        /// <param name="configurationKey">Configuration key prefix for getting settings from host/portal settings.</param>
        public RateLimitAttribute(int maxRequests = -1, int timeWindowMinutes = -1, string configurationKey = "CSPReport")
        {
            this.MaxRequests = maxRequests;
            this.TimeWindowMinutes = timeWindowMinutes;
            this.ConfigurationKey = configurationKey;
        }

        /// <summary>
        /// Gets the maximum number of requests allowed per time window.
        /// </summary>
        public int MaxRequests { get; }

        /// <summary>
        /// Gets the time window in minutes for the rate limit.
        /// </summary>
        public int TimeWindowMinutes { get; }

        /// <summary>
        /// Gets the configuration key prefix for settings.
        /// </summary>
        public string ConfigurationKey { get; }

        /// <inheritdoc/>
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            // Check if rate limiting is enabled
            if (!this.IsRateLimitingEnabled())
            {
                base.OnActionExecuting(filterContext);
                return;
            }

            // Check if global rate limit is exceeded
            if (!this.IsRequestAllowed())
            {
                Logger.Warn("Global rate limit exceeded for CSP reports");
                
                filterContext.Result = new HttpStatusCodeResult(429, "Too Many Requests");
                return;
            }

            base.OnActionExecuting(filterContext);
        }


        private bool IsRateLimitingEnabled()
        {
            if (this.ConfigurationKey == "CSPReport")
            {
                return RateLimitSettings.CspReportRateLimitEnabled;
            }

            // Default implementation for other configuration keys
            return true;
        }

        private int GetEffectiveMaxRequests()
        {
            if (this.MaxRequests > 0)
            {
                return this.MaxRequests;
            }

            if (this.ConfigurationKey == "CSPReport")
            {
                return RateLimitSettings.CspReportMaxRequests;
            }

            return 10; // Default fallback
        }

        private int GetEffectiveTimeWindowMinutes()
        {
            if (this.TimeWindowMinutes > 0)
            {
                return this.TimeWindowMinutes;
            }

            if (this.ConfigurationKey == "CSPReport")
            {
                return RateLimitSettings.CspReportTimeWindowMinutes;
            }

            return 1; // Default fallback
        }

        private bool IsRequestAllowed()
        {
            try
            {
                var maxRequests = this.GetEffectiveMaxRequests();
                var timeWindowMinutes = this.GetEffectiveTimeWindowMinutes();
                var now = DateTime.UtcNow;
                var key = this.ConfigurationKey.ToLowerInvariant();

                lock (LockObject)
                {
                    // Get or create the request history for this configuration key
                    var requestTimes = RequestHistory.GetOrAdd(key, k => new List<DateTime>());

                    // Remove requests outside the time window
                    var windowStart = now.AddMinutes(-timeWindowMinutes);
                    requestTimes.RemoveAll(t => t <= windowStart);

                    // Check if limit is exceeded
                    if (requestTimes.Count >= maxRequests)
                    {
                        return false;
                    }

                    // Add current request
                    requestTimes.Add(now);

                    // Cleanup old entries from other keys periodically (every 100th request)
                    if (requestTimes.Count % 100 == 0)
                    {
                        this.CleanupOldEntries();
                    }

                    return true;
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Error checking global rate limit", ex);
                // Allow request on error to avoid blocking legitimate traffic
                return true;
            }
        }

        private void CleanupOldEntries()
        {
            try
            {
                var cutoffTime = DateTime.UtcNow.AddHours(-1); // Clean entries older than 1 hour
                var keysToRemove = new List<string>();

                foreach (var kvp in RequestHistory)
                {
                    // Remove old entries from the list
                    kvp.Value.RemoveAll(t => t < cutoffTime);
                    
                    // If no entries remain, mark the key for removal
                    if (kvp.Value.Count == 0)
                    {
                        keysToRemove.Add(kvp.Key);
                    }
                }

                // Remove empty entries
                foreach (var key in keysToRemove)
                {
                    RequestHistory.TryRemove(key, out _);
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Error during cleanup of old rate limit entries", ex);
            }
        }

    }
}
