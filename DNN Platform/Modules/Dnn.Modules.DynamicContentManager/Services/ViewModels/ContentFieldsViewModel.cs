// Copyright (c) DNN Software. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections;
using System.Collections.Generic;
using Dnn.DynamicContent;
using DotNetNuke.Collections;
using Newtonsoft.Json;

namespace Dnn.Modules.DynamicContentManager.Services.ViewModels
{
    /// <summary>
    /// ContentFieldsViewModel represents a collection of Content Field object within the ContentType Web Service API
    /// </summary>
    
    public class ContentFieldsViewModel
    {
        /// <summary>
        /// Constructs a ContentFieldsViewModel from a collection of field definitions
        /// </summary>
        /// <param name="fieldDefinitions"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        public ContentFieldsViewModel(IList<FieldDefinition> fieldDefinitions, int pageIndex = 0, int pageSize = 5)
        {
            Fields = new List<ContentFieldViewModel>();
            foreach (var definition in new PagedList<FieldDefinition>(fieldDefinitions, pageIndex, pageSize))
            {
                Fields.Add(new ContentFieldViewModel(definition));
            }

            TotalResults = fieldDefinitions.Count;
        }

        /// <summary>
        /// A collection of content fields
        /// </summary>
        [JsonProperty("fields")]
        public IList<ContentFieldViewModel> Fields { get; set; }

        /// <summary>
        /// The total number of fields in the collection
        /// </summary>
        [JsonProperty("totalResults")]
        public int TotalResults { get; set; }
    }
}