// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Web.UI.WebControls
{
    using System;
    using System.Collections.Generic;
    using System.Web.UI.WebControls;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Content.Taxonomy;
    using Telerik.Web.UI;

    public class TermsList : WebControl
    {
        private bool _IsHeirarchical;
        private DnnListBox _ListBox;

        private DnnTreeView _TreeView;

        public event EventHandler<TermsEventArgs> SelectedTermChanged;

        public bool IsHeirarchical
        {
            get
            {
                return this._IsHeirarchical;
            }
        }

        public Term SelectedTerm
        {
            get
            {
                Term _SelectedTerm = null;
                if (!string.IsNullOrEmpty(this.SelectedValue))
                {
                    int _TermId = int.Parse(this.SelectedValue);
                    foreach (Term term in this.Terms)
                    {
                        if (term.TermId == _TermId)
                        {
                            _SelectedTerm = term;
                            break;
                        }
                    }
                }

                return _SelectedTerm;
            }
        }

        public string SelectedValue
        {
            get
            {
                string _SelectedValue = Null.NullString;
                if (this.IsHeirarchical)
                {
                    _SelectedValue = this._TreeView.SelectedValue;
                }
                else
                {
                    _SelectedValue = this._ListBox.SelectedValue;
                }

                return _SelectedValue;
            }
        }

        public List<Term> Terms
        {
            get
            {
                object _DataSource = null;
                if (this.IsHeirarchical)
                {
                    _DataSource = this._TreeView.DataSource;
                }
                else
                {
                    _DataSource = this._ListBox.DataSource;
                }

                return _DataSource as List<Term>;
            }
        }

        public void BindTerms(List<Term> terms, bool isHeirarchical, bool dataBind)
        {
            this._IsHeirarchical = isHeirarchical;

            this._ListBox.DataSource = terms;
            this._TreeView.DataSource = terms;

            if (dataBind)
            {
                this._ListBox.DataBind();
                this._TreeView.DataBind();
            }
        }

        public void ClearSelectedTerm()
        {
            this._ListBox.SelectedIndex = Null.NullInteger;
            this._TreeView.UnselectAllNodes();
        }

        protected override void CreateChildControls()
        {
            this.Controls.Clear();

            this._ListBox = new DnnListBox();
            this._ListBox.ID = string.Concat(this.ID, "_List");
            this._ListBox.DataTextField = "Name";
            this._ListBox.DataValueField = "TermId";
            this._ListBox.AutoPostBack = true;
            this._ListBox.SelectedIndexChanged += this.ListBoxSelectedIndexChanged;

            this._TreeView = new DnnTreeView();
            this._TreeView.ID = string.Concat(this.ID, "_Tree");
            this._TreeView.DataTextField = "Name";
            this._TreeView.DataValueField = "TermId";
            this._TreeView.DataFieldID = "TermId";
            this._TreeView.DataFieldParentID = "ParentTermId";
            this._TreeView.NodeClick += this.TreeViewNodeClick;

            this.Controls.Add(this._ListBox);
            this.Controls.Add(this._TreeView);
        }

        protected override void OnInit(EventArgs e)
        {
            this.EnsureChildControls();
        }

        protected override void OnPreRender(EventArgs e)
        {
            this._ListBox.Visible = !this.IsHeirarchical;
            this._TreeView.Visible = this.IsHeirarchical;

            this._ListBox.Height = this.Height;
            this._ListBox.Width = this.Width;
            this._TreeView.Height = this.Height;
            this._TreeView.Width = this.Width;

            this._TreeView.ExpandAllNodes();

            base.OnPreRender(e);
        }

        protected virtual void OnSelectedTermChanged(TermsEventArgs e)
        {
            // Raise the SelectedTermChanged Event
            if (this.SelectedTermChanged != null)
            {
                this.SelectedTermChanged(this, e);
            }
        }

        private void ListBoxSelectedIndexChanged(object sender, EventArgs e)
        {
            // Raise the SelectedTermChanged Event
            this.OnSelectedTermChanged(new TermsEventArgs(this.SelectedTerm));
        }

        private void TreeViewNodeClick(object sender, RadTreeNodeEventArgs e)
        {
            // Raise the SelectedTermChanged Event
            this.OnSelectedTermChanged(new TermsEventArgs(this.SelectedTerm));
        }
    }
}
