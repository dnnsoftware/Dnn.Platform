#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2013
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
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;

using DotNetNuke.Entities.Content.Common;
using DotNetNuke.Entities.Content.Taxonomy;
using DotNetNuke.Framework;

using Telerik.Web.UI;

#endregion

namespace DotNetNuke.Web.UI.WebControls
{
    public class TermsSelector : DnnComboBox
    {
        public event EventHandler DataSourceChanged;
        public TermsSelector()
        {
            IncludeSystemVocabularies = false;
            IncludeTags = true;
        }

        #region "Public Properties"

        public int PortalId
        {
            get
            {
                return Convert.ToInt32(ViewState["PortalId"]);
            }
            set
            {
                ViewState["PortalId"] = value;
                OnDataSourceChanged();
            }
        }

        public bool IncludeSystemVocabularies
        {
            get
            {
                return Convert.ToBoolean(ViewState["IncludeSystemVocabularies"]);
            }
            set
            {
                ViewState["IncludeSystemVocabularies"] = value;
                OnDataSourceChanged();
            }
            
        }

        public bool IncludeTags
        {
            get
            {
                return Convert.ToBoolean(ViewState["IncludeTags"]);
            }
            set
            {
                ViewState["IncludeTags"] = value;
                OnDataSourceChanged();
            }

        }

		public List<Term> Terms
		{
			get
			{
				return ViewState["Terms"] as List<Term>;
			}
			set
			{
				ViewState["Terms"] = value;
			}
		}

        #endregion

        #region "Protected Methods"

        protected override void OnInit(EventArgs e)
        {
            ItemTemplate = new TreeViewTemplate();
            Items.Add(new RadComboBoxItem());
            base.OnInit(e);

			OnClientDropDownOpened = "dnn.controls.termsSelector.OnClientDropDownOpened";
			if (!string.IsNullOrEmpty(CssClass))
			{
				CssClass = string.Format("{0} TermsSelector", CssClass);
			}
			else
			{
				CssClass = "TermsSelector";
			}
        }

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			if(Page.IsPostBack)
			{
				if(Terms == null)
				{
					Terms = new List<Term>();
				}
				else
				{
					Terms.Clear();
				}
				
				if (!string.IsNullOrEmpty(SelectedValue))
				{
					foreach (var id in SelectedValue.Split(','))
					{
						var termId = Convert.ToInt32(id.Trim());
						var term = Util.GetTermController().GetTerm(termId);
						if (term != null)
						{
							Terms.Add(term);
						}
					}

					//clear the append item by client side
					if(Items.Count > 1)
					{
						Items.Remove(1);
					}
				}
			}
		}

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            Text = Terms.ToDelimittedString(", ");
            ToolTip = Terms.ToDelimittedString(", ");
        }

        protected override void LoadViewState(object savedState)
        {
            base.LoadViewState(savedState);

            OnDataSourceChanged();
        }

		protected override object SaveViewState()
		{
			Page.ClientScript.RegisterClientScriptResource(GetType(), "DotNetNuke.Web.UI.WebControls.Resources.TermsSelector.js");

			return base.SaveViewState();
		}

        protected void OnDataSourceChanged()
        {
            if(DataSourceChanged != null)
            {
                DataSourceChanged(this, new EventArgs());
            }
        }
        #endregion

        #region "Private Template Class"

        public class TreeViewTemplate : ITemplate
		{
			#region Private Fields

			private RadComboBoxItem _container;
            private List<Term> _terms;
            private TermsSelector _termsSelector;

            private DnnTreeView _tree;

			#endregion

			#region Private Properties

			private bool IncludeSystemVocabularies
            {
                get
                {
                    return _termsSelector.IncludeSystemVocabularies;
                }
            }

            private bool IncludeTags
            {
                get
                {
                    return _termsSelector.IncludeTags;
                }
            }

            private int PortalId
            {
                get
                {
                    return _termsSelector.PortalId;
                }
            }

            private List<Term> SelectedTerms
            {
                get
                {
                    return _termsSelector.Terms;
                }
            }

            private List<Term> Terms
            {
                get
                {
                    if (_terms == null)
                    {
                         IVocabularyController vocabRep = Util.GetVocabularyController();
                        _terms = new List<Term>();
                        var vocabularies = from v in vocabRep.GetVocabularies() where v.ScopeType.ScopeType == "Application" || (v.ScopeType.ScopeType == "Portal" && v.ScopeId == PortalId) select v;

                        foreach (Vocabulary v in vocabularies)
                        {
                            if(v.IsSystem)
                            {
                                if (IncludeSystemVocabularies || (IncludeTags && v.Name == "Tags"))
                                {
                                    AddTerms(v);
                                }
                            }
                            else
                            {
                                AddTerms(v);
                            }
                        }
                    }
                    return _terms;
                }
            }

			#endregion

			#region Private Methods

			private void AddTerms(Vocabulary v)
            {
                ITermController termRep = Util.GetTermController();
                
                //Add a dummy parent term if simple vocabulary
                if (v.Type == VocabularyType.Simple)
                {
                    Term dummyTerm = new Term(v.VocabularyId);
                    dummyTerm.ParentTermId = null;
                    dummyTerm.Name = v.Name;
                    dummyTerm.TermId = -v.VocabularyId;
                    _terms.Add(dummyTerm);
                }
                foreach (Term t in termRep.GetTermsByVocabulary(v.VocabularyId))
                {
                    if (v.Type == VocabularyType.Simple)
                    {
                        t.ParentTermId = -v.VocabularyId;
                    }
                    _terms.Add(t);
                }
                
            }

			#endregion

			#region ITemplate Members

			public void InstantiateIn(Control container)
            {
                _container = (RadComboBoxItem) container;
                _termsSelector = (TermsSelector) container.Parent;

                _tree = new DnnTreeView();
	            _tree.ID = string.Format("{0}_TreeView", _termsSelector.ID);
                _tree.DataTextField = "Name";
                _tree.DataValueField = "TermId";
                _tree.DataFieldID = "TermId";
                _tree.DataFieldParentID = "ParentTermId";
                _tree.CheckBoxes = true;
                _tree.ExpandAllNodes();

				//bind client-side events
	            _tree.OnClientNodeChecked = "dnn.controls.termsSelector.OnClientNodeChecked";

                _tree.DataSource = Terms;

                _tree.NodeDataBound += TreeNodeDataBound;
                _tree.DataBound += TreeDataBound;

                _container.Controls.Add(_tree);

                _termsSelector.DataSourceChanged += TermsSelector_DataSourceChanged;
            }

            #endregion

            private void TreeDataBound(object sender, EventArgs e)
            {
                _tree.ExpandAllNodes();
            }

            private void TreeNodeDataBound(object sender, RadTreeNodeEventArgs e)
            {
                RadTreeNode node = e.Node;
                Term term = node.DataItem as Term;

                if (term.TermId < 0)
                {
                    node.Checkable = false;
                }
                foreach (Term tag in SelectedTerms)
                {
                    if (tag.TermId == term.TermId)
                    {
                        node.Checked = true;
                        break;
                    }
                }
            }

            private void TermsSelector_DataSourceChanged(object sender, EventArgs e)
            {
                _terms = null;
                _tree.DataSource = Terms;
            }
        }

        #endregion
    }
}