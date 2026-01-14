// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.HttpModules.Config
{
    using System;

    /// <summary>An analytics engine from the <c>SiteAnalytics.config</c> file.</summary>
    [Serializable]
    public class AnalyticsEngine
    {
        /// <summary>Gets or sets the engine type.</summary>
        public string EngineType { get; set; }

        /// <summary>Gets or sets the script template.</summary>
        public string ScriptTemplate { get; set; }

        /// <summary>Gets or sets the element ID.</summary>
        public string ElementId { get; set; }

        /// <summary>Gets or sets a value indicating whether to inject the script into the top or bottom of the element.</summary>
        public bool InjectTop { get; set; }
    }
}
