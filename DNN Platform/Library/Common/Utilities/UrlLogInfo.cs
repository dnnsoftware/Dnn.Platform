// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
#region Usings

using System;

#endregion

namespace DotNetNuke.Common.Utilities
{
    public class UrlLogInfo
    {
        public int UrlLogID { get; set; }

        public int UrlTrackingID { get; set; }

        public DateTime ClickDate { get; set; }

        public int UserID { get; set; }

        public string FullName { get; set; }
    }
}
