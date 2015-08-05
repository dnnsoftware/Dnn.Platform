// Copyright (c) DNN Software. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Web.Caching;
using DotNetNuke.ComponentModel.DataAnnotations;

namespace Dnn.DynamicContent
{
    [Serializable]
    [TableName("ContentTypes_ValidationRuleSettings")]
    [PrimaryKey("ValidatorSettingID", "ValidatorSettingId")]
    [Cacheable("ContentTypes_ValidationRuleSettings", CacheItemPriority.Normal, 20)]
    [Scope("ValidationRuleId")]
    public class ValidatorSetting
    {
        public ValidatorSetting()
        {
            ValidationRuleId = -1;
            ValidatorSettingId = -1;
        }

        public string SettingName { get; set; }

        public string SettingValue { get; set; }

        public int ValidationRuleId { get; set; }

        public int ValidatorSettingId { get; set; }
    }
}
