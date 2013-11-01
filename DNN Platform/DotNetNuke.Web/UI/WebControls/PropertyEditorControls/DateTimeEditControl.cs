#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2013
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
using System.Data.SqlTypes;
using System.Globalization;
using System.Web.UI;
using System.Web.UI.WebControls;

using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Instrumentation;
using DotNetNuke.Services.Localization;
using DotNetNuke.UI.WebControls;

using Telerik.Web.UI;

using Calendar = DotNetNuke.Common.Utilities.Calendar;

#endregion

namespace DotNetNuke.Web.UI.WebControls.PropertyEditorControls
{
	/// -----------------------------------------------------------------------------
	/// Project:    DotNetNuke
	/// Namespace:  DotNetNuke.UI.WebControls
	/// Class:      DateEditControl
	/// -----------------------------------------------------------------------------
	/// <summary>
	/// The DateEditControl control provides a standard UI component for editing
	/// date properties.
	/// </summary>
	/// <remarks>
	/// </remarks>
	/// <history>
	///     [cnurse]	05/10/2007	created
	/// </history>
	/// -----------------------------------------------------------------------------
	[ToolboxData("<{0}:DateTimeEditControl runat=server></{0}:DateTimeEditControl>")]
	public class DateTimeEditControl : EditControl
	{
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(DateTimeEditControl));
		private DnnDateTimePicker _dateControl;

		#region "Protected Properties"

		/// -----------------------------------------------------------------------------
		/// <summary>
		/// DateValue returns the Date representation of the Value
		/// </summary>
		/// <value>A Date representing the Value</value>
		/// <history>
		///     [cnurse]	05/22/2007	created
		/// </history>
		/// -----------------------------------------------------------------------------
		protected DateTime DateValue
		{
			get
			{
				DateTime dteValue = Null.NullDate;
				try
				{
					var dteString = Convert.ToString(Value);
					DateTime.TryParse(dteString, CultureInfo.InvariantCulture, DateTimeStyles.None, out dteValue);
				}
				catch (Exception exc)
				{
					Logger.Error(exc);

				}
				return dteValue;
			}
		}

		/// -----------------------------------------------------------------------------
		/// <summary>
		/// DefaultDateFormat is a string that will be used to format the date in the absence of a
		/// FormatAttribute
		/// </summary>
		/// <value>A String representing the default format to use to render the date</value>
		/// <returns>A Format String</returns>
		/// <history>
		///     [cnurse]	10/29/2007	created
		/// </history>
		/// -----------------------------------------------------------------------------
		protected virtual string DefaultFormat
		{
			get
			{
				return "g";
			}
		}

		/// -----------------------------------------------------------------------------
		/// <summary>
		/// Format is a string that will be used to format the date in View mode
		/// </summary>
		/// <value>A String representing the format to use to render the date</value>
		/// <returns>A Format String</returns>
		/// <history>
		///     [cnurse]	06/11/2007	created
		/// </history>
		/// -----------------------------------------------------------------------------
		protected virtual string Format
		{
			get
			{
				string _Format = DefaultFormat;
				if (CustomAttributes != null)
				{
					foreach (Attribute attribute in CustomAttributes)
					{
						if (attribute is FormatAttribute)
						{
							var formatAtt = (FormatAttribute)attribute;
							_Format = formatAtt.Format;
							break;
						}
					}
				}
				return _Format;
			}
		}

		/// -----------------------------------------------------------------------------
		/// <summary>
		/// OldDateValue returns the Date representation of the OldValue
		/// </summary>
		/// <value>A Date representing the OldValue</value>
		/// <history>
		///     [cnurse]	05/22/2007	created
		/// </history>
		/// -----------------------------------------------------------------------------
		protected DateTime OldDateValue
		{
			get
			{
				DateTime dteValue = Null.NullDate;
				try
				{
					//Try and cast the value to an DateTime
					var dteString = OldValue as string;
					dteValue = DateTime.Parse(dteString, CultureInfo.InvariantCulture);
				}
				catch (Exception exc)
				{
					Logger.Error(exc);

				}
				return dteValue;
			}
		}

		/// <summary>
		/// The Value expressed as a String
		/// </summary>
		protected override string StringValue
		{
			get
			{
				string _StringValue = Null.NullString;
                if ((DateValue.ToUniversalTime().Date != (DateTime)SqlDateTime.MinValue && DateValue != Null.NullDate))
				{
					_StringValue = DateValue.ToString(Format);
				}
				return _StringValue;
			}
			set
			{
				Value = DateTime.Parse(value);
			}
		}

		#endregion

		#region Override Properties

		public override string ID
		{
			get
			{
				return base.ID + "_control";
			}
			set
			{
				base.ID = value;
			}
		}

		#endregion

		#region Private Properties

		private DnnDateTimePicker DateControl
		{
			get
			{
				if (_dateControl == null)
				{
					_dateControl = new DnnDateTimePicker();
				}

				return _dateControl;
			}
		}

		#endregion

		protected override void CreateChildControls()
		{
			base.CreateChildControls();

			DateControl.ControlStyle.CopyFrom(ControlStyle);
			DateControl.ID = base.ID + "_control";

			Controls.Add(DateControl);
		}

		protected virtual void LoadDateControls()
		{
			if (DateValue != Null.NullDate)
			{
				DateControl.SelectedDate = DateValue.Date;
			}
		}

		public override bool LoadPostData(string postDataKey, NameValueCollection postCollection)
		{
			EnsureChildControls();
			bool dataChanged = false;
			string presentValue = StringValue;
			string postedValue = postCollection[postDataKey + "_control"];
			if (!presentValue.Equals(postedValue))
			{
				DateTime value;
				if (DateTime.TryParseExact(postedValue, "yyyy-MM-dd-HH-mm-ss", CultureInfo.InvariantCulture,
										   DateTimeStyles.None, out value))
				{
					Value = value;
					dataChanged = true;
				}
			}

			return dataChanged;
		}

		/// <summary>
		/// OnDataChanged is called by the PostBack Handler when the Data has changed
		/// </summary>
		/// <param name="e">An EventArgs object</param>
		protected override void OnDataChanged(EventArgs e)
		{
			var args = new PropertyEditorEventArgs(Name);
			args.Value = DateValue;
			args.OldValue = OldDateValue;
			args.StringValue = DateValue.ToString(CultureInfo.InvariantCulture);
			base.OnValueChanged(args);
		}

		protected override void OnPreRender(EventArgs e)
		{
			base.OnPreRender(e);

			LoadDateControls();

			if (Page != null && EditMode == PropertyEditorMode.Edit)
			{
				Page.RegisterRequiresPostBack(this);
			}
		}

		/// <summary>
		/// RenderEditMode is called by the base control to render the control in Edit Mode
		/// </summary>
		/// <param name="writer"></param>
		protected override void RenderEditMode(HtmlTextWriter writer)
		{
			RenderChildren(writer);
		}

		/// -----------------------------------------------------------------------------
		/// <summary>
		/// RenderViewMode renders the View (readonly) mode of the control
		/// </summary>
		/// <param name="writer">A HtmlTextWriter.</param>
		/// <history>
		///     [cnurse]	06/11/2007	created
		/// </history>
		/// -----------------------------------------------------------------------------
		protected override void RenderViewMode(HtmlTextWriter writer)
		{
			ControlStyle.AddAttributesToRender(writer);
			writer.RenderBeginTag(HtmlTextWriterTag.Span);
			writer.Write(StringValue);
			writer.RenderEndTag();
		}
	}
}