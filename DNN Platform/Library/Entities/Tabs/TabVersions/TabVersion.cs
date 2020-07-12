// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Entities.Tabs.TabVersions
{
    using System;

    /// <summary>
    /// Class to represent a Tab Version object.
    /// </summary>
    [Serializable]
    public class TabVersion : BaseEntityInfo
    {
        /// <summary>
        /// Gets or sets id of the TabVersion object.
        /// </summary>
        public int TabVersionId { get; set; }

        /// <summary>
        /// Gets or sets id of the related Tab.
        /// </summary>
        public int TabId { get; set; }

        /// <summary>
        /// Gets or sets version number of the TabVersion.
        /// </summary>
        public int Version { get; set; }

        /// <summary>
        /// Gets or sets timestamp of the version.
        /// </summary>
        public DateTime TimeStamp { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether true if the version is published. False if it is not published yet.
        /// </summary>
        public bool IsPublished { get; set; }
    }
}
