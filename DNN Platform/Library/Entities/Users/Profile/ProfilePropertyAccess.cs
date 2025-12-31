// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

// ReSharper disable CheckNamespace
namespace DotNetNuke.Entities.Users

// ReSharper restore CheckNamespace
{
    using System;
    using System.Globalization;
    using System.Linq;
    using System.Web;

    using DotNetNuke.Abstractions.Portals;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Internal;
    using DotNetNuke.Common.Lists;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Icons;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Profile;
    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.Entities.Users.Social;
    using DotNetNuke.Internal.SourceGenerators;
    using DotNetNuke.Security;
    using DotNetNuke.Services.Tokens;

    /// <summary>Provides access to profile properties.</summary>
    public partial class ProfilePropertyAccess : IPropertyAccess
    {
        private readonly UserInfo user;

        /// <summary>Initializes a new instance of the <see cref="ProfilePropertyAccess"/> class for a user.</summary>
        /// <param name="user">The user to use for these properties, <see cref="UserInfo"/>.</param>
        public ProfilePropertyAccess(UserInfo user)
        {
            this.user = user;
        }

        /// <inheritdoc/>
        public CacheLevel Cacheability
        {
            get
            {
                return CacheLevel.notCacheable;
            }
        }

        /// <summary>Checks whether profile property is accessible.</summary>
        /// <param name="portalSettings">The portal settings.</param>
        /// <param name="property">The property.</param>
        /// <param name="accessingUser">The accessing user.</param>
        /// <param name="targetUser">The target user.</param>
        /// <returns><see langword="true"/> if property accessible, otherwise <see langword="false"/>.</returns>
        [DnnDeprecated(9, 8, 0, "Use the overload that takes IPortalSettings instead")]
        public static partial bool CheckAccessLevel(PortalSettings portalSettings, ProfilePropertyDefinition property, UserInfo accessingUser, UserInfo targetUser)
        {
            var portalSettingsAsInterface = (IPortalSettings)portalSettings;
            return CheckAccessLevel(portalSettingsAsInterface, property, accessingUser, targetUser);
        }

        /// <summary>Checks whether profile property is accessible.</summary>
        /// <param name="portalSettings">The portal settings.</param>
        /// <param name="property">The property.</param>
        /// <param name="accessingUser">The accessing user.</param>
        /// <param name="targetUser">The target user.</param>
        /// <returns><see langword="true"/> if property accessible, otherwise <see langword="false"/>.</returns>
        public static bool CheckAccessLevel(IPortalSettings portalSettings, ProfilePropertyDefinition property, UserInfo accessingUser, UserInfo targetUser)
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

        /// <summary>Gets a human readable string representing some complex profile properties.</summary>
        /// <param name="property">The <see cref="ProfilePropertyDefinition"/> to get the string from.</param>
        /// <param name="formatString">An optional format string.</param>
        /// <param name="formatProvider">An optional <see cref="CultureInfo"/> format provider to use.</param>
        /// <returns>
        /// For booleans, will return localized values for True or False,
        /// For dates, it will return a human formatted date for the requested culture,
        /// For pages, it will return an html link to the page, etc.
        /// </returns>
        public static string GetRichValue(ProfilePropertyDefinition property, string formatString, CultureInfo formatProvider)
        {
            string result = string.Empty;
            if (!string.IsNullOrEmpty(property.PropertyValue) || DisplayDataType(property).Equals("image", StringComparison.OrdinalIgnoreCase))
            {
                switch (DisplayDataType(property).ToLowerInvariant())
                {
                    case "truefalse":
                        result = PropertyAccess.Boolean2LocalizedYesNo(Convert.ToBoolean(property.PropertyValue), formatProvider);
                        break;
                    case "date":
                    case "datetime":
                        if (string.IsNullOrEmpty(formatString))
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

        /// <summary>Gets the date type for a profile property definition.</summary>
        /// <param name="definition">The <see cref="ProfilePropertyDefinition"/> to check.</param>
        /// <returns>A string representing the data type such as: truefalse, date, datetime, integer, page, image or richtext.</returns>
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

        /// <inheritdoc/>
        public string GetProperty(string propertyName, string format, CultureInfo formatProvider, UserInfo accessingUser, Scope currentScope, ref bool propertyNotFound)
        {
            if (currentScope >= Scope.DefaultSettings && this.user != null && this.user.Profile != null)
            {
                var profile = this.user.Profile;
                var property = profile.ProfileProperties.Cast<ProfilePropertyDefinition>()
                                                        .SingleOrDefault(p => string.Equals(p.PropertyName, propertyName, StringComparison.OrdinalIgnoreCase));

                if (property != null)
                {
                    var portalSettings = PortalController.Instance.GetCurrentSettings();
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
                return property != null && property.PropertyName.Equals("photo", StringComparison.OrdinalIgnoreCase)
                    ? Globals.ApplicationPath + "/images/no_avatar.gif" : PropertyAccess.ContentLocked;
            }

            propertyNotFound = true;
            return string.Empty;
        }

        private static bool IsAdminUser(IPortalSettings portalSettings, UserInfo accessingUser, UserInfo targetUser)
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
