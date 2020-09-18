// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Entities.Profile
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Text.RegularExpressions;
    using System.Web;

    using DotNetNuke.Common.Internal;
    using DotNetNuke.Common.Lists;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Data;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Security;
    using DotNetNuke.Security.Profile;
    using DotNetNuke.Services.Exceptions;
    using DotNetNuke.Services.FileSystem;
    using DotNetNuke.Services.Log.EventLog;

    /// -----------------------------------------------------------------------------
    /// Project:    DotNetNuke
    /// Namespace:  DotNetNuke.Entities.Profile
    /// Class:      ProfileController
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The ProfileController class provides Business Layer methods for profiles and
    /// for profile property Definitions.
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// -----------------------------------------------------------------------------
    public class ProfileController
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(ProfileController));
        private static readonly DataProvider _dataProvider = DataProvider.Instance();
        private static readonly ProfileProvider _profileProvider = ProfileProvider.Instance();
        private static int _orderCounter;

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Adds the default property definitions for a portal.
        /// </summary>
        /// <param name="portalId">Id of the Portal.</param>
        /// -----------------------------------------------------------------------------
        public static void AddDefaultDefinitions(int portalId)
        {
            portalId = GetEffectivePortalId(portalId);

            _orderCounter = 1;
            var listController = new ListController();
            Dictionary<string, ListEntryInfo> dataTypes = listController.GetListEntryInfoDictionary("DataType");

            AddDefaultDefinition(portalId, "Name", "Prefix", "Text", 50, UserVisibilityMode.AllUsers, dataTypes);
            AddDefaultDefinition(portalId, "Name", "FirstName", "Text", 50, UserVisibilityMode.AllUsers, dataTypes);
            AddDefaultDefinition(portalId, "Name", "MiddleName", "Text", 50, UserVisibilityMode.AllUsers, dataTypes);
            AddDefaultDefinition(portalId, "Name", "LastName", "Text", 50, UserVisibilityMode.AllUsers, dataTypes);
            AddDefaultDefinition(portalId, "Name", "Suffix", "Text", 50, UserVisibilityMode.AllUsers, dataTypes);
            AddDefaultDefinition(portalId, "Address", "Unit", "Text", 50, UserVisibilityMode.AdminOnly, dataTypes);
            AddDefaultDefinition(portalId, "Address", "Street", "Text", 50, UserVisibilityMode.AdminOnly, dataTypes);
            AddDefaultDefinition(portalId, "Address", "City", "Text", 50, UserVisibilityMode.AdminOnly, dataTypes);
            AddDefaultDefinition(portalId, "Address", "Region", "Region", 0, UserVisibilityMode.AdminOnly, dataTypes);
            AddDefaultDefinition(portalId, "Address", "Country", "Country", 0, UserVisibilityMode.AdminOnly, dataTypes);
            AddDefaultDefinition(portalId, "Address", "PostalCode", "Text", 50, UserVisibilityMode.AdminOnly, dataTypes);
            AddDefaultDefinition(portalId, "Contact Info", "Telephone", "Text", 50, UserVisibilityMode.AdminOnly, dataTypes);
            AddDefaultDefinition(portalId, "Contact Info", "Cell", "Text", 50, UserVisibilityMode.AdminOnly, dataTypes);
            AddDefaultDefinition(portalId, "Contact Info", "Fax", "Text", 50, UserVisibilityMode.AdminOnly, dataTypes);
            AddDefaultDefinition(portalId, "Contact Info", "Website", "Text", 50, UserVisibilityMode.AdminOnly, dataTypes);
            AddDefaultDefinition(portalId, "Contact Info", "IM", "Text", 50, UserVisibilityMode.AdminOnly, dataTypes);
            AddDefaultDefinition(portalId, "Preferences", "Biography", "Multi-line Text", 0, UserVisibilityMode.AdminOnly, dataTypes);
            AddDefaultDefinition(portalId, "Preferences", "TimeZone", "TimeZone", 0, UserVisibilityMode.AdminOnly, dataTypes);
            AddDefaultDefinition(portalId, "Preferences", "PreferredTimeZone", "TimeZoneInfo", 0, UserVisibilityMode.AdminOnly, dataTypes);
            AddDefaultDefinition(portalId, "Preferences", "PreferredLocale", "Locale", 0, UserVisibilityMode.AdminOnly, dataTypes);
            AddDefaultDefinition(portalId, "Preferences", "Photo", "Image", 0, UserVisibilityMode.AllUsers, dataTypes);

            // 6.0 requires the old TimeZone property to be marked as Deleted
            ProfilePropertyDefinition pdf = GetPropertyDefinitionByName(portalId, "TimeZone");
            if (pdf != null)
            {
                DeletePropertyDefinition(pdf);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Adds a Property Defintion to the Data Store.
        /// </summary>
        /// <param name="definition">An ProfilePropertyDefinition object.</param>
        /// <returns>The Id of the definition (or if negative the errorcode of the error).</returns>
        /// -----------------------------------------------------------------------------
        public static int AddPropertyDefinition(ProfilePropertyDefinition definition)
        {
            int portalId = GetEffectivePortalId(definition.PortalId);
            if (definition.Required)
            {
                definition.Visible = true;
            }

            int intDefinition = _dataProvider.AddPropertyDefinition(
                portalId,
                definition.ModuleDefId,
                definition.DataType,
                definition.DefaultValue,
                definition.PropertyCategory,
                definition.PropertyName,
                definition.ReadOnly,
                definition.Required,
                definition.ValidationExpression,
                definition.ViewOrder,
                definition.Visible,
                definition.Length,
                (int)definition.DefaultVisibility,
                UserController.Instance.GetCurrentUserInfo().UserID);
            EventLogController.Instance.AddLog(definition, PortalController.Instance.GetCurrentPortalSettings(), UserController.Instance.GetCurrentUserInfo().UserID, string.Empty, EventLogController.EventLogType.PROFILEPROPERTY_CREATED);
            ClearProfileDefinitionCache(definition.PortalId);
            ClearAllUsersInfoProfileCacheByPortal(definition.PortalId);
            return intDefinition;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Clears the Profile Definitions Cache.
        /// </summary>
        /// <param name="portalId">Id of the Portal.</param>
        /// -----------------------------------------------------------------------------
        public static void ClearProfileDefinitionCache(int portalId)
        {
            DataCache.ClearDefinitionsCache(GetEffectivePortalId(portalId));
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Deletes a Property Defintion from the Data Store.
        /// </summary>
        /// <param name="definition">The ProfilePropertyDefinition object to delete.</param>
        /// -----------------------------------------------------------------------------
        public static void DeletePropertyDefinition(ProfilePropertyDefinition definition)
        {
            _dataProvider.DeletePropertyDefinition(definition.PropertyDefinitionId);
            EventLogController.Instance.AddLog(definition, PortalController.Instance.GetCurrentPortalSettings(), UserController.Instance.GetCurrentUserInfo().UserID, string.Empty, EventLogController.EventLogType.PROFILEPROPERTY_DELETED);
            ClearProfileDefinitionCache(definition.PortalId);
            ClearAllUsersInfoProfileCacheByPortal(definition.PortalId);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Clear profiles of all users by portal Id.
        /// </summary>
        /// -----------------------------------------------------------------------------
        public static void ClearAllUsersInfoProfileCacheByPortal(int portalId)
        {
            DataCache.ClearCache(string.Format(DataCache.UserCacheKey, portalId, string.Empty));
            DataCache.ClearCache(string.Format(DataCache.UserProfileCacheKey, portalId, string.Empty));
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets a Property Defintion from the Data Store by id.
        /// </summary>
        /// <param name="definitionId">The id of the ProfilePropertyDefinition object to retrieve.</param>
        /// <param name="portalId">Portal Id.</param>
        /// <returns>The ProfilePropertyDefinition object.</returns>
        /// -----------------------------------------------------------------------------
        public static ProfilePropertyDefinition GetPropertyDefinition(int definitionId, int portalId)
        {
            bool bFound = Null.NullBoolean;
            ProfilePropertyDefinition definition = null;
            foreach (ProfilePropertyDefinition def in GetPropertyDefinitions(GetEffectivePortalId(portalId)))
            {
                if (def.PropertyDefinitionId == definitionId)
                {
                    definition = def;
                    bFound = true;
                    break;
                }
            }

            if (!bFound)
            {
                // Try Database
                definition = FillPropertyDefinitionInfo(_dataProvider.GetPropertyDefinition(definitionId));
            }

            return definition;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets a Property Defintion from the Data Store by name.
        /// </summary>
        /// <param name="portalId">The id of the Portal.</param>
        /// <param name="name">The name of the ProfilePropertyDefinition object to retrieve.</param>
        /// <returns>The ProfilePropertyDefinition object.</returns>
        /// -----------------------------------------------------------------------------
        public static ProfilePropertyDefinition GetPropertyDefinitionByName(int portalId, string name)
        {
            portalId = GetEffectivePortalId(portalId);

            bool bFound = Null.NullBoolean;
            ProfilePropertyDefinition definition = null;
            foreach (ProfilePropertyDefinition def in GetPropertyDefinitions(portalId))
            {
                if (def.PropertyName == name)
                {
                    definition = def;
                    bFound = true;
                    break;
                }
            }

            if (!bFound)
            {
                // Try Database
                definition = FillPropertyDefinitionInfo(_dataProvider.GetPropertyDefinitionByName(portalId, name));
            }

            return definition;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets a collection of Property Defintions from the Data Store by category.
        /// </summary>
        /// <param name="portalId">The id of the Portal.</param>
        /// <param name="category">The category of the Property Defintions to retrieve.</param>
        /// <returns>A ProfilePropertyDefinitionCollection object.</returns>
        /// -----------------------------------------------------------------------------
        public static ProfilePropertyDefinitionCollection GetPropertyDefinitionsByCategory(int portalId, string category)
        {
            portalId = GetEffectivePortalId(portalId);

            var definitions = new ProfilePropertyDefinitionCollection();
            foreach (ProfilePropertyDefinition definition in GetPropertyDefinitions(portalId))
            {
                if (definition.PropertyCategory == category)
                {
                    definitions.Add(definition);
                }
            }

            return definitions;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets a collection of Property Defintions from the Data Store by portal.
        /// </summary>
        /// <param name="portalId">The id of the Portal.</param>
        /// <returns>A ProfilePropertyDefinitionCollection object.</returns>
        /// -----------------------------------------------------------------------------
        public static ProfilePropertyDefinitionCollection GetPropertyDefinitionsByPortal(int portalId)
        {
            return GetPropertyDefinitionsByPortal(portalId, true);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets a collection of Property Defintions from the Data Store by portal.
        /// </summary>
        /// <param name="portalId">The id of the Portal.</param>
        /// <param name="clone">Whether to use a clone object.</param>
        /// <returns>A ProfilePropertyDefinitionCollection object.</returns>
        /// -----------------------------------------------------------------------------
        public static ProfilePropertyDefinitionCollection GetPropertyDefinitionsByPortal(int portalId, bool clone)
        {
            return GetPropertyDefinitionsByPortal(portalId, clone, true);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets a collection of Property Defintions from the Data Store by portal.
        /// </summary>
        /// <param name="portalId">The id of the Portal.</param>
        /// <param name="clone">Whether to use a clone object.</param>
        /// <param name="includeDeleted">Whether to include deleted profile properties.</param>
        /// <returns>A ProfilePropertyDefinitionCollection object.</returns>
        /// -----------------------------------------------------------------------------
        public static ProfilePropertyDefinitionCollection GetPropertyDefinitionsByPortal(int portalId, bool clone, bool includeDeleted)
        {
            portalId = GetEffectivePortalId(portalId);

            var definitions = new ProfilePropertyDefinitionCollection();
            foreach (ProfilePropertyDefinition definition in GetPropertyDefinitions(portalId))
            {
                if (!definition.Deleted || includeDeleted)
                {
                    definitions.Add(clone ? definition.Clone() : definition);
                }
            }

            return definitions;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the Profile Information for the User.
        /// </summary>
        /// <remarks></remarks>
        /// <param name="user">The user whose Profile information we are retrieving.</param>
        /// -----------------------------------------------------------------------------
        public static void GetUserProfile(ref UserInfo user)
        {
            int portalId = user.PortalID;
            user.PortalID = GetEffectivePortalId(portalId);

            _profileProvider.GetUserProfile(ref user);
            user.PortalID = portalId;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Updates a Property Defintion in the Data Store.
        /// </summary>
        /// <param name="definition">The ProfilePropertyDefinition object to update.</param>
        /// -----------------------------------------------------------------------------
        public static void UpdatePropertyDefinition(ProfilePropertyDefinition definition)
        {
            if (definition.Required)
            {
                definition.Visible = true;
            }

            _dataProvider.UpdatePropertyDefinition(
                definition.PropertyDefinitionId,
                definition.DataType,
                definition.DefaultValue,
                definition.PropertyCategory,
                definition.PropertyName,
                definition.ReadOnly,
                definition.Required,
                definition.ValidationExpression,
                definition.ViewOrder,
                definition.Visible,
                definition.Length,
                (int)definition.DefaultVisibility,
                UserController.Instance.GetCurrentUserInfo().UserID);
            EventLogController.Instance.AddLog(definition, PortalController.Instance.GetCurrentPortalSettings(), UserController.Instance.GetCurrentUserInfo().UserID, string.Empty, EventLogController.EventLogType.PROFILEPROPERTY_UPDATED);
            ClearProfileDefinitionCache(definition.PortalId);
            ClearAllUsersInfoProfileCacheByPortal(definition.PortalId);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Updates a User's Profile.
        /// </summary>
        /// <param name="user">The use to update.</param>
        /// <remarks>
        /// </remarks>
        /// -----------------------------------------------------------------------------
        public static void UpdateUserProfile(UserInfo user)
        {
            if (!user.Profile.IsDirty)
            {
                return;
            }

            var portalId = GetEffectivePortalId(user.PortalID);
            user.PortalID = portalId;

            var oldUser = new UserInfo { UserID = user.UserID, PortalID = user.PortalID, IsSuperUser = user.IsSuperUser };
            _profileProvider.GetUserProfile(ref oldUser);

            _profileProvider.UpdateUserProfile(user);

            // Remove the UserInfo from the Cache, as it has been modified
            DataCache.ClearUserCache(user.PortalID, user.Username);

            // Raise Profile updated event
            EventManager.Instance.OnProfileUpdated(new ProfileEventArgs { User = user, OldProfile = oldUser.Profile });
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Updates a User's Profile.
        /// </summary>
        /// <param name="user">The use to update.</param>
        /// <param name="profileProperties">The collection of profile properties.</param>
        /// <returns>The updated User.</returns>
        /// -----------------------------------------------------------------------------
        public static UserInfo UpdateUserProfile(UserInfo user, ProfilePropertyDefinitionCollection profileProperties)
        {
            int portalId = GetEffectivePortalId(user.PortalID);
            user.PortalID = portalId;

            var photoChanged = Null.NullBoolean;

            // Iterate through the Definitions
            if (profileProperties != null)
            {
                foreach (ProfilePropertyDefinition propertyDefinition in profileProperties)
                {
                    string propertyName = propertyDefinition.PropertyName;
                    string propertyValue = propertyDefinition.PropertyValue;
                    if (propertyDefinition.IsDirty)
                    {
                        if (propertyName.Equals(UserProfile.USERPROFILE_Photo, StringComparison.InvariantCultureIgnoreCase))
                        {
                            photoChanged = true;
                        }

                        user.Profile.SetProfileProperty(propertyName, propertyValue);
                    }
                }

                // if user's photo changed, then create different size thumbnails of profile pictures.
                if (photoChanged)
                {
                    try
                    {
                        if (!string.IsNullOrEmpty(user.Profile.Photo) && int.Parse(user.Profile.Photo) > 0)
                        {
                            CreateThumbnails(int.Parse(user.Profile.Photo));
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(ex);
                    }
                }

                UserController.UpdateUser(portalId, user);
            }

            return user;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Validates the Profile properties for the User (determines if all required properties
        /// have been set).
        /// </summary>
        /// <param name="portalId">The Id of the portal.</param>
        /// <param name="objProfile">The profile.</param>
        /// <returns></returns>
        /// -----------------------------------------------------------------------------
        public static bool ValidateProfile(int portalId, UserProfile objProfile)
        {
            bool isValid = true;
            var imageType = new ListController().GetListEntryInfo("DataType", "Image");
            foreach (ProfilePropertyDefinition propertyDefinition in objProfile.ProfileProperties)
            {
                if (propertyDefinition.Required && string.IsNullOrEmpty(propertyDefinition.PropertyValue) && propertyDefinition.DataType != imageType.EntryID)
                {
                    isValid = false;
                    break;
                }
            }

            return isValid;
        }

        /// <summary>
        /// Searches the profile property values for a string (doesn't need to be the beginning).
        /// </summary>
        /// <param name="portalId">The portal identifier.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="searchString">The search string.</param>
        /// <returns>List of matching values.</returns>
        public static List<string> SearchProfilePropertyValues(int portalId, string propertyName, string searchString)
        {
            var res = new List<string> { };
            var autoCompleteType = new ListController().GetListEntryInfo("DataType", "AutoComplete");
            var def = GetPropertyDefinitionByName(portalId, propertyName);
            if (def.DataType != autoCompleteType.EntryID)
            {
                return res;
            }

            using (
                IDataReader ir = Data.DataProvider.Instance()
                    .SearchProfilePropertyValues(portalId, propertyName, searchString))
            {
                while (ir.Read())
                {
                    res.Add(Convert.ToString(ir[0]));
                }
            }

            return res;
        }

        [Obsolete("This method has been deprecated.  Please use GetPropertyDefinition(ByVal definitionId As Integer, ByVal portalId As Integer) instead. Scheduled removal in v11.0.0.")]
        public static ProfilePropertyDefinition GetPropertyDefinition(int definitionId)
        {
            return CBO.FillObject<ProfilePropertyDefinition>(_dataProvider.GetPropertyDefinition(definitionId));
        }

        internal static void AddDefaultDefinition(int portalId, string category, string name, string type, int length, int viewOrder, UserVisibilityMode defaultVisibility,
                                                  Dictionary<string, ListEntryInfo> types)
        {
            ListEntryInfo typeInfo = types["DataType:" + type] ?? types["DataType:Unknown"];
            var propertyDefinition = new ProfilePropertyDefinition(portalId)
            {
                DataType = typeInfo.EntryID,
                DefaultValue = string.Empty,
                ModuleDefId = Null.NullInteger,
                PropertyCategory = category,
                PropertyName = name,
                Required = false,
                ViewOrder = viewOrder,
                Visible = true,
                Length = length,
                DefaultVisibility = defaultVisibility,
            };
            AddPropertyDefinition(propertyDefinition);
        }

        private static void AddDefaultDefinition(int portalId, string category, string name, string strType, int length, UserVisibilityMode defaultVisibility, Dictionary<string, ListEntryInfo> types)
        {
            _orderCounter += 2;
            AddDefaultDefinition(portalId, category, name, strType, length, _orderCounter, defaultVisibility, types);
        }

        private static ProfilePropertyDefinition FillPropertyDefinitionInfo(IDataReader dr)
        {
            ProfilePropertyDefinition definition = null;
            try
            {
                definition = FillPropertyDefinitionInfo(dr, true);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
            finally
            {
                CBO.CloseDataReader(dr, true);
            }

            return definition;
        }

        private static ProfilePropertyDefinition FillPropertyDefinitionInfo(IDataReader dr, bool checkForOpenDataReader)
        {
            ProfilePropertyDefinition definition = null;

            // read datareader
            bool canContinue = true;
            if (checkForOpenDataReader)
            {
                canContinue = false;
                if (dr.Read())
                {
                    canContinue = true;
                }
            }

            if (canContinue)
            {
                int portalid = 0;
                portalid = Convert.ToInt32(Null.SetNull(dr["PortalId"], portalid));
                definition = new ProfilePropertyDefinition(portalid);
                definition.PropertyDefinitionId = Convert.ToInt32(Null.SetNull(dr["PropertyDefinitionId"], definition.PropertyDefinitionId));
                definition.ModuleDefId = Convert.ToInt32(Null.SetNull(dr["ModuleDefId"], definition.ModuleDefId));
                definition.DataType = Convert.ToInt32(Null.SetNull(dr["DataType"], definition.DataType));
                definition.DefaultValue = Convert.ToString(Null.SetNull(dr["DefaultValue"], definition.DefaultValue));
                definition.PropertyCategory = Convert.ToString(Null.SetNull(dr["PropertyCategory"], definition.PropertyCategory));
                definition.PropertyName = Convert.ToString(Null.SetNull(dr["PropertyName"], definition.PropertyName));
                definition.Length = Convert.ToInt32(Null.SetNull(dr["Length"], definition.Length));
                if (dr.GetSchemaTable().Select("ColumnName = 'ReadOnly'").Length > 0)
                {
                    definition.ReadOnly = Convert.ToBoolean(Null.SetNull(dr["ReadOnly"], definition.ReadOnly));
                }

                definition.Required = Convert.ToBoolean(Null.SetNull(dr["Required"], definition.Required));
                definition.ValidationExpression = Convert.ToString(Null.SetNull(dr["ValidationExpression"], definition.ValidationExpression));
                definition.ViewOrder = Convert.ToInt32(Null.SetNull(dr["ViewOrder"], definition.ViewOrder));
                definition.Visible = Convert.ToBoolean(Null.SetNull(dr["Visible"], definition.Visible));
                definition.DefaultVisibility = (UserVisibilityMode)Convert.ToInt32(Null.SetNull(dr["DefaultVisibility"], definition.DefaultVisibility));
                definition.ProfileVisibility = new ProfileVisibility
                {
                    VisibilityMode = definition.DefaultVisibility,
                };
                definition.Deleted = Convert.ToBoolean(Null.SetNull(dr["Deleted"], definition.Deleted));
            }

            return definition;
        }

        private static List<ProfilePropertyDefinition> FillPropertyDefinitionInfoCollection(IDataReader dr)
        {
            var arr = new List<ProfilePropertyDefinition>();
            try
            {
                while (dr.Read())
                {
                    // fill business object
                    ProfilePropertyDefinition definition = FillPropertyDefinitionInfo(dr, false);

                    // add to collection
                    arr.Add(definition);
                }
            }
            catch (Exception exc)
            {
                Exceptions.LogException(exc);
            }
            finally
            {
                // close datareader
                CBO.CloseDataReader(dr, true);
            }

            return arr;
        }

        private static int GetEffectivePortalId(int portalId)
        {
            return PortalController.GetEffectivePortalId(portalId);
        }

        private static IEnumerable<ProfilePropertyDefinition> GetPropertyDefinitions(int portalId)
        {
            // Get the Cache Key
            string key = string.Format(DataCache.ProfileDefinitionsCacheKey, portalId);

            // Try fetching the List from the Cache
            var definitions = (List<ProfilePropertyDefinition>)DataCache.GetCache(key);
            if (definitions == null)
            {
                // definitions caching settings
                int timeOut = DataCache.ProfileDefinitionsCacheTimeOut * Convert.ToInt32(Host.Host.PerformanceSetting);

                // Get the List from the database
                definitions = FillPropertyDefinitionInfoCollection(_dataProvider.GetPropertyDefinitionsByPortal(portalId));

                // Cache the List
                if (timeOut > 0)
                {
                    DataCache.SetCache(key, definitions, TimeSpan.FromMinutes(timeOut));
                }
            }

            return definitions;
        }

        private static void CreateThumbnails(int fileId)
        {
            CreateThumbnail(fileId, "l", 64, 64);
            CreateThumbnail(fileId, "s", 50, 50);
            CreateThumbnail(fileId, "xs", 32, 32);
        }

        private static void CreateThumbnail(int fileId, string type, int width, int height)
        {
            var file = FileManager.Instance.GetFile(fileId);
            if (file != null)
            {
                var folder = FolderManager.Instance.GetFolder(file.FolderId);
                var extension = "." + file.Extension;
                var sizedPhoto = file.FileName.Replace(extension, "_" + type + extension);
                if (!FileManager.Instance.FileExists(folder, sizedPhoto))
                {
                    using (var content = FileManager.Instance.GetFileContent(file))
                    {
                        var sizedContent = ImageUtils.CreateImage(content, height, width, extension);

                        FileManager.Instance.AddFile(folder, sizedPhoto, sizedContent);
                    }
                }
            }
        }
    }
}
