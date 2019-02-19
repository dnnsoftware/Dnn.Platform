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
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Xml;
using Dnn.PersonaBar.Library;
using Dnn.PersonaBar.Library.Attributes;
using Dnn.PersonaBar.Security.Services.Dto;
using DotNetNuke.Application;
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
using Dnn.PersonaBar.Security.Helper;

namespace Dnn.PersonaBar.Security.Services
{
    [MenuPermission(MenuName = Components.Constants.MenuName)]
    public class SecurityController : PersonaBarApiController
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(SecurityController));
        private readonly Components.SecurityController _controller;
        private readonly IPortalAliasController _portalAliasController;
        private const string BULLETIN_XMLNODE_PATH = "//channel/item";

        public SecurityController()
            : this(
                new Components.SecurityController(),
                PortalAliasController.Instance
            )
        {
        }

        internal SecurityController(
            Components.SecurityController controller,
            IPortalAliasController portalAliasController
            )
        {
            _controller = controller;
            _portalAliasController = portalAliasController;
        }

        #region Login Settings

        /// GET: api/Security/GetBasicLoginSettings
        /// <summary>
        /// Gets portal's basic login settings
        /// </summary>
        /// <param name="cultureCode"></param>
        /// <returns>Portal's basic login settings</returns>
        [HttpGet]
        [AdvancedPermission(MenuName = Components.Constants.MenuName, Permission = Components.Constants.BasicLoginSettingsView)]
        public HttpResponseMessage GetBasicLoginSettings(string cultureCode)
        {
            try
            {
                cultureCode = string.IsNullOrEmpty(cultureCode)
                    ? LocaleController.Instance.GetCurrentLocale(PortalId).Code
                    : cultureCode;

                var portal = PortalController.Instance.GetPortal(PortalId, cultureCode);
                var portalSettings = new PortalSettings(portal);

                dynamic settings = new ExpandoObject();
                settings.DefaultAuthProvider = PortalController.GetPortalSetting("DefaultAuthProvider", PortalId, "DNN");
                settings.PrimaryAdministratorId = PortalSettings.Current.AdministratorId;
                settings.RedirectAfterLoginTabId = ValidateTabId(portalSettings.Registration.RedirectAfterLogin);
                settings.RedirectAfterLoginTabName = GetTabName(portalSettings.Registration.RedirectAfterLogin);
                settings.RedirectAfterLoginTabPath = GetTabPath(portalSettings.Registration.RedirectAfterLogin);
                settings.RedirectAfterLogoutTabId = ValidateTabId(portalSettings.Registration.RedirectAfterLogout);
                settings.RedirectAfterLogoutTabName = GetTabName(portalSettings.Registration.RedirectAfterLogout);
                settings.RedirectAfterLogoutTabPath = GetTabPath(portalSettings.Registration.RedirectAfterLogout);
                settings.RequireValidProfileAtLogin = PortalController.GetPortalSettingAsBoolean("Security_RequireValidProfileAtLogin", PortalId, true);
                settings.CaptchaLogin = PortalController.GetPortalSettingAsBoolean("Security_CaptchaLogin", PortalId, false);
                settings.CaptchaRetrivePassword = PortalController.GetPortalSettingAsBoolean("Security_CaptchaRetrivePassword", PortalId, false);
                settings.CaptchaChangePassword = PortalController.GetPortalSettingAsBoolean("Security_CaptchaChangePassword", PortalId, false);
                settings.HideLoginControl = PortalSettings.HideLoginControl;
                settings.cultureCode = cultureCode;

                var authProviders = _controller.GetAuthenticationProviders().Select(v => new
                {
                    Name = v,
                    Value = v
                }).ToList();

                var adminUsers = _controller.GetAdminUsers(PortalId).Select(v => new
                {
                    v.UserID,
                    v.FullName
                }).ToList();

                var response = new
                {
                    Success = true,
                    Results = new
                    {
                        Settings = settings,
                        AuthProviders = authProviders,
                        Administrators = adminUsers
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

        /// POST: api/Security/UpdateBasicLoginSettings
        /// <summary>
        /// Updates an existing log settings
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AdvancedPermission(MenuName = Components.Constants.MenuName, Permission = Components.Constants.BasicLoginSettingsView + "&" + Components.Constants.BasicLoginSettingsEdit)]
        public HttpResponseMessage UpdateBasicLoginSettings(UpdateBasicLoginSettingsRequest request)
        {
            if (!ModelState.IsValid)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
            }

            try
            {
                var cultureCode = string.IsNullOrEmpty(request.CultureCode)
                    ? LocaleController.Instance.GetCurrentLocale(PortalId).Code
                    : request.CultureCode;

                var portalInfo = PortalController.Instance.GetPortal(PortalId);
                portalInfo.AdministratorId = Convert.ToInt32(request.PrimaryAdministratorId);
                PortalController.Instance.UpdatePortalInfo(portalInfo);

                PortalController.UpdatePortalSetting(PortalId, "DefaultAuthProvider", request.DefaultAuthProvider);
                PortalController.UpdatePortalSetting(PortalId, "Redirect_AfterLogin", request.RedirectAfterLoginTabId.ToString(), cultureCode);
                PortalController.UpdatePortalSetting(PortalId, "Redirect_AfterLogout", request.RedirectAfterLogoutTabId.ToString(), cultureCode);
                PortalController.UpdatePortalSetting(PortalId, "Security_RequireValidProfile", request.RequireValidProfileAtLogin.ToString(), false);
                PortalController.UpdatePortalSetting(PortalId, "Security_CaptchaLogin", request.CaptchaLogin.ToString(), false);
                PortalController.UpdatePortalSetting(PortalId, "Security_CaptchaRetrivePassword", request.CaptchaRetrivePassword.ToString(), false);
                PortalController.UpdatePortalSetting(PortalId, "Security_CaptchaChangePassword", request.CaptchaChangePassword.ToString(), false);
                PortalController.UpdatePortalSetting(PortalId, "HideLoginControl", request.HideLoginControl.ToString(), false);

                return Request.CreateResponse(HttpStatusCode.OK, new { Success = true });
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        #endregion

        #region IP Filters

        /// GET: api/Security/GetIpFilters
        /// <summary>
        /// Gets list of IP filters
        /// </summary>
        /// <param></param>
        /// <returns>List of IP filters</returns>
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
                    v.RuleType
                }).ToList();
                var response = new
                {
                    Success = true,
                    Results = new
                    {
                        Filters = filters,
                        EnableIPChecking = Host.EnableIPChecking
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

        /// GET: api/Security/GetIpFilter
        /// <summary>
        /// Gets an IP filter
        /// </summary>
        /// <param name="filterId"></param>
        /// <returns>IP filter</returns>
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
                        filter.SubnetMask
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

        /// POST: api/Security/UpdateIpFilter
        /// <summary>
        /// Updates an IP filter
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
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

                if ((ipf.IPAddress == "127.0.0.1" || ipf.IPAddress == "localhost" || ipf.IPAddress == "::1" || ipf.IPAddress == "*") && ipf.RuleType == 2)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, string.Format(Localization.GetString("CannotDeleteLocalhost.Text", Components.Constants.LocalResourcesFile)));
                }

                if (IPFilterController.Instance.IsAllowableDeny(HttpContext.Current.Request.UserHostAddress, ipf) == false)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, string.Format(Localization.GetString("CannotDeleteIPInUse.Text", Components.Constants.LocalResourcesFile)));
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
                return Request.CreateResponse(HttpStatusCode.OK, new {Success = true});
            }
            catch (ArgumentException exc)
            {
                Logger.Info(exc);
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, exc.Message);
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        /// POST: api/Security/DeleteIpFilter
        /// <summary>
        /// Deletes an IP filter
        /// </summary>
        /// <param name="filterId"></param>
        /// <returns></returns>
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
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, string.Format(Localization.GetString("CannotDelete.Text", Components.Constants.LocalResourcesFile)));
                }
                else
                {
                    var ipf = new IPFilterInfo();
                    ipf.IPFilterID = filterId;
                    IPFilterController.Instance.DeleteIPFilter(ipf);
                    return Request.CreateResponse(HttpStatusCode.OK, new { Success = true });
                }
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        #endregion

        #region Member Accounts

        /// GET: api/Security/GetMemberSettings
        /// <summary>
        /// Gets portal's member settings
        /// </summary>
        /// <returns>Portal's member settings</returns>
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
                            Host.PasswordExpiryReminder
                        }
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

        /// POST: api/Security/UpdateMemberSettings
        /// <summary>
        /// Updates member settings
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
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

                return Request.CreateResponse(HttpStatusCode.OK, new { Success = true });
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        /// GET: api/Security/GetRegistrationSettings
        /// <summary>
        /// Gets portal's registration settings
        /// </summary>
        /// <returns>Portal's registration settings</returns>
        [HttpGet]
        [AdvancedPermission(MenuName = Components.Constants.MenuName, Permission = Components.Constants.RegistrationSettingsView)]
        public HttpResponseMessage GetRegistrationSettings()
        {
            try
            {
                List<KeyValuePair<string, int>> userRegistrationOptions = RegistrationSettingsHelper.GetUserRegistrationOptions();
                List<KeyValuePair<string, int>> registrationFormTypeOptions = RegistrationSettingsHelper.GetRegistrationFormOptions();

                var activeLanguage = LocaleController.Instance.GetDefaultLocale(PortalId).Code;
                var portal = PortalController.Instance.GetPortal(PortalId, activeLanguage);

                var response = new
                {
                    Success = true,
                    Results = new
                    {
                        Settings = new
                        {
                            portal.UserRegistration,
                            EnableRegisterNotification = PortalController.GetPortalSettingAsBoolean("EnableRegisterNotification", PortalId, true),
                            UseAuthenticationProviders = PortalController.GetPortalSettingAsBoolean("Registration_UseAuthProviders", PortalId, false),
                            ExcludedTerms = PortalController.GetPortalSetting("Registration_ExcludeTerms", PortalId, string.Empty),
                            UseProfanityFilter = PortalController.GetPortalSettingAsBoolean("Registration_UseProfanityFilter", PortalId, false),
                            PortalSettings.Registration.RegistrationFormType,
                            PortalSettings.Registration.RegistrationFields,
                            UseEmailAsUsername = PortalController.GetPortalSettingAsBoolean("Registration_UseEmailAsUserName", PortalId, false),
                            RequireUniqueDisplayName = PortalController.GetPortalSettingAsBoolean("Registration_RequireUniqueDisplayName", PortalId, false),
                            DisplayNameFormat = PortalController.GetPortalSetting("Security_DisplayNameFormat", PortalId, string.Empty),
                            UserNameValidation = PortalController.GetPortalSetting("Security_UserNameValidation", PortalId, Globals.glbUserNameRegEx),
                            EmailAddressValidation = PortalController.GetPortalSetting("Security_EmailValidation", PortalId, Globals.glbEmailRegEx),
                            UseRandomPassword = PortalController.GetPortalSettingAsBoolean("Registration_RandomPassword", PortalId, false),
                            RequirePasswordConfirmation = PortalController.GetPortalSettingAsBoolean("Registration_RequireConfirmPassword", PortalId, true),
                            RequireValidProfile = PortalController.GetPortalSettingAsBoolean("Security_RequireValidProfile", PortalId, false),
                            UseCaptchaRegister = PortalController.GetPortalSettingAsBoolean("Security_CaptchaRegister", PortalId, false),
                            RedirectAfterRegistrationTabId = ValidateTabId(PortalSettings.Registration.RedirectAfterRegistration),
                            RedirectAfterRegistrationTabName = GetTabName(PortalSettings.Registration.RedirectAfterRegistration),
                            RedirectAfterRegistrationTabPath = GetTabPath(PortalSettings.Registration.RedirectAfterRegistration),
                            RequiresUniqueEmail = MembershipProviderConfig.RequiresUniqueEmail.ToString(CultureInfo.InvariantCulture),
                            PasswordFormat = MembershipProviderConfig.PasswordFormat.ToString(),
                            PasswordRetrievalEnabled = MembershipProviderConfig.PasswordRetrievalEnabled.ToString(CultureInfo.InvariantCulture),
                            PasswordResetEnabled = MembershipProviderConfig.PasswordResetEnabled.ToString(CultureInfo.InvariantCulture),
                            MinPasswordLength = MembershipProviderConfig.MinPasswordLength.ToString(CultureInfo.InvariantCulture),
                            MinNonAlphanumericCharacters = MembershipProviderConfig.MinNonAlphanumericCharacters.ToString(CultureInfo.InvariantCulture),
                            RequiresQuestionAndAnswer = MembershipProviderConfig.RequiresQuestionAndAnswer.ToString(CultureInfo.InvariantCulture),
                            MembershipProviderConfig.PasswordStrengthRegularExpression,
                            MaxInvalidPasswordAttempts = MembershipProviderConfig.MaxInvalidPasswordAttempts.ToString(CultureInfo.InvariantCulture),
                            PasswordAttemptWindow = MembershipProviderConfig.PasswordAttemptWindow.ToString(CultureInfo.InvariantCulture)
                        },
                        UserRegistrationOptions = userRegistrationOptions,
                        RegistrationFormTypeOptions = registrationFormTypeOptions
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

        private int ValidateTabId(int tabId)
        {
            var tab = TabController.Instance.GetTab(tabId, PortalId);
            return tab?.TabID ?? Null.NullInteger;
        }

        private string GetTabName(int tabId)
        {
            if (tabId == Null.NullInteger)
            {
                return "";
            }
            else
            {
                var tab = TabController.Instance.GetTab(tabId, PortalId);
                return tab != null ? tab.TabName : "";
            }
        }

        private string GetTabPath(int tabId)
        {
            if (tabId == Null.NullInteger)
            {
                return "";
            }
            else
            {
                var tab = TabController.Instance.GetTab(tabId, PortalId);
                return tab != null ? tab.TabPath : "";
            }
        }

        /// POST: api/Security/UpdateRegistrationSettings
        /// <summary>
        /// Updates registration settings
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AdvancedPermission(MenuName = Components.Constants.MenuName, Permission = Components.Constants.RegistrationSettingsView + "&" + Components.Constants.RegistrationSettingsEdit)]
        public HttpResponseMessage UpdateRegistrationSettings(UpdateRegistrationSettingsRequest request)
        {
            if (!ModelState.IsValid)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
            }

            try
            {
                var setting = request.RegistrationFields;
                PortalController.UpdatePortalSetting(PortalId, "Registration_RegistrationFields", setting);
                PortalController.UpdatePortalSetting(PortalId, "Registration_RegistrationFormType", request.RegistrationFormType.ToString(), false);
                PortalController.UpdatePortalSetting(PortalId, "Registration_UseEmailAsUserName", request.UseEmailAsUsername.ToString(), false);

                var portalInfo = PortalController.Instance.GetPortal(PortalId);
                portalInfo.UserRegistration = Convert.ToInt32(request.UserRegistration);
                PortalController.Instance.UpdatePortalInfo(portalInfo);

                PortalController.UpdatePortalSetting(PortalId, "EnableRegisterNotification", request.EnableRegisterNotification.ToString(), false);
                PortalController.UpdatePortalSetting(PortalId, "Registration_UseAuthProviders", request.UseAuthenticationProviders.ToString(), false);
                PortalController.UpdatePortalSetting(PortalId, "Registration_ExcludeTerms", request.ExcludedTerms, false);
                PortalController.UpdatePortalSetting(PortalId, "Registration_UseProfanityFilter", request.UseProfanityFilter.ToString(), false);
                PortalController.UpdatePortalSetting(PortalId, "Registration_RequireUniqueDisplayName", request.RequireUniqueDisplayName.ToString(), false);
                PortalController.UpdatePortalSetting(PortalId, "Security_DisplayNameFormat", request.DisplayNameFormat, false);
                PortalController.UpdatePortalSetting(PortalId, "Security_UserNameValidation", request.UserNameValidation, false);
                PortalController.UpdatePortalSetting(PortalId, "Security_EmailValidation", request.EmailAddressValidation, false);
                PortalController.UpdatePortalSetting(PortalId, "Registration_RandomPassword", request.UseRandomPassword.ToString(), false);
                PortalController.UpdatePortalSetting(PortalId, "Registration_RequireConfirmPassword", request.RequirePasswordConfirmation.ToString(), true);
                PortalController.UpdatePortalSetting(PortalId, "Security_RequireValidProfile", request.RequireValidProfile.ToString(), false);
                PortalController.UpdatePortalSetting(PortalId, "Security_CaptchaRegister", request.UseCaptchaRegister.ToString(), false);
                PortalController.UpdatePortalSetting(PortalId, "Redirect_AfterRegistration", request.RedirectAfterRegistrationTabId.ToString(), LocaleController.Instance.GetCurrentLocale(PortalId).Code);

                return Request.CreateResponse(HttpStatusCode.OK, new { Success = true });
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        #endregion

        #region SSL Settings

        /// GET: api/Security/GetSslSettings
        /// <summary>
        /// Gets portal's SSL settings
        /// </summary>
        /// <returns>Portal's ssl settings</returns>
        [HttpGet]
        [DnnAuthorize(StaticRoles = Constants.AdminsRoleName)]
        public HttpResponseMessage GetSslSettings()
        {
            try
            {
                dynamic settings = new ExpandoObject();
                settings.SSLEnabled = PortalController.GetPortalSettingAsBoolean("SSLEnabled", PortalId, false);
                settings.SSLEnforced = PortalController.GetPortalSettingAsBoolean("SSLEnforced", PortalId, false);
                settings.SSLURL = PortalController.GetPortalSetting("SSLURL", PortalId, Null.NullString);
                settings.STDURL = PortalController.GetPortalSetting("STDURL", PortalId, Null.NullString);

                if (UserInfo.IsSuperUser)
                {
                    settings.SSLOffloadHeader = HostController.Instance.GetString("SSLOffloadHeader", "");
                }

                var response = new
                {
                    Success = true,
                    Results = new
                    {
                        Settings = settings
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

        /// POST: api/Security/UpdateSslSettings
        /// <summary>
        /// Updates SSL settings
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [DnnAuthorize(StaticRoles = Constants.AdminsRoleName)]
        public HttpResponseMessage UpdateSslSettings(UpdateSslSettingsRequest request)
        {
            try
            {
                PortalController.UpdatePortalSetting(PortalId, "SSLEnabled", request.SSLEnabled.ToString(), false);
                PortalController.UpdatePortalSetting(PortalId, "SSLEnforced", request.SSLEnforced.ToString(), false);
                PortalController.UpdatePortalSetting(PortalId, "SSLURL", AddPortalAlias(request.SSLURL, PortalId), false);
                PortalController.UpdatePortalSetting(PortalId, "STDURL", AddPortalAlias(request.STDURL, PortalId), false);

                if (UserInfo.IsSuperUser)
                {
                    HostController.Instance.Update("SSLOffloadHeader", request.SSLOffloadHeader);
                }

                DataCache.ClearPortalCache(PortalId, false);

                return Request.CreateResponse(HttpStatusCode.OK, new { Success = true });
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        #endregion

        #region Security Bulletins

        /// GET: api/Security/GetSecurityBulletins
        /// <summary>
        /// Gets security bulletins
        /// </summary>
        /// <returns>Security bulletins</returns>
        [HttpGet]
        [RequireHost]
        public HttpResponseMessage GetSecurityBulletins()
        {
            try
            {
                var plartformVersion = System.Reflection.Assembly.LoadFrom(Globals.ApplicationMapPath + @"\bin\DotNetNuke.dll").GetName().Version;
                string sRequest = string.Format("http://update.dotnetnuke.com/security.aspx?type={0}&name={1}&version={2}",
                    DotNetNukeContext.Current.Application.Type,
                    "DNNCORP.CE",
                    Globals.FormatVersion(plartformVersion, "00", 3, ""));

                //format for display with "." delimiter
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
                    if (PortalSecurity.IsInRoles(PortalSettings.AdministratorRoleId.ToString()))
                    {
                        return Request.CreateErrorResponse(HttpStatusCode.BadRequest, string.Format(Localization.GetString("RequestFailed_Admin.Text", Components.Constants.LocalResourcesFile), sRequest));
                    }
                    else
                    {
                        return Request.CreateErrorResponse(HttpStatusCode.BadRequest, Localization.GetString("RequestFailed_User.Text", Components.Constants.LocalResourcesFile) + oExc.Message);
                    }
                }

                // load XML document
                StreamReader oReader = new StreamReader(oStream);
                XmlDocument oDoc = new XmlDocument { XmlResolver = null };
                oDoc.LoadXml(oReader.ReadToEnd());

                List<object> items = new List<object>();
                foreach (XmlNode selectNode in oDoc.SelectNodes(BULLETIN_XMLNODE_PATH))
                {
                    items.Add(new
                    {
                        Title = selectNode.SelectSingleNode("title") != null ? selectNode.SelectSingleNode("title").InnerText : "",
                        Link = selectNode.SelectSingleNode("link") != null ? selectNode.SelectSingleNode("link").InnerText : "",
                        Description = selectNode.SelectSingleNode("description") != null ? selectNode.SelectSingleNode("description").InnerText : "",
                        Author = selectNode.SelectSingleNode("author") != null ? selectNode.SelectSingleNode("author").InnerText : "",
                        PubDate = selectNode.SelectSingleNode("pubDate") != null ? selectNode.SelectSingleNode("pubDate").InnerText.Split(' ')[0] : ""
                    });
                }

                var response = new
                {
                    Success = true,
                    Results = new
                    {
                        PlatformVersion = sVersion,
                        SecurityBulletins = items
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

        #endregion

        #region Other Settings

        /// GET: api/Security/GetOtherSettings
        /// <summary>
        /// Gets host other settings
        /// </summary>
        /// <returns>Portal's ssl settings</returns>
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
                            Host.DisplayCopyright,
                            Host.ShowCriticalErrors,
                            Host.DebugMode,
                            Host.RememberCheckbox,
                            Host.AutoAccountUnlockDuration,
                            Host.AsyncTimeout,
                            MaxUploadSize = Config.GetMaxUploadSize() / (1024 * 1024),
                            RangeUploadSize = Config.GetRequestFilterSize(),
                            AllowedExtensionWhitelist = Host.AllowedExtensionWhitelist.ToStorageString()
                        }
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

        /// POST: api/Security/UpdateOtherSettings
        /// <summary>
        /// Updates other settings
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [RequireHost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage UpdateOtherSettings(UpdateOtherSettingsRequest request)
        {
            try
            {
                HostController.Instance.Update("Copyright", request.DisplayCopyright ? "Y" : "N", false);
                HostController.Instance.Update("ShowCriticalErrors", request.ShowCriticalErrors ? "Y" : "N", false);
                HostController.Instance.Update("DebugMode", request.DebugMode ? "True" : "False", false);
                HostController.Instance.Update("RememberCheckbox", request.RememberCheckbox ? "Y" : "N", false);
                HostController.Instance.Update("AutoAccountUnlockDuration", request.AutoAccountUnlockDuration.ToString(), false);
                HostController.Instance.Update("AsyncTimeout", request.AsyncTimeout.ToString(), false);
                HostController.Instance.Update("FileExtensions", request.AllowedExtensionWhitelist, false);

                var maxCurrentRequest = Config.GetMaxUploadSize();
                var maxUploadByMb = request.MaxUploadSize * 1024 * 1024;
                if (maxCurrentRequest != maxUploadByMb)
                {
                    Config.SetMaxUploadSize(maxUploadByMb);
                }

                DataCache.ClearCache();

                return Request.CreateResponse(HttpStatusCode.OK, new { Success = true });
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        #endregion

        #region Security Analyzer

        /// GET: api/Security/GetAuditCheckResults
        /// <summary>
        /// Gets audit check results
        /// </summary>
        /// <returns>audit check results</returns>
        [HttpGet]
        [RequireHost]
        public HttpResponseMessage GetAuditCheckResults([FromUri] bool checkAll = false)
        {
            try
            {
                var audit = new Components.AuditChecks();
                var results = audit.DoChecks(checkAll);
                var response = new
                {
                    Success = true,
                    Results = results
                };

                return Request.CreateResponse(HttpStatusCode.OK, response);
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        /// GET: api/Security/GetAuditCheckResult?id={id}
        /// <summary>
        /// Gets audit check result for a specific checker.
        /// </summary>
        /// <returns>audit check result</returns>
        [HttpGet]
        [RequireHost]
        public HttpResponseMessage GetAuditCheckResult([FromUri] string id)
        {
            try
            {
                var audit = new Components.AuditChecks();
                var result = audit.DoCheck(id);
                var response = new
                {
                    Success = true,
                    Result = result
                };

                return Request.CreateResponse(HttpStatusCode.OK, response);
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        /// GET: api/Security/GetSuperuserActivities
        /// <summary>
        /// Gets super user activities.
        /// </summary>
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
                    CreatedDate = DisplayDate(u.Membership.CreatedDate),
                    LastLoginDate = DisplayDate(u.Membership.LastLoginDate),
                    LastActivityDate = DisplayDate(u.Membership.LastActivityDate)
                }).ToList();

                var response = new
                {
                    Success = true,
                    Results = new
                    {
                        Activities = users
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

        /// GET: api/Security/SearchFileSystemAndDatabase
        /// <summary>
        /// Searchs file system and database
        /// </summary>
        /// <returns>Searchs file system and database</returns>
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
                        FoundInDatabase = foundindb
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

        /// GET: api/Security/GetLastModifiedFiles
        /// <summary>
        /// Gets recently modified files
        /// </summary>
        /// <returns>last modified files</returns>
        [HttpGet]
        [RequireHost]
        public HttpResponseMessage GetLastModifiedFiles()
        {
            try
            {
                var highRiskFiles = Components.Utility.GetLastModifiedExecutableFiles().Select(f => new
                {
                    FilePath = GetFilePath(f.FullName),
                    LastWriteTime = DisplayDate(f.LastWriteTime)
                });
                var lowRiskFiles = Components.Utility.GetLastModifiedFiles().Select(f => new
                {
                    FilePath = GetFilePath(f.FullName),
                    LastWriteTime = DisplayDate(f.LastWriteTime)
                });
                var response = new
                {
                    Success = true,
                    Results = new
                    {
                        HighRiskFiles = highRiskFiles,
                        LowRiskFiles = lowRiskFiles
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

        /// GET: api/Security/GetRecentlyModifiedSettings
        /// <summary>
        /// Gets last modified settings
        /// </summary>
        /// <returns>last modified settings</returns>
        [HttpGet]
        [RequireHost]
        public HttpResponseMessage GetLastModifiedSettings()
        {
            try
            {
                var settings = _controller.GetModifiedSettings();
                var portalSettings = (from DataRow dr in settings[0].Rows
                                      select new SettingsDto
                                      {
                                          PortalId = Convert.ToInt32(dr["PortalID"] != DBNull.Value ? dr["PortalID"] : Null.NullInteger),
                                          SettingName = Convert.ToString(dr["SettingName"]),
                                          SettingValue = Convert.ToString(dr["SettingValue"]),
                                          LastModifiedByUserId = Convert.ToInt32(dr["LastModifiedByUserID"]),
                                          LastModifiedOnDate = DisplayDate(Convert.ToDateTime(dr["LastModifiedOnDate"]))
                                      }).ToList();

                var hostSettings = (from DataRow dr in settings[1].Rows
                                    select new SettingsDto
                                    {
                                        SettingName = Convert.ToString(dr["SettingName"]),
                                        SettingValue = Convert.ToString(dr["SettingValue"]),
                                        LastModifiedByUserId = Convert.ToInt32(dr["LastModifiedByUserID"]),
                                        LastModifiedOnDate = DisplayDate(Convert.ToDateTime(dr["LastModifiedOnDate"]))
                                    }).ToList();

                var tabSettings = (from DataRow dr in settings[2].Rows
                                   select new SettingsDto
                                   {
                                       TabId = Convert.ToInt32(dr["TabID"]),
                                       PortalId = Convert.ToInt32(dr["PortalID"] != DBNull.Value ? dr["PortalID"] : Null.NullInteger),
                                       SettingName = Convert.ToString(dr["SettingName"]),
                                       SettingValue = Convert.ToString(dr["SettingValue"]),
                                       LastModifiedByUserId = Convert.ToInt32(dr["LastModifiedByUserID"]),
                                       LastModifiedOnDate = DisplayDate(Convert.ToDateTime(dr["LastModifiedOnDate"]))
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
                                          LastModifiedOnDate = DisplayDate(Convert.ToDateTime(dr["LastModifiedOnDate"]))
                                      }).ToList();

                var response = new
                {
                    Success = true,
                    Results = new
                    {
                        PortalSettings = portalSettings,
                        HostSettings = hostSettings,
                        TabSettings = tabSettings,
                        ModuleSettings = moduleSettings
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

        #endregion

        #region Helpers

        internal string AddPortalAlias(string portalAlias, int portalId)
        {
            if (!String.IsNullOrEmpty(portalAlias))
            {
                portalAlias = portalAlias.ToLowerInvariant().Trim('/');
                if (portalAlias.IndexOf("://", StringComparison.Ordinal) != -1)
                {
                    portalAlias = portalAlias.Remove(0, portalAlias.IndexOf("://", StringComparison.Ordinal) + 3);
                }
                var alias = _portalAliasController.GetPortalAlias(portalAlias, portalId);
                if (alias == null)
                {
                    alias = new PortalAliasInfo { PortalID = portalId, HTTPAlias = portalAlias };
                    _portalAliasController.AddPortalAlias(alias);
                }
            }
            return portalAlias;
        }

        private string DisplayDate(DateTime userDate)
        {
            var date = Null.NullString;
            date = !Null.IsNull(userDate) ? userDate.ToString(CultureInfo.InvariantCulture) : "";
            return date;
        }

        private string GetFilePath(string filePath)
        {
            var path = Regex.Replace(filePath, Regex.Escape(Globals.ApplicationMapPath), string.Empty, RegexOptions.IgnoreCase);
            return path.TrimStart('\\');
        }

        #endregion
    }
}
