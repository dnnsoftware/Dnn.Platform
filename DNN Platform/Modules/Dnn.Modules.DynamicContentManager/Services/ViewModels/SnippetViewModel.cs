// Copyright (c) DNN Software. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using Newtonsoft.Json;

namespace Dnn.Modules.DynamicContentManager.Services.ViewModels
{
    /// <summary>
    /// SnippetViewModel represents a Code Snippet object within the ContentType Web Service API
    /// </summary>
    public class SnippetViewModel
    {
        /// <summary>
        /// Name of the Snippet
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// Code content of the Snippet
        /// </summary>
        [JsonProperty("snippet")]
        public string Snippet { get; set; }
    }
}
