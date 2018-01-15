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
using System.Collections;
using System.Web.UI.WebControls;

using DotNetNuke.Framework;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Services.Localization;

#endregion

namespace DotNetNuke.UI.UserControls
{
    public abstract class DualListControl : UserControlBase
    {
		#region "Private Members"
        protected Label Label1;
        protected Label Label2;
        private string MyFileName = "DualListControl.ascx";
        private ArrayList _Assigned;
        private ArrayList _Available;
        private string _DataTextField = "";
        private string _DataValueField = "";
        private bool _Enabled = true;
        private string _ListBoxHeight = "";
        private string _ListBoxWidth = "";
        protected LinkButton cmdAdd;
        protected LinkButton cmdAddAll;
        protected LinkButton cmdRemove;
        protected LinkButton cmdRemoveAll;
        protected ListBox lstAssigned;
        protected ListBox lstAvailable;
		
		#endregion
		
		#region "Public Properties"

        public string ListBoxWidth
        {
            get
            {
                return Convert.ToString(ViewState[ClientID + "_ListBoxWidth"]);
            }
            set
            {
                _ListBoxWidth = value;
            }
        }

        public string ListBoxHeight
        {
            get
            {
                return Convert.ToString(ViewState[ClientID + "_ListBoxHeight"]);
            }
            set
            {
                _ListBoxHeight = value;
            }
        }

        public ArrayList Available
        {
            get
            {
                var objList = new ArrayList();
                foreach (ListItem objListItem in lstAvailable.Items)
                {
                    objList.Add(objListItem);
                }
                return objList;
            }
            set
            {
                _Available = value;
            }
        }

        public ArrayList Assigned
        {
            get
            {
                var objList = new ArrayList();
                foreach (ListItem objListItem in lstAssigned.Items)
                {
                    objList.Add(objListItem);
                }
                return objList;
            }
            set
            {
                _Assigned = value;
            }
        }

        public string DataTextField
        {
            set
            {
                _DataTextField = value;
            }
        }

        public string DataValueField
        {
            set
            {
                _DataValueField = value;
            }
        }

        public bool Enabled
        {
            set
            {
                _Enabled = value;
            }
        }
		
		#endregion
		
		#region "Protected Event Handlers"

        /// <summary>The Page_Load server event handler on this page is used to populate the role information for the page</summary>
		protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            cmdAdd.Click += cmdAdd_Click;
            cmdAddAll.Click += cmdAddAll_Click;
            cmdRemove.Click += cmdRemove_Click;
            cmdRemoveAll.Click += cmdRemoveAll_Click;

            try
            {
                //Localization
                Label1.Text = Localization.GetString("Available", Localization.GetResourceFile(this, MyFileName));
                Label2.Text = Localization.GetString("Assigned", Localization.GetResourceFile(this, MyFileName));
                cmdAdd.ToolTip = Localization.GetString("Add", Localization.GetResourceFile(this, MyFileName));
                cmdAddAll.ToolTip = Localization.GetString("AddAll", Localization.GetResourceFile(this, MyFileName));
                cmdRemove.ToolTip = Localization.GetString("Remove", Localization.GetResourceFile(this, MyFileName));
                cmdRemoveAll.ToolTip = Localization.GetString("RemoveAll", Localization.GetResourceFile(this, MyFileName));

                if (!Page.IsPostBack)
                {
					//set dimensions of control
                    if (!String.IsNullOrEmpty(_ListBoxWidth))
                    {
                        lstAvailable.Width = Unit.Parse(_ListBoxWidth);
                        lstAssigned.Width = Unit.Parse(_ListBoxWidth);
                    }
                    if (!String.IsNullOrEmpty(_ListBoxHeight))
                    {
                        lstAvailable.Height = Unit.Parse(_ListBoxHeight);
                        lstAssigned.Height = Unit.Parse(_ListBoxHeight);
                    }
					
                    //load available
                    lstAvailable.DataTextField = _DataTextField;
                    lstAvailable.DataValueField = _DataValueField;
                    lstAvailable.DataSource = _Available;
                    lstAvailable.DataBind();
                    Sort(lstAvailable);

                    //load selected
                    lstAssigned.DataTextField = _DataTextField;
                    lstAssigned.DataValueField = _DataValueField;
                    lstAssigned.DataSource = _Assigned;
                    lstAssigned.DataBind();
                    Sort(lstAssigned);

                    //set enabled
                    lstAvailable.Enabled = _Enabled;
                    lstAssigned.Enabled = _Enabled;

                    //save persistent values
                    ViewState[ClientID + "_ListBoxWidth"] = _ListBoxWidth;
                    ViewState[ClientID + "_ListBoxHeight"] = _ListBoxHeight;
                }
            }
            catch (Exception exc) //Module failed to load
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        private void cmdAdd_Click(object sender, EventArgs e)
        {
            var objList = new ArrayList();
            foreach (ListItem objListItem in lstAvailable.Items)
            {
                objList.Add(objListItem);
            }
            foreach (ListItem objListItem in objList)
            {
                if (objListItem.Selected)
                {
                    lstAvailable.Items.Remove(objListItem);
                    lstAssigned.Items.Add(objListItem);
                }
            }
            lstAvailable.ClearSelection();
            lstAssigned.ClearSelection();
            Sort(lstAssigned);
        }

        private void cmdRemove_Click(object sender, EventArgs e)
        {
            var objList = new ArrayList();
            foreach (ListItem objListItem in lstAssigned.Items)
            {
                objList.Add(objListItem);
            }
            foreach (ListItem objListItem in objList)
            {
                if (objListItem.Selected)
                {
                    lstAssigned.Items.Remove(objListItem);
                    lstAvailable.Items.Add(objListItem);
                }
            }
            lstAvailable.ClearSelection();
            lstAssigned.ClearSelection();
            Sort(lstAvailable);
        }

        private void cmdAddAll_Click(object sender, EventArgs e)
        {
            foreach (ListItem objListItem in lstAvailable.Items)
            {
                lstAssigned.Items.Add(objListItem);
            }
            lstAvailable.Items.Clear();
            lstAvailable.ClearSelection();
            lstAssigned.ClearSelection();
            Sort(lstAssigned);
        }

        private void cmdRemoveAll_Click(object sender, EventArgs e)
        {
            foreach (ListItem objListItem in lstAssigned.Items)
            {
                lstAvailable.Items.Add(objListItem);
            }
            lstAssigned.Items.Clear();
            lstAvailable.ClearSelection();
            lstAssigned.ClearSelection();
            Sort(lstAvailable);
        }
		
		#endregion
		
		#region "Private Methods"

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
		
		#endregion
    }

    public class ListItemComparer : IComparer
    {
        #region IComparer Members

        public int Compare(object x, object y)
        {
            var a = (ListItem) x;
            var b = (ListItem) y;
            var c = new CaseInsensitiveComparer();
            return c.Compare(a.Text, b.Text);
        }

        #endregion
    }
}