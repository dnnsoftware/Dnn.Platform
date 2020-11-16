// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.UI.WebControls
{
    using System;

    /// -----------------------------------------------------------------------------
    /// Project:    DotNetNuke
    /// Namespace:  DotNetNuke.UI.WebControls
    /// Class:      EditControlFactory
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The EditControlFactory control provides a factory for creating the
    /// appropriate Edit Control.
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// -----------------------------------------------------------------------------
    public class EditControlFactory
    {
        /// -----------------------------------------------------------------------------
        /// <summary>
        /// CreateEditControl creates the appropriate Control based on the EditorField or
        /// TypeDataField.
        /// </summary>
        /// <param name="editorInfo">An EditorInfo object.</param>
        /// <returns></returns>
        /// -----------------------------------------------------------------------------
        public static EditControl CreateEditControl(EditorInfo editorInfo)
        {
            EditControl propEditor;

            if (editorInfo.Editor == "UseSystemType")
            {
                // Use System Type
                propEditor = CreateEditControlInternal(editorInfo.Type);
            }
            else
            {
                // Use Editor
                try
                {
                    Type editType = Type.GetType(editorInfo.Editor, true, true);
                    propEditor = (EditControl)Activator.CreateInstance(editType);
                }
                catch (TypeLoadException)
                {
                    // Use System Type
                    propEditor = CreateEditControlInternal(editorInfo.Type);
                }
            }

            propEditor.ID = editorInfo.Name;
            propEditor.Name = editorInfo.Name;
            propEditor.DataField = editorInfo.Name;
            propEditor.Category = editorInfo.Category;

            propEditor.EditMode = editorInfo.EditMode;
            propEditor.Required = editorInfo.Required;

            propEditor.Value = editorInfo.Value;
            propEditor.OldValue = editorInfo.Value;

            propEditor.CustomAttributes = editorInfo.Attributes;

            return propEditor;
        }

        private static EditControl CreateEditControlInternal(string systemType)
        {
            EditControl propEditor = null;
            try
            {
                var type = Type.GetType(systemType);
                if (type != null)
                {
                    switch (type.FullName)
                    {
                        case "System.DateTime":
                            propEditor = new DateTimeEditControl();
                            break;
                        case "System.Boolean":
                            propEditor = new CheckEditControl();
                            break;
                        case "System.Int32":
                        case "System.Int16":
                            propEditor = new IntegerEditControl();
                            break;
                        default:
                            if (type.IsEnum)
                            {
                                propEditor = new EnumEditControl(systemType);
                            }

                            break;
                    }
                }
            }
            catch (Exception)
            {
                // ignore
            }

            return propEditor ?? new TextEditControl(systemType);
        }
    }
}
