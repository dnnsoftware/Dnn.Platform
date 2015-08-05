// Copyright (c) DNN Software. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Caching;
using DotNetNuke.ComponentModel.DataAnnotations;

// ReSharper disable ConvertPropertyToExpressionBody

namespace Dnn.DynamicContent
{
    [Serializable]
    [TableName("ContentTypes_FieldDefinitions")]
    [PrimaryKey("FieldDefinitionID", "FieldDefinitionId")]
    [Cacheable(FieldDefinitionManager.FieldDefinitionCacheKey, CacheItemPriority.Normal, 20)]
    [Scope(FieldDefinitionManager.FieldDefinitionScope)]
    public class FieldDefinition
    {
        private DataType _dataType;
        private IList<ValidationRule> _validationRules;

        public FieldDefinition() : this(-1) { }

        public FieldDefinition(int portalId)
        {
            FieldDefinitionId = -1;
            ContentTypeId = -1;
            DataTypeId = -1;
            Name = String.Empty;
            Label = String.Empty;
            Description = String.Empty;
            PortalId = portalId;
        }

        public int ContentTypeId { get; set; }

        public int DataTypeId { get; set; }

        [IgnoreColumn]
        public DataType DataType
        {
            get
            {
                if (_dataType == null)
                {
                    _dataType = DataTypeManager.Instance.GetDataTypes(PortalId, true).SingleOrDefault(dt => dt.DataTypeId == DataTypeId);
                }
                return _dataType;
            }
        }

        public string Description { get; set; }

        public int FieldDefinitionId { get; set; }

        public string Label { get; set; }

        public string Name { get; set; }

        public int PortalId { get; set; }

        [IgnoreColumn]
        public IList<ValidationRule> ValidationRules
        {
            get
            {
                // ReSharper disable once ConvertIfStatementToNullCoalescingExpression
                if (_validationRules == null)
                {
                    _validationRules = (FieldDefinitionId == -1)
                                        ? new List<ValidationRule>() 
                                        : ValidationRuleManager.Instance.GetValidationRules(FieldDefinitionId).ToList();
                }
                return _validationRules;
            }
        }
    }
}
