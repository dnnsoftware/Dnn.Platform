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
    /// Class:      EditControlFactory
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The EditControlFactory control provides a factory for creating the
    /// appropriate Edit Control
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// -----------------------------------------------------------------------------
    public class EditControlFactory
    {
        /// -----------------------------------------------------------------------------
        /// <summary>
        /// CreateEditControl creates the appropriate Control based on the EditorField or
        /// TypeDataField
        /// </summary>
        /// <param name="editorInfo">An EditorInfo object</param>
        /// -----------------------------------------------------------------------------
        public static EditControl CreateEditControl(EditorInfo editorInfo)
        {
            EditControl propEditor;

            if (editorInfo.Editor == "UseSystemType")
            {
                //Use System Type
                propEditor = CreateEditControlInternal(editorInfo.Type);
            }
            else
            {
                //Use Editor
                try
                {
                    Type editType = Type.GetType(editorInfo.Editor, true, true);
                    propEditor = (EditControl)Activator.CreateInstance(editType);
                }
                catch (TypeLoadException)
                {
                    //Use System Type
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
                //ignore
            }
            return propEditor ?? new TextEditControl(systemType);
        }
    }
}

