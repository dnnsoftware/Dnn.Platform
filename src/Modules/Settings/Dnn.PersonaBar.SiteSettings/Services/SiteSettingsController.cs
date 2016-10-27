#region Copyright
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2016
// by DotNetNuke Corporation
// All Rights Reserved
#endregion

using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Web.Http;
using Dnn.PersonaBar.Library;
using Dnn.PersonaBar.Library.Attributes;
using Dnn.PersonaBar.SiteSettings.Services.Dto;
using DotNetNuke.Common.Lists;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Icons;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Profile;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Entities.Urls;
using DotNetNuke.Entities.Users;
using DotNetNuke.Instrumentation;
using DotNetNuke.Services.Localization;
using DotNetNuke.UI.Internals;
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
                        return Request.CreateResponse(HttpStatusCode.OK, new {Success = true});
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
                        return Request.CreateResponse(HttpStatusCode.OK, new {Success = true});
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

        #region Private Methods

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

        #endregion
    }
}
