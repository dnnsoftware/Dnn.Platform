// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Abstractions.Framework
{
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
    }
}
