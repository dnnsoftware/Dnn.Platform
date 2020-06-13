// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Mobile
{
    using System;
    using System.Collections.Generic;
    using System.Web;

    public interface IRedirectionController
    {
        void Save(IRedirection redirection);

        void PurgeInvalidRedirections(int portalId);

        void Delete(int portalId, int id);

        void DeleteRule(int portalId, int redirectionId, int ruleId);

        IList<IRedirection> GetAllRedirections();

        IList<IRedirection> GetRedirectionsByPortal(int portalId);

        IRedirection GetRedirectionById(int portalId, int id);

        string GetRedirectUrl(string userAgent, int portalId, int currentTabId);

        string GetRedirectUrl(string userAgent);

        string GetFullSiteUrl();

        string GetFullSiteUrl(int portalId, int currentTabId);

        string GetMobileSiteUrl();

        string GetMobileSiteUrl(int portalId, int currentTabId);

        bool IsRedirectAllowedForTheSession(HttpApplication app);
    }
}
