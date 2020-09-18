// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Entities.Urls
{
    using System;
    using System.Reflection;
    using System.Text.RegularExpressions;
    using System.Web;
    using System.Web.Caching;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Services.Cache;
    using DotNetNuke.Services.Exceptions;
    using DotNetNuke.Services.Log.EventLog;

    public static class UrlRewriterUtils
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(UrlRewriterUtils));

        /// <summary>
        /// Return a FriendlyUrlOptions object from the provider settings.
        /// </summary>
        /// <param name="settings"></param>
        /// <returns></returns>
        public static FriendlyUrlOptions GetOptionsFromSettings(FriendlyUrlSettings settings)
        {
            var options = new FriendlyUrlOptions
            {
                PunctuationReplacement = (settings.ReplaceSpaceWith != FriendlyUrlSettings.ReplaceSpaceWithNothing)
                                                    ? settings.ReplaceSpaceWith
                                                    : string.Empty,
                SpaceEncoding = settings.SpaceEncodingValue,
                MaxUrlPathLength = 200,
                ConvertDiacriticChars = settings.AutoAsciiConvert,
                RegexMatch = settings.RegexMatch,
                IllegalChars = settings.IllegalChars,
                ReplaceChars = settings.ReplaceChars,
                ReplaceDoubleChars = settings.ReplaceDoubleChars,
                ReplaceCharWithChar = settings.ReplaceCharacterDictionary,
                PageExtension = (settings.PageExtensionUsageType == PageExtensionUsageType.Never)
                                            ? string.Empty
                                            : settings.PageExtension,
            };
            return options;
        }

        /// <summary>
        /// Return an extended FriendlyUrlOptions object for Custom URLs checkings.
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        public static FriendlyUrlOptions ExtendOptionsForCustomURLs(FriendlyUrlOptions options)
        {
            var result = new FriendlyUrlOptions
            {
                PunctuationReplacement = options.PunctuationReplacement,
                SpaceEncoding = options.SpaceEncoding,
                MaxUrlPathLength = options.MaxUrlPathLength,
                ConvertDiacriticChars = options.ConvertDiacriticChars,
                RegexMatch = options.RegexMatch.Replace("[^", "[^./"),
                IllegalChars = options.IllegalChars.Replace("/", string.Empty),
                ReplaceChars = options.ReplaceChars.Replace("/", string.Empty),
                ReplaceDoubleChars = options.ReplaceDoubleChars,
                ReplaceCharWithChar = options.ReplaceCharWithChar,
                PageExtension = options.PageExtension,
            };
            return result;
        }

        /// <summary>
        /// Logs the 404 error to a table for later checking.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="settings"></param>
        /// <param name="result"></param>
        public static void Log404(HttpRequest request, FriendlyUrlSettings settings, UrlAction result)
        {
            var log = new LogInfo
            {
                LogTypeKey = EventLogController.EventLogType.PAGE_NOT_FOUND_404.ToString(),
                LogPortalID = (result.PortalAlias != null) ? result.PortalId : -1,
            };
            log.LogProperties.Add(new LogDetailInfo("TabId", (result.TabId > 0) ? result.TabId.ToString() : string.Empty));
            log.LogProperties.Add(new LogDetailInfo("PortalAlias", (result.PortalAlias != null) ? result.PortalAlias.HTTPAlias : string.Empty));
            log.LogProperties.Add(new LogDetailInfo("OriginalUrl", result.RawUrl));

            if (request != null)
            {
                try
                {
                    if (request.UrlReferrer != null)
                    {
                        log.LogProperties.Add(new LogDetailInfo("Referer", request.UrlReferrer.AbsoluteUri));
                    }
                }
                catch (UriFormatException)
                {
                    log.LogProperties.Add(new LogDetailInfo("Referer", request.Headers["Referer"]));
                }

                log.LogProperties.Add(new LogDetailInfo("Url", request.Url.AbsoluteUri));
                log.LogProperties.Add(new LogDetailInfo("UserAgent", request.UserAgent));
                log.LogProperties.Add(new LogDetailInfo("HostAddress", request.UserHostAddress));
                log.LogProperties.Add(new LogDetailInfo("HostName", request.UserHostName));
            }

            LogController.Instance.AddLog(log);
        }

        /// <summary>
        /// logs an exception once per cache-lifetime.
        /// </summary>
        /// <param name="ex"></param>
        /// <param name="status"></param>
        /// <param name="result"></param>
        public static void LogExceptionInRequest(Exception ex, string status, UrlAction result)
        {
            if (ex != null)
            {
                // 831 : improve exception logging by logging custom properties instead of just the raw exception
                // this logic prevents a site logging an exception for every request made.  Instead
                // the exception will be logged once for the life of the cache / application restart or 1 hour, whichever is shorter.
                // create a cache key for this exception type
                string cacheKey = ex.GetType().ToString();

                // see if there is an existing object logged for this exception type
                object existingEx = DataCache.GetCache(cacheKey);
                if (existingEx == null)
                {
                    // if there was no existing object logged for this exception type, this is a new exception
                    DateTime expire = DateTime.Now.AddHours(1);
                    DataCache.SetCache(cacheKey, cacheKey, expire);

                    // just store the cache key - it doesn't really matter
                    // create a log event
                    var log = new LogInfo { LogTypeKey = "GENERAL_EXCEPTION" };
                    log.AddProperty("Url Processing Exception", "Exception in Url Rewriting Process");
                    log.AddProperty("Http Status", status);
                    if (result != null)
                    {
                        log.AddProperty("Original Path", result.OriginalPath ?? "null");
                        log.AddProperty("Raw Url", result.RawUrl ?? "null");
                        log.AddProperty("Final Url", result.FinalUrl ?? "null");

                        log.AddProperty("Rewrite Result", !string.IsNullOrEmpty(result.RewritePath)
                                                                     ? result.RewritePath
                                                                     : "[no rewrite]");
                        log.AddProperty("Redirect Location", string.IsNullOrEmpty(result.FinalUrl)
                                                                    ? "[no redirect]"
                                                                    : result.FinalUrl);
                        log.AddProperty("Action", result.Action.ToString());
                        log.AddProperty("Reason", result.Reason.ToString());
                        log.AddProperty("Portal Id", result.PortalId.ToString());
                        log.AddProperty("Tab Id", result.TabId.ToString());
                        log.AddProperty("Http Alias", result.PortalAlias != null
                                                                ? result.PortalAlias.HTTPAlias
                                                                : "Null");

                        if (result.DebugMessages != null)
                        {
                            int i = 1;
                            foreach (string msg in result.DebugMessages)
                            {
                                log.AddProperty("Debug Message " + i.ToString(), msg);
                                i++;
                            }
                        }
                    }
                    else
                    {
                        log.AddProperty("Result", "Result value null");
                    }

                    log.AddProperty("Exception Type", ex.GetType().ToString());
                    log.AddProperty("Message", ex.Message);
                    log.AddProperty("Stack Trace", ex.StackTrace);
                    if (ex.InnerException != null)
                    {
                        log.AddProperty("Inner Exception Message", ex.InnerException.Message);
                        log.AddProperty("Inner Exception Stacktrace", ex.InnerException.StackTrace);
                    }

                    log.BypassBuffering = true;
                    LogController.Instance.AddLog(log);

                    // Log this error in lig4net
                    Logger.Error(ex);
                }
            }
        }

        /// <summary>
        /// Clean Page name to remove page extension.
        /// </summary>
        /// <param name="value">page name.</param>
        /// <param name="settings">friendly url settings.</param>
        /// <param name="langParms">language.</param>
        /// <returns></returns>
        public static string CleanExtension(string value, FriendlyUrlSettings settings, string langParms)
        {
            bool replaced;
            return RewriteController.CleanExtension(value, settings, langParms, out replaced);
        }
    }
}
