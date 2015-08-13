// Copyright (c) DNN Software. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using Dnn.DynamicContent;

namespace Dnn.Modules.DynamicContentViewer.Models
{
    /// <summary>
    /// Setting class manages the settings for the module instance
    /// </summary>
    public class Settings
    {
        /// <summary>
        /// The Id of the Content Type
        /// </summary>
        public int ContentTypeId { get; set; }

        /// <summary>
        /// A list of available content types for the site
        /// </summary>
        public IList<DynamicContentType> ContentTypes { get; set; }

        /// <summary>
        /// The Id of the Edit Template
        /// </summary>
        public int EditTemplateId { get; set; }

        /// <summary>
        /// The Id of the View Template
        /// </summary>
        public int ViewTemplateId { get; set; }
    }
}
