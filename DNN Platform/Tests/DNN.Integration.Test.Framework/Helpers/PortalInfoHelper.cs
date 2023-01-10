// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DNN.Integration.Test.Framework.Helpers
{
    using System;
    using System.Linq;

    using DotNetNuke.Entities.Portals;

    public class PortalInfoHelper
    {
        public static PortalInfo GetPortalInfo(int portalId)
        {
            return DatabaseHelper.ExecuteStoredProcedure<PortalInfo>("GetPortals", "en-US").First(p => p.PortalID == portalId);
        }

        public static void UpdatePortalInfo(PortalInfo portal)
        {
            DatabaseHelper.ExecuteStoredProcedure(
                "UpdatePortalInfo",
                portal.PortalID,
                portal.PortalGroupID,
                portal.PortalName,
                portal.LogoFile,
                portal.FooterText,
                DBNull.Value, // portal expiry date
                portal.UserRegistration,
                portal.BannerAdvertising,
                portal.Currency,
                portal.AdministratorId,
                portal.HostFee,
                portal.HostSpace,
                portal.PageQuota,
                portal.UserQuota,
                portal.PaymentProcessor,
                portal.ProcessorUserId,
                portal.ProcessorPassword,
                portal.Description,
                portal.KeyWords,
                portal.BackgroundFile,
                portal.SiteLogHistory,
                portal.SplashTabId,
                portal.HomeTabId,
                portal.LoginTabId,
                portal.RegisterTabId,
                portal.UserTabId,
                portal.SearchTabId,
                portal.Custom404TabId,
                portal.Custom500TabId,
                portal.TermsTabId,
                portal.PrivacyTabId,
                portal.DefaultLanguage,
                portal.HomeDirectory,
                portal.LastModifiedByUserID,
                portal.CultureCode);
        }
    }
}
