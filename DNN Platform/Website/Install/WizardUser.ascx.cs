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
            else if (!Globals.EmailValidatorRegex.IsMatch(txtEmail.Text))
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
