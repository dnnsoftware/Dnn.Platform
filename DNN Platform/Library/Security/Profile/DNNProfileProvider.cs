// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
// ReSharper disable InconsistentNaming
// ReSharper disable CheckNamespace
namespace DotNetNuke.Security.Profile

// ReSharper restore CheckNamespace
{
    using System;
    using System.Globalization;

    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Data;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Profile;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.Services.Log.EventLog;

    /// -----------------------------------------------------------------------------
    /// Project:    DotNetNuke
    /// Namespace:  DotNetNuke.Security.Profile
    /// Class:      DNNProfileProvider
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The DNNProfileProvider overrides the default ProfileProvider to provide
    /// a purely DotNetNuke implementation.
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// -----------------------------------------------------------------------------
    public class DNNProfileProvider : ProfileProvider
    {
        private readonly DataProvider _dataProvider = DataProvider.Instance();

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets a value indicating whether gets whether the Provider Properties can be edited.
        /// </summary>
        /// <returns>A Boolean.</returns>
        /// -----------------------------------------------------------------------------
        public override bool CanEditProviderProperties
        {
            get
            {
                return true;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// GetUserProfile retrieves the UserProfile information from the Data Store.
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="user">The user whose Profile information we are retrieving.</param>
        /// -----------------------------------------------------------------------------
        public override void GetUserProfile(ref UserInfo user)
        {
            ProfilePropertyDefinition profProperty;

            int portalId = user.IsSuperUser ? Globals.glbSuperUserAppName : user.PortalID;
            var properties = ProfileController.GetPropertyDefinitionsByPortal(portalId, true, false);

            // Load the Profile properties
            if (user.UserID > Null.NullInteger)
            {
                var key = this.GetProfileCacheKey(user);
                var cachedProperties = (ProfilePropertyDefinitionCollection)DataCache.GetCache(key);
                if (cachedProperties != null)
                {
                    properties = cachedProperties;
                }
                else
                {
                    using (var dr = this._dataProvider.GetUserProfile(user.UserID))
                    {
                        while (dr.Read())
                        {
                            // Ensure the data reader returned is valid
                            if (!string.Equals(dr.GetName(0), "ProfileID", StringComparison.InvariantCultureIgnoreCase))
                            {
                                break;
                            }

                            int definitionId = Convert.ToInt32(dr["PropertyDefinitionId"]);
                            profProperty = properties.GetById(definitionId);
                            if (profProperty != null)
                            {
                                profProperty.PropertyValue = Convert.ToString(dr["PropertyValue"]);
                                var extendedVisibility = string.Empty;
                                var schemaTable = dr.GetSchemaTable();
                                if (schemaTable != null && schemaTable.Select("ColumnName = 'ExtendedVisibility'").Length > 0)
                                {
                                    extendedVisibility = Convert.ToString(dr["ExtendedVisibility"]);
                                }

                                profProperty.ProfileVisibility = new ProfileVisibility(portalId, extendedVisibility)
                                {
                                    VisibilityMode = (UserVisibilityMode)dr["Visibility"],
                                };
                            }
                        }

                        if (properties.Count > 0)
                        {
                            DataCache.SetCache(key, properties, TimeSpan.FromMinutes(DataCache.UserProfileCacheTimeOut));
                        }
                    }
                }
            }

            // Clear the profile
            user.Profile.ProfileProperties.Clear();

            // Add the properties to the profile
            foreach (ProfilePropertyDefinition property in properties)
            {
                profProperty = property;
                if (string.IsNullOrEmpty(profProperty.PropertyValue) && !string.IsNullOrEmpty(profProperty.DefaultValue))
                {
                    profProperty.PropertyValue = profProperty.DefaultValue;
                }

                user.Profile.ProfileProperties.Add(profProperty);
            }

            // Clear IsDirty Flag
            user.Profile.ClearIsDirty();

            // Ensure old and new TimeZone properties are in synch
            this.UpdateTimeZoneInfo(user, properties);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// UpdateUserProfile persists a user's Profile to the Data Store.
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="user">The user to persist to the Data Store.</param>
        /// -----------------------------------------------------------------------------
        public override void UpdateUserProfile(UserInfo user)
        {
            var key = this.GetProfileCacheKey(user);
            DataCache.ClearCache(key);

            ProfilePropertyDefinitionCollection properties = user.Profile.ProfileProperties;

            // Ensure old and new TimeZone properties are in synch
            var newTimeZone = properties["PreferredTimeZone"];
            var oldTimeZone = properties["TimeZone"];
            if (oldTimeZone != null && newTimeZone != null)
            { // preference given to new property, if new is changed then old should be updated as well.
                if (newTimeZone.IsDirty && !string.IsNullOrEmpty(newTimeZone.PropertyValue))
                {
                    var timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(newTimeZone.PropertyValue);
                    if (timeZoneInfo != null)
                    {
                        oldTimeZone.PropertyValue = timeZoneInfo.BaseUtcOffset.TotalMinutes.ToString(CultureInfo.InvariantCulture);
                    }
                }

                // however if old is changed, we need to update new as well
                else if (oldTimeZone.IsDirty)
                {
                    int oldOffset;
                    int.TryParse(oldTimeZone.PropertyValue, out oldOffset);
                    newTimeZone.PropertyValue = Localization.ConvertLegacyTimeZoneOffsetToTimeZoneInfo(oldOffset).Id;
                }
            }

            foreach (ProfilePropertyDefinition profProperty in properties)
            {
                if ((profProperty.PropertyValue != null) && profProperty.IsDirty)
                {
                    var objSecurity = PortalSecurity.Instance;
                    string propertyValue = objSecurity.InputFilter(profProperty.PropertyValue, PortalSecurity.FilterFlag.NoScripting);
                    this._dataProvider.UpdateProfileProperty(Null.NullInteger, user.UserID, profProperty.PropertyDefinitionId,
                                                propertyValue, (int)profProperty.ProfileVisibility.VisibilityMode,
                                                profProperty.ProfileVisibility.ExtendedVisibilityString(), DateTime.Now);
                    EventLogController.Instance.AddLog(user, PortalController.Instance.GetCurrentPortalSettings(), UserController.Instance.GetCurrentUserInfo().UserID, string.Empty, "USERPROFILE_UPDATED");
                }
            }
        }

        private void UpdateTimeZoneInfo(UserInfo user, ProfilePropertyDefinitionCollection properties)
        {
            ProfilePropertyDefinition newTimeZone = properties["PreferredTimeZone"];
            ProfilePropertyDefinition oldTimeZone = properties["TimeZone"];
            if (newTimeZone != null && oldTimeZone != null)
            {
                // Old timezone is present but new is not...we will set that up.
                if (!string.IsNullOrEmpty(oldTimeZone.PropertyValue) && string.IsNullOrEmpty(newTimeZone.PropertyValue))
                {
                    int oldOffset;
                    int.TryParse(oldTimeZone.PropertyValue, out oldOffset);
                    TimeZoneInfo timeZoneInfo = Localization.ConvertLegacyTimeZoneOffsetToTimeZoneInfo(oldOffset);
                    newTimeZone.PropertyValue = timeZoneInfo.Id;
                    this.UpdateUserProfile(user);
                }

                // It's also possible that the new value is set but not the old value. We need to make them backwards compatible
                else if (!string.IsNullOrEmpty(newTimeZone.PropertyValue) && string.IsNullOrEmpty(oldTimeZone.PropertyValue))
                {
                    TimeZoneInfo timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(newTimeZone.PropertyValue);
                    if (timeZoneInfo != null)
                    {
                        oldTimeZone.PropertyValue = timeZoneInfo.BaseUtcOffset.TotalMinutes.ToString(CultureInfo.InvariantCulture);
                    }
                }
            }
        }

        private string GetProfileCacheKey(UserInfo user)
        {
            return string.Format(DataCache.UserProfileCacheKey, user.PortalID, user.Username);
        }
    }
}
