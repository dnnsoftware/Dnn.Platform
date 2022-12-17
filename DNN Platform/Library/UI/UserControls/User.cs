// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.UI.UserControls
{
    using System;
    using System.Web.UI.HtmlControls;
    using System.Web.UI.WebControls;

    using DotNetNuke.Framework;
    using DotNetNuke.Services.Exceptions;
    using DotNetNuke.Services.Localization;

    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The User UserControl is used to manage User Details.
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// -----------------------------------------------------------------------------
    public abstract class User : UserControlBase
    {
        protected HtmlTableRow ConfirmPasswordRow;
        protected HtmlTableRow PasswordRow;
        protected Label lblUsername;
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
        private string myFileName = "User.ascx";
        private string confirm;
        private string controlColumnWidth = string.Empty;
        private string email;
        private string firstName;
        private string im;
        private string labelColumnWidth = string.Empty;
        private string lastName;
        private int moduleId;
        private string password;
        private bool showPassword;
        private int startTabIndex = 1;
        private string userName;
        private string website;

        public string LocalResourceFile
        {
            get
            {
                return Localization.GetResourceFile(this, this.myFileName);
            }
        }

        public int ModuleId
        {
            get
            {
                return Convert.ToInt32(this.ViewState["ModuleId"]);
            }

            set
            {
                this.moduleId = value;
            }
        }

        public string LabelColumnWidth
        {
            get
            {
                return Convert.ToString(this.ViewState["LabelColumnWidth"]);
            }

            set
            {
                this.labelColumnWidth = value;
            }
        }

        public string ControlColumnWidth
        {
            get
            {
                return Convert.ToString(this.ViewState["ControlColumnWidth"]);
            }

            set
            {
                this.controlColumnWidth = value;
            }
        }

        public string FirstName
        {
            get
            {
                return this.txtFirstName.Text;
            }

            set
            {
                this.firstName = value;
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
                this.lastName = value;
            }
        }

        public string UserName
        {
            get
            {
                return this.txtUsername.Text;
            }

            set
            {
                this.userName = value;
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
                this.password = value;
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
                this.confirm = value;
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
                this.email = value;
            }
        }

        public string Website
        {
            get
            {
                return this.txtWebsite.Text;
            }

            set
            {
                this.website = value;
            }
        }

        public string IM
        {
            get
            {
                return this.txtIM.Text;
            }

            set
            {
                this.im = value;
            }
        }

        public int StartTabIndex
        {
            set
            {
                this.startTabIndex = value;
            }
        }

        public bool ShowPassword
        {
            set
            {
                this.showPassword = value;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Page_Load runs when the control is loaded.
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// -----------------------------------------------------------------------------
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            try
            {
                if (this.Page.IsPostBack == false)
                {
                    this.txtFirstName.TabIndex = Convert.ToInt16(this.startTabIndex);
                    this.txtLastName.TabIndex = Convert.ToInt16(this.startTabIndex + 1);
                    this.txtUsername.TabIndex = Convert.ToInt16(this.startTabIndex + 2);
                    this.txtPassword.TabIndex = Convert.ToInt16(this.startTabIndex + 3);
                    this.txtConfirm.TabIndex = Convert.ToInt16(this.startTabIndex + 4);
                    this.txtEmail.TabIndex = Convert.ToInt16(this.startTabIndex + 5);
                    this.txtWebsite.TabIndex = Convert.ToInt16(this.startTabIndex + 6);
                    this.txtIM.TabIndex = Convert.ToInt16(this.startTabIndex + 7);
                    this.txtFirstName.Text = this.FirstName;
                    this.txtLastName.Text = this.LastName;
                    this.txtEmail.Text = this.Email;
                    this.txtUsername.Text = this.UserName;
                    this.lblUsername.Text = this.UserName;
                    this.txtPassword.Text = this.Password;
                    this.txtConfirm.Text = this.Confirm;
                    this.txtWebsite.Text = this.Website;
                    this.txtIM.Text = this.IM;
                    if (!string.IsNullOrEmpty(this.ControlColumnWidth))
                    {
                        this.txtFirstName.Width = Unit.Parse(this.ControlColumnWidth);
                        this.txtLastName.Width = Unit.Parse(this.ControlColumnWidth);
                        this.txtEmail.Width = Unit.Parse(this.ControlColumnWidth);
                        this.txtUsername.Width = Unit.Parse(this.ControlColumnWidth);
                        this.txtPassword.Width = Unit.Parse(this.ControlColumnWidth);
                        this.txtConfirm.Width = Unit.Parse(this.ControlColumnWidth);
                        this.txtWebsite.Width = Unit.Parse(this.ControlColumnWidth);
                        this.txtIM.Width = Unit.Parse(this.ControlColumnWidth);
                    }

                    if (!this.showPassword)
                    {
                        this.valPassword.Enabled = false;
                        this.valConfirm1.Enabled = false;
                        this.valConfirm2.Enabled = false;
                        this.PasswordRow.Visible = false;
                        this.ConfirmPasswordRow.Visible = false;
                        this.txtUsername.Visible = false;
                        this.valUsername.Enabled = false;
                        this.lblUsername.Visible = true;
                        this.lblUsernameAsterisk.Visible = false;
                    }
                    else
                    {
                        this.txtUsername.Visible = true;
                        this.valUsername.Enabled = true;
                        this.lblUsername.Visible = false;
                        this.lblUsernameAsterisk.Visible = true;
                        this.valPassword.Enabled = true;
                        this.valConfirm1.Enabled = true;
                        this.valConfirm2.Enabled = true;
                        this.PasswordRow.Visible = true;
                        this.ConfirmPasswordRow.Visible = true;
                    }

                    this.ViewState["ModuleId"] = Convert.ToString(this.ModuleId);
                    this.ViewState["LabelColumnWidth"] = this.LabelColumnWidth;
                    this.ViewState["ControlColumnWidth"] = this.ControlColumnWidth;
                }

                this.txtPassword.Attributes.Add("value", this.txtPassword.Text);
                this.txtConfirm.Attributes.Add("value", this.txtConfirm.Text);
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }
    }
}
