// Copyright (c) DNN Software. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Web.Caching;
using DotNetNuke.ComponentModel.DataAnnotations;
using DotNetNuke.Data;

namespace Dnn.DynamicContent.Localization
{
    [Serializable]
    [TableName("ContentTypes_Localizations")]
    [PrimaryKey("LocalizationID", "LocalizationId")]
    [Cacheable(ContentTypeLocalizationManager.CacheKey, CacheItemPriority.Normal, 20)]
    [Scope(ContentTypeLocalizationManager.Scope)]
    public class ContentTypeLocalization
    {
        /// <summary>
        /// The unique id of the localized value
        /// </summary>
        public int LocalizationId { get; set; }

        /// <summary>
        /// The Id of the portal
        /// </summary>
        public int PortalId { get; set; }

        /// <summary>
        /// The culture code of the Localizated value
        /// </summary>
        public string CultureCode { get; set; }

        /// <summary>
        /// The localization key
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// The localized value
        /// </summary>
        public string Value { get; set; }
    }
}
