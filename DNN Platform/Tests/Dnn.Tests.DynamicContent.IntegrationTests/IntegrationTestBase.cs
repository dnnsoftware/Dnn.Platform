// Copyright (c) DNN Software. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

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
        public int CreatedByUserId = 1;
        public int LastModifiedByUserId = 2;

        private const string CreateContentTypeTableSql = @"
            CREATE TABLE ContentTypes(
	            ContentTypeID int IDENTITY(1,1) NOT NULL,
	            ContentType nvarchar(100) NOT NULL,
	            PortalID int NOT NULL DEFAULT -1,
	            IsDynamic bit NOT NULL DEFAULT 0,
                CreatedByUserID int NOT NULL,
                CreatedOnDate datetime NOT NULL DEFAULT getdate(),
                LastModifiedByUserID int NOT NULL,
                LastModifiedOnDate datetime NOT NULL DEFAULT getdate()
            )";

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
                PortalID int NOT NULL,
	            Name nvarchar(100) NOT NULL,
                UnderlyingDataType int NOT NULL,
                CreatedByUserID int NOT NULL,
                CreatedOnDate datetime NOT NULL DEFAULT getdate(),
                LastModifiedByUserID int NOT NULL,
                LastModifiedOnDate datetime NOT NULL DEFAULT getdate()
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

        private const string InsertContentTypeSql = @"INSERT INTO ContentTypes 
                                                            (ContentType, PortalID, IsDynamic, CreatedByUserID, LastModifiedByUserID) 
                                                            VALUES ('{0}',{1}, {2}, {3}, {4})";

        private const string InsertDataTypeSql = @"INSERT INTO ContentTypes_DataTypes 
                                                            (PortalID, Name, UnderlyingDataType, CreatedByUserID, LastModifiedByUserID) 
                                                            VALUES ({0}, '{1}', {2}, {3}, {4})";

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
                int isDynamic = 0;
                int portalId = -1;
                if (i % 2 == 0)
                {
                    isDynamic = 1;
                    portalId = PortalId;
                }
                DataUtil.ExecuteNonQuery(DatabaseName, String.Format(InsertContentTypeSql, String.Format("Type_{0}", i), portalId, isDynamic, CreatedByUserId, LastModifiedByUserId));
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
                DataUtil.ExecuteNonQuery(DatabaseName, String.Format(InsertDataTypeSql, i, String.Format("Type_{0}", i), dataType, CreatedByUserId, LastModifiedByUserId));
            }
        }

        protected void SetUpDataTypes(int count, int portalId)
        {
            for (int i = 0; i < count; i++)
            {
                var dataType = i;
                if (dataType > 8)
                {
                    dataType = 0;
                }
                DataUtil.ExecuteNonQuery(DatabaseName, String.Format(InsertDataTypeSql, portalId, String.Format("Type_{0}", i), dataType, CreatedByUserId, LastModifiedByUserId));
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
