#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2014
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
using System.Text.RegularExpressions;
using System.Web.UI;

using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Security.Membership;

#endregion

namespace DotNetNuke.Services.Install
{
    public partial class WizardUser : UserControl
    {
        public string FirstName
        {
            get
            {
                return txtFirstName.Text;
            }
            set
            {
                txtFirstName.Text = value;
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
                txtLastName.Text = value;
            }
        }

        public string UserName
        {
            get
            {
                return txtUserName.Text;
            }
            set
            {
                txtUserName.Text = value;
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
                txtPassword.Text = value;
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
                txtConfirm.Text = value;
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
                txtEmail.Text = value;
            }
        }

        public string FirstNameLabel
        {
            get
            {
                return lblFirstName.Text;
            }
            set
            {
                lblFirstName.Text = value;
            }
        }

        public string LastNameLabel
        {
            get
            {
                return lblLastName.Text;
            }
            set
            {
                lblLastName.Text = value;
            }
        }

        public string UserNameLabel
        {
            get
            {
                return lblUserName.Text;
            }
            set
            {
                lblUserName.Text = value;
            }
        }

        public string PasswordLabel
        {
            get
            {
                return lblPassword.Text;
            }
            set
            {
                lblPassword.Text = value;
            }
        }

        public string ConfirmLabel
        {
            get
            {
                return lblConfirm.Text;
            }
            set
            {
                lblConfirm.Text = value;
            }
        }

        public string EmailLabel
        {
            get
            {
                return lblEmail.Text;
            }
            set
            {
                lblEmail.Text = value;
            }
        }

        public string Validate()
        {
            string strErrorMessage = Null.NullString;
            if (txtUserName.Text.Length < 4)
            {
                strErrorMessage = "MinUserNamelength";
            }
            else if (string.IsNullOrEmpty(txtPassword.Text))
            {
                strErrorMessage = "NoPassword";
            }
            else if (txtUserName.Text == txtPassword.Text)
            {
                strErrorMessage = "PasswordUser";
            }
            else if (txtPassword.Text.Length < MembershipProviderConfig.MinPasswordLength)
            {
                strErrorMessage = "PasswordLength";
            }
            else if (txtPassword.Text != txtConfirm.Text)
            {
                strErrorMessage = "ConfirmPassword";
            }
            else if (!Regex.IsMatch(txtEmail.Text, Globals.glbEmailRegEx))
            {
                strErrorMessage = "InValidEmail";
            }
            return strErrorMessage;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
		/// OnLoad runs just before the page is rendered
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// -----------------------------------------------------------------------------
        protected override void OnLoad(EventArgs e)
        {
			base.OnLoad(e);

			if(IsPostBack)
			{
				if(!string.IsNullOrEmpty(txtPassword.Text))
				{
					ViewState["Password"] = txtPassword.Text;
				}
				else if(ViewState["Password"] != null)
				{
					txtPassword.Text = ViewState["Password"].ToString();
				}

				if (!string.IsNullOrEmpty(txtConfirm.Text))
				{
					ViewState["Confirm"] = txtConfirm.Text;
				}
				else if (ViewState["Confirm"] != null)
				{
					txtConfirm.Text = ViewState["Confirm"].ToString();
				}
			}
        }

		/// -----------------------------------------------------------------------------
		/// <summary>
		/// OnLoad runs just before the page is rendered
		/// </summary>
		/// <remarks>
		/// </remarks>
		/// <history>
		/// 	[cnurse]	02/15/2007	Created
		/// </history>
		/// -----------------------------------------------------------------------------
		protected override void OnPreRender(EventArgs e)
		{
			base.OnPreRender(e);

			//Make sure that the password is not cleared on pastback
			txtConfirm.Attributes["value"] = txtConfirm.Text;
			txtPassword.Attributes["value"] = txtPassword.Text;
		}
    }
}