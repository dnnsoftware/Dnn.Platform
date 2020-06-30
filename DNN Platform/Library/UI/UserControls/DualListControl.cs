// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.UI.UserControls
{
    using System;
    using System.Collections;
    using System.Web.UI.WebControls;

    using DotNetNuke.Framework;
    using DotNetNuke.Services.Exceptions;
    using DotNetNuke.Services.Localization;

    public abstract class DualListControl : UserControlBase
    {
        protected Label Label1;
        protected Label Label2;
        protected LinkButton cmdAdd;
        protected LinkButton cmdAddAll;
        protected LinkButton cmdRemove;
        protected LinkButton cmdRemoveAll;
        protected ListBox lstAssigned;
        protected ListBox lstAvailable;
        private string MyFileName = "DualListControl.ascx";
        private ArrayList _Assigned;
        private ArrayList _Available;
        private string _DataTextField = string.Empty;
        private string _DataValueField = string.Empty;
        private bool _Enabled = true;
        private string _ListBoxHeight = string.Empty;
        private string _ListBoxWidth = string.Empty;

        public string ListBoxWidth
        {
            get
            {
                return Convert.ToString(this.ViewState[this.ClientID + "_ListBoxWidth"]);
            }

            set
            {
                this._ListBoxWidth = value;
            }
        }

        public string ListBoxHeight
        {
            get
            {
                return Convert.ToString(this.ViewState[this.ClientID + "_ListBoxHeight"]);
            }

            set
            {
                this._ListBoxHeight = value;
            }
        }

        public ArrayList Available
        {
            get
            {
                var objList = new ArrayList();
                foreach (ListItem objListItem in this.lstAvailable.Items)
                {
                    objList.Add(objListItem);
                }

                return objList;
            }

            set
            {
                this._Available = value;
            }
        }

        public ArrayList Assigned
        {
            get
            {
                var objList = new ArrayList();
                foreach (ListItem objListItem in this.lstAssigned.Items)
                {
                    objList.Add(objListItem);
                }

                return objList;
            }

            set
            {
                this._Assigned = value;
            }
        }

        public string DataTextField
        {
            set
            {
                this._DataTextField = value;
            }
        }

        public string DataValueField
        {
            set
            {
                this._DataValueField = value;
            }
        }

        public bool Enabled
        {
            set
            {
                this._Enabled = value;
            }
        }

        /// <summary>The Page_Load server event handler on this page is used to populate the role information for the page.</summary>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            this.cmdAdd.Click += this.cmdAdd_Click;
            this.cmdAddAll.Click += this.cmdAddAll_Click;
            this.cmdRemove.Click += this.cmdRemove_Click;
            this.cmdRemoveAll.Click += this.cmdRemoveAll_Click;

            try
            {
                // Localization
                this.Label1.Text = Localization.GetString("Available", Localization.GetResourceFile(this, this.MyFileName));
                this.Label2.Text = Localization.GetString("Assigned", Localization.GetResourceFile(this, this.MyFileName));
                this.cmdAdd.ToolTip = Localization.GetString("Add", Localization.GetResourceFile(this, this.MyFileName));
                this.cmdAddAll.ToolTip = Localization.GetString("AddAll", Localization.GetResourceFile(this, this.MyFileName));
                this.cmdRemove.ToolTip = Localization.GetString("Remove", Localization.GetResourceFile(this, this.MyFileName));
                this.cmdRemoveAll.ToolTip = Localization.GetString("RemoveAll", Localization.GetResourceFile(this, this.MyFileName));

                if (!this.Page.IsPostBack)
                {
                    // set dimensions of control
                    if (!string.IsNullOrEmpty(this._ListBoxWidth))
                    {
                        this.lstAvailable.Width = Unit.Parse(this._ListBoxWidth);
                        this.lstAssigned.Width = Unit.Parse(this._ListBoxWidth);
                    }

                    if (!string.IsNullOrEmpty(this._ListBoxHeight))
                    {
                        this.lstAvailable.Height = Unit.Parse(this._ListBoxHeight);
                        this.lstAssigned.Height = Unit.Parse(this._ListBoxHeight);
                    }

                    // load available
                    this.lstAvailable.DataTextField = this._DataTextField;
                    this.lstAvailable.DataValueField = this._DataValueField;
                    this.lstAvailable.DataSource = this._Available;
                    this.lstAvailable.DataBind();
                    this.Sort(this.lstAvailable);

                    // load selected
                    this.lstAssigned.DataTextField = this._DataTextField;
                    this.lstAssigned.DataValueField = this._DataValueField;
                    this.lstAssigned.DataSource = this._Assigned;
                    this.lstAssigned.DataBind();
                    this.Sort(this.lstAssigned);

                    // set enabled
                    this.lstAvailable.Enabled = this._Enabled;
                    this.lstAssigned.Enabled = this._Enabled;

                    // save persistent values
                    this.ViewState[this.ClientID + "_ListBoxWidth"] = this._ListBoxWidth;
                    this.ViewState[this.ClientID + "_ListBoxHeight"] = this._ListBoxHeight;
                }
            }
            catch (Exception exc) // Module failed to load
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        private void cmdAdd_Click(object sender, EventArgs e)
        {
            var objList = new ArrayList();
            foreach (ListItem objListItem in this.lstAvailable.Items)
            {
                objList.Add(objListItem);
            }

            foreach (ListItem objListItem in objList)
            {
                if (objListItem.Selected)
                {
                    this.lstAvailable.Items.Remove(objListItem);
                    this.lstAssigned.Items.Add(objListItem);
                }
            }

            this.lstAvailable.ClearSelection();
            this.lstAssigned.ClearSelection();
            this.Sort(this.lstAssigned);
        }

        private void cmdRemove_Click(object sender, EventArgs e)
        {
            var objList = new ArrayList();
            foreach (ListItem objListItem in this.lstAssigned.Items)
            {
                objList.Add(objListItem);
            }

            foreach (ListItem objListItem in objList)
            {
                if (objListItem.Selected)
                {
                    this.lstAssigned.Items.Remove(objListItem);
                    this.lstAvailable.Items.Add(objListItem);
                }
            }

            this.lstAvailable.ClearSelection();
            this.lstAssigned.ClearSelection();
            this.Sort(this.lstAvailable);
        }

        private void cmdAddAll_Click(object sender, EventArgs e)
        {
            foreach (ListItem objListItem in this.lstAvailable.Items)
            {
                this.lstAssigned.Items.Add(objListItem);
            }

            this.lstAvailable.Items.Clear();
            this.lstAvailable.ClearSelection();
            this.lstAssigned.ClearSelection();
            this.Sort(this.lstAssigned);
        }

        private void cmdRemoveAll_Click(object sender, EventArgs e)
        {
            foreach (ListItem objListItem in this.lstAssigned.Items)
            {
                this.lstAvailable.Items.Add(objListItem);
            }

            this.lstAssigned.Items.Clear();
            this.lstAvailable.ClearSelection();
            this.lstAssigned.ClearSelection();
            this.Sort(this.lstAvailable);
        }

        private void Sort(ListBox ctlListBox)
        {
            var arrListItems = new ArrayList();
            foreach (ListItem objListItem in ctlListBox.Items)
            {
                arrListItems.Add(objListItem);
            }

            arrListItems.Sort(new ListItemComparer());
            ctlListBox.Items.Clear();
            foreach (ListItem objListItem in arrListItems)
            {
                ctlListBox.Items.Add(objListItem);
            }
        }
    }

    public class ListItemComparer : IComparer
    {
        public int Compare(object x, object y)
        {
            var a = (ListItem)x;
            var b = (ListItem)y;
            var c = new CaseInsensitiveComparer();
            return c.Compare(a.Text, b.Text);
        }
    }
}
