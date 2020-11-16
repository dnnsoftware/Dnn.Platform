// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.UI.WebControls
{
    using System;
    using System.Collections.Generic;

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
        /// Initializes a new instance of the <see cref="DualListBoxEventArgs"/> class.
        /// Constructs a new DualListBoxEventArgs.
        /// </summary>
        /// <param name="items">The items.</param>
        /// -----------------------------------------------------------------------------
        public DualListBoxEventArgs(List<string> items)
        {
            this.Items = items;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets and sets the Items.
        /// </summary>
        /// <value>A List(Of String).</value>
        /// -----------------------------------------------------------------------------
        public List<string> Items { get; set; }
    }
}
