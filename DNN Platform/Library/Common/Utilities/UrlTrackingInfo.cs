// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Common.Utilities
{
    using System;

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
