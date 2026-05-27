// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Framework
{
    using System;
    using System.Collections;
    using System.Collections.Generic;

    using DotNetNuke.Abstractions.Framework;

    public static class PagePipelineExtensions
    {
        /// <summary>
        /// Gets the portal rendering pipeline configuration from the specified dictionary.
        /// </summary>
        /// <param name="input">The dictionary containing the portal settings.</param>
        /// <param name="settingName">The name of the setting to retrieve.</param>
        /// <returns>The portal rendering pipeline configuration, or WebForms if not found or invalid.</returns>
        public static PagePipeline.PortalRenderingPipeline GetPortalPipeline(this Dictionary<string, string> input, string settingName)
        {
            if (input != null && input.TryGetValue(settingName, out var pipeline))
            {
                return string.IsNullOrEmpty(pipeline) ?
                    PagePipeline.PortalRenderingPipeline.WebForms :
                    Enum.TryParse<PagePipeline.PortalRenderingPipeline>(pipeline, true, out var result) ? result : PagePipeline.PortalRenderingPipeline.WebForms;
            }

            return PagePipeline.PortalRenderingPipeline.WebForms;
        }

        /// <summary>
        /// Gets the page rendering pipeline configuration from the specified hashtable.
        /// </summary>
        /// <param name="input">The hashtable containing the page settings.</param>
        /// <param name="settingName">The name of the setting to retrieve.</param>
        /// <returns>The page rendering pipeline configuration, or Inherited if not found or invalid.</returns>
        public static PagePipeline.PageRenderingPipeline GetPagePipeline(this Hashtable input, string settingName)
        {
            if (input != null && input.ContainsKey(settingName))
            {
                var pipeline = Convert.ToString(input[settingName], System.Globalization.CultureInfo.InvariantCulture);
                return string.IsNullOrEmpty(pipeline) ?
                    PagePipeline.PageRenderingPipeline.Inherited :
                    Enum.TryParse<PagePipeline.PageRenderingPipeline>(pipeline, true, out var result) ? result : PagePipeline.PageRenderingPipeline.Inherited;
            }

            return PagePipeline.PageRenderingPipeline.Inherited;
        }
    }
}
