// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
using System;

namespace DotNetNuke.Entities.Tabs.TabVersions
{
    ///<summary>
    ///Class to represent a Tab Version object
    ///</summary>    
    [Serializable]
    public class TabVersion : BaseEntityInfo
    {       
        #region Public Properties       
 
        /// <summary>
        /// Id of the TabVersion object
        /// </summary>
        public int TabVersionId { get; set; }

        /// <summary>
        /// Id of the related Tab
        /// </summary>
        public int TabId { get; set; }

        /// <summary>
        /// Version number of the TabVersion
        /// </summary>
        public int Version { get; set; }

        /// <summary>
        /// Timestamp of the version
        /// </summary>
        public DateTime TimeStamp { get; set; }

        /// <summary>
        /// True if the version is published. False if it is not published yet
        /// </summary>
        public bool IsPublished { get; set; }
        #endregion
    }
}
