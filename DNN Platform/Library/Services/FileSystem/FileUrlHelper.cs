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

using System;
using System.Collections.Specialized;
using System.Text.RegularExpressions;
using DotNetNuke.Common.Utilities;

namespace DotNetNuke.Services.FileSystem
{
    public class FileUrlHelper
    {
        private static readonly Regex RegexStandardFile =
            new Regex(@"^/portals/(?<portal>[0-9]+|_default)/(?<filePath>.*\.[a-z0-9]*)$", RegexOptions.Compiled);

        public static bool IsStandardFileURLFormat(string requestPath, out IFileInfo fileRequested)
        {
            var match = RegexStandardFile.Match(requestPath.ToLower());
            if (match.Success)
            {
                var filePath = match.Groups["filePath"].Value;
                var portal = match.Groups["portal"].Value;

                var portalId = Null.NullInteger;
                if (portal != "_default")
                {
                    portalId = int.Parse(portal);
                }
                fileRequested = FileManager.Instance.GetFile(portalId, filePath);
                return true;
            }
            fileRequested = null;
            return false;
        }

        public static bool IsLinkClickURLFormat(string requestPath, NameValueCollection requestQueryString, out IFileInfo fileRequested)
        {
            if (requestPath.EndsWith(@"/LinkClick.aspx", StringComparison.OrdinalIgnoreCase) && requestQueryString["fileticket"] != null)
            {
                int fileId = FileLinkClickController.Instance.GetFileIdFromLinkClick(requestQueryString);
                fileRequested = FileManager.Instance.GetFile(fileId);
                return true;
            }
            fileRequested = null;
            return false;
        }
    }
}
