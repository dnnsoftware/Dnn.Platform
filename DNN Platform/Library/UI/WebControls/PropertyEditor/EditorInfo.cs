#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2018
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
using System.Web.UI.WebControls;

using DotNetNuke.Common.Lists;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Users;
using DotNetNuke.Entities.Profile;

#endregion

namespace DotNetNuke.UI.WebControls
{
    /// -----------------------------------------------------------------------------
    /// Project:    DotNetNuke
    /// Namespace:  DotNetNuke.UI.WebControls
    /// Class:      EditorInfo
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The EditorInfo class provides a helper class for the Property Editor
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// -----------------------------------------------------------------------------
    public class EditorInfo
    {
        public EditorInfo()
        {
            Visible = true;
        }

        public object[] Attributes { get; set; }

        public string Category { get; set; }

        public Style ControlStyle { get; set; }

        public PropertyEditorMode EditMode { get; set; }

        public string Editor { get; set; }

        public LabelMode LabelMode { get; set; }

        public string Name { get; set; }

        public bool Required { get; set; }

        public string ResourceKey { get; set; }

        public string Type { get; set; }

        public UserInfo User { get; set; }

        public object Value { get; set; }

        public string ValidationExpression { get; set; }

        public bool Visible { get; set; }

        public ProfileVisibility ProfileVisibility { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// GetEditor gets the appropriate Editor based on ID
        /// properties
        /// </summary>
        /// <param name="editorType">The Id of the Editor</param>
        /// -----------------------------------------------------------------------------
        public static string GetEditor(int editorType)
        {
            string editor = "UseSystemType";
            if (editorType != Null.NullInteger)
            {
                var objListController = new ListController();
                ListEntryInfo definitionEntry = objListController.GetListEntryInfo("DataType", editorType);
                if (definitionEntry != null)
                {
                    editor = definitionEntry.TextNonLocalized;
                }
            }
            return editor;
        }

        public static string GetEditor(string editorValue)
        {
            string editor = "UseSystemType";
            var objListController = new ListController();
            ListEntryInfo definitionEntry = objListController.GetListEntryInfo("DataType", editorValue);
            if (definitionEntry != null)
            {
                editor = definitionEntry.TextNonLocalized;
            }
            return editor;
        }
    }
}
