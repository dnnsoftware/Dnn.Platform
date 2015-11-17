// Copyright (c) DNN Software. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Web.Caching;
using DotNetNuke.ComponentModel.DataAnnotations;

namespace Dnn.DynamicContent
{
    /// <summary>
    /// Represents a DynamicContentValidator Type
    /// </summary>
    [Serializable]
    [TableName("ContentTypes_ValidatorTypes")]
    [PrimaryKey("ValidatorTypeID", "ValidatorTypeId")]
    [Cacheable(ValidatorTypeManager.ValidatorTypeCacheKey, CacheItemPriority.Normal, 20)]
    public class ValidatorType
    {
        /// <summary>
        /// The key used for a localized error message for this <see cref="T:DotNetNuke.Entities.Content.DynamicContentItem.ValidatorType"/>
        /// </summary>
        public string ErrorKey { get; set; }

        /// <summary>
        /// The text used for the error message for this <see cref="T:DotNetNuke.Entities.Content.DynamicContentItem.ValidatorType"/> (in the absence of a localized message)
        /// </summary>
        public string ErrorMessage { get; set; }

        /// <summary>
        /// The name used for this <see cref="T:DotNetNuke.Entities.Content.DynamicContentItem.ValidatorType"/>
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The class name for this <see cref="T:DotNetNuke.Entities.Content.DynamicContentItem.ValidatorType"/>
        /// </summary>
        public string ValidatorClassName { get; set; }

        /// <summary>
        /// The id for this <see cref="T:DotNetNuke.Entities.Content.DynamicContentItem.ValidatorType"/>
        /// </summary>
        public int ValidatorTypeId { get; set; }
    }
}
