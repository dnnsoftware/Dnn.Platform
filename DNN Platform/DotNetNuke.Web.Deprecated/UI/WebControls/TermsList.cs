// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
#region Usings

using System;
using System.Collections.Generic;
using System.Web.UI.WebControls;

using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Content.Taxonomy;

using Telerik.Web.UI;


#endregion

namespace DotNetNuke.Web.UI.WebControls
{
    public class TermsList : WebControl
    {
        private bool _IsHeirarchical;
        private DnnListBox _ListBox;

        private DnnTreeView _TreeView;

        #region "Events"

        public event EventHandler<TermsEventArgs> SelectedTermChanged;

        #endregion

        #region "Public Properties"

        public bool IsHeirarchical
        {
            get
            {
                return _IsHeirarchical;
            }
        }

        public Term SelectedTerm
        {
            get
            {
                Term _SelectedTerm = null;
                if (!string.IsNullOrEmpty(SelectedValue))
                {
                    int _TermId = int.Parse(SelectedValue);
                    foreach (Term term in Terms)
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
                if (IsHeirarchical)
                {
                    _SelectedValue = _TreeView.SelectedValue;
                }
                else
                {
                    _SelectedValue = _ListBox.SelectedValue;
                }
                return _SelectedValue;
            }
        }

        public List<Term> Terms
        {
            get
            {
                object _DataSource = null;
                if (IsHeirarchical)
                {
                    _DataSource = _TreeView.DataSource;
                }
                else
                {
                    _DataSource = _ListBox.DataSource;
                }
                return _DataSource as List<Term>;
            }
        }

        #endregion

        #region "Protected Methods"

        protected override void CreateChildControls()
        {
            Controls.Clear();

            _ListBox = new DnnListBox();
            _ListBox.ID = string.Concat(ID, "_List");
            _ListBox.DataTextField = "Name";
            _ListBox.DataValueField = "TermId";
            _ListBox.AutoPostBack = true;
            _ListBox.SelectedIndexChanged += ListBoxSelectedIndexChanged;

            _TreeView = new DnnTreeView();
            _TreeView.ID = string.Concat(ID, "_Tree");
            _TreeView.DataTextField = "Name";
            _TreeView.DataValueField = "TermId";
            _TreeView.DataFieldID = "TermId";
            _TreeView.DataFieldParentID = "ParentTermId";
            _TreeView.NodeClick += TreeViewNodeClick;

            Controls.Add(_ListBox);
            Controls.Add(_TreeView);
        }

        protected override void OnInit(EventArgs e)
        {
            EnsureChildControls();
        }

        protected override void OnPreRender(EventArgs e)
        {
            _ListBox.Visible = !IsHeirarchical;
            _TreeView.Visible = IsHeirarchical;

            _ListBox.Height = Height;
            _ListBox.Width = Width;
            _TreeView.Height = Height;
            _TreeView.Width = Width;

            _TreeView.ExpandAllNodes();

            base.OnPreRender(e);
        }

        protected virtual void OnSelectedTermChanged(TermsEventArgs e)
        {
            //Raise the SelectedTermChanged Event
            if (SelectedTermChanged != null)
            {
                SelectedTermChanged(this, e);
            }
        }

        #endregion

        #region "Event Handlers"

        private void ListBoxSelectedIndexChanged(object sender, EventArgs e)
        {
            //Raise the SelectedTermChanged Event
            OnSelectedTermChanged(new TermsEventArgs(SelectedTerm));
        }

        private void TreeViewNodeClick(object sender, RadTreeNodeEventArgs e)
        {
            //Raise the SelectedTermChanged Event
            OnSelectedTermChanged(new TermsEventArgs(SelectedTerm));
        }

        #endregion

        #region "Public Methods"

        public void BindTerms(List<Term> terms, bool isHeirarchical, bool dataBind)
        {
            _IsHeirarchical = isHeirarchical;

            _ListBox.DataSource = terms;
            _TreeView.DataSource = terms;

            if (dataBind)
            {
                _ListBox.DataBind();
                _TreeView.DataBind();
            }
        }

        public void ClearSelectedTerm()
        {
            _ListBox.SelectedIndex = Null.NullInteger;
            _TreeView.UnselectAllNodes();
        }

        #endregion
    }
}
