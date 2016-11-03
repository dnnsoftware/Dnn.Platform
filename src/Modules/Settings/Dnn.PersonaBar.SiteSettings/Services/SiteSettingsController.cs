#region Copyright
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2016
// by DotNetNuke Corporation
// All Rights Reserved
#endregion

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Web;
using System.Web.Http;
using Dnn.PersonaBar.Library;
using Dnn.PersonaBar.Library.Attributes;
using Dnn.PersonaBar.SiteSettings.Services.Dto;
using DotNetNuke.Common;
using DotNetNuke.Common.Lists;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Controllers;
using DotNetNuke.Entities.Icons;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Profile;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Entities.Urls;
using DotNetNuke.Entities.Users;
using DotNetNuke.Instrumentation;
using DotNetNuke.Services.Localization;
using DotNetNuke.Services.Personalization;
using DotNetNuke.Services.Search.Internals;
using DotNetNuke.UI.Internals;
using DotNetNuke.UI.Skins;
using DotNetNuke.Web.Api;

namespace Dnn.PersonaBar.SiteSettings.Services
{
    [ServiceScope(Scope = ServiceScope.Admin, Identifier = "SiteSettings")]
    public class SiteSettingsController : PersonaBarApiController
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(SiteSettingsController));
        private readonly Components.SiteSettingsController _controller = new Components.SiteSettingsController();
        private static readonly string LocalResourcesFile = Path.Combine("~/admin/Dnn.PersonaBar/App_LocalResources/SiteSettings.resx");
        private static readonly string ProfileResourceFile = "~/DesktopModules/Admin/Security/App_LocalResources/Profile.ascx";

        //Field Boost Settings - they are scaled down by 10.
        private const int DefaultSearchTitleBoost = 50;
        private const int DefaultSearchTagBoost = 40;
        private const int DefaultSearchContentBoost = 35;
        private const int DefaultSearchDescriptionBoost = 20;
        private const int DefaultSearchAuthorBoost = 15;

        //Field Bosst Setting Names
        private const string SearchTitleBoostSetting = "Search_Title_Boost";
        private const string SearchTagBoostSetting = "Search_Tag_Boost";
        private const string SearchContentBoostSetting = "Search_Content_Boost";
        private const string SearchDescriptionBoostSetting = "Search_Description_Boost";
        private const string SearchAuthorBoostSetting = "Search_Author_Boost";

        /// GET: api/SiteSettings/GetPortalSettings
        /// <summary>
        /// Gets site settings
        /// </summary>
        /// <param name="portalId"></param>
        /// <returns>site settings</returns>
        [HttpGet]
        public HttpResponseMessage GetPortalSettings([FromUri] int? portalId)
        {
            try
            {
                var pid = portalId ?? PortalId;
                var portalSettings = new PortalSettings(pid);
                var portal = PortalController.Instance.GetPortal(pid);
                var cultureCode = LocaleController.Instance.GetCurrentLocale(pid).Code;
                var settings = new
                {
                    portal.PortalName,
                    portal.Description,
                    portal.KeyWords,
                    GUID = portal.GUID.ToString().ToUpper(),
                    portal.FooterText,
                    TimeZone = portalSettings.TimeZone.Id,
                    portal.HomeDirectory,
                    portal.LogoFile,
                    FavIcon = new FavIcon(portal.PortalID).GetSettingPath(),
                    IconSet = PortalController.GetPortalSetting("DefaultIconLocation", pid, "Sigma", cultureCode).Replace("icons/", "")
                };
                return Request.CreateResponse(HttpStatusCode.OK, new
                {
                    Settings = settings,
                    TimeZones = TimeZoneInfo.GetSystemTimeZones().Select(z => new
                    {
                        z.Id,
                        z.DisplayName
                    }),
                    IconSets = IconController.GetIconSets()
                });
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        /// POST: api/SiteSettings/UpdatePortalSettings
        /// <summary>
        /// Updates site settings
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage UpdatePortalSettings(UpdateSiteSettingsRequest request)
        {
            try
            {
                var pid = request.PortalId ?? PortalId;
                var cultureCode = LocaleController.Instance.GetCurrentLocale(pid).Code;
                var portalInfo = PortalController.Instance.GetPortal(pid);
                portalInfo.PortalName = request.PortalName;
                portalInfo.LogoFile = request.LogoFile;
                portalInfo.FooterText = request.FooterText;
                portalInfo.Description = request.Description;
                portalInfo.KeyWords = request.KeyWords;

                PortalController.Instance.UpdatePortalInfo(portalInfo);
                PortalController.UpdatePortalSetting(pid, "TimeZone", request.TimeZone, false);
                new FavIcon(pid).Update(request.FavIcon);
                PortalController.UpdatePortalSetting(pid, "DefaultIconLocation", "icons/" + request.IconSet, false, cultureCode);

                return Request.CreateResponse(HttpStatusCode.OK, new { Success = true });
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        /// GET: api/SiteSettings/GetDefaultPagesSettings
        /// <summary>
        /// Gets default pages settings
        /// </summary>
        /// <param name="portalId"></param>
        /// <returns>default pages settings</returns>
        [HttpGet]
        public HttpResponseMessage GetDefaultPagesSettings([FromUri] int? portalId)
        {
            try
            {
                var pid = portalId ?? PortalId;
                var portal = PortalController.Instance.GetPortal(pid);
                var portalSettings = new PortalSettings(pid);

                return Request.CreateResponse(HttpStatusCode.OK, new
                {
                    Settings = new
                    {
                        portal.SplashTabId,
                        SplashTabName = portal.SplashTabId != Null.NullInteger ? TabController.Instance.GetTab(portal.SplashTabId, pid).TabName : string.Empty,
                        portal.HomeTabId,
                        HomeTabName = portal.HomeTabId != Null.NullInteger ? TabController.Instance.GetTab(portal.HomeTabId, pid).TabName : string.Empty,
                        portal.LoginTabId,
                        LoginTabName = portal.LoginTabId != Null.NullInteger ? TabController.Instance.GetTab(portal.LoginTabId, pid).TabName : string.Empty,
                        portal.RegisterTabId,
                        RegisterTabName = portal.RegisterTabId != Null.NullInteger ? TabController.Instance.GetTab(portal.RegisterTabId, pid).TabName : string.Empty,
                        portal.UserTabId,
                        UserTabName = portal.UserTabId != Null.NullInteger ? TabController.Instance.GetTab(portal.UserTabId, pid).TabName : string.Empty,
                        portal.SearchTabId,
                        SearchTabName = portal.SearchTabId != Null.NullInteger ? TabController.Instance.GetTab(portal.SearchTabId, pid).TabName : string.Empty,
                        portal.Custom404TabId,
                        Custom404TabName = portal.Custom404TabId != Null.NullInteger ? TabController.Instance.GetTab(portal.Custom404TabId, pid).TabName : string.Empty,
                        portal.Custom500TabId,
                        Custom500TabName = portal.Custom500TabId != Null.NullInteger ? TabController.Instance.GetTab(portal.Custom500TabId, pid).TabName : string.Empty,
                        portalSettings.PageHeadText
                    }
                });
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        /// POST: api/SiteSettings/UpdateDefaultPagesSettings
        /// <summary>
        /// Updates default pages settings
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage UpdateDefaultPagesSettings(UpdateDefaultPagesSettingsRequest request)
        {
            try
            {
                var pid = request.PortalId ?? PortalId;
                var portalInfo = PortalController.Instance.GetPortal(pid);
                portalInfo.SplashTabId = request.SplashTabId;
                portalInfo.HomeTabId = request.HomeTabId;
                portalInfo.LoginTabId = request.LoginTabId;
                portalInfo.RegisterTabId = request.RegisterTabId;
                portalInfo.UserTabId = request.UserTabId;
                portalInfo.SearchTabId = request.SearchTabId;
                portalInfo.Custom404TabId = request.Custom404TabId;
                portalInfo.Custom500TabId = request.Custom500TabId;

                PortalController.Instance.UpdatePortalInfo(portalInfo);
                PortalController.UpdatePortalSetting(pid, "PageHeadText", string.IsNullOrEmpty(request.PageHeadText) ? "false" : request.PageHeadText);

                return Request.CreateResponse(HttpStatusCode.OK, new { Success = true });
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        /// GET: api/SiteSettings/GetMessagingSettings
        /// <summary>
        /// Gets messaging settings
        /// </summary>
        /// <param name="portalId"></param>
        /// <returns>messaging settings</returns>
        [HttpGet]
        public HttpResponseMessage GetMessagingSettings([FromUri] int? portalId)
        {
            try
            {
                var pid = portalId ?? PortalId;
                var portalSettings = new PortalSettings(pid);

                return Request.CreateResponse(HttpStatusCode.OK, new
                {
                    Settings = new
                    {
                        portalSettings.DisablePrivateMessage,
                        ThrottlingInterval = PortalController.GetPortalSettingAsInteger("MessagingThrottlingInterval", pid, 0),
                        RecipientLimit = PortalController.GetPortalSettingAsInteger("MessagingRecipientLimit", pid, 5),
                        AllowAttachments = PortalController.GetPortalSettingAsBoolean("MessagingAllowAttachments", pid, false),
                        ProfanityFilters = PortalController.GetPortalSettingAsBoolean("MessagingProfanityFilters", pid, false),
                        IncludeAttachments = PortalController.GetPortalSettingAsBoolean("MessagingIncludeAttachments", pid, false),
                        SendEmail = PortalController.GetPortalSettingAsBoolean("MessagingSendEmail", pid, false)
                    }
                });
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        /// POST: api/SiteSettings/UpdateMessagingSettings
        /// <summary>
        /// Updates messaging settings
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage UpdateMessagingSettings(UpdateMessagingSettingsRequest request)
        {
            try
            {
                var pid = request.PortalId ?? PortalId;

                PortalController.UpdatePortalSetting(pid, "MessagingThrottlingInterval", request.ThrottlingInterval.ToString(), false);
                PortalController.UpdatePortalSetting(pid, "MessagingRecipientLimit", request.RecipientLimit.ToString(), false);
                PortalController.UpdatePortalSetting(pid, "MessagingAllowAttachments", request.AllowAttachments ? "YES" : "NO", false);
                PortalController.UpdatePortalSetting(pid, "MessagingIncludeAttachments", request.IncludeAttachments ? "YES" : "NO", false);

                PortalController.UpdatePortalSetting(pid, "MessagingProfanityFilters", request.ProfanityFilters ? "YES" : "NO", false);
                PortalController.UpdatePortalSetting(pid, "MessagingSendEmail", request.SendEmail ? "YES" : "NO", false);
                PortalController.UpdatePortalSetting(pid, "DisablePrivateMessage", request.DisablePrivateMessage ? "Y" : "N", false);

                DataCache.ClearPortalCache(pid, false);

                return Request.CreateResponse(HttpStatusCode.OK, new { Success = true });
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        /// GET: api/SiteSettings/GetProfileSettings
        /// <summary>
        /// Gets profile settings
        /// </summary>
        /// <param name="portalId"></param>
        /// <returns>profile settings</returns>
        [HttpGet]
        public HttpResponseMessage GetProfileSettings([FromUri] int? portalId)
        {
            try
            {
                var pid = portalId ?? PortalId;
                var urlSettings = new FriendlyUrlSettings(pid);
                var userSettings = UserController.GetUserSettings(pid);

                return Request.CreateResponse(HttpStatusCode.OK, new
                {
                    Settings = new
                    {
                        RedirectOldProfileUrl = Config.GetFriendlyUrlProvider() == "advanced" && urlSettings.RedirectOldProfileUrl,
                        urlSettings.VanityUrlPrefix,
                        ProfileDefaultVisibility = userSettings["Profile_DefaultVisibility"] == null ? (int)UserVisibilityMode.AdminOnly : Convert.ToInt32(userSettings["Profile_DefaultVisibility"]),
                        ProfileDisplayVisibility = PortalController.GetPortalSettingAsBoolean("Profile_DisplayVisibility", pid, true)
                    },
                    UserVisibilityOptions = Enum.GetValues(typeof(UserVisibilityMode)).Cast<UserVisibilityMode>().Select(
                        v => new
                        {
                            label = v.ToString(),
                            value = (int)v
                        }).ToList()
                });
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        /// POST: api/SiteSettings/UpdateProfileSettings
        /// <summary>
        /// Updates profile settings
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage UpdateProfileSettings(UpdateProfileSettingsRequest request)
        {
            try
            {
                var pid = request.PortalId ?? PortalId;

                if (Config.GetFriendlyUrlProvider() == "advanced")
                {
                    PortalController.UpdatePortalSetting(pid, FriendlyUrlSettings.RedirectOldProfileUrlSetting, request.RedirectOldProfileUrl ? "Y" : "N", false);
                }
                PortalController.UpdatePortalSetting(pid, FriendlyUrlSettings.VanityUrlPrefixSetting, request.VanityUrlPrefix, false);
                PortalController.UpdatePortalSetting(pid, "Profile_DefaultVisibility", request.ProfileDefaultVisibility.ToString(), false);
                PortalController.UpdatePortalSetting(pid, "Profile_DisplayVisibility", request.ProfileDisplayVisibility.ToString(), true);

                DataCache.ClearPortalCache(pid, false);

                return Request.CreateResponse(HttpStatusCode.OK, new { Success = true });
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        /// GET: api/SiteSettings/GetProfileProperties
        /// <summary>
        /// Gets profile properties
        /// </summary>
        /// <param name="portalId"></param>
        /// <returns>profile properties</returns>
        [HttpGet]
        public HttpResponseMessage GetProfileProperties([FromUri] int? portalId)
        {
            try
            {
                var pid = portalId ?? PortalId;
                var profileProperties = ProfileController.GetPropertyDefinitionsByPortal(pid, false, false).Cast<ProfilePropertyDefinition>().Select(v => new
                {
                    v.PropertyDefinitionId,
                    v.PropertyName,
                    DataType = DisplayDataType(v.DataType),
                    DefaultVisibility = v.DefaultVisibility.ToString(),
                    v.Required,
                    v.Visible,
                    CanDelete = CanDeleteProperty(v)
                });

                return Request.CreateResponse(HttpStatusCode.OK, new
                {
                    ProfileProperties = profileProperties
                });
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        private string DisplayDataType(int dataType)
        {
            var retValue = Null.NullString;
            var listController = new ListController();
            var definitionEntry = listController.GetListEntryInfo("DataType", dataType);
            if (definitionEntry != null)
            {
                retValue = definitionEntry.Value;
            }
            return retValue;
        }

        /// GET: api/SiteSettings/GetProfileProperty
        /// <summary>
        /// Gets profile property by id
        /// </summary>
        /// <param name="propertyId"></param>
        /// <param name="portalId"></param>
        /// <returns>profile property</returns>
        [HttpGet]
        public HttpResponseMessage GetProfileProperty([FromUri]int? propertyId, [FromUri] int? portalId)
        {
            try
            {
                var pid = portalId ?? PortalId;
                var profileProperty = ProfileController.GetPropertyDefinition(propertyId ?? -1, pid);
                var listController = new ListController();

                var cultureList = Localization.LoadCultureInListItems(CultureDropDownTypes.NativeName, Thread.CurrentThread.CurrentUICulture.Name, "", false);

                var response = new
                {
                    Success = true,
                    ProfileProperty = profileProperty != null ? new
                    {
                        profileProperty.PropertyDefinitionId,
                        profileProperty.PropertyName,
                        profileProperty.DataType,
                        profileProperty.PropertyCategory,
                        profileProperty.Length,
                        profileProperty.DefaultValue,
                        profileProperty.ValidationExpression,
                        profileProperty.Required,
                        profileProperty.ReadOnly,
                        profileProperty.Visible,
                        profileProperty.ViewOrder,
                        DefaultVisibility = (int)profileProperty.DefaultVisibility
                    } : null,
                    UserVisibilityOptions = Enum.GetValues(typeof(UserVisibilityMode)).Cast<UserVisibilityMode>().Select(
                        v => new
                        {
                            label = v.ToString(),
                            value = (int)v
                        }).ToList(),
                    DataTypeOptions = listController.GetListEntryInfoItems("DataType").Select(t => new
                    {
                        t.EntryID,
                        t.Value
                    }),
                    LanguageOptions = cultureList.Select(c => new
                    {
                        c.Text,
                        c.Value
                    })
                };
                return Request.CreateResponse(HttpStatusCode.OK, response);
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        /// GET: api/SiteSettings/GetProfilePropertyLocalization
        /// <summary>
        /// Gets profile property localization
        /// </summary>
        /// <param name="propertyName"></param>
        /// <param name="propertyCategory"></param>
        /// <param name="cultureCode"></param>
        /// <returns>profile property</returns>
        [HttpGet]
        public HttpResponseMessage GetProfilePropertyLocalization(string propertyName, string propertyCategory, [FromUri] string cultureCode)
        {
            try
            {
                if (string.IsNullOrEmpty(cultureCode))
                {
                    cultureCode = PortalSettings.CultureCode;
                }

                var response = new
                {
                    Success = true,
                    PropertyLocalization = new
                    {
                        Language = cultureCode,
                        PropertyName = Localization.GetString("ProfileProperties_" + propertyName, ProfileResourceFile, cultureCode),
                        PropertyHelp = Localization.GetString("ProfileProperties_" + propertyName + ".Help", ProfileResourceFile, cultureCode),
                        PropertyRequired = Localization.GetString("ProfileProperties_" + propertyName + ".Required", ProfileResourceFile, cultureCode),
                        PropertyValidation = Localization.GetString("ProfileProperties_" + propertyName + ".Validation", ProfileResourceFile, cultureCode),
                        CategoryName = Localization.GetString("ProfileProperties_" + propertyCategory + ".Header", ProfileResourceFile, cultureCode)
                    }
                };
                return Request.CreateResponse(HttpStatusCode.OK, response);
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        /// POST: api/SiteSettings/UpdateProfilePropertyLocalization
        /// <summary>
        /// Updates profile property localization
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage UpdateProfilePropertyLocalization(UpdateProfilePropertyLocalizationRequest request)
        {
            try
            {
                var pid = request.PortalId ?? PortalId;
                _controller.SaveLocalizedKeys(pid, request.PropertyName, request.PropertyCategory, request.Language, request.PropertyNameString,
                    request.PropertyHelpString, request.PropertyRequiredString, request.PropertyValidationString, request.CategoryNameString);

                return Request.CreateResponse(HttpStatusCode.OK, new { Success = true });
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        /// POST: api/SiteSettings/AddProfileProperty
        /// <summary>
        /// Creates profile property
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage AddProfileProperty(UpdateProfilePropertyRequest request)
        {
            try
            {
                var pid = request.PortalId ?? PortalId;
                var property = new ProfilePropertyDefinition(pid)
                {
                    DataType = request.DataType,
                    DefaultValue = request.DefaultValue,
                    PropertyCategory = request.PropertyCategory,
                    PropertyName = request.PropertyName,
                    ReadOnly = request.ReadOnly,
                    Required = !UserInfo.IsSuperUser && request.Required,
                    ValidationExpression = request.ValidationExpression,
                    ViewOrder = request.ViewOrder,
                    Visible = request.Visible,
                    Length = request.Length,
                    DefaultVisibility = (UserVisibilityMode)request.DefaultVisibility
                };

                if (ValidateProperty(property))
                {
                    var propertyId = ProfileController.AddPropertyDefinition(property);
                    if (propertyId < Null.NullInteger)
                    {
                        return Request.CreateErrorResponse(HttpStatusCode.BadRequest,
                            string.Format(Localization.GetString("DuplicateName", LocalResourcesFile)));
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.OK, new { Success = true });
                    }
                }
                else
                {
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest,
                            string.Format(Localization.GetString("RequiredTextBox", LocalResourcesFile)));
                }
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        private bool ValidateProperty(ProfilePropertyDefinition definition)
        {
            bool isValid = true;
            var objListController = new ListController();
            string strDataType = objListController.GetListEntryInfo("DataType", definition.DataType).Value;

            switch (strDataType)
            {
                case "Text":
                    if (definition.Required && definition.Length == 0)
                    {
                        isValid = Null.NullBoolean;
                    }
                    break;
            }
            return isValid;
        }

        /// POST: api/SiteSettings/UpdateProfileProperty
        /// <summary>
        /// Updates profile property
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage UpdateProfileProperty(UpdateProfilePropertyRequest request)
        {
            try
            {
                var pid = request.PortalId ?? PortalId;
                var definitionId = request.PropertyDefinitionId ?? Null.NullInteger;

                if (definitionId != Null.NullInteger)
                {
                    var property = new ProfilePropertyDefinition(pid)
                    {
                        PropertyDefinitionId = definitionId,
                        DataType = request.DataType,
                        DefaultValue = request.DefaultValue,
                        PropertyCategory = request.PropertyCategory,
                        PropertyName = request.PropertyName,
                        ReadOnly = request.ReadOnly,
                        Required = request.Required,
                        ValidationExpression = request.ValidationExpression,
                        ViewOrder = request.ViewOrder,
                        Visible = request.Visible,
                        Length = request.Length,
                        DefaultVisibility = (UserVisibilityMode)request.DefaultVisibility
                    };

                    if (ValidateProperty(property))
                    {
                        ProfileController.UpdatePropertyDefinition(property);
                        return Request.CreateResponse(HttpStatusCode.OK, new { Success = true });
                    }
                    else
                    {
                        return Request.CreateErrorResponse(HttpStatusCode.BadRequest,
                            string.Format(Localization.GetString("RequiredTextBox", LocalResourcesFile)));
                    }
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, new { Success = false });
                }
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        /// POST: api/SiteSettings/DeleteProfileProperty
        /// <summary>
        /// Deletes profile property
        /// </summary>
        /// <param name="propertyId"></param>
        /// <param name="portalId"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage DeleteProfileProperty(int propertyId, [FromUri] int? portalId)
        {
            try
            {
                var pid = portalId ?? PortalId;
                var propertyDefinition = new ProfilePropertyDefinition(pid)
                {
                    PropertyDefinitionId = propertyId
                };

                if (!CanDeleteProperty(propertyDefinition))
                {
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "ForbiddenDelete");
                }

                ProfileController.DeletePropertyDefinition(propertyDefinition);

                return Request.CreateResponse(HttpStatusCode.OK, new { Success = true });
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        private bool CanDeleteProperty(ProfilePropertyDefinition definition)
        {
            switch (definition.PropertyName.ToLowerInvariant())
            {
                case "lastname":
                case "firstname":
                case "preferredtimezone":
                case "preferredlocale":
                    return false;
                default:
                    return true;
            }
        }

        /// GET: api/SiteSettings/GetUrlMappingSettings
        /// <summary>
        /// Gets Url mapping settings
        /// </summary>
        /// <param name="portalId"></param>
        /// <returns>Url mapping settings</returns>
        [HttpGet]
        [DnnAuthorize(StaticRoles = "Superusers")]
        public HttpResponseMessage GetUrlMappingSettings([FromUri] int? portalId)
        {
            try
            {
                var pid = portalId ?? PortalId;
                Dictionary<string, string> settings = PortalController.Instance.GetPortalSettings(pid);
                string portalAliasMapping;
                if (settings.TryGetValue("PortalAliasMapping", out portalAliasMapping))
                {
                    if (string.IsNullOrEmpty(portalAliasMapping))
                    {
                        portalAliasMapping = "CANONICALURL";
                    }
                }
                else
                {
                    portalAliasMapping = "CANONICALURL";
                }

                var portalAliasMappingModes = new List<KeyValuePair<string, string>>();
                portalAliasMappingModes.Add(new KeyValuePair<string, string>(Localization.GetString("Canonical", LocalResourcesFile), "CANONICALURL"));
                portalAliasMappingModes.Add(new KeyValuePair<string, string>(Localization.GetString("Redirect", LocalResourcesFile), "REDIRECT"));
                portalAliasMappingModes.Add(new KeyValuePair<string, string>(Localization.GetString("None", LocalResourcesFile), "NONE"));

                var response = new
                {
                    Success = true,
                    Settings = new
                    {
                        PortalAliasMapping = portalAliasMapping,
                        AutoAddPortalAliasEnabled = !(PortalController.Instance.GetPortals().Count > 1),
                        AutoAddPortalAlias = PortalController.Instance.GetPortals().Count <= 1 && HostController.Instance.GetBoolean("AutoAddPortalAlias")
                    },
                    PortalAliasMappingModes = portalAliasMappingModes
                };
                return Request.CreateResponse(HttpStatusCode.OK, response);
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        /// POST: api/SiteSettings/UpdateUrlMappingSettings
        /// <summary>
        /// Updates Url mapping settings
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [DnnAuthorize(StaticRoles = "Superusers")]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage UpdateUrlMappingSettings(UpdateUrlMappingSettingsRequest request)
        {
            try
            {
                var pid = request.PortalId ?? PortalId;
                PortalController.UpdatePortalSetting(pid, "PortalAliasMapping", request.PortalAliasMapping, false);
                HostController.Instance.Update("AutoAddPortalAlias", request.AutoAddPortalAlias ? "Y" : "N", true);

                DataCache.ClearPortalCache(pid, false);

                return Request.CreateResponse(HttpStatusCode.OK, new { Success = true });
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        /// GET: api/SiteSettings/getSiteAliases
        /// <summary>
        /// Gets site aliases
        /// </summary>
        /// <param name="portalId"></param>
        /// <returns>site aliases</returns>
        [HttpGet]
        [DnnAuthorize(StaticRoles = "Superusers")]
        public HttpResponseMessage GetSiteAliases([FromUri] int? portalId)
        {
            try
            {
                var pid = portalId ?? PortalId;
                var portal = PortalController.Instance.GetPortal(pid);
                var aliases = PortalAliasController.Instance.GetPortalAliasesByPortalId(pid).Select(a => new
                {
                    a.PortalAliasID,
                    a.HTTPAlias,
                    BrowserType = a.BrowserType.ToString(),
                    a.Skin,
                    a.IsPrimary,
                    a.CultureCode,
                    Deletable = a.PortalAliasID != PortalSettings.PortalAlias.PortalAliasID && !a.IsPrimary,
                    Editable = a.PortalAliasID != PortalSettings.PortalAlias.PortalAliasID
                });

                var response = new
                {
                    Success = true,
                    PortalAliases = aliases,
                    BrowserTypes = Enum.GetNames(typeof(BrowserTypes)),
                    Languages = LocaleController.Instance.GetLocales(pid).Select(l => new
                    {
                        l.Key,
                        Value = l.Key
                    }),
                    Skins = SkinController.GetSkins(portal, SkinController.RootSkin, SkinScope.All)
                };
                return Request.CreateResponse(HttpStatusCode.OK, response);
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        /// GET: api/SiteSettings/GetSiteAlias
        /// <summary>
        /// Gets site alias by id
        /// </summary>
        /// <param name="portalAliasId"></param>
        /// <returns>site alias</returns>
        [HttpGet]
        [DnnAuthorize(StaticRoles = "Superusers")]
        public HttpResponseMessage GetSiteAlias([FromUri]int portalAliasId)
        {
            try
            {
                var alias = PortalAliasController.Instance.GetPortalAliasByPortalAliasID(portalAliasId);

                var response = new
                {
                    Success = true,
                    PortalAlias = new
                    {
                        alias.PortalAliasID,
                        alias.HTTPAlias,
                        BrowserType = alias.BrowserType.ToString(),
                        alias.Skin,
                        alias.IsPrimary,
                        alias.CultureCode
                    }
                };
                return Request.CreateResponse(HttpStatusCode.OK, response);
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        /// POST: api/SiteSettings/AddSiteAlias
        /// <summary>
        /// Adds site alias
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [DnnAuthorize(StaticRoles = "Superusers")]
        public HttpResponseMessage AddSiteAlias(UpdateSiteAliasRequest request)
        {
            try
            {
                var pid = request.PortalId ?? PortalId;
                string strAlias = request.HTTPAlias;
                if (!string.IsNullOrEmpty(strAlias))
                {
                    strAlias = strAlias.Trim();
                }

                if (IsHttpAliasValid(strAlias))
                {
                    var aliases = PortalAliasController.Instance.GetPortalAliases();
                    if (aliases.Contains(strAlias))
                    {
                        return Request.CreateErrorResponse(HttpStatusCode.BadRequest,
                            string.Format(Localization.GetString("DuplicateAlias", LocalResourcesFile)));
                    }

                    BrowserTypes browser;
                    Enum.TryParse(request.BrowserType, out browser);
                    PortalAliasInfo portalAlias = new PortalAliasInfo()
                    {
                        PortalID = pid,
                        HTTPAlias = strAlias,
                        Skin = request.Skin,
                        CultureCode = request.CultureCode,
                        BrowserType = browser,
                        IsPrimary = request.IsPrimary
                    };

                    PortalAliasController.Instance.AddPortalAlias(portalAlias);
                }
                else
                {
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest,
                            string.Format(Localization.GetString("InvalidAlias", LocalResourcesFile)));
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { Success = true });
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        /// POST: api/SiteSettings/UpdateSiteAlias
        /// <summary>
        /// Updates site alias
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [DnnAuthorize(StaticRoles = "Superusers")]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage UpdateSiteAlias(UpdateSiteAliasRequest request)
        {
            try
            {
                var pid = request.PortalId ?? PortalId;
                string strAlias = request.HTTPAlias;
                if (!string.IsNullOrEmpty(strAlias))
                {
                    strAlias = strAlias.Trim();
                }

                if (IsHttpAliasValid(strAlias))
                {
                    BrowserTypes browser;
                    Enum.TryParse(request.BrowserType, out browser);
                    PortalAliasInfo portalAlias = new PortalAliasInfo()
                    {
                        PortalID = pid,
                        PortalAliasID = request.PortalAliasID.Value,
                        HTTPAlias = strAlias,
                        Skin = request.Skin,
                        CultureCode = request.CultureCode,
                        BrowserType = browser,
                        IsPrimary = request.IsPrimary
                    };

                    PortalAliasController.Instance.UpdatePortalAlias(portalAlias);
                }
                else
                {
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest,
                            string.Format(Localization.GetString("InvalidAlias", LocalResourcesFile)));
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { Success = true });
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        private bool IsHttpAliasValid(string strAlias)
        {
            bool isValid = true;
            if (string.IsNullOrEmpty(strAlias))
            {
                isValid = false;
            }
            else
            {
                if (strAlias.IndexOf("://", StringComparison.Ordinal) != -1)
                {
                    strAlias = strAlias.Remove(0, strAlias.IndexOf("://", StringComparison.Ordinal) + 3);
                }
                if (strAlias.IndexOf("\\\\", StringComparison.Ordinal) != -1)
                {
                    strAlias = strAlias.Remove(0, strAlias.IndexOf("\\\\", StringComparison.Ordinal) + 2);
                }

                //Validate Alias, this needs to be done with lowercase, downstream we only check with lowercase variables
                if (!PortalAliasController.ValidateAlias(strAlias.ToLowerInvariant(), false))
                {
                    isValid = false;
                }
            }
            return isValid;
        }

        /// POST: api/SiteSettings/DeleteSiteAlias
        /// <summary>
        /// Deletes site alias
        /// </summary>
        /// <param name="portalAliasId"></param>
        /// <returns></returns>
        [HttpPost]
        [DnnAuthorize(StaticRoles = "Superusers")]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage DeleteSiteAlias(int portalAliasId)
        {
            try
            {
                var portalAlias = PortalAliasController.Instance.GetPortalAliasByPortalAliasID(portalAliasId);
                PortalAliasController.Instance.DeletePortalAlias(portalAlias);

                var portalFolder = PortalController.GetPortalFolder(portalAlias.HTTPAlias);
                var serverPath = GetAbsoluteServerPath();

                if (!string.IsNullOrEmpty(portalFolder) && Directory.Exists(serverPath + portalFolder))
                {
                    PortalController.DeletePortalFolder(serverPath, portalFolder);
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { Success = true });
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        private string GetAbsoluteServerPath()
        {
            var httpContext = Request.Properties["MS_HttpContext"] as HttpContextWrapper;
            string strServerPath = string.Empty;
            strServerPath = httpContext.Request.MapPath(httpContext.Request.ApplicationPath);
            if (!strServerPath.EndsWith("\\"))
            {
                strServerPath += "\\";
            }
            return strServerPath;
        }

        /// POST: api/SiteSettings/SetPrimarySiteAlias
        /// <summary>
        /// Sets primary site alias
        /// </summary>
        /// <param name="portalAliasId"></param>
        /// <returns></returns>
        [HttpPost]
        [DnnAuthorize(StaticRoles = "Superusers")]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage SetPrimarySiteAlias([FromUri]int portalAliasId)
        {
            try
            {
                var alias = PortalAliasController.Instance.GetPortalAliasByPortalAliasID(portalAliasId);
                PortalAliasInfo portalAlias = new PortalAliasInfo()
                {
                    PortalID = alias.PortalID,
                    PortalAliasID = portalAliasId,
                    IsPrimary = true
                };

                PortalAliasController.Instance.UpdatePortalAlias(portalAlias);

                return Request.CreateResponse(HttpStatusCode.OK, new { Success = true });
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        /// GET: api/SiteSettings/GetBasicSearchSettings
        /// <summary>
        /// Gets basic search settings
        /// </summary>
        /// <returns>basic search settings</returns>
        [HttpGet]
        [DnnAuthorize(StaticRoles = "Superusers")]
        public HttpResponseMessage GetBasicSearchSettings()
        {
            try
            {
                var searchStatistics = InternalSearchController.Instance.GetSearchStatistics();
                var response = new
                {
                    Success = true,
                    Settings = new
                    {
                        MinWordLength = HostController.Instance.GetInteger("Search_MinKeyWordLength", 3),
                        MaxWordLength = HostController.Instance.GetInteger("Search_MaxKeyWordLength", 255),
                        AllowLeadingWildcard = HostController.Instance.GetString("Search_AllowLeadingWildcard", "N") == "Y",
                        SearchCustomAnalyzer = HostController.Instance.GetString("Search_CustomAnalyzer", string.Empty),
                        TitleBoost = HostController.Instance.GetInteger(SearchTitleBoostSetting, DefaultSearchTitleBoost),
                        TagBoost = HostController.Instance.GetInteger(SearchTagBoostSetting, DefaultSearchTagBoost),
                        ContentBoost = HostController.Instance.GetInteger(SearchContentBoostSetting, DefaultSearchContentBoost),
                        DescriptionBoost = HostController.Instance.GetInteger(SearchDescriptionBoostSetting, DefaultSearchDescriptionBoost),
                        AuthorBoost = HostController.Instance.GetInteger(SearchAuthorBoostSetting, DefaultSearchAuthorBoost),
                        SearchIndexPath = Path.Combine(Globals.ApplicationMapPath, HostController.Instance.GetString("SearchFolder", @"App_Data\Search")),
                        SearchIndexDbSize = ((searchStatistics.IndexDbSize / 1024f) / 1024f).ToString("N") + " MB",
                        SearchIndexLastModifedOn = DateUtils.CalculateDateForDisplay(searchStatistics.LastModifiedOn),
                        SearchIndexTotalActiveDocuments = searchStatistics.TotalActiveDocuments.ToString(CultureInfo.InvariantCulture),
                        SearchIndexTotalDeletedDocuments = searchStatistics.TotalDeletedDocuments.ToString(CultureInfo.InvariantCulture)
                    },
                    SearchCustomAnalyzers = _controller.GetAvailableAnalyzers()
                };
                return Request.CreateResponse(HttpStatusCode.OK, response);
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        /// POST: api/SiteSettings/UpdateBasicSearchSettings
        /// <summary>
        /// Updates basic search settings
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [DnnAuthorize(StaticRoles = "Superusers")]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage UpdateBasicSearchSettings(UpdateBasicSearchSettingsRequest request)
        {
            try
            {
                if (request.MinWordLength == Null.NullInteger || request.MinWordLength == 0)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest,
                            string.Format(Localization.GetString("valIndexWordMinLengthRequired.Error", LocalResourcesFile)));
                }
                else if (request.MaxWordLength == Null.NullInteger || request.MaxWordLength == 0)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest,
                            string.Format(Localization.GetString("valIndexWordMaxLengthRequired.Error", LocalResourcesFile)));
                }
                else if (request.MinWordLength >= request.MaxWordLength)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest,
                            string.Format(Localization.GetString("valIndexWordMaxLengthRequired.Error", LocalResourcesFile)));
                }

                var oldMinLength = HostController.Instance.GetInteger("Search_MinKeyWordLength", 3);
                if (request.MinWordLength != oldMinLength)
                {
                    HostController.Instance.Update("Search_MinKeyWordLength", request.MinWordLength.ToString());
                }

                var oldMaxLength = HostController.Instance.GetInteger("Search_MaxKeyWordLength", 255);
                if (request.MaxWordLength != oldMaxLength)
                {
                    HostController.Instance.Update("Search_MaxKeyWordLength", request.MaxWordLength.ToString());
                }

                HostController.Instance.Update("Search_AllowLeadingWildcard", request.AllowLeadingWildcard ? "Y" : "N");
                HostController.Instance.Update(SearchTitleBoostSetting, (request.TitleBoost == Null.NullInteger) ? DefaultSearchTitleBoost.ToString() : request.TitleBoost.ToString());
                HostController.Instance.Update(SearchTagBoostSetting, (request.TagBoost == Null.NullInteger) ? DefaultSearchTagBoost.ToString() : request.TagBoost.ToString());
                HostController.Instance.Update(SearchContentBoostSetting, (request.ContentBoost == Null.NullInteger) ? DefaultSearchContentBoost.ToString() : request.ContentBoost.ToString());
                HostController.Instance.Update(SearchDescriptionBoostSetting, (request.DescriptionBoost == Null.NullInteger) ? DefaultSearchDescriptionBoost.ToString() : request.DescriptionBoost.ToString());
                HostController.Instance.Update(SearchAuthorBoostSetting, (request.AuthorBoost == Null.NullInteger) ? DefaultSearchAuthorBoost.ToString() : request.AuthorBoost.ToString());

                var oldAnalyzer = HostController.Instance.GetString("Search_CustomAnalyzer", string.Empty);
                var newAnalyzer = request.SearchCustomAnalyzer.Trim();
                if (!oldAnalyzer.Equals(newAnalyzer))
                {
                    HostController.Instance.Update("Search_CustomAnalyzer", newAnalyzer);
                    //force the app restart to use new analyzer.
                    Config.Touch();
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { Success = true });
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        /// POST: api/SiteSettings/CompactSearchIndex
        /// <summary>
        /// Compacts search index
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage CompactSearchIndex()
        {
            try
            {
                SearchHelper.Instance.SetSearchReindexRequestTime(true);
                return Request.CreateResponse(HttpStatusCode.OK, new { Success = true });
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        /// POST: api/SiteSettings/HostSearchReindex
        /// <summary>
        /// Re-index host search
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage HostSearchReindex()
        {
            try
            {
                SearchHelper.Instance.SetSearchReindexRequestTime(-1);
                return Request.CreateResponse(HttpStatusCode.OK, new { Success = true });
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        /// POST: api/SiteSettings/PortalSearchReindex
        /// <summary>
        /// Re-index portal search
        /// </summary>
        /// <param name="portalId"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage PortalSearchReindex([FromUri] int? portalId)
        {
            try
            {
                var pid = portalId ?? PortalId;
                SearchHelper.Instance.SetSearchReindexRequestTime(pid);
                return Request.CreateResponse(HttpStatusCode.OK, new { Success = true });
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        /// GET: api/SiteSettings/GetSynonymsGroups
        /// <summary>
        /// Gets Synonyms Groups
        /// </summary>
        /// <param name="portalId"></param>
        /// <param name="cultureCode"></param>
        /// <returns>Synonyms Groups</returns>
        [HttpGet]
        public HttpResponseMessage GetSynonymsGroups([FromUri]int? portalId, string cultureCode)
        {
            try
            {
                var pid = portalId ?? PortalId;
                var groups = SearchHelper.Instance.GetSynonymsGroups(pid, string.IsNullOrEmpty(cultureCode) ? LocaleController.Instance.GetCurrentLocale(pid).Code : cultureCode);

                var response = new
                {
                    Success = true,
                    SynonymsGroups = groups.Select(g => new
                    {
                        g.PortalId,
                        g.SynonymsGroupId,
                        g.SynonymsTags
                    }),
                    Languages = LocaleController.Instance.GetLocales(pid).Select(l => new
                    {
                        l.Key,
                        Value = l.Key
                    })
                };
                return Request.CreateResponse(HttpStatusCode.OK, response);
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        /// POST: api/SiteSettings/AddSynonymsGroup
        /// <summary>
        /// Adds Synonyms Group
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage AddSynonymsGroup(UpdateSynonymsGroupRequest request)
        {
            try
            {
                var pid = request.PortalId ?? PortalId;
                string cultureCode = string.IsNullOrEmpty(request.CultureCode)
                    ? LocaleController.Instance.GetCurrentLocale(pid).Code
                    : request.CultureCode;
                string duplicateWord;
                var synonymsGroupId = SearchHelper.Instance.AddSynonymsGroup(request.SynonymsTags, pid, cultureCode, out duplicateWord);
                if (synonymsGroupId > 0)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { Success = true, Id = synonymsGroupId });
                }
                else
                {
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "[" + duplicateWord + "] " +
                            string.Format(Localization.GetString("SynonymsTagDuplicated", LocalResourcesFile)));
                }
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        /// POST: api/SiteSettings/UpdateSynonymsGroup
        /// <summary>
        /// Updates Synonyms Group
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage UpdateSynonymsGroup(UpdateSynonymsGroupRequest request)
        {
            try
            {
                var pid = request.PortalId ?? PortalId;
                string cultureCode = string.IsNullOrEmpty(request.CultureCode)
                    ? LocaleController.Instance.GetCurrentLocale(pid).Code
                    : request.CultureCode;
                string duplicateWord;
                var synonymsGroupId = SearchHelper.Instance.UpdateSynonymsGroup(request.SynonymsGroupID.Value, request.SynonymsTags, pid, cultureCode, out duplicateWord);
                if (synonymsGroupId > 0)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { Success = true });
                }
                else
                {
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "[" + duplicateWord + "] " +
                            string.Format(Localization.GetString("SynonymsTagDuplicated", LocalResourcesFile)));
                }
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        /// POST: api/SiteSettings/DeleteSynonymsGroup
        /// <summary>
        /// Deletes Synonyms Group
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage DeleteSynonymsGroup(UpdateSynonymsGroupRequest request)
        {
            try
            {
                var pid = request.PortalId ?? PortalId;
                SearchHelper.Instance.DeleteSynonymsGroup(request.SynonymsGroupID.Value, pid, request.CultureCode);
                return Request.CreateResponse(HttpStatusCode.OK, new { Success = true });
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        /// GET: api/SiteSettings/GetIgnoreWords
        /// <summary>
        /// Gets ignore words
        /// </summary>
        /// <param name="portalId"></param>
        /// <param name="cultureCode"></param>
        /// <returns>ignore words</returns>
        [HttpGet]
        public HttpResponseMessage GetIgnoreWords([FromUri]int? portalId, string cultureCode)
        {
            try
            {
                var pid = portalId ?? PortalId;
                var words = SearchHelper.Instance.GetSearchStopWords(pid, string.IsNullOrEmpty(cultureCode) ? LocaleController.Instance.GetCurrentLocale(pid).Code : cultureCode);

                var response = new
                {
                    Success = true,
                    IgnoreWords = words == null ? null : new
                    {
                        words.PortalId,
                        words.StopWordsId,
                        words.CultureCode,
                        words.StopWords
                    }
                };
                return Request.CreateResponse(HttpStatusCode.OK, response);
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        /// POST: api/SiteSettings/AddIgnoreWords
        /// <summary>
        /// Adds ignore words
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage AddIgnoreWords(UpdateIgnoreWordsRequest request)
        {
            try
            {
                var pid = request.PortalId ?? PortalId;
                string cultureCode = string.IsNullOrEmpty(request.CultureCode)
                    ? LocaleController.Instance.GetCurrentLocale(pid).Code
                    : request.CultureCode;
                var stopWordsId = SearchHelper.Instance.AddSearchStopWords(request.StopWords, pid, cultureCode);
                return Request.CreateResponse(HttpStatusCode.OK, new { Success = true, Id = stopWordsId });
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        /// POST: api/SiteSettings/UpdateIgnoreWords
        /// <summary>
        /// Updates ignore words
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage UpdateIgnoreWords(UpdateIgnoreWordsRequest request)
        {
            try
            {
                var pid = request.PortalId ?? PortalId;
                string cultureCode = string.IsNullOrEmpty(request.CultureCode)
                    ? LocaleController.Instance.GetCurrentLocale(pid).Code
                    : request.CultureCode;
                SearchHelper.Instance.UpdateSearchStopWords(request.StopWordsId, request.StopWords, pid, cultureCode);
                return Request.CreateResponse(HttpStatusCode.OK, new { Success = true });
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        /// POST: api/SiteSettings/DeleteSynonymsGroup
        /// <summary>
        /// Deletes Synonyms Group
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage DeleteIgnoreWords(UpdateIgnoreWordsRequest request)
        {
            try
            {
                var pid = request.PortalId ?? PortalId;
                string cultureCode = string.IsNullOrEmpty(request.CultureCode)
                    ? LocaleController.Instance.GetCurrentLocale(pid).Code
                    : request.CultureCode;
                SearchHelper.Instance.DeleteSearchStopWords(request.StopWordsId, pid, cultureCode);
                return Request.CreateResponse(HttpStatusCode.OK, new { Success = true });
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        /// GET: api/SiteSettings/GetLanguageSettings
        /// <summary>
        /// Gets language settings
        /// </summary>
        /// <param name="portalId"></param>
        /// <returns>language settings</returns>
        [HttpGet]
        public HttpResponseMessage GetLanguageSettings([FromUri] int? portalId)
        {
            try
            {
                var pid = portalId ?? PortalId;
                var portalSettings = new PortalSettings(pid);

                var languageDisplayModes = new List<KeyValuePair<string, string>>();
                languageDisplayModes.Add(new KeyValuePair<string, string>(Localization.GetString("NativeName", LocalResourcesFile), "NATIVE"));
                languageDisplayModes.Add(new KeyValuePair<string, string>(Localization.GetString("EnglishName", LocalResourcesFile), "ENGLISH"));

                return Request.CreateResponse(HttpStatusCode.OK, new
                {
                    Settings = new
                    {
                        portalSettings.ContentLocalizationEnabled,
                        SystemDefaultLanguage = string.IsNullOrEmpty(Localization.SystemLocale) ? Localization.GetString("NeutralCulture", Localization.GlobalResourceFile)
                            : Localization.GetLocaleName(Localization.SystemLocale, GetCultureDropDownType(pid)),
                        SiteDefaultLanguage = portalSettings.DefaultLanguage,
                        LanguageDisplayMode = GetLanguageDisplayMode(pid),
                        portalSettings.EnableUrlLanguage,
                        portalSettings.EnableBrowserLanguage,
                        portalSettings.AllowUserUICulture
                    },
                    Languages = LocaleController.Instance.GetCultures(LocaleController.Instance.GetLocales(Null.NullInteger)).Select(l => new
                    {
                        l.NativeName,
                        l.EnglishName,
                        l.Name
                    }),
                    LanguageDisplayModes = languageDisplayModes
                });
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        /// POST: api/SiteSettings/UpdateLanguageSettings
        /// <summary>
        /// Updates language settings
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage UpdateLanguageSettings(UpdateLanguageSettingsRequest request)
        {
            try
            {
                var pid = request.PortalId ?? PortalId;
                var portalSettings = new PortalSettings(pid);

                PortalController.UpdatePortalSetting(pid, "EnableBrowserLanguage", request.EnableBrowserLanguage.ToString());
                PortalController.UpdatePortalSetting(pid, "AllowUserUICulture", request.AllowUserUICulture.ToString());

                if (!portalSettings.ContentLocalizationEnabled)
                {
                    // first check whether or not portal default language has changed
                    string newDefaultLanguage = request.SiteDefaultLanguage;
                    if (newDefaultLanguage != portalSettings.DefaultLanguage)
                    {
                        var needToRemoveOldDefaultLanguage = LocaleController.Instance.GetLocales(pid).Count == 1;
                        var oldDefaultLanguage = LocaleController.Instance.GetLocale(portalSettings.DefaultLanguage);
                        if (!IsLanguageEnabled(pid, newDefaultLanguage))
                        {
                            var language = LocaleController.Instance.GetLocale(newDefaultLanguage);
                            Localization.AddLanguageToPortal(pid, language.LanguageId, true);
                        }

                        // update portal default language
                        var portal = PortalController.Instance.GetPortal(PortalId);
                        portal.DefaultLanguage = newDefaultLanguage;
                        PortalController.Instance.UpdatePortalInfo(portal);

                        if (needToRemoveOldDefaultLanguage)
                        {
                            Localization.RemoveLanguageFromPortal(PortalId, oldDefaultLanguage.LanguageId);
                        }
                    }

                    PortalController.UpdatePortalSetting(pid, "EnableUrlLanguage", request.EnableUrlLanguage.ToString());
                }

                var oldLanguageDisplayMode = Convert.ToString(Personalization.GetProfile("LanguageDisplayMode", "ViewType" + pid));
                if (request.LanguageDisplayMode != oldLanguageDisplayMode)
                {
                    var personalizationController = new PersonalizationController();
                    var personalization = personalizationController.LoadProfile(UserInfo.UserID, pid);
                    Personalization.SetProfile(personalization, "LanguageDisplayMode", "ViewType" + pid, request.LanguageDisplayMode);
                    personalizationController.SaveProfile(personalization);
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { Success = true });
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        /// GET: api/SiteSettings/GetLanguages
        /// <summary>
        /// Gets languages
        /// </summary>
        /// <param name="portalId"></param>
        /// <returns>languages</returns>
        [HttpGet]
        public HttpResponseMessage GetLanguages([FromUri] int? portalId)
        {
            try
            {
                var pid = portalId ?? PortalId;
                var portalSettings = new PortalSettings(pid);

                return Request.CreateResponse(HttpStatusCode.OK, new
                {
                    Languages = LocaleController.Instance.GetLocales(pid).Values.Select(l => new
                    {
                        l.LanguageId,
                        Icon = string.IsNullOrEmpty(l.Code) ? "/images/Flags/none.gif" : string.Format("/images/Flags/{0}.gif", l.Code),
                        l.Code,
                        Name = Localization.GetLocaleName(l.Code, GetCultureDropDownType(pid)),
                        Enabled = IsLanguageEnabled(pid, l.Code),
                        IsDefault = l.Code == portalSettings.DefaultLanguage
                    })
                });
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        /// GET: api/SiteSettings/GetLanguage
        /// <summary>
        /// Gets language
        /// </summary>
        /// <param name="portalId"></param>
        /// <param name="languageId"></param>
        /// <returns>language</returns>
        [HttpGet]
        public HttpResponseMessage GetLanguage([FromUri] int? portalId, [FromUri] int? languageId)
        {
            try
            {
                var pid = portalId ?? PortalId;
                var lid = languageId ?? Null.NullInteger;
                var portalSettings = new PortalSettings(pid);
                var language = lid != Null.NullInteger ? LocaleController.Instance.GetLocale(lid) : null;

                var fallbacks = language != null ? LocaleController.Instance.GetCultures(LocaleController.Instance.GetLocales(Null.NullInteger))
                    .Where(l => l.Name != language.Code)
                    .Select(l => new
                    {
                        l.NativeName,
                        l.EnglishName,
                        l.Name,
                        Icon =
                            string.IsNullOrEmpty(l.Name)
                                ? "/images/Flags/none.gif"
                                : string.Format("/images/Flags/{0}.gif", l.Name)
                    }).ToList() : LocaleController.Instance.GetCultures(LocaleController.Instance.GetLocales(Null.NullInteger))
                    .Select(l => new
                    {
                        l.NativeName,
                        l.EnglishName,
                        l.Name,
                        Icon =
                            string.IsNullOrEmpty(l.Name)
                                ? "/images/Flags/none.gif"
                                : string.Format("/images/Flags/{0}.gif", l.Name)
                    }).ToList();

                fallbacks.Insert(0, new
                {
                    NativeName = Localization.GetString("System_Default", LocalResourcesFile),
                    EnglishName = Localization.GetString("System_Default", LocalResourcesFile),
                    Name = "",
                    Icon = "/images/Flags/none.gif"
                });

                return Request.CreateResponse(HttpStatusCode.OK, new
                {
                    Language = language != null ? new
                    {
                        language.NativeName,
                        language.EnglishName,
                        language.Code,
                        language.Fallback,
                        Enabled = IsLanguageEnabled(pid, language.Code),
                        IsDefault = language.Code == portalSettings.DefaultLanguage
                    } : new
                    {
                        NativeName = "",
                        EnglishName = "",
                        Code ="",
                        Fallback = "",
                        Enabled = false,
                        IsDefault = false
                    },
                    SupportedFallbacks = fallbacks
                });
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        /// GET: api/SiteSettings/GetAllLanguages
        /// <summary>
        /// Gets language
        /// </summary>
        /// <param name="portalId"></param>
        /// <returns>all languages</returns>
        [HttpGet]
        public HttpResponseMessage GetAllLanguages([FromUri] int? portalId)
        {
            try
            {
                var pid = portalId ?? PortalId;
                var supportedLanguages = LocaleController.Instance.GetCultures(LocaleController.Instance.GetLocales(Null.NullInteger));
                var cultures = new List<CultureInfo>(CultureInfo.GetCultures(CultureTypes.SpecificCultures));

                foreach (CultureInfo info in supportedLanguages)
                {
                    string cultureCode = info.Name;
                    CultureInfo culture = cultures.Where(c => c.Name == cultureCode).SingleOrDefault();
                    if (culture != null)
                    {
                        cultures.Remove(culture);
                    }
                }

                return Request.CreateResponse(HttpStatusCode.OK, new
                {
                    FullLanguageList = cultures.Select(c => new
                    {
                        c.NativeName,
                        c.EnglishName,
                        c.Name
                    })
                });
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        private bool IsLanguageEnabled(int portalId, string code)
        {
            Locale enabledLanguage;
            return LocaleController.Instance.GetLocales(portalId).TryGetValue(code, out enabledLanguage);
        }

        private CultureDropDownTypes GetCultureDropDownType(int portalId)
        {
            CultureDropDownTypes displayType;
            string viewTypePersonalizationKey = "ViewType" + portalId;
            string viewType = Convert.ToString(Personalization.GetProfile("LanguageDisplayMode", viewTypePersonalizationKey));
            switch (viewType)
            {
                case "NATIVE":
                    displayType = CultureDropDownTypes.NativeName;
                    break;
                case "ENGLISH":
                    displayType = CultureDropDownTypes.EnglishName;
                    break;
                default:
                    displayType = CultureDropDownTypes.DisplayName;
                    break;
            }
            return displayType;
        }

        private string GetLanguageDisplayMode(int portalId)
        {
            string viewTypePersonalizationKey = "ViewType" + portalId;
            string viewType = Convert.ToString(Personalization.GetProfile("LanguageDisplayMode", viewTypePersonalizationKey));
            return string.IsNullOrEmpty(viewType) ? "NATIVE" : viewType;
        }
    }
}
