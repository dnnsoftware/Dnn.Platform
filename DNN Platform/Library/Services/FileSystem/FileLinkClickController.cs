#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2013
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
using DotNetNuke.Common;
using DotNetNuke.Common.Internal;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Host;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Framework;

namespace DotNetNuke.Services.FileSystem
{
    public class FileLinkClickController : ServiceLocator<IFileLinkClickController, FileLinkClickController>, IFileLinkClickController
    {
        private LinkClickPortalSettings GetPortalSettingsForLinkClick(int portalId)
        {
            if (portalId == Null.NullInteger)
            {
                return new LinkClickPortalSettings
                    {
                        PortalGUID = Host.GUID,
                        EnableUrlLanguage = Host.EnableUrlLanguage
                    };            
            }
            var portalSettings = new PortalSettings(portalId);
            return new LinkClickPortalSettings
                {
                    PortalGUID = portalSettings.GUID.ToString(),
                    EnableUrlLanguage = portalSettings.EnableUrlLanguage
                };
        }

        private int GetPortalIdFromLinkClick(NameValueCollection queryParams)
        {
            if (queryParams["hf"] != null && queryParams["hf"] == "1")
            {
                return Null.NullInteger;
            }
            if (queryParams["portalid"] != null)
            {
                return Convert.ToInt32(queryParams["portalid"]);
            }
            return PortalSettings.Current.PortalId;
        }

        public string GetFileLinkClick(IFileInfo file)
        {
            Requires.NotNull("file", file);
            var portalId = file.PortalId;
            var linkClickPortalSettigns = GetPortalSettingsForLinkClick(portalId);
            
            return TestableGlobals.Instance.LinkClick(String.Format("fileid={0}", file.FileId), Null.NullInteger, Null.NullInteger, true, false, portalId, linkClickPortalSettigns.EnableUrlLanguage, linkClickPortalSettigns.PortalGUID);
        }

        public int GetFileIdFromLinkClick(NameValueCollection queryParams)
        {
            var linkClickPortalSettings = GetPortalSettingsForLinkClick(GetPortalIdFromLinkClick(queryParams));
            var strFileId = UrlUtils.DecryptParameter(queryParams["fileticket"], linkClickPortalSettings.PortalGUID);
            return Convert.ToInt32(strFileId);
        }

        protected override Func<IFileLinkClickController> GetFactory()
        {
            return () => new FileLinkClickController();
        }
    }

    class LinkClickPortalSettings
    {
        public string PortalGUID;
        public bool EnableUrlLanguage;
    }
}
