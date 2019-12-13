// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
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
            var match = RegexStandardFile.Match(requestPath.ToLowerInvariant());
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
