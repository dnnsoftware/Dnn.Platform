#region Copyright

// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2017
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

#region Usings

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

#endregion

// ReSharper disable CheckNamespace
namespace DotNetNuke.Entities.Users
// ReSharper restore CheckNamespace
{
    public class ProfilePropertyAccess : IPropertyAccess
    {
        private readonly UserInfo user;

        public ProfilePropertyAccess(UserInfo user)
        {
            this.user = user;
        }

        #region Private Members

        internal static bool CheckAccessLevel(PortalSettings portalSettings, ProfilePropertyDefinition property, UserInfo accessingUser, UserInfo targetUser)
        {
			var isAdminUser = IsAdminUser(portalSettings, accessingUser, targetUser);

            //Use properties visible property but admins and hosts can always see the property
            var isVisible = property.Visible || isAdminUser;

            if (isVisible && !isAdminUser)
            {
                switch (property.ProfileVisibility.VisibilityMode)
                {
                    case UserVisibilityMode.FriendsAndGroups:
						isVisible = IsUser(accessingUser, targetUser);
                        if(!isVisible)
                        {
                            //Relationships
                            foreach (Relationship relationship in property.ProfileVisibility.RelationshipVisibilities)
                            {
								if (targetUser.Social.UserRelationships.Any(userRelationship =>
                                                                          (userRelationship.RelationshipId == relationship.RelationshipId
                                                                              && userRelationship.Status == RelationshipStatus.Accepted
																			  && ((userRelationship.RelatedUserId == accessingUser.UserID && userRelationship.UserId == targetUser.UserID)
																					|| (userRelationship.RelatedUserId == targetUser.UserID && userRelationship.UserId == accessingUser.UserID)))
                                                                      ))
                                {
                                    isVisible = true;
                                    break;
                                }
                            }
                            //Groups/Roles
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
                        //accessing user not admin user so property is hidden (unless it is the user him/herself)
						isVisible = IsUser(accessingUser, targetUser);
                        break;
                }               
            }

            return isVisible;
        }

		private static bool IsAdminUser(PortalSettings portalSettings, UserInfo accessingUser, UserInfo targetUser)
        {
            bool isAdmin = false;

            if (accessingUser != null)
            {
                //Is Super User?
                isAdmin = accessingUser.IsSuperUser;

				if (!isAdmin && targetUser.PortalID != -1)
                {
                    //Is Administrator
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
            return (accessingUser != null && accessingUser.UserID != -1);
        }

		private static bool IsUser(UserInfo accessingUser, UserInfo targetUser)
        {
			return (accessingUser != null && accessingUser.UserID == targetUser.UserID);
        }

        #endregion

        #region IPropertyAccess Members

        public string GetProperty(string propertyName, string format, CultureInfo formatProvider, UserInfo accessingUser, Scope currentScope, ref bool propertyNotFound)
        {
            if (currentScope >= Scope.DefaultSettings && user != null && user.Profile != null)
            {
                var profile = user.Profile;
                var property = profile.ProfileProperties.Cast<ProfilePropertyDefinition>()
                                                        .SingleOrDefault(p => p.PropertyName.ToLower() == propertyName.ToLower());

                if(property != null)
                {
					var portalSettings = PortalController.Instance.GetCurrentPortalSettings();
					if (CheckAccessLevel(portalSettings, property, accessingUser, user))
                    {
                        switch (property.PropertyName.ToLower())
                        {
                            case "photo":
                                return user.Profile.PhotoURL;
                            case "country":
                                return user.Profile.Country;
                            case "region":
                                return user.Profile.Region;
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

        public CacheLevel Cacheability
        {
            get
            {
                return CacheLevel.notCacheable;
            }
        }

        #endregion

        public static string GetRichValue(ProfilePropertyDefinition property, string formatString, CultureInfo formatProvider)
        {
            string result = "";
            if (!String.IsNullOrEmpty(property.PropertyValue) || DisplayDataType(property).ToLowerInvariant() == "image")
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
                        //File is stored as a FileID
                        int fileID;
                        if (Int32.TryParse(property.PropertyValue, out fileID) && fileID > 0)
                        {
                            result = Globals.LinkClick(String.Format("fileid={0}", fileID), Null.NullInteger, Null.NullInteger);
                        }
                        else
                        {
                            result = IconController.IconURL("Spacer","1X1");
                        }
                        break;
                    case "richtext":
                        var objSecurity = new PortalSecurity();
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
            string strDataType = Convert.ToString(DataCache.GetCache(cacheKey)) + "";
            if (strDataType == string.Empty)
            {
                var objListController = new ListController();
                strDataType = objListController.GetListEntryInfo("DataType", definition.DataType).Value;
                DataCache.SetCache(cacheKey, strDataType);
            }
            return strDataType;
        }
    }
}
