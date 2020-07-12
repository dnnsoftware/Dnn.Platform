// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.UI.WebControls
{
    using System;
    using System.Web.UI.WebControls;

    using DotNetNuke.Common.Lists;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Profile;
    using DotNetNuke.Entities.Users;

    /// -----------------------------------------------------------------------------
    /// Project:    DotNetNuke
    /// Namespace:  DotNetNuke.UI.WebControls
    /// Class:      EditorInfo
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The EditorInfo class provides a helper class for the Property Editor.
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// -----------------------------------------------------------------------------
    public class EditorInfo
    {
        public EditorInfo()
        {
            this.Visible = true;
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
        /// properties.
        /// </summary>
        /// <param name="editorType">The Id of the Editor.</param>
        /// <returns></returns>
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
