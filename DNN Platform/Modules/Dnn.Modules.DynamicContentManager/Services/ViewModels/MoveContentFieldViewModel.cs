// Copyright (c) DNN Software. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Newtonsoft.Json;

namespace Dnn.Modules.DynamicContentManager.Services.ViewModels
{
    /// <summary>
    /// 
    /// </summary>
    public class MoveContentFieldViewModel
    {
        /// <summary>
        /// The Id of the aprent Content Type
        /// </summary>
        [JsonProperty("contentTypeId")]
        public int ContentTypeId { get; set; }

        /// <summary>
        /// The source index of the field being moved
        /// </summary>
        [JsonProperty("sourceIndex")]
        public int SourceIndex { get; set; }

        /// <summary>
        /// The target index of the field being moved
        /// </summary>
        [JsonProperty("targetIndex")]
        public int TargetIndex { get; set; }
    }
}
