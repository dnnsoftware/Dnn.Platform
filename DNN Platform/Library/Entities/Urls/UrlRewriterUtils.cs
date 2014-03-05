#region Copyright

// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2014
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
using System.Reflection;
using System.Web;

using DotNetNuke.Common.Utilities;
using DotNetNuke.Instrumentation;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Services.Log.EventLog;

#endregion

namespace DotNetNuke.Entities.Urls
{
    public class UrlRewriterUtils
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(UrlRewriterUtils));

        /// <summary>
        /// Return a FriendlyUrlOptions object from the provider settings
        /// </summary>
        /// <param name="settings"></param>
        /// <returns></returns>
        public static FriendlyUrlOptions GetOptionsFromSettings(FriendlyUrlSettings settings)
        {
            var options = new FriendlyUrlOptions
                {
                    PunctuationReplacement = (settings.ReplaceSpaceWith != FriendlyUrlSettings.ReplaceSpaceWithNothing) 
                                                    ? settings.ReplaceSpaceWith 
                                                    : String.Empty,
                    SpaceEncoding = settings.SpaceEncodingValue,
                    MaxUrlPathLength = 200,
                    ConvertDiacriticChars = settings.AutoAsciiConvert,
                    RegexMatch = settings.RegexMatch,
                    IllegalChars = settings.IllegalChars,
                    ReplaceChars = settings.ReplaceChars,
                    ReplaceDoubleChars = settings.ReplaceDoubleChars,
                    ReplaceCharWithChar = settings.ReplaceCharacterDictionary,
                    PageExtension = (settings.PageExtensionUsageType == PageExtensionUsageType.Never) 
                                            ? "" 
                                            : settings.PageExtension
                };
            return options;
        }

        /// <summary>
        /// Return an extended FriendlyUrlOptions object for Custom URLs checkings
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
                IllegalChars = options.IllegalChars.Replace("/", ""),
                ReplaceChars = options.ReplaceChars.Replace("/", ""),
                ReplaceDoubleChars = options.ReplaceDoubleChars,
                ReplaceCharWithChar = options.ReplaceCharWithChar,
                PageExtension = options.PageExtension
            };
            return result;
        }

        /// <summary>
        /// Logs the 404 error to a table for later checking 
        /// </summary>
        /// <param name="request"></param>
        /// <param name="settings"></param>
        /// <param name="result"></param>
        public static void Log404(HttpRequest request, FriendlyUrlSettings settings, UrlAction result)
        {
            var controller = new LogController();
            var log = new LogInfo
                {
                    LogTypeKey = EventLogController.EventLogType.PAGE_NOT_FOUND_404.ToString(),
                    LogPortalID = (result.PortalAlias != null) ? result.PortalId : -1
                };
            log.LogProperties.Add(new LogDetailInfo("TabId", (result.TabId > 0) ? result.TabId.ToString() : String.Empty));
            log.LogProperties.Add(new LogDetailInfo("PortalAlias",  (result.PortalAlias != null) ? result.PortalAlias.HTTPAlias : String.Empty));
            log.LogProperties.Add(new LogDetailInfo("OriginalUrl",  result.RawUrl));

            if (request != null)
            {
                if (request.UrlReferrer != null)
                {
                    log.LogProperties.Add(new LogDetailInfo("Referer", request.UrlReferrer.AbsoluteUri));
                }
                log.LogProperties.Add(new LogDetailInfo("Url", request.Url.AbsoluteUri));
                log.LogProperties.Add(new LogDetailInfo("UserAgent", request.UserAgent));
                log.LogProperties.Add(new LogDetailInfo("HostAddress", request.UserHostAddress));
                log.LogProperties.Add(new LogDetailInfo("HostName", request.UserHostName));
            }

            controller.AddLog(log);
        }

        /// <summary>
        /// logs an exception once per cache-lifetime
        /// </summary>
        /// <param name="ex"></param>
        /// <param name="status"></param>
        /// <param name="result"></param>
        public static void LogExceptionInRequest(Exception ex, string status, UrlAction result)
        {
            if (ex != null)
            {
                //831 : improve exception logging by logging custom properties instead of just the raw exception
                //this logic prevents a site logging an exception for every request made.  Instead 
                //the exception will be logged once for the life of the cache / application restart or 1 hour, whichever is shorter.
                //create a cache key for this exception type
                string cacheKey = ex.GetType().ToString();
                //see if there is an existing object logged for this exception type
                object existingEx = DataCache.GetCache(cacheKey);
                if (existingEx == null)
                {
                    //if there was no existing object logged for this exception type, this is a new exception
                    DateTime expire = DateTime.Now.AddHours(1);
                    DataCache.SetCache(cacheKey, cacheKey, expire);
                    //just store the cache key - it doesn't really matter
                    //create a log event
                    var elc = new EventLogController();
                    var logEntry = new LogInfo { LogTypeKey = "GENERAL_EXCEPTION" };
                    logEntry.AddProperty("Url Processing Exception", "Exception in Url Rewriting Process");
                    logEntry.AddProperty("Http Status", status);
                    if (result != null)
                    {
                        logEntry.AddProperty("Original Path", result.OriginalPath ?? "null");
                        logEntry.AddProperty("Raw Url", result.RawUrl ?? "null");
                        logEntry.AddProperty("Final Url", result.FinalUrl ?? "null");

                        logEntry.AddProperty("Rewrite Result", !string.IsNullOrEmpty(result.RewritePath)
                                                                     ? result.RewritePath
                                                                     : "[no rewrite]");
                        logEntry.AddProperty("Redirect Location", string.IsNullOrEmpty(result.FinalUrl)
                                                                    ? "[no redirect]"
                                                                    : result.FinalUrl);
                        logEntry.AddProperty("Action", result.Action.ToString());
                        logEntry.AddProperty("Reason", result.Reason.ToString());
                        logEntry.AddProperty("Portal Id", result.PortalId.ToString());
                        logEntry.AddProperty("Tab Id", result.TabId.ToString());
                        logEntry.AddProperty("Http Alias", result.PortalAlias != null
                                                                ? result.PortalAlias.HTTPAlias
                                                                : "Null");

                        if (result.DebugMessages != null)
                        {
                            int i = 1;
                            foreach (string msg in result.DebugMessages)
                            {
                                logEntry.AddProperty("Debug Message " + i.ToString(), msg);
                                i++;
                            }
                        }
                    }
                    else
                    {
                        logEntry.AddProperty("Result", "Result value null");
                    }
                    logEntry.AddProperty("Exception Type", ex.GetType().ToString());
                    logEntry.AddProperty("Message", ex.Message);
                    logEntry.AddProperty("Stack Trace", ex.StackTrace);
                    if (ex.InnerException != null)
                    {
                        logEntry.AddProperty("Inner Exception Message", ex.InnerException.Message);
                        logEntry.AddProperty("Inner Exception Stacktrace", ex.InnerException.StackTrace);
                    }
                    logEntry.BypassBuffering = true;
                    elc.AddLog(logEntry);

                    //Log this error in lig4net
                    Logger.Error(ex);
                }
            }
        }

    }
}