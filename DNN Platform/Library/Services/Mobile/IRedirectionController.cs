﻿#region Usings

using System;
using System.Collections.Generic;
using System.Web;

#endregion

namespace DotNetNuke.Services.Mobile
{
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
