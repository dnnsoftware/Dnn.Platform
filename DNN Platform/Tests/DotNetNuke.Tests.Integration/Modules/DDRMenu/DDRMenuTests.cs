#region Copyright
//
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2017
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
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Tests.Integration.Executers;
using DotNetNuke.Tests.Integration.Executers.Builders;
using DotNetNuke.Tests.Integration.Executers.Dto;
using DotNetNuke.Tests.Integration.Framework;
using DotNetNuke.Tests.Integration.Framework.Controllers;
using DotNetNuke.Tests.Integration.Framework.Helpers;
using DotNetNuke.Web.Api;
using NUnit.Framework;

namespace DotNetNuke.Tests.Integration.Modules.DDRMenu
{
    [TestFixture]
    public class DDRMenuTests : IntegrationTestBase
    {
        #region Fields

        private readonly string _hostName;
        private readonly string _hostPass;

        private readonly int PortalId = 0;

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
