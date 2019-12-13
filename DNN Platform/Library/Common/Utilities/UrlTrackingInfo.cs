// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
#region Usings

using System;

#endregion

namespace DotNetNuke.Common.Utilities
{
    [Serializable]
    public class UrlTrackingInfo
    {
        public int UrlTrackingID { get; set; }

        public int PortalID { get; set; }

        public string Url { get; set; }

        public string UrlType { get; set; }

        public int Clicks { get; set; }

        public DateTime LastClick { get; set; }

        public DateTime CreatedDate { get; set; }

        public bool LogActivity { get; set; }

        public bool TrackClicks { get; set; }

        public int ModuleID { get; set; }

        public bool NewWindow { get; set; }
    }
}
