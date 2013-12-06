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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web.Script.Serialization;
using System.Web.UI;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Content.Common;
using DotNetNuke.Entities.Content.Taxonomy;
using DotNetNuke.Framework;
using DotNetNuke.UI.Utilities;
using Telerik.Web.UI;

#endregion

namespace DotNetNuke.Web.UI.WebControls
{
    public class TermsSelector : DnnComboBox, IClientAPICallbackEventHandler
    {
        public TermsSelector()
        {
            IncludeSystemVocabularies = false;
            IncludeTags = true;
            EnableViewState = false;
        }

        #region Public Properties

		public int PortalId { get; set; }

		public bool IncludeSystemVocabularies { get; set; }

		public bool IncludeTags { get; set; }

		public List<Term> Terms { get; set; }

        #endregion

        #region Protected Methods

        protected override void OnInit(EventArgs e)
        {
            ItemTemplate = new TreeViewTemplate();
            Items.Add(new RadComboBoxItem());
            base.OnInit(e);

			OnClientDropDownOpened = "webcontrols.termsSelector.OnClientDropDownOpened";
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

			Page.ClientScript.RegisterClientScriptResource(GetType(), "DotNetNuke.Web.UI.WebControls.Resources.TermsSelector.js");

			ClientAPI.RegisterClientVariable(Page, "TermsSelectorCallback",
				ClientAPI.GetCallbackEventReference(this, "'[PARAMS]'", "webcontrols.termsSelector.itemDataLoaded", "this", "webcontrols.termsSelector.itemDataLoadError"), true);
		}

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            if (Terms != null)
            {
                Attributes.Add("SelectedTerms", String.Join(",", Terms.Select(t => t.TermId.ToString()).ToArray()));
            }
			Attributes.Add("IncludeSystemVocabularies", IncludeSystemVocabularies.ToString().ToLowerInvariant());
			Attributes.Add("IncludeTags", IncludeTags.ToString().ToLowerInvariant());
			Attributes.Add("PortalId", PortalId.ToString());
        }

        #endregion

        #region Private Template Class

        public class TreeViewTemplate : ITemplate
		{
			#region Private Fields

			private RadComboBoxItem _container;
            private TermsSelector _termsSelector;

            private DnnTreeView _tree;

			#endregion

			#region ITemplate Members

			public void InstantiateIn(Control container)
            {
                _container = (RadComboBoxItem) container;
                _termsSelector = (TermsSelector) container.Parent;

                _tree = new DnnTreeView();
	            _tree.ID = string.Format("{0}_TreeView", _termsSelector.ID);
                _tree.CheckBoxes = true;
				_tree.EnableViewState = false;

				//bind client-side events
				_tree.OnClientNodeChecked = "webcontrols.termsSelector.OnClientNodeChecked";

                _container.Controls.Add(_tree);
            }


            #endregion
        }

        #endregion

		#region IClientAPICallbackEventHandler Implementation

		public string RaiseClientAPICallbackEvent(string eventArgument)
		{
			var parameters = eventArgument.Split('-');
			PortalId = Convert.ToInt32(parameters[1]);
			IncludeTags = Convert.ToBoolean(parameters[2]);
			IncludeSystemVocabularies = Convert.ToBoolean(parameters[3]);
			var terms = GetTerms();
			terms.Insert(0, new { clientId = parameters[0]});
			var serializer = new JavaScriptSerializer();
			return serializer.Serialize(terms);
		}

		#endregion

		#region Private Methods

		private ArrayList GetTerms()
		{
			var vocabRep = Util.GetVocabularyController();
			var terms = new ArrayList();
			var vocabularies = from v in vocabRep.GetVocabularies() where v.ScopeType.ScopeType == "Application" || (v.ScopeType.ScopeType == "Portal" && v.ScopeId == PortalId) select v;

			foreach (Vocabulary v in vocabularies)
			{
				if (v.IsSystem)
				{
					if (IncludeSystemVocabularies || (IncludeTags && v.Name == "Tags"))
					{
						AddTerms(v, terms);
					}
				}
				else
				{
					AddTerms(v, terms);
				}
			}

			return terms;
		}

		private void AddTerms(Vocabulary v, ArrayList terms)
		{
			ITermController termRep = Util.GetTermController();

			//Add a dummy parent term if simple vocabulary
			if (v.Type == VocabularyType.Simple)
			{
				terms.Add(new { termId = -v.VocabularyId, name = v.Name, parentTermId = Null.NullInteger});
			}
			foreach (Term t in termRep.GetTermsByVocabulary(v.VocabularyId))
			{
				if (v.Type == VocabularyType.Simple)
				{
					t.ParentTermId = -v.VocabularyId;
				}
				terms.Add(new { termId = t.TermId, name = t.Name, parentTermId = t.ParentTermId });
			}

		}

		#endregion
	}
}