﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace Dnn.PersonaBar.Security.Services
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Dynamic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Web;
    using System.Web.Http;
    using System.Xml;

    using Dnn.PersonaBar.Extensions.Components.Security.Ssl;
    using Dnn.PersonaBar.Extensions.Services.Dto;
    using Dnn.PersonaBar.Library;
    using Dnn.PersonaBar.Library.Attributes;
    using Dnn.PersonaBar.Pages.Components;
    using Dnn.PersonaBar.Security.Helper;
    using Dnn.PersonaBar.Security.Services.Dto;

    using DotNetNuke.Abstractions.Portals;
    using DotNetNuke.Application;
    using DotNetNuke.Collections;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Common.Utils;
    using DotNetNuke.Entities.Controllers;
    using DotNetNuke.Entities.Host;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Security;
    using DotNetNuke.Security.Membership;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.Web.Api;
    using DotNetNuke.Web.Api.Auth.ApiTokens;
    using DotNetNuke.Web.Api.Auth.ApiTokens.Models;
    using Constants = Dnn.PersonaBar.Library.Constants;
    using Localization = DotNetNuke.Services.Localization.Localization;

    [MenuPermission(MenuName = Components.Constants.MenuName)]
    public class SecurityController : PersonaBarApiController
    {
        private const string BULLETINXMLNODEPATH = "//channel/item";
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(SecurityController));
        private readonly Components.SecurityController controller;
        private readonly IPagesController pagesController;
        private readonly IPortalAliasService portalAliasService;

        /// <summary>Initializes a new instance of the <see cref="SecurityController"/> class.</summary>
        /// <param name="pagesController">The pages controller.</param>
        /// <param name="portalAliasService">The portal alias service.</param>
        public SecurityController(IPagesController pagesController, IPortalAliasService portalAliasService)
            : this(
                new Components.SecurityController(),
                pagesController,
                portalAliasService)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="SecurityController"/> class.</summary>
        /// <param name="controller">The security controller.</param>
        /// <param name="pagesController">The pages controller.</param>
        /// <param name="portalAliasService">The portal alias service.</param>
        internal SecurityController(
            Components.SecurityController controller,
            IPagesController pagesController,
            IPortalAliasService portalAliasService)
        {
            this.pagesController = pagesController;
            this.controller = controller;
            this.portalAliasService = portalAliasService;
        }

        /// GET: api/Security/GetBasicLoginSettings
        /// <summary>Gets portal's basic login settings.</summary>
        /// <param name="cultureCode">The culture code or <see langword="null"/> or empty to use the current locale.</param>
        /// <returns>Portal's basic login settings.</returns>
        [HttpGet]
        [AdvancedPermission(MenuName = Components.Constants.MenuName, Permission = Components.Constants.BasicLoginSettingsView)]

        public HttpResponseMessage GetBasicLoginSettings(string cultureCode)
        {
            try
            {
                cultureCode = string.IsNullOrEmpty(cultureCode)
                    ? LocaleController.Instance.GetCurrentLocale(this.PortalId).Code
                    : cultureCode;

                var portal = PortalController.Instance.GetPortal(this.PortalId, cultureCode);
                var portalSettings = new PortalSettings(portal);

                dynamic settings = new ExpandoObject();
                settings.DefaultAuthProvider = PortalController.GetPortalSetting("DefaultAuthProvider", this.PortalId, "DNN");
                settings.PrimaryAdministratorId = PortalSettings.Current.AdministratorId;
                settings.RequireValidProfileAtLogin = PortalController.GetPortalSettingAsBoolean("Security_RequireValidProfileAtLogin", this.PortalId, true);
                settings.CaptchaLogin = PortalController.GetPortalSettingAsBoolean("Security_CaptchaLogin", this.PortalId, false);
                settings.CaptchaRetrivePassword = PortalController.GetPortalSettingAsBoolean("Security_CaptchaRetrivePassword", this.PortalId, false);
                settings.CaptchaChangePassword = PortalController.GetPortalSettingAsBoolean("Security_CaptchaChangePassword", this.PortalId, false);
                settings.HideLoginControl = this.PortalSettings.HideLoginControl;
                settings.cultureCode = cultureCode;

                var authProviders = this.controller.GetAuthenticationProviders().Select(v => new
                {
                    Name = v,
                    Value = v,
                }).ToList();

                var adminUsers = this.controller.GetAdminUsers(this.PortalId).Select(v => new
                {
                    v.UserID,
                    v.FullName,
                }).ToList();

                var response = new
                {
                    Success = true,
                    Results = new
                    {
                        Settings = settings,
                        AuthProviders = authProviders,
                        Administrators = adminUsers,
                    },
                };

                return this.Request.CreateResponse(HttpStatusCode.OK, response);
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        /// POST: api/Security/UpdateBasicLoginSettings
        /// <summary>Updates an existing log settings.</summary>
        /// <param name="request">The update request.</param>
        /// <returns>A response indicating success.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AdvancedPermission(MenuName = Components.Constants.MenuName, Permission = Components.Constants.BasicLoginSettingsView + "&" + Components.Constants.BasicLoginSettingsEdit)]
        public HttpResponseMessage UpdateBasicLoginSettings(UpdateBasicLoginSettingsRequest request)
        {
            if (!this.ModelState.IsValid)
            {
                return this.Request.CreateErrorResponse(HttpStatusCode.BadRequest, this.ModelState);
            }

            try
            {
                var cultureCode = string.IsNullOrEmpty(request.CultureCode)
                    ? LocaleController.Instance.GetCurrentLocale(this.PortalId).Code
                    : request.CultureCode;

                var portalInfo = PortalController.Instance.GetPortal(this.PortalId);
                portalInfo.AdministratorId = Convert.ToInt32(request.PrimaryAdministratorId);
                PortalController.Instance.UpdatePortalInfo(portalInfo);

                PortalController.UpdatePortalSetting(this.PortalId, "DefaultAuthProvider", request.DefaultAuthProvider);
                PortalController.UpdatePortalSetting(this.PortalId, "Security_RequireValidProfile", request.RequireValidProfileAtLogin.ToString(), false);
                PortalController.UpdatePortalSetting(this.PortalId, "Security_CaptchaLogin", request.CaptchaLogin.ToString(), false);
                PortalController.UpdatePortalSetting(this.PortalId, "Security_CaptchaRetrivePassword", request.CaptchaRetrivePassword.ToString(), false);
                PortalController.UpdatePortalSetting(this.PortalId, "Security_CaptchaChangePassword", request.CaptchaChangePassword.ToString(), false);
                PortalController.UpdatePortalSetting(this.PortalId, "HideLoginControl", request.HideLoginControl.ToString(), false);

                return this.Request.CreateResponse(HttpStatusCode.OK, new { Success = true });
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        /// GET: api/Security/GetIpFilters
        /// <summary>Gets list of IP filters.</summary>
        /// <returns>List of IP filters.</returns>
        [HttpGet]
        [RequireHost]
        public HttpResponseMessage GetIpFilters()
        {
            try
            {
                var filters = IPFilterController.Instance.GetIPFilters().Select(v => new
                {
                    v.IPFilterID,
                    IPFilter = NetworkUtils.FormatAsCidr(v.IPAddress, v.SubnetMask),
                    v.RuleType,
                    v.Notes,
                }).ToList();
                var response = new
                {
                    Success = true,
                    Results = new
                    {
                        Filters = filters,
                        Host.EnableIPChecking,
                    },
                };

                return this.Request.CreateResponse(HttpStatusCode.OK, response);
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        /// GET: api/Security/GetIpFilter
        /// <summary>Gets an IP filter.</summary>
        /// <param name="filterId">The ID of the IP filter to get.</param>
        /// <returns>IP filter.</returns>
        [HttpGet]
        [RequireHost]
        public HttpResponseMessage GetIpFilter(int filterId)
        {
            try
            {
                IPFilterInfo filter = IPFilterController.Instance.GetIPFilter(filterId);
                var response = new
                {
                    Success = true,
                    Results = new
                    {
                        filter.IPAddress,
                        filter.IPFilterID,
                        filter.RuleType,
                        filter.SubnetMask,
                        filter.Notes,
                    },
                };

                return this.Request.CreateResponse(HttpStatusCode.OK, response);
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        /// POST: api/Security/UpdateIpFilter
        /// <summary>Updates an IP filter.</summary>
        /// <param name="request"><see cref="UpdateIpFilterRequest"/>.</param>
        /// <returns>Ok or BadRequest or InternalServerError.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequireHost]
        public HttpResponseMessage UpdateIpFilter(UpdateIpFilterRequest request)
        {
            try
            {
                var ipf = new IPFilterInfo();
                ipf.IPAddress = request.IPAddress;
                ipf.SubnetMask = request.SubnetMask;
                ipf.RuleType = request.RuleType;
                ipf.Notes = request.Notes;

                if ((ipf.IPAddress == "127.0.0.1" || ipf.IPAddress == "localhost" || ipf.IPAddress == "::1" || ipf.IPAddress == "*") && ipf.RuleType == 2)
                {
                    return this.Request.CreateErrorResponse(HttpStatusCode.BadRequest, string.Format(Localization.GetString("CannotDeleteLocalhost.Text", Components.Constants.LocalResourcesFile)));
                }

                if (IPFilterController.Instance.IsAllowableDeny(HttpContext.Current.Request.UserHostAddress, ipf) == false)
                {
                    return this.Request.CreateErrorResponse(HttpStatusCode.BadRequest, string.Format(Localization.GetString("CannotDeleteIPInUse.Text", Components.Constants.LocalResourcesFile)));
                }

                if (request.IPFilterID > 0)
                {
                    ipf.IPFilterID = request.IPFilterID;
                    IPFilterController.Instance.UpdateIPFilter(ipf);
                }
                else
                {
                    IPFilterController.Instance.AddIPFilter(ipf);
                }

                return this.Request.CreateResponse(HttpStatusCode.OK, new { Success = true });
            }
            catch (ArgumentException exc)
            {
                Logger.Info(exc);
                return this.Request.CreateErrorResponse(HttpStatusCode.BadRequest, exc.Message);
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        /// POST: api/Security/DeleteIpFilter
        /// <summary>Deletes an IP filter.</summary>
        /// <param name="filterId">The filter ID.</param>
        /// <returns>A response indicating success.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequireHost]
        public HttpResponseMessage DeleteIpFilter(int filterId)
        {
            try
            {
                IList<IPFilterInfo> currentRules = IPFilterController.Instance.GetIPFilters();
                List<IPFilterInfo> currentWithDeleteRemoved = (from p in currentRules where p.IPFilterID != filterId select p).ToList();

                if (IPFilterController.Instance.CanIPStillAccess(HttpContext.Current.Request.UserHostAddress, currentWithDeleteRemoved) == false)
                {
                    return this.Request.CreateErrorResponse(HttpStatusCode.BadRequest, string.Format(Localization.GetString("CannotDelete.Text", Components.Constants.LocalResourcesFile)));
                }
                else
                {
                    var ipf = new IPFilterInfo();
                    ipf.IPFilterID = filterId;
                    IPFilterController.Instance.DeleteIPFilter(ipf);
                    return this.Request.CreateResponse(HttpStatusCode.OK, new { Success = true });
                }
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        /// GET: api/Security/GetMemberSettings
        /// <summary>Gets portal's member settings.</summary>
        /// <returns>Portal's member settings.</returns>
        [HttpGet]
        [RequireHost]
        public HttpResponseMessage GetMemberSettings()
        {
            try
            {
                var response = new
                {
                    Success = true,
                    Results = new
                    {
                        Settings = new
                        {
                            Host.MembershipResetLinkValidity,
                            Host.AdminMembershipResetLinkValidity,
                            Host.EnablePasswordHistory,
                            Host.MembershipNumberPasswords,
                            Host.MembershipDaysBeforePasswordReuse,
                            Host.EnableBannedList,
                            Host.EnableStrengthMeter,
                            Host.EnableIPChecking,
                            Host.PasswordExpiry,
                            Host.PasswordExpiryReminder,
                            ForceLogoutAfterPasswordChanged = HostController.Instance.GetBoolean("ForceLogoutAfterPasswordChanged"),
                        },
                    },
                };

                return this.Request.CreateResponse(HttpStatusCode.OK, response);
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        /// POST: api/Security/UpdateMemberSettings
        /// <summary>Updates member settings.</summary>
        /// <param name="request">The update request.</param>
        /// <returns>A response indicating success.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequireHost]
        public HttpResponseMessage UpdateMemberSettings(UpdateMemberSettingsRequest request)
        {
            try
            {
                HostController.Instance.Update("EnableBannedList", request.EnableBannedList ? "Y" : "N", false);
                HostController.Instance.Update("EnableStrengthMeter", request.EnableStrengthMeter ? "Y" : "N", false);
                HostController.Instance.Update("EnableIPChecking", request.EnableIPChecking ? "Y" : "N", false);
                HostController.Instance.Update("EnablePasswordHistory", request.EnablePasswordHistory ? "Y" : "N", false);
                HostController.Instance.Update("MembershipResetLinkValidity", request.MembershipResetLinkValidity.ToString(), false);
                HostController.Instance.Update("AdminMembershipResetLinkValidity", request.AdminMembershipResetLinkValidity.ToString(), false);
                HostController.Instance.Update("MembershipNumberPasswords", request.MembershipNumberPasswords.ToString(), false);
                HostController.Instance.Update("MembershipDaysBeforePasswordReuse", request.MembershipDaysBeforePasswordReuse.ToString(), false);
                HostController.Instance.Update("PasswordExpiry", request.PasswordExpiry.ToString());
                HostController.Instance.Update("PasswordExpiryReminder", request.PasswordExpiryReminder.ToString());
                HostController.Instance.Update("ForceLogoutAfterPasswordChanged", request.ForceLogoutAfterPasswordChanged ? "Y" : "N", false);

                return this.Request.CreateResponse(HttpStatusCode.OK, new { Success = true });
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        /// GET: api/Security/GetRegistrationSettings
        /// <summary>Gets portal's registration settings.</summary>
        /// <returns>Portal's registration settings.</returns>
        [HttpGet]
        [AdvancedPermission(MenuName = Components.Constants.MenuName, Permission = Components.Constants.RegistrationSettingsView)]
        public HttpResponseMessage GetRegistrationSettings()
        {
            try
            {
                List<KeyValuePair<string, int>> userRegistrationOptions = RegistrationSettingsHelper.GetUserRegistrationOptions();
                List<KeyValuePair<string, int>> registrationFormTypeOptions = RegistrationSettingsHelper.GetRegistrationFormOptions();

                var activeLanguage = LocaleController.Instance.GetDefaultLocale(this.PortalId).Code;
                var portal = PortalController.Instance.GetPortal(this.PortalId, activeLanguage);

                var response = new
                {
                    Success = true,
                    Results = new
                    {
                        Settings = new
                        {
                            portal.UserRegistration,
                            EnableRegisterNotification = PortalController.GetPortalSettingAsBoolean("EnableRegisterNotification", this.PortalId, true),
                            UseAuthenticationProviders = PortalController.GetPortalSettingAsBoolean("Registration_UseAuthProviders", this.PortalId, false),
                            ExcludedTerms = PortalController.GetPortalSetting("Registration_ExcludeTerms", this.PortalId, string.Empty),
                            UseProfanityFilter = PortalController.GetPortalSettingAsBoolean("Registration_UseProfanityFilter", this.PortalId, false),
                            this.PortalSettings.Registration.RegistrationFormType,
                            this.PortalSettings.Registration.RegistrationFields,
                            UseEmailAsUsername = PortalController.GetPortalSettingAsBoolean("Registration_UseEmailAsUserName", this.PortalId, false),
                            RequireUniqueDisplayName = PortalController.GetPortalSettingAsBoolean("Registration_RequireUniqueDisplayName", this.PortalId, false),
                            DisplayNameFormat = PortalController.GetPortalSetting("Security_DisplayNameFormat", this.PortalId, string.Empty),
                            UserNameMinLength = PortalController.GetPortalSettingAsInteger("Security_UserNameMinLength", this.PortalId, Globals.glbUserNameMinLength),
                            UserNameValidation = PortalController.GetPortalSetting("Security_UserNameValidation", this.PortalId, Globals.glbUserNameRegEx),
                            EmailAddressValidation = PortalController.GetPortalSetting("Security_EmailValidation", this.PortalId, Globals.glbEmailRegEx),
                            UseRandomPassword = PortalController.GetPortalSettingAsBoolean("Registration_RandomPassword", this.PortalId, false),
                            RequirePasswordConfirmation = PortalController.GetPortalSettingAsBoolean("Registration_RequireConfirmPassword", this.PortalId, true),
                            RequireValidProfile = PortalController.GetPortalSettingAsBoolean("Security_RequireValidProfile", this.PortalId, false),
                            UseCaptchaRegister = PortalController.GetPortalSettingAsBoolean("Security_CaptchaRegister", this.PortalId, false),
                            RequiresUniqueEmail = MembershipProviderConfig.RequiresUniqueEmail.ToString(CultureInfo.InvariantCulture),
                            PasswordFormat = MembershipProviderConfig.PasswordFormat.ToString(),
                            PasswordRetrievalEnabled = MembershipProviderConfig.PasswordRetrievalEnabled.ToString(CultureInfo.InvariantCulture),
                            PasswordResetEnabled = MembershipProviderConfig.PasswordResetEnabled.ToString(CultureInfo.InvariantCulture),
                            MinPasswordLength = MembershipProviderConfig.MinPasswordLength.ToString(CultureInfo.InvariantCulture),
                            MinNonAlphanumericCharacters = MembershipProviderConfig.MinNonAlphanumericCharacters.ToString(CultureInfo.InvariantCulture),
                            RequiresQuestionAndAnswer = MembershipProviderConfig.RequiresQuestionAndAnswer.ToString(CultureInfo.InvariantCulture),
                            MembershipProviderConfig.PasswordStrengthRegularExpression,
                            MaxInvalidPasswordAttempts = MembershipProviderConfig.MaxInvalidPasswordAttempts.ToString(CultureInfo.InvariantCulture),
                            PasswordAttemptWindow = MembershipProviderConfig.PasswordAttemptWindow.ToString(CultureInfo.InvariantCulture),
                        },
                        UserRegistrationOptions = userRegistrationOptions,
                        RegistrationFormTypeOptions = registrationFormTypeOptions,
                    },
                };

                return this.Request.CreateResponse(HttpStatusCode.OK, response);
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        /// GET: api/Security/GetSslSettings
        /// <summary>Gets portal's SSL settings.</summary>
        /// <returns>Portal's SSL settings.</returns>
        [HttpGet]
        [DnnAuthorize(StaticRoles = Constants.AdminsRoleName)]
        public HttpResponseMessage GetSslSettings()
        {
            try
            {
                dynamic settings = new ExpandoObject();
                settings.SSLSetup = PortalController.GetPortalSettingAsInteger("SSLSetup", this.PortalId, 0);
                settings.SSLEnforced = PortalController.GetPortalSettingAsBoolean("SSLEnforced", this.PortalId, false);
                settings.SSLURL = PortalController.GetPortalSetting("SSLURL", this.PortalId, Null.NullString);
                settings.STDURL = PortalController.GetPortalSetting("STDURL", this.PortalId, Null.NullString);

                var portalStats = SslController.Instance.GetPortalStats(this.PortalId);
                settings.NumberOfSecureTabs = portalStats.NumberOfSecureTabs;
                settings.NumberOfNonSecureTabs = portalStats.NumberOfNonSecureTabs;

                if (this.UserInfo.IsSuperUser)
                {
                    settings.SSLOffloadHeader = HostController.Instance.GetString("SSLOffloadHeader", string.Empty);
                }

                var response = new
                {
                    Success = true,
                    Results = new
                    {
                        Settings = settings,
                    },
                };

                return this.Request.CreateResponse(HttpStatusCode.OK, response);
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        /// POST: api/Security/UpdateRegistrationSettings
        /// <summary>Updates registration settings.</summary>
        /// <param name="request">The update request.</param>
        /// <returns>A response indicating success.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AdvancedPermission(MenuName = Components.Constants.MenuName, Permission = Components.Constants.RegistrationSettingsView + "&" + Components.Constants.RegistrationSettingsEdit)]
        public HttpResponseMessage UpdateRegistrationSettings(UpdateRegistrationSettingsRequest request)
        {
            if (!this.ModelState.IsValid)
            {
                return this.Request.CreateErrorResponse(HttpStatusCode.BadRequest, this.ModelState);
            }

            try
            {
                var setting = request.RegistrationFields;
                PortalController.UpdatePortalSetting(this.PortalId, "Registration_RegistrationFields", setting);
                PortalController.UpdatePortalSetting(this.PortalId, "Registration_RegistrationFormType", request.RegistrationFormType.ToString(), false);
                PortalController.UpdatePortalSetting(this.PortalId, "Registration_UseEmailAsUserName", request.UseEmailAsUsername.ToString(), false);

                var portalInfo = PortalController.Instance.GetPortal(this.PortalId);
                portalInfo.UserRegistration = Convert.ToInt32(request.UserRegistration);
                PortalController.Instance.UpdatePortalInfo(portalInfo);

                PortalController.UpdatePortalSetting(this.PortalId, "EnableRegisterNotification", request.EnableRegisterNotification.ToString(), false);
                PortalController.UpdatePortalSetting(this.PortalId, "Registration_UseAuthProviders", request.UseAuthenticationProviders.ToString(), false);
                PortalController.UpdatePortalSetting(this.PortalId, "Registration_ExcludeTerms", request.ExcludedTerms, false);
                PortalController.UpdatePortalSetting(this.PortalId, "Registration_UseProfanityFilter", request.UseProfanityFilter.ToString(), false);
                PortalController.UpdatePortalSetting(this.PortalId, "Registration_RequireUniqueDisplayName", request.RequireUniqueDisplayName.ToString(), false);
                PortalController.UpdatePortalSetting(this.PortalId, "Security_DisplayNameFormat", request.DisplayNameFormat, false);
                PortalController.UpdatePortalSetting(this.PortalId, "Security_UserNameMinLength", request.UserNameMinLength, false);
                PortalController.UpdatePortalSetting(this.PortalId, "Security_UserNameValidation", request.UserNameValidation, false);
                PortalController.UpdatePortalSetting(this.PortalId, "Security_EmailValidation", request.EmailAddressValidation, false);
                PortalController.UpdatePortalSetting(this.PortalId, "Registration_RandomPassword", request.UseRandomPassword.ToString(), false);
                PortalController.UpdatePortalSetting(this.PortalId, "Registration_RequireConfirmPassword", request.RequirePasswordConfirmation.ToString(), true);
                PortalController.UpdatePortalSetting(this.PortalId, "Security_RequireValidProfile", request.RequireValidProfile.ToString(), false);
                PortalController.UpdatePortalSetting(this.PortalId, "Security_CaptchaRegister", request.UseCaptchaRegister.ToString(), false);

                return this.Request.CreateResponse(HttpStatusCode.OK, new { Success = true });
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        /// POST: api/Security/UpdateSslSettings
        /// <summary>Updates SSL settings.</summary>
        /// <param name="request">The update request.</param>
        /// <returns>A response indicating success.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [DnnAuthorize(StaticRoles = Constants.AdminsRoleName)]
        public HttpResponseMessage UpdateSslSettings(UpdateSslSettingsRequest request)
        {
            try
            {
                switch (request.SSLSetup)
                {
                    case 0:
                        request.SSLEnforced = false;
                        request.SSLURL = string.Empty;
                        request.STDURL = string.Empty;
                        break;
                    case 1:
                        request.SSLEnforced = false;
                        request.SSLURL = string.Empty;
                        request.STDURL = string.Empty;
                        request.SSLOffloadHeader = string.Empty;
                        break;
                }

                PortalController.UpdatePortalSetting(this.PortalId, "SSLSetup", request.SSLSetup.ToString(), false);
                PortalController.UpdatePortalSetting(this.PortalId, "SSLEnforced", request.SSLEnforced.ToString(), false);
                PortalController.UpdatePortalSetting(this.PortalId, "SSLURL", this.AddPortalAlias(request.SSLURL, this.PortalId), false);
                PortalController.UpdatePortalSetting(this.PortalId, "STDURL", this.AddPortalAlias(request.STDURL, this.PortalId), false);

                if (this.UserInfo.IsSuperUser)
                {
                    HostController.Instance.Update("SSLOffloadHeader", request.SSLOffloadHeader);
                }

                DataCache.ClearPortalCache(this.PortalId, false);

                return this.Request.CreateResponse(HttpStatusCode.OK, new { Success = true });
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        /// POST: api/Security/SetAllPagesSecure
        /// <summary>Sets all pages in the portal to be secure.</summary>
        /// <returns>A response indicating success.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [DnnAuthorize(StaticRoles = Constants.AdminsRoleName)]
        public HttpResponseMessage SetAllPagesSecure()
        {
            try
            {
                DotNetNuke.Data.DataProvider.Instance().SetAllPortalTabsSecure(this.PortalId, true);
                return this.Request.CreateResponse(HttpStatusCode.OK, new { Success = true });
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        /// GET: api/Security/GetSecurityBulletins
        /// <summary>Gets security bulletins.</summary>
        /// <returns>Security bulletins.</returns>
        [HttpGet]
        [RequireHost]
        public HttpResponseMessage GetSecurityBulletins()
        {
            try
            {
                var plartformVersion = System.Reflection.Assembly.LoadFrom(Globals.ApplicationMapPath + @"\bin\DotNetNuke.dll").GetName().Version;
                string sRequest = string.Format(
                    "https://dnnplatform.io/security.aspx?type={0}&name={1}&version={2}",
                    DotNetNukeContext.Current.Application.Type,
                    "DNNCORP.CE",
                    Globals.FormatVersion(plartformVersion, "00", 3, string.Empty));

                // format for display with "." delimiter
                string sVersion = Globals.FormatVersion(plartformVersion, "00", 3, ".");

                // make remote request
                Stream oStream = null;
                try
                {
                    HttpWebRequest oRequest = Globals.GetExternalRequest(sRequest);
                    oRequest.Timeout = 10000; // 10 seconds
                    WebResponse oResponse = oRequest.GetResponse();
                    oStream = oResponse.GetResponseStream();
                }
                catch (Exception oExc)
                {
                    // connectivity issues
                    if (PortalSecurity.IsInRoles(this.PortalSettings.AdministratorRoleId.ToString()))
                    {
                        return this.Request.CreateErrorResponse(HttpStatusCode.BadRequest, string.Format(Localization.GetString("RequestFailed_Admin.Text", Components.Constants.LocalResourcesFile), sRequest));
                    }
                    else
                    {
                        return this.Request.CreateErrorResponse(HttpStatusCode.BadRequest, Localization.GetString("RequestFailed_User.Text", Components.Constants.LocalResourcesFile) + oExc.Message);
                    }
                }

                // load XML document
                StreamReader oReader = new StreamReader(oStream);
                XmlDocument oDoc = new XmlDocument { XmlResolver = null };
                oDoc.LoadXml(oReader.ReadToEnd());

                List<object> items = new List<object>();
                foreach (XmlNode selectNode in oDoc.SelectNodes(BULLETINXMLNODEPATH))
                {
                    items.Add(new
                    {
                        Title = selectNode.SelectSingleNode("title") != null ? selectNode.SelectSingleNode("title").InnerText : string.Empty,
                        Link = selectNode.SelectSingleNode("link") != null ? selectNode.SelectSingleNode("link").InnerText : string.Empty,
                        Description = selectNode.SelectSingleNode("description") != null ? selectNode.SelectSingleNode("description").InnerText : string.Empty,
                        Author = selectNode.SelectSingleNode("author") != null ? selectNode.SelectSingleNode("author").InnerText : string.Empty,
                        PubDate = selectNode.SelectSingleNode("pubDate") != null ? selectNode.SelectSingleNode("pubDate").InnerText.Split(' ')[0] : string.Empty,
                    });
                }

                var response = new
                {
                    Success = true,
                    Results = new
                    {
                        PlatformVersion = sVersion,
                        SecurityBulletins = items,
                    },
                };

                return this.Request.CreateResponse(HttpStatusCode.OK, response);
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        /// GET: api/Security/GetOtherSettings
        /// <summary>Gets host other settings.</summary>
        /// <returns>Portal's other settings.</returns>
        [HttpGet]
        [RequireHost]
        public HttpResponseMessage GetOtherSettings()
        {
            try
            {
                var response = new
                {
                    Success = true,
                    Results = new
                    {
                        Settings = new
                        {
                            Host.ShowCriticalErrors,
                            Host.DebugMode,
                            Host.RememberCheckbox,
                            Host.AutoAccountUnlockDuration,
                            Host.AsyncTimeout,
                            MaxUploadSize = Config.GetMaxUploadSize() / (1024 * 1024),
                            RangeUploadSize = Config.GetRequestFilterSize(),
                            AllowedExtensionWhitelist = Host.AllowedExtensionWhitelist.ToStorageString(),
                            DefaultEndUserExtensionWhitelist = Host.DefaultEndUserExtensionWhitelist.ToStorageString(),
                        },
                    },
                };

                return this.Request.CreateResponse(HttpStatusCode.OK, response);
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        /// POST: api/Security/UpdateOtherSettings
        /// <summary>Updates other settings.</summary>
        /// <param name="request">The update request.</param>
        /// <returns>A response indicating success.</returns>
        [HttpPost]
        [RequireHost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage UpdateOtherSettings(UpdateOtherSettingsRequest request)
        {
            try
            {
                HostController.Instance.Update("ShowCriticalErrors", request.ShowCriticalErrors ? "Y" : "N", false);
                HostController.Instance.Update("DebugMode", request.DebugMode ? "True" : "False", false);
                HostController.Instance.Update("RememberCheckbox", request.RememberCheckbox ? "Y" : "N", false);
                HostController.Instance.Update("AutoAccountUnlockDuration", request.AutoAccountUnlockDuration.ToString(), false);
                HostController.Instance.Update("AsyncTimeout", request.AsyncTimeout.ToString(), false);
                var oldExtensionList = Host.AllowedExtensionWhitelist.ToStorageString();
                var fileExtensions = new FileExtensionWhitelist(request.AllowedExtensionWhitelist);
                var newExtensionList = fileExtensions.ToStorageString();
                HostController.Instance.Update("FileExtensions", newExtensionList, false);
                if (oldExtensionList != newExtensionList)
                {
                    PortalSecurity.Instance.CheckAllPortalFileExtensionWhitelists(newExtensionList);
                }

                var defaultEndUserExtensionWhitelist = new FileExtensionWhitelist(request.DefaultEndUserExtensionWhitelist);
                defaultEndUserExtensionWhitelist = defaultEndUserExtensionWhitelist.RestrictBy(fileExtensions);
                HostController.Instance.Update("DefaultEndUserExtensionWhitelist", defaultEndUserExtensionWhitelist.ToStorageString(), false);

                var maxCurrentRequest = Config.GetMaxUploadSize();
                var maxUploadByMb = request.MaxUploadSize * 1024 * 1024;
                if (maxCurrentRequest != maxUploadByMb)
                {
                    Config.SetMaxUploadSize(maxUploadByMb);
                }

                DataCache.ClearCache();

                return this.Request.CreateResponse(HttpStatusCode.OK, new { Success = true });
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        /// GET: api/Security/GetAuditCheckResults
        /// <summary>Gets audit check results.</summary>
        /// <returns>audit check results.</returns>
        [HttpGet]
        [RequireHost]
        public HttpResponseMessage GetAuditCheckResults([FromUri] bool checkAll = false)
        {
            try
            {
                var audit = new Components.AuditChecks(this.pagesController);
                var results = audit.DoChecks(checkAll);
                var response = new
                {
                    Success = true,
                    Results = results,
                };

                return this.Request.CreateResponse(HttpStatusCode.OK, response);
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        /// GET: api/Security/GetAuditCheckResult?id={id}
        /// <summary> Gets audit check result for a specific checker.</summary>
        /// <returns>audit check result.</returns>
        [HttpGet]
        [RequireHost]
        public HttpResponseMessage GetAuditCheckResult([FromUri] string id)
        {
            try
            {
                var audit = new Components.AuditChecks(this.pagesController);
                var result = audit.DoCheck(id);
                var response = new
                {
                    Success = true,
                    Result = result,
                };

                return this.Request.CreateResponse(HttpStatusCode.OK, response);
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        /// GET: api/Security/GetSuperuserActivities
        /// <summary>Gets super user activities.</summary>
        /// <returns>super user activities.</returns>
        [HttpGet]
        [RequireHost]
        public HttpResponseMessage GetSuperuserActivities()
        {
            try
            {
                var users = UserController.GetUsers(true, true, -1).Cast<UserInfo>().Select(u => new
                {
                    u.Username,
                    u.FirstName,
                    u.LastName,
                    u.DisplayName,
                    u.Email,
                    CreatedDate = this.DisplayDate(u.Membership.CreatedDate),
                    LastLoginDate = this.DisplayDate(u.Membership.LastLoginDate),
                    LastActivityDate = this.DisplayDate(u.Membership.LastActivityDate),
                }).ToList();

                var response = new
                {
                    Success = true,
                    Results = new
                    {
                        Activities = users,
                    },
                };

                return this.Request.CreateResponse(HttpStatusCode.OK, response);
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        /// GET: api/Security/SearchFileSystemAndDatabase
        /// <summary>Searches file system and database.</summary>
        /// <returns>The search results from files and database.</returns>
        [HttpGet]
        [RequireHost]
        public HttpResponseMessage SearchFileSystemAndDatabase(string term)
        {
            try
            {
                // run these in parallel
                var task1 = Task.Factory.StartNew(() => Components.Utility.SearchFiles(term));
                var task2 = Task.Factory.StartNew(() => Components.Utility.SearchDatabase(term));
                Task.WhenAll(task1, task2).Wait();

                var foundinfiles = task1.Result;
                var foundindb = task2.Result;
                var response = new
                {
                    Success = true,
                    Results = new
                    {
                        FoundInFiles = foundinfiles,
                        FoundInDatabase = foundindb,
                    },
                };

                return this.Request.CreateResponse(HttpStatusCode.OK, response);
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        /// GET: api/Security/GetLastModifiedFiles
        /// <summary>Gets recently modified files.</summary>
        /// <returns>last modified files.</returns>
        [HttpGet]
        [RequireHost]
        public HttpResponseMessage GetLastModifiedFiles()
        {
            try
            {
                var highRiskFiles = Components.Utility.GetLastModifiedExecutableFiles().Select(f => new
                {
                    FilePath = this.GetFilePath(f.FullName),
                    LastWriteTime = this.DisplayDate(f.LastWriteTime),
                });
                var lowRiskFiles = Components.Utility.GetLastModifiedFiles().Select(f => new
                {
                    FilePath = this.GetFilePath(f.FullName),
                    LastWriteTime = this.DisplayDate(f.LastWriteTime),
                });
                var response = new
                {
                    Success = true,
                    Results = new
                    {
                        HighRiskFiles = highRiskFiles,
                        LowRiskFiles = lowRiskFiles,
                    },
                };

                return this.Request.CreateResponse(HttpStatusCode.OK, response);
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        /// GET: api/Security/GetRecentlyModifiedSettings
        /// <summary>Gets last modified settings.</summary>
        /// <returns>last modified settings.</returns>
        [HttpGet]
        [RequireHost]
        public HttpResponseMessage GetLastModifiedSettings()
        {
            try
            {
                var settings = this.controller.GetModifiedSettings();
                var portalSettings = (from DataRow dr in settings[0].Rows
                                      select new SettingsDto
                                      {
                                          PortalId = Convert.ToInt32(dr["PortalID"] != DBNull.Value ? dr["PortalID"] : Null.NullInteger),
                                          SettingName = Convert.ToString(dr["SettingName"]),
                                          SettingValue = Convert.ToString(dr["SettingValue"]),
                                          LastModifiedByUserId = Convert.ToInt32(dr["LastModifiedByUserID"]),
                                          LastModifiedOnDate = this.DisplayDate(Convert.ToDateTime(dr["LastModifiedOnDate"])),
                                      }).ToList();

                var hostSettings = (from DataRow dr in settings[1].Rows
                                    select new SettingsDto
                                    {
                                        SettingName = Convert.ToString(dr["SettingName"]),
                                        SettingValue = Convert.ToString(dr["SettingValue"]),
                                        LastModifiedByUserId = Convert.ToInt32(dr["LastModifiedByUserID"]),
                                        LastModifiedOnDate = this.DisplayDate(Convert.ToDateTime(dr["LastModifiedOnDate"])),
                                    }).ToList();

                var tabSettings = (from DataRow dr in settings[2].Rows
                                   select new SettingsDto
                                   {
                                       TabId = Convert.ToInt32(dr["TabID"]),
                                       PortalId = Convert.ToInt32(dr["PortalID"] != DBNull.Value ? dr["PortalID"] : Null.NullInteger),
                                       SettingName = Convert.ToString(dr["SettingName"]),
                                       SettingValue = Convert.ToString(dr["SettingValue"]),
                                       LastModifiedByUserId = Convert.ToInt32(dr["LastModifiedByUserID"]),
                                       LastModifiedOnDate = this.DisplayDate(Convert.ToDateTime(dr["LastModifiedOnDate"])),
                                   }).ToList();

                var moduleSettings = (from DataRow dr in settings[3].Rows
                                      select new SettingsDto
                                      {
                                          ModuleId = Convert.ToInt32(dr["ModuleID"]),
                                          PortalId = Convert.ToInt32(dr["PortalID"] != DBNull.Value ? dr["PortalID"] : Null.NullInteger),
                                          Type = Convert.ToString(dr["Type"]),
                                          SettingName = Convert.ToString(dr["SettingName"]),
                                          SettingValue = Convert.ToString(dr["SettingValue"]),
                                          LastModifiedByUserId = Convert.ToInt32(dr["LastModifiedByUserID"]),
                                          LastModifiedOnDate = this.DisplayDate(Convert.ToDateTime(dr["LastModifiedOnDate"])),
                                      }).ToList();

                var response = new
                {
                    Success = true,
                    Results = new
                    {
                        PortalSettings = portalSettings,
                        HostSettings = hostSettings,
                        TabSettings = tabSettings,
                        ModuleSettings = moduleSettings,
                    },
                };

                return this.Request.CreateResponse(HttpStatusCode.OK, response);
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        /// <summary>
        /// Retrieves a paged list of API tokens for the specified portal and page, with the specified page size.
        /// </summary>
        /// <param name="portalId">The ID of the portal for which to retrieve API tokens. Use -2 for all portals.</param>
        /// <param name="filter">Value indicating which tokens to return based on status.</param>
        /// <param name="apiKey">API key to filter the results by.</param>
        /// <param name="scope">Filter the results by scope or use -2 for no scope.</param>
        /// <param name="pageIndex">The page index (starting from 0) of the API token list to retrieve.</param>
        /// <param name="pageSize">The number of API tokens per page to retrieve.</param>
        /// <returns>A paged list of `ApiToken` objects for the specified portal and page.</returns>
        [HttpGet]
        [AdvancedPermission(MenuName = Components.Constants.MenuName, Permission = Components.Constants.ManageApiTokens)]

        public HttpResponseMessage GetApiTokens(int portalId, int filter, string apiKey, int scope, int pageIndex, int pageSize)
        {
            if (portalId < 0)
            {
                portalId = -1;
            }

            var noScopeDefined = scope == -2;
            var requestedScope = ApiTokenScope.User;
            var user = this.UserInfo;

            if (user.IsSuperUser)
            {
                if (noScopeDefined)
                {
                    requestedScope = ApiTokenScope.Host;
                }
                else
                {
                    requestedScope = (ApiTokenScope)scope;
                }
            }
            else if (user.IsAdmin)
            {
                portalId = PortalSettings.Current.PortalId;
                if (noScopeDefined)
                {
                    requestedScope = ApiTokenScope.Portal;
                }
                else if (scope > 1)
                {
                    return this.Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Invalid scope");
                }
                else
                {
                    requestedScope = (ApiTokenScope)scope;
                }
            }
            else
            {
                portalId = PortalSettings.Current.PortalId;
                if (scope > 1)
                {
                    return this.Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Invalid scope");
                }
            }

            var response = ApiTokenController.Instance.GetApiTokens(requestedScope, noScopeDefined, portalId, -1, (ApiTokenFilter)filter, apiKey, pageIndex, pageSize);
            return this.Request.CreateResponse(HttpStatusCode.OK, response.Serialize());
        }

        /// <summary>
        /// Retrieves a dictionary with the API token keyword and its corresponding attribute based on the user scope.
        /// </summary>
        /// <returns>A dictionary with the API token keyword and its corresponding attribute.</returns>
        [HttpGet]
        [AdvancedPermission(MenuName = Components.Constants.MenuName, Permission = Components.Constants.ManageApiTokens)]
        public HttpResponseMessage GetApiTokenKeys()
        {
            // The response.
            var response = new SortedDictionary<string, ApiTokenAttribute>();

            // Checks if the user is authorized.
            var user = this.UserInfo;

            if (user.IsSuperUser)
            {
                // If the user is a superuser, sets the API token scope to host.
                response = ApiTokenController.Instance.ApiTokenKeyList(ApiTokenScope.Host, Thread.CurrentThread.CurrentUICulture.Name);
            }
            else if (user.IsAdmin)
            {
                // If the user is an admin, sets the API token scope to portal.
                response = ApiTokenController.Instance.ApiTokenKeyList(ApiTokenScope.Portal, Thread.CurrentThread.CurrentUICulture.Name);
            }
            else
            {
                // If the user is regular, set the API token scope to user.
                response = ApiTokenController.Instance.ApiTokenKeyList(ApiTokenScope.User, Thread.CurrentThread.CurrentUICulture.Name);
            }

            // Returns the response.
            return this.Request.CreateResponse(HttpStatusCode.OK, response.Values);
        }

        internal string AddPortalAlias(string portalAlias, int portalId)
        {
            if (!string.IsNullOrEmpty(portalAlias))
            {
                portalAlias = portalAlias.ToLowerInvariant().Trim('/');
                if (portalAlias.IndexOf("://", StringComparison.Ordinal) != -1)
                {
                    portalAlias = portalAlias.Remove(0, portalAlias.IndexOf("://", StringComparison.Ordinal) + 3);
                }

                var alias = this.portalAliasService.GetPortalAlias(portalAlias, portalId);
                if (alias == null)
                {
                    alias = new PortalAliasInfo { PortalID = portalId, HTTPAlias = portalAlias };
                    this.portalAliasService.AddPortalAlias(alias);
                }
            }

            return portalAlias;
        }

        private int ValidateTabId(int tabId)
        {
            var tab = TabController.Instance.GetTab(tabId, this.PortalId);
            return tab?.TabID ?? Null.NullInteger;
        }

        private string GetTabName(int tabId)
        {
            if (tabId == Null.NullInteger)
            {
                return string.Empty;
            }
            else
            {
                var tab = TabController.Instance.GetTab(tabId, this.PortalId);
                return tab != null ? tab.TabName : string.Empty;
            }
        }

        private string GetTabPath(int tabId)
        {
            if (tabId == Null.NullInteger)
            {
                return string.Empty;
            }
            else
            {
                var tab = TabController.Instance.GetTab(tabId, this.PortalId);
                return tab != null ? tab.TabPath : string.Empty;
            }
        }

        private string DisplayDate(DateTime userDate)
        {
            var date = Null.NullString;
            date = !Null.IsNull(userDate) ? userDate.ToString(CultureInfo.InvariantCulture) : string.Empty;
            return date;
        }

        private string GetFilePath(string filePath)
        {
            var path = Regex.Replace(filePath, Regex.Escape(Globals.ApplicationMapPath), string.Empty, RegexOptions.IgnoreCase);
            return path.TrimStart('\\');
        }
    }
}
