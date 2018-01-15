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
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

using DotNetNuke.Framework;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Services.Localization;

#endregion

namespace DotNetNuke.UI.UserControls
{
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The User UserControl is used to manage User Details
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// -----------------------------------------------------------------------------
    public abstract class User : UserControlBase
    {
		#region "Private Members"

        protected HtmlTableRow ConfirmPasswordRow;
        private string MyFileName = "User.ascx";
        protected HtmlTableRow PasswordRow;
        private string _Confirm;
        private string _ControlColumnWidth = "";
        private string _Email;
        private string _FirstName;
        private string _IM;
        private string _LabelColumnWidth = "";
        private string _LastName;
        private int _ModuleId;
        private string _Password;
        private bool _ShowPassword;
        private int _StartTabIndex = 1;
        private string _UserName;
        private string _Website;
        protected Label lblUsername;

		#endregion

		#region "Protected Members"

		protected Label lblUsernameAsterisk;
        protected LabelControl plConfirm;
        protected LabelControl plEmail;
        protected LabelControl plFirstName;
        protected LabelControl plIM;
        protected LabelControl plLastName;
        protected LabelControl plPassword;
        protected LabelControl plUserName;
        protected LabelControl plWebsite;
        protected TextBox txtConfirm;
        protected TextBox txtEmail;
        protected TextBox txtFirstName;
        protected TextBox txtIM;
        protected TextBox txtLastName;
        protected TextBox txtPassword;
        protected TextBox txtUsername;
        protected TextBox txtWebsite;
        protected RequiredFieldValidator valConfirm1;
        protected CompareValidator valConfirm2;
        protected RequiredFieldValidator valEmail1;
        protected RegularExpressionValidator valEmail2;
        protected RequiredFieldValidator valFirstName;
        protected RequiredFieldValidator valLastName;
        protected RequiredFieldValidator valPassword;
        protected RequiredFieldValidator valUsername;

		#endregion

		#region "Public Properties"

		public int ModuleId
        {
            get
            {
                return Convert.ToInt32(ViewState["ModuleId"]);
            }
            set
            {
                _ModuleId = value;
            }
        }

        public string LabelColumnWidth
        {
            get
            {
                return Convert.ToString(ViewState["LabelColumnWidth"]);
            }
            set
            {
                _LabelColumnWidth = value;
            }
        }

        public string ControlColumnWidth
        {
            get
            {
                return Convert.ToString(ViewState["ControlColumnWidth"]);
            }
            set
            {
                _ControlColumnWidth = value;
            }
        }

        public string FirstName
        {
            get
            {
                return txtFirstName.Text;
            }
            set
            {
                _FirstName = value;
            }
        }

        public string LastName
        {
            get
            {
                return txtLastName.Text;
            }
            set
            {
                _LastName = value;
            }
        }

        public string UserName
        {
            get
            {
                return txtUsername.Text;
            }
            set
            {
                _UserName = value;
            }
        }

        public string Password
        {
            get
            {
                return txtPassword.Text;
            }
            set
            {
                _Password = value;
            }
        }

        public string Confirm
        {
            get
            {
                return txtConfirm.Text;
            }
            set
            {
                _Confirm = value;
            }
        }

        public string Email
        {
            get
            {
                return txtEmail.Text;
            }
            set
            {
                _Email = value;
            }
        }

        public string Website
        {
            get
            {
                return txtWebsite.Text;
            }
            set
            {
                _Website = value;
            }
        }

        public string IM
        {
            get
            {
                return txtIM.Text;
            }
            set
            {
                _IM = value;
            }
        }

        public int StartTabIndex
        {
            set
            {
                _StartTabIndex = value;
            }
        }

        public bool ShowPassword
        {
            set
            {
                _ShowPassword = value;
            }
        }

        public string LocalResourceFile
        {
            get
            {
                return Localization.GetResourceFile(this, MyFileName);
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
            try
            {
                if (Page.IsPostBack == false)
                {
                    txtFirstName.TabIndex = Convert.ToInt16(_StartTabIndex);
                    txtLastName.TabIndex = Convert.ToInt16(_StartTabIndex + 1);
                    txtUsername.TabIndex = Convert.ToInt16(_StartTabIndex + 2);
                    txtPassword.TabIndex = Convert.ToInt16(_StartTabIndex + 3);
                    txtConfirm.TabIndex = Convert.ToInt16(_StartTabIndex + 4);
                    txtEmail.TabIndex = Convert.ToInt16(_StartTabIndex + 5);
                    txtWebsite.TabIndex = Convert.ToInt16(_StartTabIndex + 6);
                    txtIM.TabIndex = Convert.ToInt16(_StartTabIndex + 7);
                    txtFirstName.Text = _FirstName;
                    txtLastName.Text = _LastName;
                    txtUsername.Text = _UserName;
                    lblUsername.Text = _UserName;
                    txtPassword.Text = _Password;
                    txtConfirm.Text = _Confirm;
                    txtEmail.Text = _Email;
                    txtWebsite.Text = _Website;
                    txtIM.Text = _IM;
                    if (!String.IsNullOrEmpty(_ControlColumnWidth))
                    {
                        txtFirstName.Width = Unit.Parse(_ControlColumnWidth);
                        txtLastName.Width = Unit.Parse(_ControlColumnWidth);
                        txtUsername.Width = Unit.Parse(_ControlColumnWidth);
                        txtPassword.Width = Unit.Parse(_ControlColumnWidth);
                        txtConfirm.Width = Unit.Parse(_ControlColumnWidth);
                        txtEmail.Width = Unit.Parse(_ControlColumnWidth);
                        txtWebsite.Width = Unit.Parse(_ControlColumnWidth);
                        txtIM.Width = Unit.Parse(_ControlColumnWidth);
                    }
                    if (!_ShowPassword)
                    {
                        valPassword.Enabled = false;
                        valConfirm1.Enabled = false;
                        valConfirm2.Enabled = false;
                        PasswordRow.Visible = false;
                        ConfirmPasswordRow.Visible = false;
                        txtUsername.Visible = false;
                        valUsername.Enabled = false;
                        lblUsername.Visible = true;
                        lblUsernameAsterisk.Visible = false;
                    }
                    else
                    {
                        txtUsername.Visible = true;
                        valUsername.Enabled = true;
                        lblUsername.Visible = false;
                        lblUsernameAsterisk.Visible = true;
                        valPassword.Enabled = true;
                        valConfirm1.Enabled = true;
                        valConfirm2.Enabled = true;
                        PasswordRow.Visible = true;
                        ConfirmPasswordRow.Visible = true;
                    }
                    ViewState["ModuleId"] = Convert.ToString(_ModuleId);
                    ViewState["LabelColumnWidth"] = _LabelColumnWidth;
                    ViewState["ControlColumnWidth"] = _ControlColumnWidth;
                }
                txtPassword.Attributes.Add("value", txtPassword.Text);
                txtConfirm.Attributes.Add("value", txtConfirm.Text);
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
		}

		#endregion
	}
}
