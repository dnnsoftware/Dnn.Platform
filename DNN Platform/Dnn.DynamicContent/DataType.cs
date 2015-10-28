// Copyright (c) DNN Software. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Threading;
using System.Web.Caching;
using Dnn.DynamicContent.Localization;
using DotNetNuke.Common.Utilities;
using DotNetNuke.ComponentModel.DataAnnotations;

// ReSharper disable ConvertPropertyToExpressionBody

namespace Dnn.DynamicContent
{
    [Serializable]
    [TableName("ContentTypes_DataTypes")]
    [PrimaryKey("DataTypeID", "FieldTypeId")]
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

        /// <summary>
        /// True if the data type is defined to be available for all portals, false otherwise
        /// </summary>
        [IgnoreColumn]
        public bool IsSystem
        {
            get
            {
                return PortalId == Null.NullInteger;
            }
        }

        public string Name { get; set; }

        [IgnoreColumn]
        public string LocalizedName
        {
            get
            {
                var key = String.Format(DataTypeManager.NameKey, DataTypeId);
                var code = Thread.CurrentThread.CurrentUICulture.ToString();
                var localizedName = ContentTypeLocalizationManager.Instance.GetLocalizedValue(key, code, PortalId);
                return  (String.IsNullOrEmpty(LocalizedName)) ? Name : localizedName;
            }
        }

        public int PortalId { get; set; }

        public UnderlyingDataType UnderlyingDataType { get; set; }
    }
}
