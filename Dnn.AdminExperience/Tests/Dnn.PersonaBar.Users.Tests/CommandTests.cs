using System.Linq;
using NUnit.Framework;
using Dnn.PersonaBar.Library.Prompt;
using DotNetNuke.Entities.Portals;
using Dnn.PersonaBar.Library.Prompt.Models;
using DotNetNuke.Entities.Users;

namespace Dnn.PersonaBar.Users.Tests
{
    public abstract class CommandTests<T> where T : ConsoleCommandBase
    {
        protected abstract string CommandName { get; }
        protected abstract T CreateCommand();
        protected abstract void ChildSetup();        

        protected int testPortalId { get; set; }
        protected PortalSettings portalSettings { get; set; }
        protected ConsoleErrorResultModel errorResultModel { get; set; }        

        [SetUp]
        protected void Setup()
        {
            ChildSetup();
            errorResultModel = null;
            portalSettings = new PortalSettings();
            portalSettings.PortalId = testPortalId;
            testPortalId = 0;
        }

        protected UserInfo GetUser(int userId, bool isDeleted)
        {
            var userInfo = new UserInfo();
            var profile = new UserProfile();
            profile.FirstName = "testUser";
            userInfo.UserID = userId;
            userInfo.Profile = profile;
            userInfo.IsDeleted = isDeleted;
            userInfo.PortalID = testPortalId;
            return userInfo;
        }

        protected ConsoleResultModel RunCommand(params string[] args)
        {
            var command = CreateCommand();
            command.Initialize((new[] { CommandName }.Concat(args)).ToArray(), portalSettings, null, -1);
            return command.Run();
        }
    }
}