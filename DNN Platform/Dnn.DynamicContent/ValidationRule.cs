// Copyright (c) DNN Software. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Caching;
using DotNetNuke.ComponentModel.DataAnnotations;

namespace Dnn.DynamicContent
{
    [Serializable]
    [TableName("ContentTypes_ValidationRules")]
    [PrimaryKey("ValidationRuleID", "ValidationRuleId")]
    [Cacheable(ValidationRuleController.ValidationRuleCacheKey, CacheItemPriority.Normal, 20)]
    [Scope(ValidationRuleController.ValidationRuleScope)]
    public class ValidationRule
    {
        private ValidatorType _validatorType;
        private IDictionary<string, ValidatorSetting> _validationSettings;

        public ValidationRule()
        {
            ValidatorTypeId = -1;
            ValidationRuleId = -1;
            FieldDefinitionId = -1;
        }

        public int ValidationRuleId { get; set; }

        public int FieldDefinitionId { get; set; }

        public int ValidatorTypeId { get; set; }

        [IgnoreColumn]
        public IDictionary<string, ValidatorSetting> ValidationSettings
        {
            get
            {
                // ReSharper disable once ConvertIfStatementToNullCoalescingExpression
                if (_validationSettings == null)
                {
                    _validationSettings = (ValidationRuleId == -1)
                                        ? new Dictionary<string, ValidatorSetting>()
                                        : ValidationRuleController.Instance.GetValidationSettings(ValidationRuleId);
                }
                return _validationSettings;
            }
        }

        [IgnoreColumn]
        public ValidatorType ValidatorType
        {
            get { return _validatorType ?? (_validatorType = ValidatorTypeController.Instance.GetValidatorTypes()
                                                    .SingleOrDefault(vt => vt.ValidatorTypeId == ValidatorTypeId)); }
        }
    }
}
