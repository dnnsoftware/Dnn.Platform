#region Copyright
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2016
// by DotNetNuke Corporation
// All Rights Reserved
#endregion

using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using Dnn.PersonaBar.Library;
using Dnn.PersonaBar.Library.Attributes;
using Dnn.PersonaBar.Users.Components;
using Dnn.PersonaBar.Users.Components.Contracts;
using Dnn.PersonaBar.Users.Components.Dto;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Host;
using DotNetNuke.Entities.Profile;
using DotNetNuke.Entities.Users;
using DotNetNuke.Entities.Users.Social;
using DotNetNuke.Instrumentation;
using DotNetNuke.Security;
using DotNetNuke.Security.Membership;
using DotNetNuke.Services.Localization;
using DotNetNuke.Services.Log.EventLog;
using DotNetNuke.Services.Mail;
using DotNetNuke.Web.Api;

namespace Dnn.PersonaBar.Users.Services
{
    [ServiceScope(Scope = ServiceScope.Regular, Identifier = "Users")]
    public class UsersController : PersonaBarApiController
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(UsersController));

        #region Users API

        /// <summary>
        /// Create a User.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage CreateUser(CreateUserContract contract)
        {
            try
            {
                var settings = new RegisterationDetails
                {
                    PortalSettings = PortalSettings,
                    Email = contract.Email,
                    FirstName = contract.FirstName,
                    LastName = contract.LastName,
                    UserName = contract.UserName,
                    Password = contract.Password,

                    // For Community Manager, he can always add user
                    // whatever registration mode is set
                    // Please check below issue for this property
                    // https://dnntracker.atlassian.net/browse/SOCIAL-3158
                    IgnoreRegistrationMode = true
                };
                string message;
                var userInfo = RegisterController.Instance.Register(settings, out message);
                if (userInfo == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                                           string.Format(Localization.GetString("RegisterationFailed", Constants.SharedResources), message));
                }

                return Request.CreateResponse(HttpStatusCode.OK, userInfo.UserID);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        /// <summary>
        /// Perform a search on Users registered in the Site.
        /// </summary>
        /// <param name="searchText">Search filter text (if any)</param>
        /// <param name="pageIndex">Page index to begin from (0, 1, 2)</param>
        /// <param name="pageSize">Number of records to return per page</param>
        /// <param name="sortColumn">Column to sort on</param>
        /// <param name="sortAscending">Sort ascending or descending</param>
        [HttpGet]
        public HttpResponseMessage GetUsers(string searchText, int pageIndex, int pageSize,
                                                        string sortColumn,
                                                        bool sortAscending)
        {
            try
            {
                int totalRecords;
                var getUsersContract = new GetUsersContract
                {
                    SearchText = searchText,
                    PageIndex = pageIndex,
                    PageSize = pageSize,
                    SortColumn = sortColumn,
                    SortAscending = sortAscending,
                    PortalId = PortalSettings.PortalId
                };

                var results = Components.UsersController.Instance.GetUsers(getUsersContract, out totalRecords);
                var response = new
                {
                    Success = true,
                    Results = results,
                    TotalResults = totalRecords
                };

                return Request.CreateResponse(HttpStatusCode.OK, response);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        /// <summary>
        /// Get User Detail Info.
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        [HttpGet]
        public HttpResponseMessage GetUserDetail(int userId)
        {
            try
            {
                var userDetail = Components.UsersController.Instance.GetUserDetail(PortalId, userId);
                if (userDetail == null)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.NotFound, "UserNotFound");
                }

                return Request.CreateResponse(HttpStatusCode.OK, userDetail);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpGet]
        public HttpResponseMessage ChangePasswordAvailable()
        {
            return Request.CreateResponse(HttpStatusCode.OK, new { Enabled = !MembershipProviderConfig.RequiresQuestionAndAnswer });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage ChangePassword(ChangePasswordDto changePasswordDto)
        {
            try
            {
                string errorMessage;
                var userId = changePasswordDto.UserId;
                var password = changePasswordDto.Password;
                var controller = Components.UsersController.Instance;
                var passwordChanged = controller.ChangePassword(PortalId, userId, password, out errorMessage);

                return Request.CreateResponse(HttpStatusCode.OK, new { Success = passwordChanged, Message = errorMessage});
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage ForceChangePassword([FromUri]int userId)
        {
            try
            {
                var user = UserController.Instance.GetUserById(PortalId, userId);
                if (user == null)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.NotFound, "UserNotFound");
                }

                if (MembershipProviderConfig.PasswordRetrievalEnabled || MembershipProviderConfig.PasswordResetEnabled)
                {
                    UserController.ResetPasswordToken(user);
                }
                bool canSend = Mail.SendMail(user, MessageType.PasswordReminder, PortalSettings) == string.Empty;
                if (canSend)
                {
                    user.Membership.UpdatePassword = true;

                    //Update User
                    UserController.UpdateUser(PortalId, user);
                    return Request.CreateResponse(HttpStatusCode.OK, new { Success = true });
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { Success = false, Message = "OptionUnavailable" });
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage SendPasswordResetLink([FromUri]int userId)
        {
            try
            {
                var user = UserController.Instance.GetUserById(PortalId, userId);
                if (user == null)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.NotFound, "UserNotFound");
                }

                string errorMessage = string.Empty;
                if (MembershipProviderConfig.RequiresQuestionAndAnswer)
                {
                    errorMessage = "OptionUnavailable";
                }
                else
                {
                    try
                    {
                        //create resettoken
                        UserController.ResetPasswordToken(user, Host.AdminMembershipResetLinkValidity);

                        bool canSend = Mail.SendMail(user, MessageType.PasswordReminder, PortalSettings) == string.Empty;
                        if (!canSend)
                        {
                            errorMessage = "OptionUnavailable";
                        }
                    }
                    catch (ArgumentException exc)
                    {
                        Logger.Error(exc);
                        errorMessage = "InvalidPasswordAnswer";
                    }
                    catch (Exception exc)
                    {
                        Logger.Error(exc);
                        errorMessage = "PasswordResetFailed";
                    }
                }

                return Request.CreateResponse(HttpStatusCode.OK, new
                    {
                        Success = string.IsNullOrEmpty(errorMessage),
                        Message = errorMessage
                    });
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage UpdateAuthorizeStatus([FromUri]int userId, [FromUri]bool authorized)
        {
            try
            {
                var user = UserController.Instance.GetUserById(PortalId, userId);
                if (user == null)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.NotFound, "UserNotFound");
                }

                user.Membership.Approved = authorized;

                //Update User
                UserController.UpdateUser(PortalId, user);

                return Request.CreateResponse(HttpStatusCode.OK, new { Success = true });
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage DeleteUser([FromUri]int userId)
        {
            try
            {
                var user = UserController.Instance.GetUserById(PortalId, userId);
                if (user == null)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.NotFound, "UserNotFound");
                }

                var errorMessage = string.Empty;
                var deleted = UserController.DeleteUser(ref user, true, false);
                if (!deleted)
                {
                    errorMessage = "UserDeleteError";
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { Success = deleted, Message = errorMessage });
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [DnnAuthorize(StaticRoles = "Administrators")]
        public HttpResponseMessage LoginAsUser([FromUri]int userId)
        {
            try
            {
                var user = UserController.Instance.GetUserById(PortalId, userId);
                if (user == null)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.NotFound, "UserNotFound");
                }

                EventLogController.Instance.AddLog("Username", user.Username, PortalSettings, user.UserID, EventLogController.EventLogType.USER_IMPERSONATED);

                //Remove user from cache
                DataCache.ClearUserCache(PortalId, UserInfo.Username);

                new PortalSecurity().SignOut();

                UserController.UserLogin(user.PortalID, user, PortalSettings.PortalName, HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"], false);

                return Request.CreateResponse(HttpStatusCode.OK, new { Success = true });
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequireHost]
        public HttpResponseMessage UpdateSuperUserStatus([FromUri]int userId, [FromUri]bool setSuperUser)
        {
            try
            {
                var user = UserController.Instance.GetUserById(PortalId, userId);
                if (user == null)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.NotFound, "UserNotFound");
                }

                user.IsSuperUser = setSuperUser;

                //Update User
                UserController.UpdateUser(PortalId, user);
                DataCache.ClearCache();

                return Request.CreateResponse(HttpStatusCode.OK, new { Success = true });
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        #endregion

        #region Profiles API

        [HttpGet]
        public HttpResponseMessage GetRelationships()
        {
            try
            {
                var relationships = RelationshipController.Instance.GetRelationshipsByPortalId(PortalId);

                var relationshipTypes = RelationshipController.Instance.GetAllRelationshipTypes();

                return Request.CreateResponse(HttpStatusCode.OK, new { relationships, relationshipTypes });
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
            
        }

        [HttpGet]
        public HttpResponseMessage GetProfileDefinitions()
        {
            var profileDefinitions = ProfileController.GetPropertyDefinitionsByPortal(PortalId)
                .Cast<ProfilePropertyDefinition>().Select(d => new ProfileDefinitionDto(d));

            return Request.CreateResponse(HttpStatusCode.OK, profileDefinitions);
        }

        #endregion
    }
}
