﻿// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
#region Usings

using System;
using System.Globalization;
using System.Web.UI;

using DotNetNuke.Services.Localization;

#endregion

namespace DotNetNuke.UI.WebControls
{
    /// -----------------------------------------------------------------------------
    /// Project:    DotNetNuke
    /// Namespace:  DotNetNuke.UI.WebControls
    /// Class:      EnumEditControl
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The EnumEditControl control provides a standard UI component for editing
    /// enumerated properties.
    /// </summary>
    /// -----------------------------------------------------------------------------
    [ToolboxData("<{0}:EnumEditControl runat=server></{0}:EnumEditControl>")]
    public class EnumEditControl : EditControl
    {
        private readonly Type EnumType;

		#region Constructors

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Constructs an EnumEditControl
        /// </summary>
        /// -----------------------------------------------------------------------------
        public EnumEditControl()
        {
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Constructs an EnumEditControl
        /// </summary>
        /// -----------------------------------------------------------------------------
        public EnumEditControl(string type)
        {
            SystemType = type;
            EnumType = Type.GetType(type);
        }
		
		#endregion

		#region Public Properties

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// StringValue is the value of the control expressed as a String
        /// </summary>
        /// <value>A string representing the Value</value>
        /// -----------------------------------------------------------------------------
        protected override string StringValue
        {
            get
            {
                var retValue = Convert.ToInt32(Value);
                return retValue.ToString(CultureInfo.InvariantCulture);
            }
            set
            {
                int setValue = Int32.Parse(value);
                Value = setValue;
            }
        }

		#endregion

		#region Protected Methods

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// OnDataChanged runs when the PostbackData has changed.  It raises the ValueChanged
        /// Event
        /// </summary>
        /// -----------------------------------------------------------------------------
        protected override void OnDataChanged(EventArgs e)
        {
            int intValue = Convert.ToInt32(Value);
            int intOldValue = Convert.ToInt32(OldValue);

            var args = new PropertyEditorEventArgs(Name)
                           {Value = Enum.ToObject(EnumType, intValue), OldValue = Enum.ToObject(EnumType, intOldValue)};

            base.OnValueChanged(args);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// RenderEditMode renders the Edit mode of the control
        /// </summary>
        /// <param name="writer">A HtmlTextWriter.</param>
        /// -----------------------------------------------------------------------------
        protected override void RenderEditMode(HtmlTextWriter writer)
        {
            Int32 propValue = Convert.ToInt32(Value);
            Array enumValues = Enum.GetValues(EnumType);

            //Render the Select Tag
            ControlStyle.AddAttributesToRender(writer);
            writer.AddAttribute(HtmlTextWriterAttribute.Type, "text");
            writer.AddAttribute(HtmlTextWriterAttribute.Name, UniqueID);
            writer.AddAttribute(HtmlTextWriterAttribute.Id, ClientID);
            writer.RenderBeginTag(HtmlTextWriterTag.Select);

            for (int I = 0; I <= enumValues.Length - 1; I++)
            {
                int enumValue = Convert.ToInt32(enumValues.GetValue(I));
                string enumName = Enum.GetName(EnumType, enumValue);
                enumName = Localization.GetString(enumName, LocalResourceFile);

                //Add the Value Attribute
                writer.AddAttribute(HtmlTextWriterAttribute.Value, enumValue.ToString(CultureInfo.InvariantCulture));

                if (enumValue == propValue)
                {
					//Add the Selected Attribute
                    writer.AddAttribute(HtmlTextWriterAttribute.Selected, "selected");
                }
				
                //Render Option Tag
                writer.RenderBeginTag(HtmlTextWriterTag.Option);
                writer.Write(enumName);
                writer.RenderEndTag();
            }
			
            //Close Select Tag
            writer.RenderEndTag();
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// RenderViewMode renders the View (readonly) mode of the control
        /// </summary>
        /// <param name="writer">A HtmlTextWriter.</param>
        /// -----------------------------------------------------------------------------
        protected override void RenderViewMode(HtmlTextWriter writer)
        {
            Int32 propValue = Convert.ToInt32(Value);
            string enumValue = Enum.Format(EnumType, propValue, "G");

            ControlStyle.AddAttributesToRender(writer);
            writer.RenderBeginTag(HtmlTextWriterTag.Span);
            writer.Write(enumValue);
            writer.RenderEndTag();
        }
		
		#endregion
    }
}
