// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Entities.Urls
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Web;

    using DotNetNuke.Common;
    using DotNetNuke.Entities.Controllers;

    public class RewriterUtils
    {
        internal static void RewriteUrl(HttpContext context, string sendToUrl)
        {
            // first strip the querystring, if any
            var queryString = string.Empty;
            string sendToUrlLessQString = sendToUrl;
            if (sendToUrl.IndexOf("?", StringComparison.Ordinal) > 0)
            {
                sendToUrlLessQString = sendToUrl.Substring(0, sendToUrl.IndexOf("?", StringComparison.Ordinal));
                queryString = sendToUrl.Substring(sendToUrl.IndexOf("?", StringComparison.Ordinal) + 1);

                // Encode querystring values to support unicode characters by M.Kermani
                var parameters = new List<string>();
                foreach (var parameter in queryString.Split('&'))
                {
                    var i = parameter.IndexOf('=');
                    if (i >= 0)
                    {
                        var value = HttpUtility.UrlEncode(HttpUtility.UrlDecode(parameter.Substring(i + 1)));
                        parameters.Add(parameter.Substring(0, i) + "=" + value);
                    }
                    else
                    {
                        parameters.Add(parameter);
                    }
                }

                queryString = string.Join("&", parameters);
            }

            // rewrite the path..
            context.RewritePath(sendToUrlLessQString, string.Empty, queryString);

            // NOTE!  The above RewritePath() overload is only supported in the .NET Framework 1.1
            // If you are using .NET Framework 1.0, use the below form instead:
            // context.RewritePath(sendToUrl);
        }

        internal static string ResolveUrl(string appPath, string url)
        {
            // String is Empty, just return Url
            if (string.IsNullOrEmpty(url))
            {
                return url;
            }

            // String does not contain a ~, so just return Url
            if (url.StartsWith("~") == false)
            {
                return url;
            }

            // There is just the ~ in the Url, return the appPath
            if (url.Length == 1)
            {
                return appPath;
            }

            var seperatorChar = url.ToCharArray()[1];
            if (seperatorChar == '/' || seperatorChar == '\\')
            {
                // Url looks like ~/ or ~\
                if (appPath.Length > 1)
                {
                    return appPath + "/" + url.Substring(2);
                }

                return "/" + url.Substring(2);
            }

            // Url look like ~something
            if (appPath.Length > 1)
            {
                return appPath + "/" + url.Substring(1);
            }

            return appPath + url.Substring(1);
        }

        internal static bool OmitFromRewriteProcessing(string localPath)
        {
            var omitSettings = string.Empty;
            if (Globals.Status == Globals.UpgradeStatus.None)
            {
                omitSettings = HostController.Instance.GetString("OmitFromRewriteProcessing");
            }

            if (string.IsNullOrEmpty(omitSettings))
            {
                omitSettings = "scriptresource.axd|webresource.axd|gif|ico|jpg|jpeg|png|css|js";
            }

            omitSettings = omitSettings.ToLowerInvariant();
            localPath = localPath.ToLowerInvariant();

            var omissions = omitSettings.Split(new[] { '|' });

            bool shouldOmit = omissions.Any(x => localPath.EndsWith(x));

            if (!shouldOmit)
            {
                shouldOmit = Globals.ServicesFrameworkRegex.IsMatch(localPath);
            }

            return shouldOmit;
        }
    }
}
