#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2014
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

using System.Collections.Generic;
using DotNetNuke.ComponentModel;
using DotNetNuke.Data;
using DotNetNuke.Entities.Controllers;
using DotNetNuke.Services.Cache;
using DotNetNuke.Services.Log.EventLog;
using DotNetNuke.Tests.Utilities.Mocks;
using Moq;
using DataUtil = DotNetNuke.Tests.Data.DataUtil;

namespace DotNetNuke.Tests.Content.Integration
{
    public abstract class IntegrationTestBase
    {
        protected Mock<CachingProvider> MockCache;

        protected const string DatabaseName = "Test.sdf";
        protected const string ConnectionStringName = "PetaPoco";

        protected const int PortalId = 0;
        protected const int RecordCount = 10;

        protected void SetUpInternal()
        {
            ComponentFactory.Container = new SimpleContainer();
            ComponentFactory.RegisterComponentInstance<DataProvider>(new SqlDataProvider());
            ComponentFactory.RegisterComponentSettings<SqlDataProvider>(new Dictionary<string, string>()
                                {
                                    {"name", "SqlDataProvider"},
                                    {"type", "DotNetNuke.Data.SqlDataProvider, DotNetNuke"},
                                    {"connectionStringName", "SiteSqlServer"},
                                    {"objectQualifier", ""},
                                    {"databaseOwner", "dbo."}
                                });

            MockCache = MockComponentProvider.CreateNew<CachingProvider>();

            var mockHostController = new Mock<IHostController>();
            mockHostController.Setup(c => c.GetString("PerformanceSetting")).Returns("3");
            HostController.RegisterInstance(mockHostController.Object);

            var mockLogController = new Mock<ILogController>();
            LogController.SetTestableInstance(mockLogController.Object);
        }

        public void TearDownInternal()
        {
            DataUtil.DeleteDatabase(DatabaseName);
            LogController.ClearInstance();
        }
    }
}
