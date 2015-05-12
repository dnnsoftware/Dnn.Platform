// Copyright (c) DNN Software. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
using Dnn.DynamicContent.Exceptions;
using DotNetNuke.Collections;
using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Data;
using DotNetNuke.Entities.Content;
using DotNetNuke.Entities.Users;

namespace Dnn.DynamicContent
{
    public class DataTypeManager : ControllerBase<DataType, IDataTypeManager, DataTypeManager>, IDataTypeManager
    {
        internal const string FindWhereDataTypeSql = "WHERE DataTypeId = @0";
        internal const string DataTypeCacheKey = "ContentTypes_DataTypes";
        internal const string PortalScope = "PortalId";

        protected override Func<IDataTypeManager> GetFactory()
        {
            return () => new DataTypeManager();
        }

        public DataTypeManager() : this(DotNetNuke.Data.DataContext.Instance()) { }

        public DataTypeManager(IDataContext dataContext) : base(dataContext) { }

        /// <summary>
        /// Adds a new data type for use with Structured(Dynamic) Content Types.
        /// </summary>
        /// <param name="dataType">The data type to add.</param>
        /// <returns>data type id.</returns>
        /// <exception cref="System.ArgumentNullException">data type is null.</exception>
        /// <exception cref="System.ArgumentException">dataType.Name is empty.</exception>
        public int AddDataType(DataType dataType)
        {
            //Argument Contract
            Requires.PropertyNotNullOrEmpty(dataType, "Name");

            dataType.CreatedByUserId = UserController.Instance.GetCurrentUserInfo().UserID;
            dataType.CreatedOnDate = DateUtils.GetDatabaseTime();

            Add(dataType);

            return dataType.DataTypeId;
        }

        /// <summary>
        /// Deletes the data type for use with Structured(Dynamic) Content Types.
        /// </summary>
        /// <param name="dataType">The data type to delete.</param>
        /// <exception cref="System.ArgumentNullException">data type is null.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">data type id is less than 0.</exception>
        public void DeleteDataType(DataType dataType)
        {
            //Argument Contract
            Requires.NotNull(dataType);
            Requires.PropertyNotNull(dataType, "DataTypeId");
            Requires.PropertyNotNegative(dataType, "DataTypeId");

            using (DataContext)
            {
                var fieldDefinitionRepository = DataContext.GetRepository<FieldDefinition>();

                var definitions = fieldDefinitionRepository.Find(FindWhereDataTypeSql, dataType.DataTypeId);

                if (!definitions.Any())
                {
                    var dataTypeRepository = DataContext.GetRepository<DataType>();

                    dataTypeRepository.Delete(dataType);
                }
                else
                {
                    throw new DataTypeInUseException(dataType);
                }
            }
        }

        /// <summary>
        /// GetDataType retrieves a data type for a portal, optionally including system types
        /// </summary>
        /// <param name="dataTypeId">The Id of the dataType</param>
        /// <param name="portalId">The Id of the portal</param>
        /// <param name="includeSystem">A flag to determine if System Data Types (ie. Data Types that are available for all portals)
        /// should be returned. Defaults to false</param>
        /// <returns>data type</returns>
        //TODO add Unit Tests for this method
        public DataType GetDataType(int dataTypeId, int portalId, bool includeSystem = false)
        {
            DataType dataType = Get(portalId).SingleOrDefault(dt => dt.DataTypeId == dataTypeId);

            if (dataType == null && includeSystem)
            {
                dataType = Get(-1).SingleOrDefault(dt => dt.DataTypeId == dataTypeId);
            }
            return dataType;
        }

        /// <summary>
        /// This GetDataTypes overloads retrieves all the data types for a portal, optionally including system types
        /// </summary>
        /// <param name="portalId">The Id of the portal</param>
        /// <param name="includeSystem">A flag to determine if System Data Types (ie. Data Types that are available for all portals)
        /// should be returned. Defaults to false</param>
        /// <returns>data type collection.</returns>
        public IQueryable<DataType> GetDataTypes(int portalId, bool includeSystem = false)
        {
            List<DataType> dataTypes = Get(portalId).ToList();
            if (includeSystem)
            {
                dataTypes.AddRange(Get(-1));
            }
            return dataTypes.AsQueryable();
        }

        /// <summary>
        /// This GetDataTypes overload retrieves a page of data types for a given portal, based on a Search Term that can appear anywhere in the Name.
        /// </summary>
        /// <param name="searchTerm">The search term to use</param>
        /// <param name="portalId">The Id of the portal</param>
        /// <param name="pageIndex">The page number - 0 represents the first page</param>
        /// <param name="pageSize">The size of the page</param>
        /// <param name="includeSystem">A flag to determine if System Data Types (ie. Data Types that are available for all portals)
        /// should be returned. Defaults to false</param>
        /// <returns>a PagedList of DataTypes</returns>
        public IPagedList<DataType> GetDataTypes(string searchTerm, int portalId, int pageIndex, int pageSize, bool includeSystem = false)
        {
            var dataTypes = GetDataTypes(portalId, includeSystem);

            if (!String.IsNullOrEmpty(searchTerm))
            {
                dataTypes = dataTypes.Where(dt => dt.Name.ToLowerInvariant().Contains(searchTerm.ToLowerInvariant()));
            }

            return new PagedList<DataType>(dataTypes, pageIndex, pageSize);
        }

        /// <summary>
        /// Updates the data type.
        /// </summary>
        /// <param name="dataType">The data type.</param>
        /// <param name="overrideWarning">An optional flag that allows developers to update DataTypes even if they are in use</param>
        /// <exception cref="System.ArgumentNullException">data type is null.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">data type id is less than 0.</exception>
        /// <exception cref="System.ArgumentException">dataType.Name is empty.</exception>
        public void UpdateDataType(DataType dataType, bool overrideWarning = false)
        {
            //Argument Contract
            Requires.NotNull(dataType);
            Requires.PropertyNotNull(dataType, "DataTypeId");
            Requires.PropertyNotNegative(dataType, "DataTypeId");
            Requires.PropertyNotNullOrEmpty(dataType, "Name");

            dataType.LastModifiedByUserId = UserController.Instance.GetCurrentUserInfo().UserID;
            dataType.LastModifiedOnDate = DateUtils.GetDatabaseTime();

            using (DataContext)
            {
                var canUpdate = true;

                if (!overrideWarning)
                {
                    var fieldDefinitionRepository = DataContext.GetRepository<FieldDefinition>();

                    var definitions = fieldDefinitionRepository.Find(FindWhereDataTypeSql, dataType.DataTypeId);

                    // ReSharper disable once LoopCanBeConvertedToQuery
                    foreach (var definition in definitions)
                    {
                        var contentItems = ContentController.Instance.GetContentItemsByContentType(definition.ContentTypeId);

                        if (contentItems.Any())
                        {
                            canUpdate = false;
                            break;
                        }
                    }
                }

                if (canUpdate)
                {
                    var rep = DataContext.GetRepository<DataType>();

                    rep.Update(dataType);
                }
                else
                {
                    throw new DataTypeInUseException(dataType);
                }
            }
        }
    }
}
