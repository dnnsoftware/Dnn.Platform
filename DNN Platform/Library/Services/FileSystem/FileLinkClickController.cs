// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.FileSystem
{
    using System;
    using System.Collections.Specialized;

    using DotNetNuke.Common;
    using DotNetNuke.Common.Internal;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Host;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Framework;

    public class FileLinkClickController : ServiceLocator<IFileLinkClickController, FileLinkClickController>, IFileLinkClickController
    {
        public string GetFileLinkClick(IFileInfo file)
        {
            Requires.NotNull("file", file);
            var portalId = file.PortalId;
            var linkClickPortalSettigns = this.GetPortalSettingsForLinkClick(portalId);

            return TestableGlobals.Instance.LinkClick(string.Format("fileid={0}", file.FileId), Null.NullInteger, Null.NullInteger, true, false, portalId, linkClickPortalSettigns.EnableUrlLanguage, linkClickPortalSettigns.PortalGUID);
        }

        public int GetFileIdFromLinkClick(NameValueCollection queryParams)
        {
            var linkClickPortalSettings = this.GetPortalSettingsForLinkClick(this.GetPortalIdFromLinkClick(queryParams));
            var strFileId = UrlUtils.DecryptParameter(queryParams["fileticket"], linkClickPortalSettings.PortalGUID);
            int fileId;
            return int.TryParse(strFileId, out fileId) ? fileId : -1;
        }

        protected override Func<IFileLinkClickController> GetFactory()
        {
            return () => new FileLinkClickController();
        }

        private LinkClickPortalSettings GetPortalSettingsForLinkClick(int portalId)
        {
            if (portalId == Null.NullInteger)
            {
                return new LinkClickPortalSettings
                {
                    PortalGUID = Host.GUID,
                    EnableUrlLanguage = Host.EnableUrlLanguage,
                };
            }

            var portalSettings = new PortalSettings(portalId);
            return new LinkClickPortalSettings
            {
                PortalGUID = portalSettings.GUID.ToString(),
                EnableUrlLanguage = portalSettings.EnableUrlLanguage,
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
                int portalId;
                if (int.TryParse(queryParams["portalid"], out portalId))
                {
                    return portalId;
                }
            }

            return PortalSettings.Current.PortalId;
        }
    }
}
