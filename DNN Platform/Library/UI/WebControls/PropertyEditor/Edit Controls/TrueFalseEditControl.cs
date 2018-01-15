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
using System.Collections.Specialized;
using System.Web.UI;

using DotNetNuke.Common.Utilities;
using DotNetNuke.Instrumentation;
using DotNetNuke.Services.Localization;

#endregion

namespace DotNetNuke.UI.WebControls
{
    /// -----------------------------------------------------------------------------
    /// Project:    DotNetNuke
    /// Namespace:  DotNetNuke.UI.WebControls
    /// Class:      TrueFalseEditControl
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The TrueFalseEditControl control provides a standard UI component for editing
    /// true/false (boolean) properties.
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// -----------------------------------------------------------------------------
    [ToolboxData("<{0}:TrueFalseEditControl runat=server></{0}:TrueFalseEditControl>")]
    public class TrueFalseEditControl : EditControl
    {
    	private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof (TrueFalseEditControl));
		#region "Constructors"

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Constructs a TrueFalseEditControl
        /// </summary>
        /// -----------------------------------------------------------------------------
        public TrueFalseEditControl()
        {
            SystemType = "System.Boolean";
        }

		#endregion

		#region "Public Properties"

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// BooleanValue returns the Boolean representation of the Value
        /// </summary>
        /// <value>A Boolean representing the Value</value>
        /// -----------------------------------------------------------------------------
        protected bool BooleanValue
        {
            get
            {
                bool boolValue = Null.NullBoolean;
                try
                {
					//Try and cast the value to an Boolean
                    boolValue = Convert.ToBoolean(Value);
                }
                catch (Exception exc)
                {
                    Logger.Error(exc);

                }
                return boolValue;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// OldBooleanValue returns the Boolean representation of the OldValue
        /// </summary>
        /// <value>A Boolean representing the OldValue</value>
        /// -----------------------------------------------------------------------------
        protected bool OldBooleanValue
        {
            get
            {
                bool boolValue = Null.NullBoolean;
                try
                {
					//Try and cast the value to an Boolean
                    boolValue = Convert.ToBoolean(OldValue);
                }
                catch (Exception exc)
                {
                    Logger.Error(exc);

                }
                return boolValue;
            }
        }

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
                return BooleanValue.ToString();
            }
            set
            {
                bool setValue = bool.Parse(value);
                Value = setValue;
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
            var args = new PropertyEditorEventArgs(Name);
            args.Value = BooleanValue;
            args.OldValue = OldBooleanValue;
            args.StringValue = StringValue;
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
            writer.AddAttribute(HtmlTextWriterAttribute.Type, "radio");
            if ((BooleanValue))
            {
                writer.AddAttribute(HtmlTextWriterAttribute.Checked, "checked");
            }
            writer.AddAttribute(HtmlTextWriterAttribute.Value, "True");
            writer.AddAttribute(HtmlTextWriterAttribute.Name, UniqueID);
            writer.RenderBeginTag(HtmlTextWriterTag.Input);
            writer.RenderEndTag();
            ControlStyle.AddAttributesToRender(writer);
            writer.RenderBeginTag(HtmlTextWriterTag.Span);
            writer.Write(Localization.GetString("True", Localization.SharedResourceFile));
            writer.RenderEndTag();
            writer.AddAttribute(HtmlTextWriterAttribute.Type, "radio");
            if ((!BooleanValue))
            {
                writer.AddAttribute(HtmlTextWriterAttribute.Checked, "checked");
            }
            writer.AddAttribute(HtmlTextWriterAttribute.Value, "False");
            writer.AddAttribute(HtmlTextWriterAttribute.Name, UniqueID);
            writer.RenderBeginTag(HtmlTextWriterTag.Input);
            writer.RenderEndTag();
            ControlStyle.AddAttributesToRender(writer);
            writer.RenderBeginTag(HtmlTextWriterTag.Span);
            writer.Write(Localization.GetString("False", Localization.SharedResourceFile));
            writer.RenderEndTag();
        }
		
		#endregion
    }
}