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

using System;
using System.Collections.Generic;
using DotNetNuke.ComponentModel;
using DotNetNuke.Data;
using DotNetNuke.Data.PetaPoco;
using DotNetNuke.Entities.Controllers;
using DotNetNuke.Services.Cache;
using DotNetNuke.Services.Log.EventLog;
using DotNetNuke.Tests.Utilities.Mocks;
using Moq;
using DataUtil = DotNetNuke.Tests.Data.DataUtil;

namespace Dnn.Tests.DynamicContent.IntegrationTests
{
    public abstract class IntegrationTestBase
    {
        private const string CreateContentTypeTableSql = @"
            CREATE TABLE ContentTypes(
	            ContentTypeID int IDENTITY(1,1) NOT NULL,
	            ContentType nvarchar(100) NOT NULL,
	            PortalID int NOT NULL DEFAULT -1,
	            IsDynamic bit NOT NULL DEFAULT 0)";

        private const string CreateContentTemplateTableSql = @"
            CREATE TABLE ContentTypes_Templates(
			    [TemplateID] [int] IDENTITY(1,1) NOT NULL,
			    [ContentTypeID] [int] NOT NULL,
			    [Name] [nvarchar](100) NOT NULL,
			    [TemplateFileID] [int] NOT NULL
            )";

        private const string CreateValidatorTypeTableSql = @"
            CREATE TABLE ContentTypes_ValidatorTypes(
	            ValidatorTypeID int IDENTITY(1,1) NOT NULL,
	            Name nvarchar(100) NOT NULL,
	            ValidatorClassName nvarchar(1000) NOT NULL,
	            ErrorKey nvarchar(100) NULL,
	            ErrorMessage nvarchar(1000) NULL
            )";

        private const string CreateValidationRuleTableSql = @"
            CREATE TABLE ContentTypes_ValidationRules(
	            ValidationRuleID int IDENTITY(1,1) NOT NULL,
                FieldDefinitionID int NOT NULL,
                ValidatorTypeID int NOT NULL
            )";

        private const string CreateDataTypeTableSql = @"
            CREATE TABLE ContentTypes_DataTypes(
	            DataTypeID int IDENTITY(1,1) NOT NULL,
	            Name nvarchar(100) NOT NULL,
                UnderlyingDataType int NOT NULL
            )";

        private const string CreateFieldDefinitionTableSql = @"
            CREATE TABLE ContentTypes_FieldDefinitions(
	            FieldDefinitionID int IDENTITY(1,1) NOT NULL,
                ContentTypeID int NOT NULL,
                DataTypeID int NOT NULL,
	            Name nvarchar(100) NOT NULL,
	            Label nvarchar(100) NOT NULL,
	            Description nvarchar(2000) NULL
            )";

        private const string InsertValidationRuleSql = @"INSERT INTO ContentTypes_ValidationRules 
                                                            (FieldDefinitionID, ValidatorTypeID) 
                                                            VALUES ({0}, {1})";

        private const string InsertValidatorTypeSql = @"INSERT INTO ContentTypes_ValidatorTypes (Name, ValidatorClassName) 
                                                            VALUES ('{0}', '{1}')";

        private const string InsertFieldDefinitionSql = @"INSERT INTO ContentTypes_FieldDefinitions 
                                                            (ContentTypeID, DataTypeID, Name, Label, Description) 
                                                            VALUES ({0}, {1}, '{2}', '{3}', '{4}')";

        private const string InsertContentTypeSql = "INSERT INTO ContentTypes (ContentType, PortalID, IsDynamic) VALUES ('{0}',{1}, {2})";

        private const string InsertDataTypeSql = "INSERT INTO ContentTypes_DataTypes (Name, UnderlyingDataType) VALUES ('{0}', {1})";

        private const string InsertContentTemplateSql = @"INSERT INTO ContentTypes_Templates 
                                                            (ContentTypeID, Name, TemplateFileID) 
                                                            VALUES ({0}, '{1}', {2})";


        protected Mock<CachingProvider> MockCache;

        protected const string DatabaseName = "Test.sdf";
        protected const string ConnectionStringName = "PetaPoco";

        protected const int PortalId = 0;
        protected const int RecordCount = 10;

        protected void SetUpInternal()
        {
            ComponentFactory.Container = new SimpleContainer();
            ComponentFactory.RegisterComponentInstance<IDataContext>(new PetaPocoDataContext(ConnectionStringName));
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

            DataUtil.CreateDatabase(DatabaseName);
            DataUtil.ExecuteNonQuery(DatabaseName, CreateFieldDefinitionTableSql);
            DataUtil.ExecuteNonQuery(DatabaseName, CreateValidationRuleTableSql);
            DataUtil.ExecuteNonQuery(DatabaseName, CreateValidatorTypeTableSql);
            DataUtil.ExecuteNonQuery(DatabaseName, CreateContentTypeTableSql);
            DataUtil.ExecuteNonQuery(DatabaseName, CreateDataTypeTableSql);
            DataUtil.ExecuteNonQuery(DatabaseName, CreateContentTemplateTableSql);
        }

        public void TearDownInternal()
        {
            DataUtil.DeleteDatabase(DatabaseName);
            LogController.ClearInstance();
        }

        protected void SetUpContentTypes(int count)
        {
            for (int i = 0; i < count; i++)
            {
                int isStructured = 0;
                int portalId = -1;
                if (i % 2 == 0)
                {
                    isStructured = 1;
                    portalId = PortalId;
                }
                DataUtil.ExecuteNonQuery(DatabaseName, String.Format(InsertContentTypeSql, String.Format("Type_{0}", i), portalId, isStructured));
            }
        }

        protected void SetUpFieldDefinitions(int count)
        {
            for (int i = 0; i < count; i++)
            {
                DataUtil.ExecuteNonQuery(DatabaseName, string.Format(InsertFieldDefinitionSql, i, i, string.Format("Name_{0}", i), string.Format("Label_{0}", i), String.Format("Description_{0}", i)));
            }
        }

        protected void SetUpDataTypes(int count)
        {
            for (int i = 0; i < count; i++)
            {
                var dataType = i;
                if (dataType > 8)
                {
                    dataType = 0;
                }
                DataUtil.ExecuteNonQuery(DatabaseName, String.Format(InsertDataTypeSql, String.Format("Type_{0}", i), dataType));
            }
        }

        protected void SetUpValidationRules(int count)
        {
            for (int i = 0; i < count; i++)
            {
                DataUtil.ExecuteNonQuery(DatabaseName, string.Format(InsertValidationRuleSql, i, i));
            }
        }

        protected void SetUpValidatorTypes(int count)
        {
            for (int i = 0; i < count; i++)
            {
                DataUtil.ExecuteNonQuery(DatabaseName, String.Format(InsertValidatorTypeSql, String.Format("Type_{0}", i), String.Format("ClassName_{0}", i)));
            }
        }

        protected void SetUpContentTemplates(int count)
        {
            for (int i = 0; i < count; i++)
            {
                DataUtil.ExecuteNonQuery(DatabaseName, String.Format(InsertContentTemplateSql, i, String.Format("Type_{0}", i), i));
            }
        }
    }
}
