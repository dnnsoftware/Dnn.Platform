using System;
using Dnn.PersonaBar.Library.Prompt;
using DotNetNuke.Entities.Portals;
using Dnn.PersonaBar.Library.Prompt.Models;
using DotNetNuke.Entities.Users;

namespace Dnn.PersonaBar.Users.Tests
{
    public abstract class CommandTests
    {
        protected ConsoleCommandBase command;
        protected PortalSettings portalSettings;
        protected ConsoleErrorResultModel errorResultModel;

        protected int testPortalId = 0;

        protected abstract ConsoleCommandBase DependencyInitializer();

        protected void Reset()
        {
            errorResultModel = null;
            portalSettings = new PortalSettings();
            portalSettings.PortalId = testPortalId;
        }

        protected ConsoleCommandBase SetupCommand(params string[] args)
        {
            var command = DependencyInitializer();
            command.Initialize(args, portalSettings, null, -1);
            return command;
        }

        protected UserInfo GetUser(int userId, bool isDeleted)
        {
            UserInfo userInfo = new UserInfo();
            var profile = new UserProfile();
            profile.FirstName = "testUser";
            userInfo.UserID = userId;
            userInfo.Profile = profile;
            userInfo.IsDeleted = isDeleted;
            userInfo.PortalID = testPortalId;
            return userInfo;
        }
    }
}