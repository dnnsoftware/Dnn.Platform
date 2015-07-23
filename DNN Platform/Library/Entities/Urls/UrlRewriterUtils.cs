﻿#region Copyright

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

            LogController.Instance.AddLog(log);
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

                    //Log this error in lig4net
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