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
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;

using DotNetNuke.Common;
using DotNetNuke.Entities.Controllers;

#endregion

namespace DotNetNuke.Entities.Urls
{
    public class RewriterUtils
    {
        internal static void RewriteUrl(HttpContext context, string sendToUrl)
        {
            //first strip the querystring, if any
            var queryString = string.Empty;
            string sendToUrlLessQString = sendToUrl;
            if ((sendToUrl.IndexOf("?", StringComparison.Ordinal) > 0))
            {
                sendToUrlLessQString = sendToUrl.Substring(0, sendToUrl.IndexOf("?", StringComparison.Ordinal));
                queryString = sendToUrl.Substring(sendToUrl.IndexOf("?", StringComparison.Ordinal) + 1);
                
                //Encode querystring values to support unicode characters by M.Kermani
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
                queryString = String.Join("&", parameters);
                
            }
            
            //rewrite the path..
            context.RewritePath(sendToUrlLessQString, string.Empty, queryString);
            //NOTE!  The above RewritePath() overload is only supported in the .NET Framework 1.1
            //If you are using .NET Framework 1.0, use the below form instead:
            //context.RewritePath(sendToUrl);
        }

        internal static string ResolveUrl(string appPath, string url)
        {
            //String is Empty, just return Url
            if (String.IsNullOrEmpty(url))
            {
                return url;
            }
			
            //String does not contain a ~, so just return Url
            if ((url.StartsWith("~") == false))
            {
                return url;
            }
			
            //There is just the ~ in the Url, return the appPath
            if ((url.Length == 1))
            {
                return appPath;
            }
            var seperatorChar = url.ToCharArray()[1];
            if (seperatorChar == '/' || seperatorChar == '\\')
            {
                //Url looks like ~/ or ~\
                if ((appPath.Length > 1))
                {
                    return appPath + "/" + url.Substring(2);
                }
                return "/" + url.Substring(2);
            }
            //Url look like ~something
            if ((appPath.Length > 1))
            {
                return appPath + "/" + url.Substring(1);
            }
            return appPath + url.Substring(1);
        }

        static internal bool OmitFromRewriteProcessing(string localPath)
        {
            var omitSettings = String.Empty;
            if (Globals.Status == Globals.UpgradeStatus.None)
            {
                omitSettings = HostController.Instance.GetString("OmitFromRewriteProcessing");
            }

            if (string.IsNullOrEmpty(omitSettings)) {
		        omitSettings = "scriptresource.axd|webresource.axd|gif|ico|jpg|jpeg|png|css|js";
	        }
	        omitSettings = omitSettings.ToLowerInvariant();
	        localPath = localPath.ToLowerInvariant();

	        var omissions = omitSettings.Split(new[] { '|' });

            bool shouldOmit = omissions.Any(x => localPath.EndsWith(x));

            if(!shouldOmit)
            {
                shouldOmit = Globals.ServicesFrameworkRegex.IsMatch(localPath);
            }

            return shouldOmit;
        }
    }
}
