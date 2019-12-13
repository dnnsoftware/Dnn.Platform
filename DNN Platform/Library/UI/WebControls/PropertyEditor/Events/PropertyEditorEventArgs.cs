// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
#region Usings

using System;

#endregion

namespace DotNetNuke.UI.WebControls
{
    /// -----------------------------------------------------------------------------
    /// Project:    DotNetNuke
    /// Namespace:  DotNetNuke.UI.WebControls
    /// Class:      PropertyEditorEventArgs
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The PropertyEditorEventArgs class is a cusom EventArgs class for
    /// handling Event Args from a change in value of a property.
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// -----------------------------------------------------------------------------
    public class PropertyEditorEventArgs : EventArgs
    {
        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Constructs a new PropertyEditorEventArgs
        /// </summary>
        /// <param name="name">The name of the property</param>
        /// -----------------------------------------------------------------------------
        public PropertyEditorEventArgs(string name) : this(name, null, null)
        {
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Constructs a new PropertyEditorEventArgs
        /// </summary>
        /// <param name="name">The name of the property</param>
        /// <param name="newValue">The new value of the property</param>
        /// <param name="oldValue">The old value of the property</param>
        /// -----------------------------------------------------------------------------
        public PropertyEditorEventArgs(string name, object newValue, object oldValue)
        {
            Name = name;
            Value = newValue;
            OldValue = oldValue;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets and sets whether the proeprty has changed
        /// </summary>
        /// <value>A String</value>
        /// -----------------------------------------------------------------------------
        public bool Changed { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets and sets the Index of the Item
        /// </summary>
        /// <value>An Integer</value>
        /// -----------------------------------------------------------------------------
        public int Index { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets and sets the Key of the Item
        /// </summary>
        /// <value>An Object</value>
        /// -----------------------------------------------------------------------------
        public object Key { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets and sets the Name of the Property being changed
        /// </summary>
        /// <value>A String</value>
        /// -----------------------------------------------------------------------------
        public string Name { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets and sets the OldValue of the Property being changed
        /// </summary>
        /// <value>An Object</value>
        /// -----------------------------------------------------------------------------
        public object OldValue { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets and sets the String Value of the Property being changed
        /// </summary>
        /// <value>An Object</value>
        /// -----------------------------------------------------------------------------
        public string StringValue { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets and sets the Value of the Property being changed
        /// </summary>
        /// <value>An Object</value>
        /// -----------------------------------------------------------------------------
        public object Value { get; set; }
    }
}
