// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
// ReSharper disable CheckNamespace
namespace DotNetNuke.Entities.Users

// ReSharper restore CheckNamespace
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Web;

    using DotNetNuke.Common;
    using DotNetNuke.Common.Internal;
    using DotNetNuke.Common.Lists;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Icons;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Profile;
    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.Entities.Users.Social;
    using DotNetNuke.Security;
    using DotNetNuke.Services.Tokens;

    public class ProfilePropertyAccess : IPropertyAccess
    {
        private readonly UserInfo user;

        public ProfilePropertyAccess(UserInfo user)
        {
            this.user = user;
        }

        public CacheLevel Cacheability
        {
            get
            {
                return CacheLevel.notCacheable;
            }
        }

        /// <summary>
        /// Checks whether profile property is accessible.
        /// </summary>
        /// <param name="portalSettings">The portal settings.</param>
        /// <param name="property">The property.</param>
        /// <param name="accessingUser">The accessing user.</param>
        /// <param name="targetUser">The target user.</param>
        /// <returns><c>true</c> if property accessible, otherwise <c>false</c>.</returns>
        public static bool CheckAccessLevel(PortalSettings portalSettings, ProfilePropertyDefinition property, UserInfo accessingUser, UserInfo targetUser)
        {
            var isAdminUser = IsAdminUser(portalSettings, accessingUser, targetUser);

            // Use properties visible property but admins and hosts can always see the property
            var isVisible = property.Visible || isAdminUser;

            if (isVisible && !isAdminUser)
            {
                switch (property.ProfileVisibility.VisibilityMode)
                {
                    case UserVisibilityMode.FriendsAndGroups:
                        isVisible = IsUser(accessingUser, targetUser);
                        if (!isVisible)
                        {
                            // Relationships
                            foreach (Relationship relationship in property.ProfileVisibility.RelationshipVisibilities)
                            {
                                if (targetUser.Social.UserRelationships.Any(userRelationship =>
                                                                          (userRelationship.RelationshipId == relationship.RelationshipId
                                                                              && userRelationship.Status == RelationshipStatus.Accepted
                                                                              && ((userRelationship.RelatedUserId == accessingUser.UserID && userRelationship.UserId == targetUser.UserID)
                                                                                    || (userRelationship.RelatedUserId == targetUser.UserID && userRelationship.UserId == accessingUser.UserID)))))
                                {
                                    isVisible = true;
                                    break;
                                }
                            }

                            // Groups/Roles
                            if (property.ProfileVisibility.RoleVisibilities.Any(role => accessingUser.IsInRole(role.RoleName)))
                            {
                                isVisible = true;
                            }
                        }

                        break;
                    case UserVisibilityMode.AllUsers:
                        // property is visible to everyone so do nothing
                        break;
                    case UserVisibilityMode.MembersOnly:
                        // property visible if accessing user is a member
                        isVisible = IsMember(accessingUser);
                        break;
                    case UserVisibilityMode.AdminOnly:
                        // accessing user not admin user so property is hidden (unless it is the user him/herself)
                        isVisible = IsUser(accessingUser, targetUser);
                        break;
                }
            }

            return isVisible;
        }

        public static string GetRichValue(ProfilePropertyDefinition property, string formatString, CultureInfo formatProvider)
        {
            string result = string.Empty;
            if (!string.IsNullOrEmpty(property.PropertyValue) || DisplayDataType(property).Equals("image", StringComparison.InvariantCultureIgnoreCase))
            {
                switch (DisplayDataType(property).ToLowerInvariant())
                {
                    case "truefalse":
                        result = PropertyAccess.Boolean2LocalizedYesNo(Convert.ToBoolean(property.PropertyValue), formatProvider);
                        break;
                    case "date":
                    case "datetime":
                        if (formatString == string.Empty)
                        {
                            formatString = "g";
                        }

                        result = DateTime.Parse(property.PropertyValue, CultureInfo.InvariantCulture).ToString(formatString, formatProvider);
                        break;
                    case "integer":
                        if (formatString == string.Empty)
                        {
                            formatString = "g";
                        }

                        result = int.Parse(property.PropertyValue).ToString(formatString, formatProvider);
                        break;
                    case "page":
                        int tabid;
                        if (int.TryParse(property.PropertyValue, out tabid))
                        {
                            TabInfo tab = TabController.Instance.GetTab(tabid, Null.NullInteger, false);
                            if (tab != null)
                            {
                                result = string.Format("<a href='{0}'>{1}</a>", TestableGlobals.Instance.NavigateURL(tabid), tab.LocalizedTabName);
                            }
                        }

                        break;
                    case "image":
                        // File is stored as a FileID
                        int fileID;
                        if (int.TryParse(property.PropertyValue, out fileID) && fileID > 0)
                        {
                            result = Globals.LinkClick(string.Format("fileid={0}", fileID), Null.NullInteger, Null.NullInteger);
                        }
                        else
                        {
                            result = IconController.IconURL("Spacer", "1X1");
                        }

                        break;
                    case "richtext":
                        var objSecurity = PortalSecurity.Instance;
                        result = PropertyAccess.FormatString(objSecurity.InputFilter(HttpUtility.HtmlDecode(property.PropertyValue), PortalSecurity.FilterFlag.NoScripting), formatString);
                        break;
                    default:
                        result = HttpUtility.HtmlEncode(PropertyAccess.FormatString(property.PropertyValue, formatString));
                        break;
                }
            }

            return result;
        }

        public static string DisplayDataType(ProfilePropertyDefinition definition)
        {
            string cacheKey = string.Format("DisplayDataType:{0}", definition.DataType);
            string strDataType = Convert.ToString(DataCache.GetCache(cacheKey)) + string.Empty;
            if (strDataType == string.Empty)
            {
                var objListController = new ListController();
                strDataType = objListController.GetListEntryInfo("DataType", definition.DataType).Value;
                DataCache.SetCache(cacheKey, strDataType);
            }

            return strDataType;
        }

        public string GetProperty(string propertyName, string format, CultureInfo formatProvider, UserInfo accessingUser, Scope currentScope, ref bool propertyNotFound)
        {
            if (currentScope >= Scope.DefaultSettings && this.user != null && this.user.Profile != null)
            {
                var profile = this.user.Profile;
                var property = profile.ProfileProperties.Cast<ProfilePropertyDefinition>()
                                                        .SingleOrDefault(p => string.Equals(p.PropertyName, propertyName, StringComparison.CurrentCultureIgnoreCase));

                if (property != null)
                {
                    var portalSettings = PortalController.Instance.GetCurrentPortalSettings();
                    if (CheckAccessLevel(portalSettings, property, accessingUser, this.user))
                    {
                        switch (property.PropertyName.ToLowerInvariant())
                        {
                            case "photo":
                                return this.user.Profile.PhotoURL;
                            case "country":
                                return this.user.Profile.Country;
                            case "region":
                                return this.user.Profile.Region;
                            default:
                                return GetRichValue(property, format, formatProvider);
                        }
                    }
                }

                propertyNotFound = true;
                return property != null && property.PropertyName.Equals("photo", StringComparison.InvariantCultureIgnoreCase)
                    ? Globals.ApplicationPath + "/images/no_avatar.gif" : PropertyAccess.ContentLocked;
            }

            propertyNotFound = true;
            return string.Empty;
        }

        private static bool IsAdminUser(PortalSettings portalSettings, UserInfo accessingUser, UserInfo targetUser)
        {
            bool isAdmin = false;

            if (accessingUser != null)
            {
                // Is Super User?
                isAdmin = accessingUser.IsSuperUser;

                if (!isAdmin && targetUser.PortalID != -1)
                {
                    // Is Administrator
                    var administratorRoleName = portalSettings != null
                        ? portalSettings.AdministratorRoleName
                        : PortalController.Instance.GetPortal(targetUser.PortalID).AdministratorRoleName;

                    isAdmin = accessingUser.IsInRole(administratorRoleName);
                }
            }

            return isAdmin;
        }

        private static bool IsMember(UserInfo accessingUser)
        {
            return accessingUser != null && accessingUser.UserID != -1;
        }

        private static bool IsUser(UserInfo accessingUser, UserInfo targetUser)
        {
            return accessingUser != null && accessingUser.UserID == targetUser.UserID;
        }
    }
}
