﻿// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Tests.Integration.Executers;
using DotNetNuke.Tests.Integration.Executers.Builders;
using DotNetNuke.Tests.Integration.Executers.Dto;
using DNN.Integration.Test.Framework;
using DNN.Integration.Test.Framework.Controllers;
using DNN.Integration.Test.Framework.Helpers;
using NUnit.Framework;

namespace DotNetNuke.Tests.Integration.Modules.DDRMenu
{
    [TestFixture]
    public class DDRMenuTests : IntegrationTestBase
    {
        #region Fields

        private readonly string _hostName;
        private readonly string _hostPass;

        #endregion

        #region SetUp

        public DDRMenuTests()
        {
            var url = ConfigurationManager.AppSettings["siteUrl"];
            _hostName = ConfigurationManager.AppSettings["hostUsername"];
            _hostPass = ConfigurationManager.AppSettings["hostPassword"];
        }

        [TestFixtureSetUp]
        public override void TestFixtureSetUp()
        {
            base.TestFixtureSetUp();
        }

        #endregion

        #region Tests

        [Test]
        public void Page_Should_Able_To_Duplicate_With_Ddr_Menu_On_It()
        {
            //Create new page with DDR Menu on it
            int tabId;
            CreateNewPage(Null.NullInteger, out tabId);
            int moduleId;
            AddModuleToPage(tabId, "DDRMenu", out moduleId);

            //apply module settings.
            ModuleController.SetModuleSettingValue(moduleId, "MenuStyle", "Menus/MainMenu");

            //Copy Page
            int copyTabId;
            CreateNewPage(tabId, out copyTabId);
        }

        

        #endregion

        #region Private Methods

        private IWebApiConnector CreateNewPage(int templateTabId, out int tabId)
        {
            var pagesExecuter = new PagesExecuter { Connector = WebApiTestHelper.LoginHost() };

            var pageSettingsBuilder = new PageSettingsBuilder();
            pageSettingsBuilder.WithPermission(new TabPermissionsBuilder().Build());

            if (templateTabId > 0)
            {
                pageSettingsBuilder.WithTemplateTabId(templateTabId);

                var modules = DatabaseHelper.ExecuteQuery<CopyModuleItem>($"SELECT ModuleId, ModuleTitle FROM {{objectQualifier}}TabModules WHERE TabId = {templateTabId}");
                pageSettingsBuilder.WithCopyModules(modules.ToList());
            }

            var pageDetail = pagesExecuter.SavePageDetails(pageSettingsBuilder.Build());

            Assert.NotNull(pageDetail.Page, "The system must create the page and return its details in the response");

            tabId = (int)pageDetail.Page.id;

            return pagesExecuter.Connector;
        }

        private IWebApiConnector AddModuleToPage(int tabId, string moduleName, out int moduleId)
        {
            var connector = WebApiTestHelper.LoginHost();

            var desktopModuleId = DatabaseHelper.ExecuteScalar<int>($"SELECT DesktopModuleId FROM {{objectQualifier}}DesktopModules WHERE ModuleName = '{moduleName}'");
            var postData = new
            {
                Visibility = 0,
                Position = -1,
                Module = desktopModuleId,
                Pane = "ContentPane",
                AddExistingModule = false,
                CopyModule = false,
                Sort = -1
            };
            var headers = new Dictionary<string, string> { { "TabId", tabId.ToString() } };
            var response = connector.PostJson("API/internalservices/controlbar/AddModule", postData, headers)
                .Content.ReadAsStringAsync().Result;
            moduleId = Json.Deserialize<dynamic>(response).TabModuleID;

            return connector;
        }

        #endregion
    }
}
