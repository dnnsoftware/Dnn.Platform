// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Abstractions.Portals
{
    using System;
    using System.Collections;
    using System.Collections.Generic;

    /// <summary>
    /// Represents the page pipeline configuration for rendering pages in the DNN platform.
    /// </summary>
    public static class PagePipeline
    {
        /// <summary>QueryString key.</summary>
        public const string QueryStringKey = "pipeline";

        /// <summary>QueryString WebForms.</summary>
        public const string QueryStringWebForms = "webforms";

        /// <summary>QueryString MVC.</summary>
        public const string QueryStringMvc = "mvc";

        /// <summary>Setting name for the page pipeline configuration.</summary>
        public const string SettingName = "PagePipeline";

        /// <summary>
        /// Defines the pipeline types for rendering pages in a portal.
        /// </summary>
        public enum PortalRenderingPipeline
        {
            /// <summary>
            /// Specifies that pages should be rendered using the WebForms pipeline.
            /// </summary>
            WebForms,

            /// <summary>
            /// Specifies that pages should be rendered using the MVC pipeline.
            /// </summary>
            MVC,

            /// <summary>
            /// Specifies that the pipeline type should be automatically determined.
            /// </summary>
            Auto,
        }

        /// <summary>
        /// Defines the available pipeline types for rendering pages in the DNN platform.
        /// </summary>
        public enum PageRenderingPipeline
        {
            /// <summary>
            /// Specifies that the pipeline type should be taken from the portal.
            /// </summary>
            Inherited,

            /// <summary>
            /// Specifies that pages should be rendered using the WebForms pipeline.
            /// </summary>
            WebForms,

            /// <summary>
            /// Specifies that pages should be rendered using the MVC pipeline.
            /// </summary>
            MVC,
        }

        /// <summary>
        /// Gets the portal rendering pipeline configuration from the specified dictionary.
        /// </summary>
        /// <param name="input">The dictionary containing the portal settings.</param>
        /// <param name="settingName">The name of the setting to retrieve.</param>
        /// <returns>The portal rendering pipeline configuration, or WebForms if not found or invalid.</returns>
        public static PortalRenderingPipeline GetPortalPipeline(this Dictionary<string, string> input, string settingName)
        {
            if (input != null && input.TryGetValue(settingName, out var pipeline))
            {
                return string.IsNullOrEmpty(pipeline) ?
                    PortalRenderingPipeline.WebForms :
                    Enum.TryParse<PortalRenderingPipeline>(pipeline, true, out var result) ? result : PortalRenderingPipeline.WebForms;
            }

            return PortalRenderingPipeline.WebForms;
        }

        /// <summary>
        /// Gets the page rendering pipeline configuration from the specified hashtable.
        /// </summary>
        /// <param name="input">The hashtable containing the page settings.</param>
        /// <param name="settingName">The name of the setting to retrieve.</param>
        /// <returns>The page rendering pipeline configuration, or Inherited if not found or invalid.</returns>
        public static PageRenderingPipeline GetPagePipeline(this Hashtable input, string settingName)
        {
            if (input != null && input.ContainsKey(settingName))
            {
                var pipeline = Convert.ToString(input[settingName], System.Globalization.CultureInfo.InvariantCulture);
                return string.IsNullOrEmpty(pipeline) ?
                    PageRenderingPipeline.Inherited :
                    Enum.TryParse<PageRenderingPipeline>(pipeline, true, out var result) ? result : PageRenderingPipeline.Inherited;
            }

            return PageRenderingPipeline.Inherited;
        }
    }
}
