// Copyright (c) DNN Software. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Linq;
using DotNetNuke.Collections;

namespace Dnn.DynamicContent
{
    public interface IDataTypeManager
    {
        int AddDataType(DataType dataType);

        void DeleteDataType(DataType dataType);

        /// <summary>
        /// GetDataType retrieves a data type for a portal, optionally including system types
        /// </summary>
        /// <param name="dataTypeId">The Id of the dataType</param>
        /// <param name="portalId">The Id of the portal</param>
        /// <param name="includeSystem">A flag to determine if System Data Types (ie. Data Types that are available for all portals)
        /// should be returned. Defaults to false</param>
        /// <returns>data type</returns>
        DataType GetDataType(int dataTypeId, int portalId, bool includeSystem = false);

        /// <summary>
        /// This GetDataTypes overloads retrieves all the data types for a portal, optionally including system types
        /// </summary>
        /// <param name="portalId">The Id of the portal</param>
        /// <param name="includeSystem">A flag to determine if System Data Types (ie. Data Types that are available for all portals)
        /// should be returned. Defaults to false</param>
        /// <returns>data type collection.</returns>
        IQueryable<DataType> GetDataTypes(int portalId, bool includeSystem = false);

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
        IPagedList<DataType> GetDataTypes(string searchTerm, int portalId, int pageIndex, int pageSize, bool includeSystem = false);

        /// <summary>
        /// Updates the data type.
        /// </summary>
        /// <param name="dataType">The data type.</param>
        /// <param name="overrideWarning">An optional flag that allows developers to update DataTypes even if they are in use</param>
        /// <exception cref="System.ArgumentNullException">data type is null.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">data type id is less than 0.</exception>
        /// <exception cref="System.ArgumentException">dataType.Name is empty.</exception>
        void UpdateDataType(DataType dataType, bool overrideWarning = false);
    }
}
