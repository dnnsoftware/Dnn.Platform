// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Entities.Profile
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Globalization;
    using System.IO;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Abstractions.Logging;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Lists;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Data;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Internal.SourceGenerators;
    using DotNetNuke.Security.Profile;
    using DotNetNuke.Services.Exceptions;
    using DotNetNuke.Services.FileSystem;

    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// The ProfileController class provides Business Layer methods for profiles and
    /// for profile property Definitions.
    /// </summary>
    public partial class ProfileController
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(ProfileController));
        private static readonly DataProvider DataProvider = DataProvider.Instance();
        private static readonly ProfileProvider ProfileProvider = ProfileProvider.Instance();
        private static int orderCounter;

        /// <summary>Adds the default property definitions for a portal.</summary>
        /// <param name="portalId">ID of the Portal.</param>
        [DnnDeprecated(10, 2, 4, "Please use overload with ListController")]
        public static partial void AddDefaultDefinitions(int portalId)
            => AddDefaultDefinitions(
                Globals.GetCurrentServiceProvider().GetRequiredService<ListController>(),
                Globals.GetCurrentServiceProvider().GetRequiredService<IEventLogger>(),
                Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettings>(),
                Globals.GetCurrentServiceProvider().GetRequiredService<IPortalController>(),
                Globals.GetCurrentServiceProvider().GetRequiredService<IApplicationStatusInfo>(),
                Globals.GetCurrentServiceProvider().GetRequiredService<IPortalGroupController>(),
                portalId);

        /// <summary>Adds the default property definitions for a portal.</summary>
        /// <param name="listController">The list controller.</param>
        /// <param name="eventLogger">The event logger.</param>
        /// <param name="hostSettings">The host settings.</param>
        /// <param name="portalController">The portal controller.</param>
        /// <param name="appStatus">The application status.</param>
        /// <param name="portalGroupController">The portal group controller.</param>
        /// <param name="portalId">ID of the Portal.</param>
        public static void AddDefaultDefinitions(ListController listController, IEventLogger eventLogger, IHostSettings hostSettings, IPortalController portalController, IApplicationStatusInfo appStatus, IPortalGroupController portalGroupController, int portalId)
        {
            portalId = PortalController.GetEffectivePortalId(portalController, appStatus, portalGroupController, portalId);

            orderCounter = 1;
            var dataTypes = listController.GetListEntryInfoDictionary("DataType");

            AddDefaultDefinition(eventLogger, portalController, appStatus, portalGroupController, portalId, "Name", "Prefix", "Text", 50, UserVisibilityMode.AllUsers, dataTypes);
            AddDefaultDefinition(eventLogger, portalController, appStatus, portalGroupController, portalId, "Name", "FirstName", "Text", 50, UserVisibilityMode.AllUsers, dataTypes);
            AddDefaultDefinition(eventLogger, portalController, appStatus, portalGroupController, portalId, "Name", "MiddleName", "Text", 50, UserVisibilityMode.AllUsers, dataTypes);
            AddDefaultDefinition(eventLogger, portalController, appStatus, portalGroupController, portalId, "Name", "LastName", "Text", 50, UserVisibilityMode.AllUsers, dataTypes);
            AddDefaultDefinition(eventLogger, portalController, appStatus, portalGroupController, portalId, "Name", "Suffix", "Text", 50, UserVisibilityMode.AllUsers, dataTypes);
            AddDefaultDefinition(eventLogger, portalController, appStatus, portalGroupController, portalId, "Address", "Unit", "Text", 50, UserVisibilityMode.AdminOnly, dataTypes);
            AddDefaultDefinition(eventLogger, portalController, appStatus, portalGroupController, portalId, "Address", "Street", "Text", 50, UserVisibilityMode.AdminOnly, dataTypes);
            AddDefaultDefinition(eventLogger, portalController, appStatus, portalGroupController, portalId, "Address", "City", "Text", 50, UserVisibilityMode.AdminOnly, dataTypes);
            AddDefaultDefinition(eventLogger, portalController, appStatus, portalGroupController, portalId, "Address", "Region", "Region", 0, UserVisibilityMode.AdminOnly, dataTypes);
            AddDefaultDefinition(eventLogger, portalController, appStatus, portalGroupController, portalId, "Address", "Country", "Country", 0, UserVisibilityMode.AdminOnly, dataTypes);
            AddDefaultDefinition(eventLogger, portalController, appStatus, portalGroupController, portalId, "Address", "PostalCode", "Text", 50, UserVisibilityMode.AdminOnly, dataTypes);
            AddDefaultDefinition(eventLogger, portalController, appStatus, portalGroupController, portalId, "Contact Info", "Telephone", "Text", 50, UserVisibilityMode.AdminOnly, dataTypes);
            AddDefaultDefinition(eventLogger, portalController, appStatus, portalGroupController, portalId, "Contact Info", "Cell", "Text", 50, UserVisibilityMode.AdminOnly, dataTypes);
            AddDefaultDefinition(eventLogger, portalController, appStatus, portalGroupController, portalId, "Contact Info", "Fax", "Text", 50, UserVisibilityMode.AdminOnly, dataTypes);
            AddDefaultDefinition(eventLogger, portalController, appStatus, portalGroupController, portalId, "Contact Info", "Website", "Text", 50, UserVisibilityMode.AdminOnly, dataTypes);
            AddDefaultDefinition(eventLogger, portalController, appStatus, portalGroupController, portalId, "Contact Info", "IM", "Text", 50, UserVisibilityMode.AdminOnly, dataTypes);
            AddDefaultDefinition(eventLogger, portalController, appStatus, portalGroupController, portalId, "Preferences", "Biography", "Multi-line Text", 0, UserVisibilityMode.AdminOnly, dataTypes);
            AddDefaultDefinition(eventLogger, portalController, appStatus, portalGroupController, portalId, "Preferences", "TimeZone", "TimeZone", 0, UserVisibilityMode.AdminOnly, dataTypes);
            AddDefaultDefinition(eventLogger, portalController, appStatus, portalGroupController, portalId, "Preferences", "PreferredTimeZone", "TimeZoneInfo", 0, UserVisibilityMode.AdminOnly, dataTypes);
            AddDefaultDefinition(eventLogger, portalController, appStatus, portalGroupController, portalId, "Preferences", "PreferredLocale", "Locale", 0, UserVisibilityMode.AdminOnly, dataTypes);
            AddDefaultDefinition(eventLogger, portalController, appStatus, portalGroupController, portalId, "Preferences", "Photo", "Image", 0, UserVisibilityMode.AllUsers, dataTypes);

            // 6.0 requires the old TimeZone property to be marked as Deleted
            var timeZoneProperty = GetPropertyDefinitionByName(hostSettings, portalController, appStatus, portalGroupController, portalId, "TimeZone");
            if (timeZoneProperty != null)
            {
                DeletePropertyDefinition(eventLogger, portalController, appStatus, portalGroupController, timeZoneProperty);
            }
        }

        /// <summary>Adds a Property Definition to the Data Store.</summary>
        /// <param name="definition">An ProfilePropertyDefinition object.</param>
        /// <returns>The ID of the definition (or if negative the errorcode of the error).</returns>
        [DnnDeprecated(10, 2, 2, "Use overload taking IEventLogger")]
        public static partial int AddPropertyDefinition(ProfilePropertyDefinition definition)
            => AddPropertyDefinition(
                Globals.GetCurrentServiceProvider().GetRequiredService<IEventLogger>(),
                Globals.GetCurrentServiceProvider().GetRequiredService<IPortalController>(),
                Globals.GetCurrentServiceProvider().GetRequiredService<IApplicationStatusInfo>(),
                Globals.GetCurrentServiceProvider().GetRequiredService<IPortalGroupController>(),
                definition);

        /// <summary>Adds a Property Definition to the Data Store.</summary>
        /// <param name="eventLogger">The event logger.</param>
        /// <param name="portalController">The portal controller.</param>
        /// <param name="appStatus">The application status.</param>
        /// <param name="portalGroupController">The portal group controller.</param>
        /// <param name="definition">An ProfilePropertyDefinition object.</param>
        /// <returns>The ID of the definition (or if negative the errorcode of the error).</returns>
        public static int AddPropertyDefinition(IEventLogger eventLogger, IPortalController portalController, IApplicationStatusInfo appStatus, IPortalGroupController portalGroupController, ProfilePropertyDefinition definition)
        {
            int portalId = PortalController.GetEffectivePortalId(portalController, appStatus, portalGroupController, definition.PortalId);
            if (definition.Required)
            {
                definition.Visible = true;
            }

            int intDefinition = DataProvider.AddPropertyDefinition(
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
            eventLogger.AddLog(definition, PortalController.Instance.GetCurrentSettings(), UserController.Instance.GetCurrentUserInfo().UserID, string.Empty, EventLogType.PROFILEPROPERTY_CREATED);
            ClearProfileDefinitionCache(portalController, appStatus, portalGroupController, definition.PortalId);
            ClearAllUsersInfoProfileCacheByPortal(definition.PortalId);
            return intDefinition;
        }

        /// <summary>Clears the Profile Definitions Cache.</summary>
        /// <param name="portalId">ID of the Portal.</param>
        [DnnDeprecated(10, 2, 4, "Please use overload with IPortalController")]
        public static partial void ClearProfileDefinitionCache(int portalId)
            => ClearProfileDefinitionCache(
                Globals.GetCurrentServiceProvider().GetRequiredService<IPortalController>(),
                Globals.GetCurrentServiceProvider().GetRequiredService<IApplicationStatusInfo>(),
                Globals.GetCurrentServiceProvider().GetRequiredService<IPortalGroupController>(),
                portalId);

        /// <summary>Clears the Profile Definitions Cache.</summary>
        /// <param name="portalController">The portal controller.</param>
        /// <param name="appStatus">The application status.</param>
        /// <param name="portalGroupController">The portal group controller.</param>
        /// <param name="portalId">ID of the Portal.</param>
        public static void ClearProfileDefinitionCache(IPortalController portalController, IApplicationStatusInfo appStatus, IPortalGroupController portalGroupController, int portalId)
        {
            DataCache.ClearDefinitionsCache(PortalController.GetEffectivePortalId(portalController, appStatus, portalGroupController, portalId));
        }

        /// <summary>Deletes a Property Definition from the Data Store.</summary>
        /// <param name="definition">The ProfilePropertyDefinition object to delete.</param>
        [DnnDeprecated(10, 2, 2, "Use overload taking IEventLogger")]
        public static partial void DeletePropertyDefinition(ProfilePropertyDefinition definition)
            => DeletePropertyDefinition(Globals.GetCurrentServiceProvider().GetRequiredService<IEventLogger>(), definition);

        /// <summary>Deletes a Property Definition from the Data Store.</summary>
        /// <param name="eventLogger">The event logger.</param>
        /// <param name="definition">The ProfilePropertyDefinition object to delete.</param>
        [DnnDeprecated(10, 2, 4, "Please use overload with IPortalController")]
        public static partial void DeletePropertyDefinition(IEventLogger eventLogger, ProfilePropertyDefinition definition)
            => DeletePropertyDefinition(
                eventLogger,
                Globals.GetCurrentServiceProvider().GetRequiredService<IPortalController>(),
                Globals.GetCurrentServiceProvider().GetRequiredService<IApplicationStatusInfo>(),
                Globals.GetCurrentServiceProvider().GetRequiredService<IPortalGroupController>(),
                definition);

        /// <summary>Deletes a Property Definition from the Data Store.</summary>
        /// <param name="eventLogger">The event logger.</param>
        /// <param name="portalController">The portal controller.</param>
        /// <param name="appStatus">The application status.</param>
        /// <param name="portalGroupController">The portal group controller.</param>
        /// <param name="definition">The ProfilePropertyDefinition object to delete.</param>
        public static void DeletePropertyDefinition(IEventLogger eventLogger, IPortalController portalController, IApplicationStatusInfo appStatus, IPortalGroupController portalGroupController, ProfilePropertyDefinition definition)
        {
            DataProvider.DeletePropertyDefinition(definition.PropertyDefinitionId);
            eventLogger.AddLog(definition, PortalController.Instance.GetCurrentSettings(), UserController.Instance.GetCurrentUserInfo().UserID, string.Empty, EventLogType.PROFILEPROPERTY_DELETED);
            ClearProfileDefinitionCache(portalController, appStatus, portalGroupController, definition.PortalId);
            ClearAllUsersInfoProfileCacheByPortal(definition.PortalId);
        }

        /// <summary>Clear profiles of all users by portal ID.</summary>
        /// <param name="portalId">The portal ID.</param>
        public static void ClearAllUsersInfoProfileCacheByPortal(int portalId)
        {
            DataCache.ClearCache(string.Format(CultureInfo.InvariantCulture, DataCache.UserCacheKey, portalId, string.Empty));
            DataCache.ClearCache(string.Format(CultureInfo.InvariantCulture, DataCache.UserProfileCacheKey, portalId, string.Empty));
        }

        /// <summary>Gets a Property Definition from the Data Store by ID.</summary>
        /// <param name="definitionId">The ID of the ProfilePropertyDefinition object to retrieve.</param>
        /// <param name="portalId">Portal ID.</param>
        /// <returns>The ProfilePropertyDefinition object.</returns>
        [DnnDeprecated(10, 0, 2, "Use overload taking IHostSettings")]
        public static partial ProfilePropertyDefinition GetPropertyDefinition(int definitionId, int portalId)
            => GetPropertyDefinition(Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettings>(), definitionId, portalId);

        /// <summary>Gets a Property Definition from the Data Store by ID.</summary>
        /// <param name="hostSettings">The host settings.</param>
        /// <param name="definitionId">The ID of the ProfilePropertyDefinition object to retrieve.</param>
        /// <param name="portalId">Portal ID.</param>
        /// <returns>The ProfilePropertyDefinition object.</returns>
        [DnnDeprecated(10, 2, 4, "Please use overload with IPortalController")]
        public static partial ProfilePropertyDefinition GetPropertyDefinition(IHostSettings hostSettings, int definitionId, int portalId)
            => GetPropertyDefinition(
                hostSettings,
                Globals.GetCurrentServiceProvider().GetRequiredService<IPortalController>(),
                Globals.GetCurrentServiceProvider().GetRequiredService<IApplicationStatusInfo>(),
                Globals.GetCurrentServiceProvider().GetRequiredService<IPortalGroupController>(),
                definitionId,
                portalId);

        /// <summary>Gets a Property Definition from the Data Store by ID.</summary>
        /// <param name="hostSettings">The host settings.</param>
        /// <param name="portalController">The portal controller.</param>
        /// <param name="appStatus">The application status.</param>
        /// <param name="portalGroupController">The portal group controller.</param>
        /// <param name="definitionId">The ID of the ProfilePropertyDefinition object to retrieve.</param>
        /// <param name="portalId">Portal ID.</param>
        /// <returns>The ProfilePropertyDefinition object.</returns>
        public static ProfilePropertyDefinition GetPropertyDefinition(IHostSettings hostSettings, IPortalController portalController, IApplicationStatusInfo appStatus, IPortalGroupController portalGroupController, int definitionId, int portalId)
        {
            bool bFound = Null.NullBoolean;
            ProfilePropertyDefinition definition = null;
            foreach (ProfilePropertyDefinition def in GetPropertyDefinitions(hostSettings, PortalController.GetEffectivePortalId(portalController, appStatus, portalGroupController, portalId)))
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
                definition = FillPropertyDefinitionInfo(DataProvider.GetPropertyDefinition(definitionId));
            }

            return definition;
        }

        /// <summary>Gets a Property Definition from the Data Store by name.</summary>
        /// <param name="portalId">The ID of the Portal.</param>
        /// <param name="name">The name of the ProfilePropertyDefinition object to retrieve.</param>
        /// <returns>The ProfilePropertyDefinition object.</returns>
        [DnnDeprecated(10, 0, 2, "Use overload taking IHostSettings")]
        public static partial ProfilePropertyDefinition GetPropertyDefinitionByName(int portalId, string name)
            => GetPropertyDefinitionByName(Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettings>(), portalId, name);

        /// <summary>Gets a Property Definition from the Data Store by name.</summary>
        /// <param name="hostSettings">The host settings.</param>
        /// <param name="portalId">The ID of the Portal.</param>
        /// <param name="name">The name of the ProfilePropertyDefinition object to retrieve.</param>
        /// <returns>The ProfilePropertyDefinition object.</returns>
        [DnnDeprecated(10, 2, 4, "Please use overload with IPortalController")]
        public static partial ProfilePropertyDefinition GetPropertyDefinitionByName(IHostSettings hostSettings, int portalId, string name)
            => GetPropertyDefinitionByName(
                hostSettings,
                Globals.GetCurrentServiceProvider().GetRequiredService<IPortalController>(),
                Globals.GetCurrentServiceProvider().GetRequiredService<IApplicationStatusInfo>(),
                Globals.GetCurrentServiceProvider().GetRequiredService<IPortalGroupController>(),
                portalId,
                name);

        /// <summary>Gets a Property Definition from the Data Store by name.</summary>
        /// <param name="hostSettings">The host settings.</param>
        /// <param name="portalController">The portal controller.</param>
        /// <param name="appStatus">The application status.</param>
        /// <param name="portalGroupController">The portal group controller.</param>
        /// <param name="portalId">The ID of the Portal.</param>
        /// <param name="name">The name of the ProfilePropertyDefinition object to retrieve.</param>
        /// <returns>The ProfilePropertyDefinition object.</returns>
        public static ProfilePropertyDefinition GetPropertyDefinitionByName(IHostSettings hostSettings, IPortalController portalController, IApplicationStatusInfo appStatus, IPortalGroupController portalGroupController, int portalId, string name)
        {
            portalId = PortalController.GetEffectivePortalId(portalController, appStatus, portalGroupController, portalId);

            bool bFound = Null.NullBoolean;
            ProfilePropertyDefinition definition = null;
            foreach (ProfilePropertyDefinition def in GetPropertyDefinitions(hostSettings, portalId))
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
                definition = FillPropertyDefinitionInfo(DataProvider.GetPropertyDefinitionByName(portalId, name));
            }

            return definition;
        }

        /// <summary>Gets a collection of Property Definitions from the Data Store by category.</summary>
        /// <param name="portalId">The ID of the Portal.</param>
        /// <param name="category">The category of the Property Definitions to retrieve.</param>
        /// <returns>A ProfilePropertyDefinitionCollection object.</returns>
        [DnnDeprecated(10, 0, 2, "Use overload taking IHostSettings")]
        public static partial ProfilePropertyDefinitionCollection GetPropertyDefinitionsByCategory(int portalId, string category)
            => GetPropertyDefinitionsByCategory(Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettings>(), portalId, category);

        /// <summary>Gets a collection of Property Definitions from the Data Store by category.</summary>
        /// <param name="hostSettings">The host settings.</param>
        /// <param name="portalId">The ID of the Portal.</param>
        /// <param name="category">The category of the Property Definitions to retrieve.</param>
        /// <returns>A ProfilePropertyDefinitionCollection object.</returns>
        [DnnDeprecated(10, 2, 4, "Please use overload with IPortalController")]
        public static partial ProfilePropertyDefinitionCollection GetPropertyDefinitionsByCategory(IHostSettings hostSettings, int portalId, string category)
            => GetPropertyDefinitionsByCategory(
                hostSettings,
                Globals.GetCurrentServiceProvider().GetRequiredService<IPortalController>(),
                Globals.GetCurrentServiceProvider().GetRequiredService<IApplicationStatusInfo>(),
                Globals.GetCurrentServiceProvider().GetRequiredService<IPortalGroupController>(),
                portalId,
                category);

        /// <summary>Gets a collection of Property Definitions from the Data Store by category.</summary>
        /// <param name="hostSettings">The host settings.</param>
        /// <param name="portalController">The portal controller.</param>
        /// <param name="appStatus">The application status.</param>
        /// <param name="portalGroupController">The portal group controller.</param>
        /// <param name="portalId">The ID of the Portal.</param>
        /// <param name="category">The category of the Property Definitions to retrieve.</param>
        /// <returns>A ProfilePropertyDefinitionCollection object.</returns>
        public static ProfilePropertyDefinitionCollection GetPropertyDefinitionsByCategory(IHostSettings hostSettings, IPortalController portalController, IApplicationStatusInfo appStatus, IPortalGroupController portalGroupController, int portalId, string category)
        {
            portalId = PortalController.GetEffectivePortalId(portalController, appStatus, portalGroupController, portalId);

            var definitions = new ProfilePropertyDefinitionCollection();
            foreach (ProfilePropertyDefinition definition in GetPropertyDefinitions(hostSettings, portalId))
            {
                if (definition.PropertyCategory == category)
                {
                    definitions.Add(definition);
                }
            }

            return definitions;
        }

        /// <summary>Gets a collection of Property Definitions from the Data Store by portal.</summary>
        /// <param name="portalId">The ID of the Portal.</param>
        /// <returns>A ProfilePropertyDefinitionCollection object.</returns>
        [DnnDeprecated(10, 2, 4, "Please use overload with IHostSettings")]
        public static partial ProfilePropertyDefinitionCollection GetPropertyDefinitionsByPortal(int portalId)
            => GetPropertyDefinitionsByPortal(
                Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettings>(),
                Globals.GetCurrentServiceProvider().GetRequiredService<IPortalController>(),
                Globals.GetCurrentServiceProvider().GetRequiredService<IApplicationStatusInfo>(),
                Globals.GetCurrentServiceProvider().GetRequiredService<IPortalGroupController>(),
                portalId);

        /// <summary>Gets a collection of Property Definitions from the Data Store by portal.</summary>
        /// <param name="hostSettings">The host settings.</param>
        /// <param name="portalController">The portal controller.</param>
        /// <param name="appStatus">The application status.</param>
        /// <param name="portalGroupController">The portal group controller.</param>
        /// <param name="portalId">The ID of the Portal.</param>
        /// <returns>A ProfilePropertyDefinitionCollection object.</returns>
        public static ProfilePropertyDefinitionCollection GetPropertyDefinitionsByPortal(IHostSettings hostSettings, IPortalController portalController, IApplicationStatusInfo appStatus, IPortalGroupController portalGroupController, int portalId)
            => GetPropertyDefinitionsByPortal(hostSettings, portalController, appStatus, portalGroupController, portalId, true);

        /// <summary>Gets a collection of Property Definitions from the Data Store by portal.</summary>
        /// <param name="portalId">The ID of the Portal.</param>
        /// <param name="clone">Whether to use a clone object.</param>
        /// <returns>A ProfilePropertyDefinitionCollection object.</returns>
        [DnnDeprecated(10, 2, 4, "Please use overload with IHostSettings")]
        public static partial ProfilePropertyDefinitionCollection GetPropertyDefinitionsByPortal(int portalId, bool clone)
            => GetPropertyDefinitionsByPortal(
                Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettings>(),
                Globals.GetCurrentServiceProvider().GetRequiredService<IPortalController>(),
                Globals.GetCurrentServiceProvider().GetRequiredService<IApplicationStatusInfo>(),
                Globals.GetCurrentServiceProvider().GetRequiredService<IPortalGroupController>(),
                portalId,
                clone);

        /// <summary>Gets a collection of Property Definitions from the Data Store by portal.</summary>
        /// <param name="hostSettings">The host settings.</param>
        /// <param name="portalController">The portal controller.</param>
        /// <param name="appStatus">The application status.</param>
        /// <param name="portalGroupController">The portal group controller.</param>
        /// <param name="portalId">The ID of the Portal.</param>
        /// <param name="clone">Whether to use a clone object.</param>
        /// <returns>A ProfilePropertyDefinitionCollection object.</returns>
        public static ProfilePropertyDefinitionCollection GetPropertyDefinitionsByPortal(IHostSettings hostSettings, IPortalController portalController, IApplicationStatusInfo appStatus, IPortalGroupController portalGroupController, int portalId, bool clone)
            => GetPropertyDefinitionsByPortal(hostSettings, portalController, appStatus, portalGroupController, portalId, clone, true);

        /// <summary>Gets a collection of Property Definitions from the Data Store by portal.</summary>
        /// <param name="portalId">The ID of the Portal.</param>
        /// <param name="clone">Whether to use a clone object.</param>
        /// <param name="includeDeleted">Whether to include deleted profile properties.</param>
        /// <returns>A ProfilePropertyDefinitionCollection object.</returns>
        [DnnDeprecated(10, 0, 2, "Use overload taking IHostSettings")]
        public static partial ProfilePropertyDefinitionCollection GetPropertyDefinitionsByPortal(int portalId, bool clone, bool includeDeleted)
            => GetPropertyDefinitionsByPortal(Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettings>(), portalId, clone, includeDeleted);

        /// <summary>Gets a collection of Property Definitions from the Data Store by portal.</summary>
        /// <param name="hostSettings">The host settings.</param>
        /// <param name="portalId">The ID of the Portal.</param>
        /// <param name="clone">Whether to use a clone object.</param>
        /// <param name="includeDeleted">Whether to include deleted profile properties.</param>
        /// <returns>A ProfilePropertyDefinitionCollection object.</returns>
        [DnnDeprecated(10, 2, 4, "Please use overload with IPortalController")]
        public static partial ProfilePropertyDefinitionCollection GetPropertyDefinitionsByPortal(IHostSettings hostSettings, int portalId, bool clone, bool includeDeleted)
            => GetPropertyDefinitionsByPortal(
                hostSettings,
                Globals.GetCurrentServiceProvider().GetRequiredService<IPortalController>(),
                Globals.GetCurrentServiceProvider().GetRequiredService<IApplicationStatusInfo>(),
                Globals.GetCurrentServiceProvider().GetRequiredService<IPortalGroupController>(),
                portalId,
                clone,
                includeDeleted);

        /// <summary>Gets a collection of Property Definitions from the Data Store by portal.</summary>
        /// <param name="hostSettings">The host settings.</param>
        /// <param name="portalController">The portal controller.</param>
        /// <param name="appStatus">The application status.</param>
        /// <param name="portalGroupController">The portal group controller.</param>
        /// <param name="portalId">The ID of the Portal.</param>
        /// <param name="clone">Whether to use a clone object.</param>
        /// <param name="includeDeleted">Whether to include deleted profile properties.</param>
        /// <returns>A ProfilePropertyDefinitionCollection object.</returns>
        public static ProfilePropertyDefinitionCollection GetPropertyDefinitionsByPortal(IHostSettings hostSettings, IPortalController portalController, IApplicationStatusInfo appStatus, IPortalGroupController portalGroupController, int portalId, bool clone, bool includeDeleted)
        {
            portalId = PortalController.GetEffectivePortalId(portalController, appStatus, portalGroupController, portalId);

            var definitions = new ProfilePropertyDefinitionCollection();
            foreach (ProfilePropertyDefinition definition in GetPropertyDefinitions(hostSettings, portalId))
            {
                if (!definition.Deleted || includeDeleted)
                {
                    definitions.Add(clone ? definition.Clone() : definition);
                }
            }

            return definitions;
        }

        /// <summary>Gets the Profile Information for the User.</summary>
        /// <param name="user">The user whose Profile information we are retrieving.</param>
        [DnnDeprecated(10, 2, 4, "Please use overload with IPortalController")]
        public static partial void GetUserProfile(ref UserInfo user)
            => GetUserProfile(
                Globals.GetCurrentServiceProvider().GetRequiredService<IPortalController>(),
                Globals.GetCurrentServiceProvider().GetRequiredService<IApplicationStatusInfo>(),
                Globals.GetCurrentServiceProvider().GetRequiredService<IPortalGroupController>(),
                ref user);

        /// <summary>Gets the Profile Information for the User.</summary>
        /// <param name="portalController">The portal controller.</param>
        /// <param name="appStatus">The application status.</param>
        /// <param name="portalGroupController">The portal group controller.</param>
        /// <param name="user">The user whose Profile information we are retrieving.</param>
        public static void GetUserProfile(IPortalController portalController, IApplicationStatusInfo appStatus, IPortalGroupController portalGroupController, ref UserInfo user)
        {
            int portalId = user.PortalID;
            user.PortalID = PortalController.GetEffectivePortalId(portalController, appStatus, portalGroupController, portalId);

            ProfileProvider.GetUserProfile(ref user);
            user.PortalID = portalId;
        }

        /// <summary>Updates a Property Definition in the Data Store.</summary>
        /// <param name="definition">The ProfilePropertyDefinition object to update.</param>
        [DnnDeprecated(10, 2, 2, "Use overload taking IEventLogger")]
        public static partial void UpdatePropertyDefinition(ProfilePropertyDefinition definition)
            => UpdatePropertyDefinition(Globals.GetCurrentServiceProvider().GetRequiredService<IEventLogger>(), definition);

        /// <summary>Updates a Property Definition in the Data Store.</summary>
        /// <param name="eventLogger">The event logger.</param>
        /// <param name="definition">The ProfilePropertyDefinition object to update.</param>
        [DnnDeprecated(10, 2, 4, "Please use overload with IPortalController")]
        public static partial void UpdatePropertyDefinition(IEventLogger eventLogger, ProfilePropertyDefinition definition)
            => UpdatePropertyDefinition(
                eventLogger,
                Globals.GetCurrentServiceProvider().GetRequiredService<IPortalController>(),
                Globals.GetCurrentServiceProvider().GetRequiredService<IApplicationStatusInfo>(),
                Globals.GetCurrentServiceProvider().GetRequiredService<IPortalGroupController>(),
                definition);

        /// <summary>Updates a Property Definition in the Data Store.</summary>
        /// <param name="eventLogger">The event logger.</param>
        /// <param name="portalController">The portal controller.</param>
        /// <param name="appStatus">The application status.</param>
        /// <param name="portalGroupController">The portal group controller.</param>
        /// <param name="definition">The ProfilePropertyDefinition object to update.</param>
        public static void UpdatePropertyDefinition(IEventLogger eventLogger, IPortalController portalController, IApplicationStatusInfo appStatus, IPortalGroupController portalGroupController, ProfilePropertyDefinition definition)
        {
            if (definition.Required)
            {
                definition.Visible = true;
            }

            DataProvider.UpdatePropertyDefinition(
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
            eventLogger.AddLog(definition, PortalController.Instance.GetCurrentSettings(), UserController.Instance.GetCurrentUserInfo().UserID, string.Empty, EventLogType.PROFILEPROPERTY_UPDATED);
            ClearProfileDefinitionCache(portalController, appStatus, portalGroupController, definition.PortalId);
            ClearAllUsersInfoProfileCacheByPortal(definition.PortalId);
        }

        /// <summary>Updates a User's Profile.</summary>
        /// <param name="user">The use to update.</param>
        [DnnDeprecated(10, 2, 4, "Please use overload with IPortalController")]
        public static partial void UpdateUserProfile(UserInfo user)
            => UpdateUserProfile(
                Globals.GetCurrentServiceProvider().GetRequiredService<IPortalController>(),
                Globals.GetCurrentServiceProvider().GetRequiredService<IApplicationStatusInfo>(),
                Globals.GetCurrentServiceProvider().GetRequiredService<IPortalGroupController>(),
                user);

        /// <summary>Updates a User's Profile.</summary>
        /// <param name="portalController">The portal controller.</param>
        /// <param name="appStatus">The application status.</param>
        /// <param name="portalGroupController">The portal group controller.</param>
        /// <param name="user">The use to update.</param>
        public static void UpdateUserProfile(IPortalController portalController, IApplicationStatusInfo appStatus, IPortalGroupController portalGroupController, UserInfo user)
        {
            if (!user.Profile.IsDirty)
            {
                return;
            }

            var portalId = PortalController.GetEffectivePortalId(portalController, appStatus, portalGroupController, user.PortalID);
            user.PortalID = portalId;

            var oldUser = new UserInfo { UserID = user.UserID, PortalID = user.PortalID, IsSuperUser = user.IsSuperUser };
            ProfileProvider.GetUserProfile(ref oldUser);

            ProfileProvider.UpdateUserProfile(user);

            // Remove the UserInfo from the Cache, as it has been modified
            DataCache.ClearUserCache(user.PortalID, user.Username);

            // Raise Profile updated event
            EventManager.Instance.OnProfileUpdated(new ProfileEventArgs { User = user, OldProfile = oldUser.Profile });
        }

        /// <summary>Updates a User's Profile.</summary>
        /// <param name="user">The use to update.</param>
        /// <param name="profileProperties">The collection of profile properties.</param>
        /// <returns>The updated User.</returns>
        [DnnDeprecated(10, 2, 4, "Please use overload with IPortalController")]
        public static partial UserInfo UpdateUserProfile(UserInfo user, ProfilePropertyDefinitionCollection profileProperties)
            => UpdateUserProfile(
                Globals.GetCurrentServiceProvider().GetRequiredService<IPortalController>(),
                Globals.GetCurrentServiceProvider().GetRequiredService<IApplicationStatusInfo>(),
                Globals.GetCurrentServiceProvider().GetRequiredService<IPortalGroupController>(),
                Globals.GetCurrentServiceProvider().GetRequiredService<IEventLogger>(),
                user,
                profileProperties);

        /// <summary>Updates a User's Profile.</summary>
        /// <param name="portalController">The portal controller.</param>
        /// <param name="appStatus">The application status.</param>
        /// <param name="portalGroupController">The portal group controller.</param>
        /// <param name="eventLogger">The event logger.</param>
        /// <param name="user">The use to update.</param>
        /// <param name="profileProperties">The collection of profile properties.</param>
        /// <returns>The updated User.</returns>
        public static UserInfo UpdateUserProfile(IPortalController portalController, IApplicationStatusInfo appStatus, IPortalGroupController portalGroupController, IEventLogger eventLogger, UserInfo user, ProfilePropertyDefinitionCollection profileProperties)
        {
            int portalId = PortalController.GetEffectivePortalId(portalController, appStatus, portalGroupController, user.PortalID);
            user.PortalID = portalId;

            var photoChanged = Null.NullBoolean;

            // Iterate through the Definitions
            if (profileProperties is null)
            {
                return user;
            }

            foreach (ProfilePropertyDefinition propertyDefinition in profileProperties)
            {
                string propertyName = propertyDefinition.PropertyName;
                string propertyValue = propertyDefinition.PropertyValue;
                if (propertyDefinition.IsDirty)
                {
                    if (propertyName.Equals(UserProfile.USERPROFILE_Photo, StringComparison.OrdinalIgnoreCase))
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
                    if (!string.IsNullOrEmpty(user.Profile.Photo) && int.Parse(user.Profile.Photo, CultureInfo.InvariantCulture) > 0)
                    {
                        CreateThumbnails(int.Parse(user.Profile.Photo, CultureInfo.InvariantCulture));
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error(ex);
                }
            }

            UserController.UpdateUser(eventLogger, portalId, user);

            return user;
        }

        /// <summary>Validates the Profile properties for the User (determines if all required properties have been set).</summary>
        /// <param name="portalId">The ID of the Portal.</param>
        /// <param name="objProfile">The profile.</param>
        /// <returns><see langword="true"/> if the profile is valid, otherwise <see langword="false"/>.</returns>
        [DnnDeprecated(10, 2, 4, "Please use overload with ListController")]
        public static partial bool ValidateProfile(int portalId, UserProfile objProfile)
            => ValidateProfile(Globals.GetCurrentServiceProvider().GetRequiredService<ListController>(), portalId, objProfile);

        /// <summary>Validates the Profile properties for the User (determines if all required properties have been set).</summary>
        /// <param name="listController">The list controller.</param>
        /// <param name="portalId">The ID of the Portal.</param>
        /// <param name="profile">The profile.</param>
        /// <returns><see langword="true"/> if the profile is valid, otherwise <see langword="false"/>.</returns>
        public static bool ValidateProfile(ListController listController, int portalId, UserProfile profile)
        {
            var isValid = true;
            var imageType = listController.GetListEntryInfo("DataType", "Image");
            foreach (ProfilePropertyDefinition propertyDefinition in profile.ProfileProperties)
            {
                if (propertyDefinition.Required && string.IsNullOrEmpty(propertyDefinition.PropertyValue) && propertyDefinition.DataType != imageType.EntryID)
                {
                    isValid = false;
                    break;
                }
            }

            return isValid;
        }

        /// <summary>Searches the profile property values for a string (doesn't need to be the beginning).</summary>
        /// <param name="portalId">The portal ID.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="searchString">The search string.</param>
        /// <returns>List of matching values.</returns>
        [DnnDeprecated(10, 2, 4, "Please use overload with ListController")]
        public static partial List<string> SearchProfilePropertyValues(int portalId, string propertyName, string searchString)
            => SearchProfilePropertyValues(
                Globals.GetCurrentServiceProvider().GetRequiredService<ListController>(),
                Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettings>(),
                Globals.GetCurrentServiceProvider().GetRequiredService<IPortalController>(),
                Globals.GetCurrentServiceProvider().GetRequiredService<IApplicationStatusInfo>(),
                Globals.GetCurrentServiceProvider().GetRequiredService<IPortalGroupController>(),
                portalId,
                propertyName,
                searchString);

        /// <summary>Searches the profile property values for a string (doesn't need to be the beginning).</summary>
        /// <param name="listController">The list controller.</param>
        /// <param name="hostSettings">The host settings.</param>
        /// <param name="portalController">The portal controller.</param>
        /// <param name="appStatus">The application status.</param>
        /// <param name="portalGroupController">The portal group controller.</param>
        /// <param name="portalId">The portal ID.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="searchString">The search string.</param>
        /// <returns>List of matching values.</returns>
        public static List<string> SearchProfilePropertyValues(ListController listController, IHostSettings hostSettings, IPortalController portalController, IApplicationStatusInfo appStatus, IPortalGroupController portalGroupController, int portalId, string propertyName, string searchString)
        {
            var res = new List<string>();
            var autoCompleteType = listController.GetListEntryInfo("DataType", "AutoComplete");
            var def = GetPropertyDefinitionByName(hostSettings, portalController, appStatus, portalGroupController, portalId, propertyName);
            if (def.DataType != autoCompleteType.EntryID)
            {
                return res;
            }

            using var reader = Data.DataProvider.Instance().SearchProfilePropertyValues(portalId, propertyName, searchString);
            while (reader.Read())
            {
                res.Add(Convert.ToString(reader[0], CultureInfo.InvariantCulture));
            }

            return res;
        }

        /// <inheritdoc cref="GetPropertyDefinition(int,int)"/>
        [DnnDeprecated(7, 0, 0, "Please use GetPropertyDefinition(int definitionId, int portalId) instead", RemovalVersion = 11)]
        public static partial ProfilePropertyDefinition GetPropertyDefinition(int definitionId)
        {
            return CBO.FillObject<ProfilePropertyDefinition>(DataProvider.GetPropertyDefinition(definitionId));
        }

        internal static void AddDefaultDefinition(IEventLogger eventLogger, IPortalController portalController, IApplicationStatusInfo appStatus, IPortalGroupController portalGroupController, int portalId, string category, string name, string type, int length, int viewOrder, UserVisibilityMode defaultVisibility, Dictionary<string, ListEntryInfo> types)
        {
            ListEntryInfo typeInfo = types[$"DataType:{type}"] ?? types["DataType:Unknown"];
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
            AddPropertyDefinition(eventLogger, portalController, appStatus, portalGroupController, propertyDefinition);
        }

        private static void AddDefaultDefinition(IEventLogger eventLogger, IPortalController portalController, IApplicationStatusInfo appStatus, IPortalGroupController portalGroupController, int portalId, string category, string name, string strType, int length, UserVisibilityMode defaultVisibility, Dictionary<string, ListEntryInfo> types)
        {
            orderCounter += 2;
            AddDefaultDefinition(eventLogger, portalController, appStatus, portalGroupController, portalId, category, name, strType, length, orderCounter, defaultVisibility, types);
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
            ProfilePropertyDefinition definition;

            // read datareader
            var canContinue = true;
            if (checkForOpenDataReader)
            {
                canContinue = dr.Read();
            }

            if (!canContinue)
            {
                return null;
            }

            var portalId = 0;
            portalId = Convert.ToInt32(Null.SetNull(dr["PortalId"], portalId), CultureInfo.InvariantCulture);
            definition = new ProfilePropertyDefinition(portalId);
            definition.PropertyDefinitionId = Convert.ToInt32(Null.SetNull(dr["PropertyDefinitionId"], definition.PropertyDefinitionId), CultureInfo.InvariantCulture);
            definition.ModuleDefId = Convert.ToInt32(Null.SetNull(dr["ModuleDefId"], definition.ModuleDefId), CultureInfo.InvariantCulture);
            definition.DataType = Convert.ToInt32(Null.SetNull(dr["DataType"], definition.DataType), CultureInfo.InvariantCulture);
            definition.DefaultValue = Convert.ToString(Null.SetNull(dr["DefaultValue"], definition.DefaultValue), CultureInfo.InvariantCulture);
            definition.PropertyCategory = Convert.ToString(Null.SetNull(dr["PropertyCategory"], definition.PropertyCategory), CultureInfo.InvariantCulture);
            definition.PropertyName = Convert.ToString(Null.SetNull(dr["PropertyName"], definition.PropertyName), CultureInfo.InvariantCulture);
            definition.Length = Convert.ToInt32(Null.SetNull(dr["Length"], definition.Length), CultureInfo.InvariantCulture);
            if (dr.GetSchemaTable().Select("ColumnName = 'ReadOnly'").Length > 0)
            {
                definition.ReadOnly = Convert.ToBoolean(Null.SetNull(dr["ReadOnly"], definition.ReadOnly), CultureInfo.InvariantCulture);
            }

            definition.Required = Convert.ToBoolean(Null.SetNull(dr["Required"], definition.Required), CultureInfo.InvariantCulture);
            definition.ValidationExpression = Convert.ToString(Null.SetNull(dr["ValidationExpression"], definition.ValidationExpression), CultureInfo.InvariantCulture);
            definition.ViewOrder = Convert.ToInt32(Null.SetNull(dr["ViewOrder"], definition.ViewOrder), CultureInfo.InvariantCulture);
            definition.Visible = Convert.ToBoolean(Null.SetNull(dr["Visible"], definition.Visible), CultureInfo.InvariantCulture);
            definition.DefaultVisibility = (UserVisibilityMode)Convert.ToInt32(Null.SetNull(dr["DefaultVisibility"], definition.DefaultVisibility), CultureInfo.InvariantCulture);
            definition.ProfileVisibility = new ProfileVisibility
            {
                VisibilityMode = definition.DefaultVisibility,
            };
            definition.Deleted = Convert.ToBoolean(Null.SetNull(dr["Deleted"], definition.Deleted), CultureInfo.InvariantCulture);

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

        private static List<ProfilePropertyDefinition> GetPropertyDefinitions(IHostSettings hostSettings, int portalId)
        {
            // Get the Cache Key
            string key = string.Format(CultureInfo.InvariantCulture, DataCache.ProfileDefinitionsCacheKey, portalId);

            // Try fetching the List from the Cache
            var definitions = (List<ProfilePropertyDefinition>)DataCache.GetCache(key);
            if (definitions is not null)
            {
                return definitions;
            }

            // definitions caching settings
            int timeOut = DataCache.ProfileDefinitionsCacheTimeOut * (int)hostSettings.PerformanceSetting;

            // Get the List from the database
            definitions = FillPropertyDefinitionInfoCollection(DataProvider.GetPropertyDefinitionsByPortal(portalId));

            // Cache the List
            if (timeOut > 0)
            {
                DataCache.SetCache(key, definitions, TimeSpan.FromMinutes(timeOut));
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
            if (file == null)
            {
                return;
            }

            var folder = FolderManager.Instance.GetFolder(file.FolderId);
            var extension = $".{file.Extension}";
            var sizedPhoto = file.FileName.Replace(extension, $"_{type}{extension}");
            if (FileManager.Instance.FileExists(folder, sizedPhoto))
            {
                return;
            }

            using var content = FileManager.Instance.GetFileContent(file);
            var sizedContent = ImageUtils.CreateImage(content, height, width, extension);

            const bool operationDoesNotRequirePermissionsCheck = true;
            FileManager.Instance.AddFile(folder, sizedPhoto, sizedContent, false, !operationDoesNotRequirePermissionsCheck, FileContentTypeManager.Instance.GetContentType(Path.GetExtension(sizedPhoto)));
        }
    }
}
