// Copyright (c) DNN Software. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Linq;

namespace Dnn.DynamicContent
{
    public interface IDataTypeManager
    {
        int AddDataType(DataType dataType);

        void DeleteDataType(DataType dataType);

        IQueryable<DataType> GetDataTypes();

        void UpdateDataType(DataType dataType, bool overrideWarning = false);
    }
}
