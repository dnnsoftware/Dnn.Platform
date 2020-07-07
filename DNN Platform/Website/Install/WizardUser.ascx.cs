// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Install
{
    using System;
    using System.Text.RegularExpressions;
    using System.Web.UI;

    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Security.Membership;

    public partial class WizardUser : UserControl
    {
        public string FirstName
        {
            get
            {
                return this.txtFirstName.Text;
            }

            set
            {
                this.txtFirstName.Text = value;
            }
        }

        public string LastName
        {
            get
            {
                return this.txtLastName.Text;
            }

            set
            {
                this.txtLastName.Text = value;
            }
        }

        public string UserName
        {
            get
            {
                return this.txtUserName.Text;
            }

            set
            {
                this.txtUserName.Text = value;
            }
        }

        public string Password
        {
            get
            {
                return this.txtPassword.Text;
            }

            set
            {
                this.txtPassword.Text = value;
            }
        }

        public string Confirm
        {
            get
            {
                return this.txtConfirm.Text;
            }

            set
            {
                this.txtConfirm.Text = value;
            }
        }

        public string Email
        {
            get
            {
                return this.txtEmail.Text;
            }

            set
            {
                this.txtEmail.Text = value;
            }
        }

        public string FirstNameLabel
        {
            get
            {
                return this.lblFirstName.Text;
            }

            set
            {
                this.lblFirstName.Text = value;
            }
        }

        public string LastNameLabel
        {
            get
            {
                return this.lblLastName.Text;
            }

            set
            {
                this.lblLastName.Text = value;
            }
        }

        public string UserNameLabel
        {
            get
            {
                return this.lblUserName.Text;
            }

            set
            {
                this.lblUserName.Text = value;
            }
        }

        public string PasswordLabel
        {
            get
            {
                return this.lblPassword.Text;
            }

            set
            {
                this.lblPassword.Text = value;
            }
        }

        public string ConfirmLabel
        {
            get
            {
                return this.lblConfirm.Text;
            }

            set
            {
                this.lblConfirm.Text = value;
            }
        }

        public string EmailLabel
        {
            get
            {
                return this.lblEmail.Text;
            }

            set
            {
                this.lblEmail.Text = value;
            }
        }

        public string Validate()
        {
            string strErrorMessage = Null.NullString;
            if (this.txtUserName.Text.Length < 4)
            {
                strErrorMessage = "MinUserNamelength";
            }
            else if (string.IsNullOrEmpty(this.txtPassword.Text))
            {
                strErrorMessage = "NoPassword";
            }
            else if (this.txtUserName.Text == this.txtPassword.Text)
            {
                strErrorMessage = "PasswordUser";
            }
            else if (this.txtPassword.Text.Length < MembershipProviderConfig.MinPasswordLength)
            {
                strErrorMessage = "PasswordLength";
            }
            else if (this.txtPassword.Text != this.txtConfirm.Text)
            {
                strErrorMessage = "ConfirmPassword";
            }
            else if (!Globals.EmailValidatorRegex.IsMatch(this.txtEmail.Text))
            {
                strErrorMessage = "InValidEmail";
            }

            return strErrorMessage;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// OnLoad runs just before the page is rendered.
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// -----------------------------------------------------------------------------
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (this.IsPostBack)
            {
                if (!string.IsNullOrEmpty(this.txtPassword.Text))
                {
                    this.ViewState["Password"] = this.txtPassword.Text;
                }
                else if (this.ViewState["Password"] != null)
                {
                    this.txtPassword.Text = this.ViewState["Password"].ToString();
                }

                if (!string.IsNullOrEmpty(this.txtConfirm.Text))
                {
                    this.ViewState["Confirm"] = this.txtConfirm.Text;
                }
                else if (this.ViewState["Confirm"] != null)
                {
                    this.txtConfirm.Text = this.ViewState["Confirm"].ToString();
                }
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// OnLoad runs just before the page is rendered.
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// -----------------------------------------------------------------------------
        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            // Make sure that the password is not cleared on pastback
            this.txtConfirm.Attributes["value"] = this.txtConfirm.Text;
            this.txtPassword.Attributes["value"] = this.txtPassword.Text;
        }
    }
}
