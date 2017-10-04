// DotNetNuke® - http://www.dnnsoftware.com
//
// Copyright (c) 2002-2017, DNN Corp.
// All rights reserved.

using System.Collections.Generic;
using DotNetNuke.Tests.Integration.Framework.Controllers;

namespace DotNetNuke.Tests.Integration.Framework.Helpers
{
    public static class EvoqWebApiTestHelper
    {
        /// <summary>
        /// Provides a list of Connectors just for Content - LoginContentManager and LoginContentEditor
        /// </summary>
        public static IList<IWebApiConnector> ContentLogins()
        {
            return new List<IWebApiConnector> {LoginContentManager(), LoginContentEditor()};
        }

        /// <summary>
        /// Provides a list of Connectors for Managers - LoginContentManager and LoginCommunityManager
        /// </summary>
        public static IList<IWebApiConnector> ManagerLogins()
        {
            return SolutionController.IsSocial()
                ? new List<IWebApiConnector> {LoginContentManager(), LoginCommunityManager()}
                : new List<IWebApiConnector> {LoginContentManager()};
        }

        /// <summary>
        /// Provides a list of Connectors for All the three roles - LoginContentManager, LoginContentEditor and LoginCommunityManager
        /// </summary>
        public static IList<IWebApiConnector> AllLogins()
        {
            return SolutionController.IsSocial()
                ? new List<IWebApiConnector> {LoginContentManager(), LoginCommunityManager(), LoginContentManager()}
                : new List<IWebApiConnector> {LoginContentManager(), LoginContentManager()};
        }

        /// <summary>
        /// Provides a list of Connectors for AnonymousUsers and Registered Users
        /// </summary>
        /// <remarks>This list should be used for doing security tests on API that are available for more privileged users</remarks>
        public static IList<IWebApiConnector> AnonymousAndRuLogins()
        {
            return new List<IWebApiConnector> { LoginRegisteredUser(), WebApiTestHelper.GetAnnonymousConnector() };
        }

        /// <summary>
        /// Creates a Comunity Manager and Logs that user in as well. 
        /// Password used is same as that for Host. Existing user is used if it's already present.
        /// </summary>
        /// <returns>IWebApiConnector object to perform more actions</returns>
        public static IWebApiConnector LoginCommunityManager(string firstName = EvoqConstants.CmxFirstName, string lastName = EvoqConstants.CmxLastName, string url = null)
        {
            UserController.CreateCommunityManagerUser(firstName, lastName);
            return WebApiTestHelper.LoginRegisteredUser(firstName, lastName);
        }

        /// <summary>
        /// Creates acommunity manager and returns it's UserId. If user already exists, return's its UserId.
        /// </summary>
        public static int CreateCommunityManager(string firstName = EvoqConstants.CmxFirstName, string lastName = EvoqConstants.CmxLastName, string url = null)
        {
            return UserController.CreateCommunityManagerUser(firstName, lastName);
        }

        /// <summary>
        /// Creates a Content Manager and Logs that user in as well. 
        /// Password used is same as that for Host. Existing user is used if it's already present.
        /// </summary>
        /// <returns>IWebApiConnector object to perform more actions</returns>
        public static IWebApiConnector LoginContentManager(string firstName = EvoqConstants.ConMgrFirstName, string lastName = EvoqConstants.ConMgrLastName, string url = null)
        {
            UserController.CreateContentManagerUser(firstName, lastName);
            return WebApiTestHelper.LoginUser(firstName + "." + lastName);
        }

        /// <summary>
        /// Creates a content managerr and returns it's UserId. If user already exists, return's its UserId.
        /// </summary>
        public static int CreateContentManager(string firstName = EvoqConstants.ConMgrFirstName, string lastName = EvoqConstants.ConMgrLastName, string url = null)
        {
            return UserController.CreateContentManagerUser(firstName, lastName);
        }

        public static IWebApiConnector LoginAdministrator(string firstName = EvoqConstants.AdministratorFirstName, string lastName = EvoqConstants.AdministratorLastName, string url = null)
        {
            UserController.CreateAdministratorUser(firstName, lastName);
            return WebApiTestHelper.LoginUser(firstName + "." + lastName);
        }

        public static IWebApiConnector LoginHost()
        {
            return WebApiTestHelper.LoginUser(AppConfigHelper.HostUserName);
        }

        public static IWebApiConnector LoginRegisteredUser(string firstName = IntegrationConstants.RuFirstName, string lastName = IntegrationConstants.RuLastName, string url = null)
        {
            UserController.CreateRegisteredUser(firstName, lastName);
            return WebApiTestHelper.LoginRegisteredUser(firstName, lastName);
        }

        public static int CreateAdministrator(string firstName = EvoqConstants.AdministratorFirstName, string lastName = EvoqConstants.AdministratorLastName, string url = null)
        {
            return UserController.CreateAdministratorUser(firstName, lastName);
        }

        /// <summary>
        /// Creates a Content Editor and Logs that user in as well. 
        /// Password used is same as that for Host. Existing user is used if it's already present.
        /// </summary>
        /// <returns>IWebApiConnector object to perform more actions</returns>
        public static IWebApiConnector LoginContentEditor(string firstName = EvoqConstants.ConEdtFirstName, string lastName = EvoqConstants.ConEdtLastName, string url = null)
        {
            UserController.CreateContentEditorUser(firstName, lastName);
            return WebApiTestHelper.LoginUser(firstName + "." + lastName);
        }

        /// <summary>
        /// Creates a content editor and returns it's UserId. If user already exists, return's its UserId.
        /// </summary>
        public static int CreateContentEditor(string firstName = EvoqConstants.ConEdtFirstName, string lastName = EvoqConstants.ConEdtLastName, string url = null)
        {
            return UserController.CreateContentEditorUser(firstName, lastName);
        }

        public static int CreateRegisteredUser(string firstName = IntegrationConstants.RuFirstName, string lastName = IntegrationConstants.RuLastName, string url = null)
        {
            return UserController.CreateRegisteredUser(firstName, lastName);
        }
    }
}
