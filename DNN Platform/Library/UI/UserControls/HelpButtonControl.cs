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
using System.Web.UI;
using System.Web.UI.WebControls;

using DotNetNuke.Common.Utilities;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Services.Localization;
using DotNetNuke.UI.Utilities;

#endregion

namespace DotNetNuke.UI.UserControls
{
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// HelpButtonControl is a user control that provides all the server code to display
    /// field level help button.
    /// </summary>
    /// <remarks>
    /// To implement help, the control uses the ClientAPI interface.  In particular
    ///  the javascript function __dnn_Help_OnClick()
    /// </remarks>
    /// -----------------------------------------------------------------------------
    public abstract class HelpButtonControl : UserControl
    {
		#region "Private Members"
		
        private string _HelpKey;
        private string _ResourceKey;
        protected LinkButton cmdHelp;
        protected Image imgHelp;
        protected Label lblHelp;
        protected Panel pnlHelp;
		
		#endregion
		
		#region "Public Properties"

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// ControlName is the Id of the control that is associated with the label
        /// </summary>
        /// <value>A string representing the id of the associated control</value>
        /// <remarks>
        /// </remarks>
        /// -----------------------------------------------------------------------------
        public string ControlName { get; set; } //Associated Edit Control for this Label

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// HelpKey is the Resource Key for the Help Text
        /// </summary>
        /// <value>A string representing the Resource Key for the Help Text</value>
        /// <remarks>
        /// </remarks>
        /// -----------------------------------------------------------------------------
        public string HelpKey
        {
            get
            {
                return _HelpKey;
            }
            set
            {
                _HelpKey = value;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// HelpText is value of the Help Text if no ResourceKey is provided
        /// </summary>
        /// <value>A string representing the Text</value>
        /// <remarks>
        /// </remarks>
        /// -----------------------------------------------------------------------------
        public string HelpText
        {
            get
            {
                return lblHelp.Text;
            }
            set
            {
                lblHelp.Text = value;
                imgHelp.AlternateText = HtmlUtils.Clean(value, false);
				
				//hide the help icon if the help text is ""
                if (String.IsNullOrEmpty(value))
                {
                    imgHelp.Visible = false;
                }
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// ResourceKey is the Resource Key for the Help Text
        /// </summary>
        /// <value>A string representing the Resource Key for the Label Text</value>
        /// <remarks>
        /// </remarks>
        /// -----------------------------------------------------------------------------
        public string ResourceKey
        {
            get
            {
                return _ResourceKey;
            }
            set
            {
                _ResourceKey = value;
            }
        }
		
		#endregion
		
		#region "Event Handlers"

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Page_Load runs when the control is loaded
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// -----------------------------------------------------------------------------
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            cmdHelp.Click += cmdHelp_Click;

            try
            {
                DNNClientAPI.EnableMinMax(cmdHelp, pnlHelp, true, DNNClientAPI.MinMaxPersistanceType.None);
                if (String.IsNullOrEmpty(_HelpKey))
                {
					//Set Help Key to the Resource Key plus ".Help"
                    _HelpKey = _ResourceKey + ".Help";
                }
                string helpText = Localization.GetString(_HelpKey, this);
                if (!String.IsNullOrEmpty(helpText))
                {
                    HelpText = helpText;
                }
            }
            catch (Exception exc) //Module failed to load
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        protected void cmdHelp_Click(object sender, EventArgs e)
        {
            pnlHelp.Visible = true;
        }
		
		#endregion
    }
}
