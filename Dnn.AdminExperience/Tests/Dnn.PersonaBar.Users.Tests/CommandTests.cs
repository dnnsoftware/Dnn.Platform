// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Users.Tests
{
    using System.Linq;

    using Dnn.PersonaBar.Library.Prompt;
    using Dnn.PersonaBar.Library.Prompt.Models;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;
    using NUnit.Framework;

    public abstract class CommandTests<T>
        where T : ConsoleCommandBase
    {
        protected abstract string CommandName { get; }

        protected int testPortalId { get; set; }

        protected PortalSettings portalSettings { get; set; }

        protected ConsoleErrorResultModel errorResultModel { get; set; }

        protected abstract T CreateCommand();

        protected abstract void ChildSetup();

        [SetUp]
        protected void Setup()
        {
            this.ChildSetup();
            this.errorResultModel = null;
            this.portalSettings = new PortalSettings();
            this.portalSettings.PortalId = this.testPortalId;
            this.testPortalId = 0;
        }

        protected UserInfo GetUser(int userId, bool isDeleted)
        {
            var userInfo = new UserInfo();
            var profile = new UserProfile();
            profile.FirstName = "testUser";
            userInfo.UserID = userId;
            userInfo.Profile = profile;
            userInfo.IsDeleted = isDeleted;
            userInfo.PortalID = this.testPortalId;
            return userInfo;
        }

        protected ConsoleResultModel RunCommand(params string[] args)
        {
            var command = this.CreateCommand();
            command.Initialize(new[] { this.CommandName }.Concat(args).ToArray(), this.portalSettings, null, -1);
            return command.Run();
        }
    }
}
