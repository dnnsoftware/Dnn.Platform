﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Users.Components.Prompt.Commands
{
    using System.Collections.Generic;
    using System.Linq;

    using Dnn.PersonaBar.Library.Prompt;
    using Dnn.PersonaBar.Library.Prompt.Attributes;
    using Dnn.PersonaBar.Library.Prompt.Models;
    using Dnn.PersonaBar.Users.Components.Prompt.Models;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;

    [ConsoleCommand("get-user", Constants.UsersCategory, "Prompt_GetUser_Description")]

    public class GetUser : ConsoleCommandBase
    {
        [FlagParameter("id", "Prompt_GetUser_FlagId", "Integer")]

        private const string FlagId = "id";

        [FlagParameter("email", "Prompt_GetUser_FlagEmail", "String")]

        private const string FlagEmail = "email";

        [FlagParameter("username", "Prompt_GetUser_FlagUsername", "String")]

        private const string FlagUsername = "username";

        private const int UserIdZero = 0;

        private IUserValidator userValidator;
        private IUserControllerWrapper userControllerWrapper;

        /// <summary>Initializes a new instance of the <see cref="GetUser"/> class.</summary>
        public GetUser()
            : this(new UserValidator(), new UserControllerWrapper())
        {
        }

        /// <summary>Initializes a new instance of the <see cref="GetUser"/> class.</summary>
        /// <param name="userValidator"></param>
        /// <param name="userControllerWrapper"></param>
        public GetUser(IUserValidator userValidator, IUserControllerWrapper userControllerWrapper)
        {
            this.userValidator = userValidator;
            this.userControllerWrapper = userControllerWrapper;
        }

        /// <inheritdoc/>
        public override string LocalResourceFile => Constants.LocalResourcesFile;

        private int? UserId { get; set; }

        private string Email { get; set; }

        private string Username { get; set; }

        /// <inheritdoc/>
        public override void Init(string[] args, PortalSettings portalSettings, UserInfo userInfo, int activeTabId)
        {
            this.UserId = this.GetFlagValue<int?>(FlagId, "User Id", null);
            this.Email = this.GetFlagValue(FlagEmail, "Email", string.Empty);
            this.Username = this.GetFlagValue(FlagUsername, "Username", string.Empty);

            if (args.Length != 1)
            {
                if (args.Length == 2 && !this.UserId.HasValue && string.IsNullOrEmpty(this.Email) && string.IsNullOrEmpty(this.Username))
                {
                    // only one value passed and it's not a flagged value. Try to interpret it.
                    if (args[1].Contains("@"))
                    {
                        this.Email = args[1];
                    }
                    else
                    {
                        int tmpId;
                        if (int.TryParse(args[1], out tmpId))
                        {
                            this.UserId = tmpId;
                        }
                        else
                        {
                            // assume it's a username
                            this.Username = args[1];
                        }
                    }
                }

                if (!this.UserId.HasValue && string.IsNullOrEmpty(this.Email) && string.IsNullOrEmpty(this.Username))
                {
                    this.AddMessage(this.LocalizeString("Prompt_SearchUserParameterRequired"));
                }
            }
        }

        /// <inheritdoc/>
        public override ConsoleResultModel Run()
        {
            var lst = new List<UserModel>();

            // if no argument, default to current user
            if (this.Args.Length == 1)
            {
                lst.Add(new UserModel(this.User));
            }
            else
            {
                var recCount = 0;
                var userId = this.UserId;
                if (!userId.HasValue && !string.IsNullOrEmpty(this.Username))
                {
                    // do username lookup
                    var searchTerm = this.Username.Replace("%", string.Empty).Replace("*", "%");

                    userId = this.userControllerWrapper.GetUsersByUserName(this.PortalId, searchTerm, -1, int.MaxValue, ref recCount, true, false) ?? UserIdZero;

                    // search against superusers if no regular user found
                    if (userId == UserIdZero)
                    {
                        // userId = (UserController.GetUsersByUserName(-1, searchTerm, -1, int.MaxValue, ref recCount, true, true).ToArray().FirstOrDefault() as UserInfo)?.UserID ?? UserIdZero;
                        userId = this.userControllerWrapper.GetUsersByUserName(-1, searchTerm, -1, int.MaxValue, ref recCount, true, true) ?? UserIdZero;
                    }
                }
                else if (!userId.HasValue && !string.IsNullOrEmpty(this.Email))
                {
                    // must be email
                    var searchTerm = this.Email.Replace("%", string.Empty).Replace("*", "%");

                    userId = this.userControllerWrapper.GetUsersByEmail(this.PortalId, searchTerm, -1, int.MaxValue, ref recCount, true, false) ?? UserIdZero;

                    // search against superusers if no regular user found
                    if (userId == UserIdZero)
                    {
                        userId = this.userControllerWrapper.GetUsersByEmail(-1, searchTerm, -1, int.MaxValue, ref recCount, true, true) ?? UserIdZero;
                    }
                }

                UserInfo userInfo;
                ConsoleErrorResultModel errorResultModel =
                    this.userValidator.ValidateUser(userId, this.PortalSettings, this.User, out userInfo);

                if (errorResultModel != null)
                {
                    return errorResultModel;
                }

                lst.Add(new UserModel(userInfo));
            }

            return new ConsoleResultModel(string.Empty)
            {
                Data = lst,
                Records = lst.Count,
                FieldOrder = new[]
                {
                    "UserId",
                    "Username",
                    "DisplayName",
                    "FirstName",
                    "LastName",
                    "Email",
                    "LastActivity",
                    "LastLogin",
                    "LastLockout",
                    "LastPasswordChange",
                    "IsDeleted",
                    "IsAuthorized",
                    "IsLockedOut",
                    "Created",
                },
            };
        }
    }
}
