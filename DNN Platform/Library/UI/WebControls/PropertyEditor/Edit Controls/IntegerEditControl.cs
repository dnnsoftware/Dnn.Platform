﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

#region Usings

using System;
using System.Web.UI;

using DotNetNuke.Common.Utilities;
using DotNetNuke.Instrumentation;

#endregion

namespace DotNetNuke.UI.WebControls
{
    /// -----------------------------------------------------------------------------
    /// Project:    DotNetNuke
    /// Namespace:  DotNetNuke.UI.WebControls
    /// Class:      IntegerEditControl
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The IntegerEditControl control provides a standard UI component for editing
    /// integer properties.
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// -----------------------------------------------------------------------------
    [ToolboxData("<{0}:IntegerEditControl runat=server></{0}:IntegerEditControl>")]
    public class IntegerEditControl : EditControl
    {
    	private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof (IntegerEditControl));
		#region "Constructors"

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Constructs an IntegerEditControl
        /// </summary>
        /// -----------------------------------------------------------------------------
        public IntegerEditControl()
        {
            this.SystemType = "System.Int32";
        }

		#endregion

		#region "Protected Properties"

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
                return this.IntegerValue.ToString();
            }
            set
            {
                int setValue = Int32.Parse(value);
                this.Value = setValue;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// IntegerValue returns the Integer representation of the Value
        /// </summary>
        /// <value>An integer representing the Value</value>
        /// -----------------------------------------------------------------------------
        protected int IntegerValue
        {
            get
            {
                int intValue = Null.NullInteger;
                try
                {
					//Try and cast the value to an Integer
                    if(this.Value != null)
                    {
                        Int32.TryParse(this.Value.ToString(), out intValue);
                    }
                }
                catch (Exception exc)
                {
                    Logger.Error(exc);

                }
                return intValue;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// OldIntegerValue returns the Integer representation of the OldValue
        /// </summary>
        /// <value>An integer representing the OldValue</value>
        /// -----------------------------------------------------------------------------
        protected int OldIntegerValue
        {
            get
            {
                int intValue = Null.NullInteger;
                try
                {
					//Try and cast the value to an Integer
                    int.TryParse(this.OldValue.ToString(), out intValue);
                }
                catch (Exception exc)
                {
                    Logger.Error(exc);

                }
                return intValue;
            }
        }

		#endregion

		#region "Protected Methods"

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// OnDataChanged runs when the PostbackData has changed.  It raises the ValueChanged
        /// Event
        /// </summary>
        /// -----------------------------------------------------------------------------
        protected override void OnDataChanged(EventArgs e)
        {
            var args = new PropertyEditorEventArgs(this.Name);
            args.Value = this.IntegerValue;
            args.OldValue = this.OldIntegerValue;
            args.StringValue = this.StringValue;
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
            this.ControlStyle.AddAttributesToRender(writer);
            writer.AddAttribute(HtmlTextWriterAttribute.Type, "text");
            writer.AddAttribute(HtmlTextWriterAttribute.Size, "5");
            writer.AddAttribute(HtmlTextWriterAttribute.Value, this.StringValue);
            writer.AddAttribute(HtmlTextWriterAttribute.Name, this.UniqueID);
            writer.AddAttribute(HtmlTextWriterAttribute.Id, this.ClientID);
            writer.RenderBeginTag(HtmlTextWriterTag.Input);
            writer.RenderEndTag();
        }
		
		#endregion
    }
}
