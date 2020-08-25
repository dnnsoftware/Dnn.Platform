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
        private string MyFileName = "User.ascx";
        private string _Confirm;
        private string _ControlColumnWidth = string.Empty;
        private string _Email;
        private string _FirstName;
        private string _IM;
        private string _LabelColumnWidth = string.Empty;
        private string _LastName;
        private int _ModuleId;
        private string _Password;
        private bool _ShowPassword;
        private int _StartTabIndex = 1;
        private string _UserName;
        private string _Website;

        public string LocalResourceFile
        {
            get
            {
                return Localization.GetResourceFile(this, this.MyFileName);
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
                this._ModuleId = value;
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
                this._LabelColumnWidth = value;
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
                this._ControlColumnWidth = value;
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
                this._FirstName = value;
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
                this._LastName = value;
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
                this._UserName = value;
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
                this._Password = value;
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
                this._Confirm = value;
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
                this._Email = value;
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
                this._Website = value;
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
                this._IM = value;
            }
        }

        public int StartTabIndex
        {
            set
            {
                this._StartTabIndex = value;
            }
        }

        public bool ShowPassword
        {
            set
            {
                this._ShowPassword = value;
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
                    this.txtFirstName.TabIndex = Convert.ToInt16(this._StartTabIndex);
                    this.txtLastName.TabIndex = Convert.ToInt16(this._StartTabIndex + 1);
                    this.txtUsername.TabIndex = Convert.ToInt16(this._StartTabIndex + 2);
                    this.txtPassword.TabIndex = Convert.ToInt16(this._StartTabIndex + 3);
                    this.txtConfirm.TabIndex = Convert.ToInt16(this._StartTabIndex + 4);
                    this.txtEmail.TabIndex = Convert.ToInt16(this._StartTabIndex + 5);
                    this.txtWebsite.TabIndex = Convert.ToInt16(this._StartTabIndex + 6);
                    this.txtIM.TabIndex = Convert.ToInt16(this._StartTabIndex + 7);
                    this.txtFirstName.Text = this._FirstName;
                    this.txtLastName.Text = this._LastName;
                    this.txtEmail.Text = this._Email;
                    this.txtUsername.Text = this._UserName;
                    this.lblUsername.Text = this._UserName;
                    this.txtPassword.Text = this._Password;
                    this.txtConfirm.Text = this._Confirm;
                    this.txtWebsite.Text = this._Website;
                    this.txtIM.Text = this._IM;
                    if (!string.IsNullOrEmpty(this._ControlColumnWidth))
                    {
                        this.txtFirstName.Width = Unit.Parse(this._ControlColumnWidth);
                        this.txtLastName.Width = Unit.Parse(this._ControlColumnWidth);
                        this.txtEmail.Width = Unit.Parse(this._ControlColumnWidth);
                        this.txtUsername.Width = Unit.Parse(this._ControlColumnWidth);
                        this.txtPassword.Width = Unit.Parse(this._ControlColumnWidth);
                        this.txtConfirm.Width = Unit.Parse(this._ControlColumnWidth);
                        this.txtWebsite.Width = Unit.Parse(this._ControlColumnWidth);
                        this.txtIM.Width = Unit.Parse(this._ControlColumnWidth);
                    }

                    if (!this._ShowPassword)
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

                    this.ViewState["ModuleId"] = Convert.ToString(this._ModuleId);
                    this.ViewState["LabelColumnWidth"] = this._LabelColumnWidth;
                    this.ViewState["ControlColumnWidth"] = this._ControlColumnWidth;
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
