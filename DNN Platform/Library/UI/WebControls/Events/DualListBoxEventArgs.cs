﻿// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
#region Usings

using System;
using System.Collections.Generic;

#endregion

namespace DotNetNuke.UI.WebControls
{
    /// -----------------------------------------------------------------------------
    /// Project:    DotNetNuke
    /// Namespace:  DotNetNuke.UI.WebControls
    /// Class:      DualListBoxEventArgs
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The DualListBoxEventArgs class is a cusom EventArgs class for
    /// handling Event Args in the DualListBox.
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// -----------------------------------------------------------------------------
    public class DualListBoxEventArgs : EventArgs
    {
        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Constructs a new DualListBoxEventArgs
        /// </summary>
        /// <param name="items">The items</param>
        /// -----------------------------------------------------------------------------
        public DualListBoxEventArgs(List<string> items)
        {
            Items = items;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets and sets the Items
        /// </summary>
        /// <value>A List(Of String)</value>
        /// -----------------------------------------------------------------------------
        public List<string> Items { get; set; }
    }
}
