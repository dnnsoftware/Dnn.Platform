#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2018
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

using System.Collections.Generic;
using System.ComponentModel;
using System.IO;

using DotNetNuke.Common.Utilities;
using DotNetNuke.Data;
using DotNetNuke.Entities.Users;
using DotNetNuke.Services.Localization;
using DotNetNuke.Services.Log.EventLog;

#region Usings

using System;
using System.Collections;
using System.Xml;

using DotNetNuke.Entities.Tabs;
//using DotNetNuke.Services.Upgrade.Internals.InstallConfiguration;

#endregion

// ReSharper disable once CheckNamespace
namespace DotNetNuke.Entities.Portals
{
    /// <summary>
	/// PoralController provides business layer of poatal.
	/// </summary>
	/// <remarks>
	/// DotNetNuke supports the concept of virtualised sites in a single install. This means that multiple sites, 
	/// each potentially with multiple unique URL's, can exist in one instance of DotNetNuke i.e. one set of files and one database.
	/// </remarks>
    public partial class PortalController
    {
        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Deprecated in DotNetNuke 7.3.0. Use one of the alternate overloads")]
        public int CreatePortal(string portalName, string firstName, string lastName, string username, string password, string email,
                        string description, string keyWords, string templatePath, string templateFile, string homeDirectory,
                        string portalAlias, string serverPath, string childPath, bool isChildPortal)
        {
            var adminUser = new UserInfo
            {
                FirstName = firstName,
                LastName = lastName,
                Username = username,
                DisplayName = firstName + " " + lastName,
                Membership = {Password = password},
                Email = email,
                IsSuperUser = false
            };
            adminUser.Membership.Approved = true;
            adminUser.Profile.FirstName = firstName;
            adminUser.Profile.LastName = lastName;
            return CreatePortal(portalName, adminUser, description, keyWords, templatePath, templateFile, homeDirectory, portalAlias, serverPath, childPath, isChildPortal);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Deprecated in DotNetNuke 7.3.0. Use one of the alternate overloads")]
        public int CreatePortal(string portalName, UserInfo adminUser, string description, string keyWords, string templatePath,
                        string templateFile, string homeDirectory, string portalAlias,
                        string serverPath, string childPath, bool isChildPortal)
        {
            var template = GetPortalTemplate(Path.Combine(templatePath, templateFile), null);

            return CreatePortal(portalName, adminUser, description, keyWords, template, homeDirectory, portalAlias,
                                serverPath, childPath, isChildPortal);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Deprecated in DotNetNuke 7.3.0.")]
        public void DeletePortalInfo(int portalId)
        {
            UserController.DeleteUsers(portalId, false, true);

            DataProvider.Instance().DeletePortalInfo(portalId);

            EventLogController.Instance.AddLog("PortalId", portalId.ToString(), GetCurrentPortalSettingsInternal(), UserController.Instance.GetCurrentUserInfo().UserID, EventLogController.EventLogType.PORTALINFO_DELETED);

            DataCache.ClearHostCache(true);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Deprecated in DotNetNuke 7.3. Replaced by PortalController.Instance.GetCurrentPortalSettings")]
        public static PortalSettings GetCurrentPortalSettings()
        {
            return GetCurrentPortalSettingsInternal();
        }


        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Deprecated in DNN 7.4.0.  Replaced by PortalController.Instance.GetPortalSettings")]
        public static Dictionary<string, string> GetPortalSettingsDictionary(int portalId)
        {
            return Instance.GetPortalSettings(portalId);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Deprecated in DotNetNuke 7.3.0. Use one of the other overloads.")]
        public void ParseTemplate(int portalId, string templatePath, string templateFile, int administratorId, PortalTemplateModuleAction mergeTabs, bool isNewPortal)
        {
            ParseTemplateInternal(portalId, templatePath, templateFile, administratorId, mergeTabs, isNewPortal);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Deprecated in DotNetNuke 7.3.0. Use one of the other overloads.")]
        public void ParseTemplate(int portalId, string templatePath, string templateFile, int administratorId, PortalTemplateModuleAction mergeTabs, bool isNewPortal, out LocaleCollection localeCollection)
        {
            ParseTemplateInternal(portalId, templatePath, templateFile, administratorId, mergeTabs, isNewPortal, out localeCollection);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Deprecated in DotNetNuke 7.3. Replaced by UpdatePortalExpiry(int, string)")]
        public void UpdatePortalExpiry(int portalId)
        {
            UpdatePortalExpiry(portalId, GetActivePortalLanguage(portalId));
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Deprecated in DotNetNuke 7.3.0. Use one of the other overloads.")]
        public void UpdatePortalInfo(PortalInfo portal, bool clearCache)
        {
            UpdatePortalInternal(portal, clearCache);
        }
 	}
}