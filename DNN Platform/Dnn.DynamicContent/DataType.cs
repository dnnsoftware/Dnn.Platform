// Copyright (c) DNN Software. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Web.Caching;
using DotNetNuke.ComponentModel.DataAnnotations;

// ReSharper disable ConvertPropertyToExpressionBody

namespace Dnn.DynamicContent
{
    [Serializable]
    [TableName("ContentTypes_DataTypes")]
    [PrimaryKey("DataTypeID", "DataTypeId")]
    [Cacheable(DataTypeManager.DataTypeCacheKey, CacheItemPriority.Normal, 20)]
    [Scope(DataTypeManager.PortalScope)]
    public class DataType : BaseEntity
    {
        public DataType() : this(-1) { }

        public DataType(int portalId) : base()
        {
            UnderlyingDataType = UnderlyingDataType.String;
            PortalId = portalId;
        }

        public int DataTypeId { get; set; }

        [IgnoreColumn]
        public bool IsSystem { get { return (PortalId == -1); } }

        public string Name { get; set; }

        public int PortalId { get; set; }

        public UnderlyingDataType UnderlyingDataType { get; set; }
    }
}
