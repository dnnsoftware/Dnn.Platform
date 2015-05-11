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

        IQueryable<DataType> GetDataTypes(int portalId, bool includeSystem = false);

        IPagedList<DataType> GetDataTypes(string searchTerm, int portalId, int pageIndex, int pageSize, bool includeSystem = false);

        void UpdateDataType(DataType dataType, bool overrideWarning = false);
    }
}
