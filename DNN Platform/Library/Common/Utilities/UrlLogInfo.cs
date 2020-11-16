// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Common.Utilities
{
    using System;

    public class UrlLogInfo
    {
        public int UrlLogID { get; set; }

        public int UrlTrackingID { get; set; }

        public DateTime ClickDate { get; set; }

        public int UserID { get; set; }

        public string FullName { get; set; }
    }
}
