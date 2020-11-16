// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Modules.Html
{
    using System;

    /// -----------------------------------------------------------------------------
    /// Namespace:  DotNetNuke.Modules.Html
    /// Project:    DotNetNuke
    /// Class:      HtmlTextLogInfo
    /// -----------------------------------------------------------------------------
    /// <summary>
    ///   Defines an instance of an HtmlTextLog object.
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// -----------------------------------------------------------------------------
    public class HtmlTextLogInfo
    {
        // local property declarations

        // initialization

        // public properties
        public int ItemID { get; set; }

        public int StateID { get; set; }

        public string StateName { get; set; }

        public string Comment { get; set; }

        public bool Approved { get; set; }

        public int CreatedByUserID { get; set; }

        public string DisplayName { get; set; }

        public DateTime CreatedOnDate { get; set; }
    }
}
