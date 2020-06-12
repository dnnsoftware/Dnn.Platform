// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

using System;

namespace DotNetNuke.Entities.Tabs.TabVersions
{
    /// <summary>
    /// Class to represent a Tab Version Detail object. Each detail is related with a TabVersion and also with a ModuleInfo
    /// </summary>
    [Serializable]
    public class TabVersionDetail : BaseEntityInfo
    {

        /// <summary>
        /// Id of TabVersionDetail
        /// </summary>
        public int TabVersionDetailId { get; set; }

        /// <summary>
        /// Id of the related TabVersion master of the detail
        /// </summary>
        public int TabVersionId { get; set; }

        /// <summary>
        /// Id of the Module which tracks the detail
        /// </summary>
        public int ModuleId { get; set; }

        /// <summary>
        /// Version number of the module when the detail was tracked
        /// </summary>
        public int ModuleVersion { get; set; }

        /// <summary>
        /// Pane name where the Module was when the detail was tracked
        /// </summary>
        public string PaneName { get; set; }

        /// <summary>
        /// Order into the pane where the Module was when the detail was tracked
        /// </summary>
        public int ModuleOrder { get; set; }

        /// <summary>
        /// Action which provoked the detail
        /// </summary>
        public TabVersionDetailAction Action { get; set; }
    }
}
