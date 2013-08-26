#region Copyright
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2013
// by DotNetNuke Corporation
// All Rights Reserved
#endregion

using DotNetNuke.Services.Localization;
using DotNetNuke.Subscriptions.Components.Entities;

namespace DotNetNuke.Subscriptions.Components.Controllers.Internal
{
    public class TemplateSettingsReaderImpl : ITemplateSettingsReader
    {
        #region Implementation of ITemplateSettingsReader

        public TemplateSettings GetSettings()
        {
            return new TemplateSettings
                {
                    DigestFooter = GetValue("DigestFooter"),
                    DigestHeader = GetValue("DigestHeader"),
                    DigestSubject = GetValue("DigestSubject"),
                    DigestSummary = GetValue("DigestSummary"),
                    Item = GetValue("Item"),
                    ItemFooter = GetValue("ItemFooter"),
                    ItemHeader = GetValue("ItemHeader"),
                    InstantSubject = GetValue("InstantSubject"),
                    InstantFooter = GetValue("InstantFooter"),
                    InstantHeader = GetValue("InstantHeader"),
                    Term = GetValue("Term"),
                    TermsFooter = GetValue("TermsFooter"),
                    TermsHeader = GetValue("TermsHeader")
                };
        }

        #endregion

        #region Private members

        private const string ResourceFile =
            "~/DesktopModules/DNNCorp/Subscriptions/App_LocalResources/SharedResources.resx";

        #endregion

        #region Private methods

        private static string GetValue(string key)
        {
            return Localization.GetString(string.Format("Template_{0}", key), ResourceFile);
        }

        #endregion
    }
}