// Copyright (c) DNN Software. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Web.Caching;
using DotNetNuke.ComponentModel.DataAnnotations;

namespace Dnn.DynamicContent
{
    [Serializable]
    [TableName("ContentTypes_DataTypes")]
    [PrimaryKey("DataTypeID", "DataTypeId")]
    [Cacheable(DataTypeManager.DataTypeCacheKey, CacheItemPriority.Normal, 20)]
    public class DataType
    {
        public DataType()
        {
            UnderlyingDataType = UnderlyingDataType.String;
        }

        public int DataTypeId { get; set; }

        public string Name { get; set; }

        public UnderlyingDataType UnderlyingDataType { get; set; }
    }
}
