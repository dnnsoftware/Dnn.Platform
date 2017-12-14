#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2017
// by DotNetNuke Corporation
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation 
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and 
// to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions 
// of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
// TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.
#endregion
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
