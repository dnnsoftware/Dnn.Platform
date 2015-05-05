#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2014
// by DotNetNuke Corporation
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation 
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and 
// to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions 
// of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
// TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.
#endregion

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
    [Cacheable(ValidationRuleManager.ValidationRuleCacheKey, CacheItemPriority.Normal, 20)]
    [Scope(ValidationRuleManager.ValidationRuleScope)]
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
                                        : ValidationRuleManager.Instance.GetValidationSettings(ValidationRuleId);
                }
                return _validationSettings;
            }
        }

        [IgnoreColumn]
        public ValidatorType ValidatorType
        {
            get { return _validatorType ?? (_validatorType = ValidatorTypeManager.Instance.GetValidatorTypes()
                                                    .SingleOrDefault(vt => vt.ValidatorTypeId == ValidatorTypeId)); }
        }
    }
}
