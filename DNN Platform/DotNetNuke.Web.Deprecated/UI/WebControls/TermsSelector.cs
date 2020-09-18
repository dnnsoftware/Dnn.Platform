// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Web.UI.WebControls
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web.Script.Serialization;
    using System.Web.UI;
    using System.Web.UI.WebControls;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Content.Common;
    using DotNetNuke.Entities.Content.Taxonomy;
    using DotNetNuke.Framework;
    using DotNetNuke.Framework.JavaScriptLibraries;
    using DotNetNuke.UI.Utilities;
    using DotNetNuke.Web.Client.ClientResourceManagement;
    using Telerik.Web.UI;

    public class TermsSelector : DnnComboBox, IClientAPICallbackEventHandler
    {
        public TermsSelector()
        {
            this.IncludeSystemVocabularies = false;
            this.IncludeTags = true;
            this.EnableViewState = false;
        }

        public int PortalId { get; set; }

        public bool IncludeSystemVocabularies { get; set; }

        public bool IncludeTags { get; set; }

        public List<Term> Terms { get; set; }

        public string RaiseClientAPICallbackEvent(string eventArgument)
        {
            var parameters = eventArgument.Split('-');
            this.PortalId = Convert.ToInt32(parameters[1]);
            this.IncludeTags = Convert.ToBoolean(parameters[2]);
            this.IncludeSystemVocabularies = Convert.ToBoolean(parameters[3]);
            var terms = this.GetTerms();
            terms.Insert(0, new { clientId = parameters[0] });
            var serializer = new JavaScriptSerializer();
            return serializer.Serialize(terms);
        }

        protected override void OnInit(EventArgs e)
        {
            this.ItemTemplate = new TreeViewTemplate();
            this.Items.Add(new RadComboBoxItem());
            base.OnInit(e);

            JavaScript.RequestRegistration(CommonJs.jQueryMigrate);

            this.OnClientDropDownOpened = "webcontrols.termsSelector.OnClientDropDownOpened";
            if (!string.IsNullOrEmpty(this.CssClass))
            {
                this.CssClass = string.Format("{0} TermsSelector", this.CssClass);
            }
            else
            {
                this.CssClass = "TermsSelector";
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (this.Page.IsPostBack)
            {
                if (this.Terms == null)
                {
                    this.Terms = new List<Term>();
                }
                else
                {
                    this.Terms.Clear();
                }

                if (!string.IsNullOrEmpty(this.SelectedValue))
                {
                    foreach (var id in this.SelectedValue.Split(','))
                    {
                        var termId = Convert.ToInt32(id.Trim());
                        var term = Util.GetTermController().GetTerm(termId);
                        if (term != null)
                        {
                            this.Terms.Add(term);
                        }
                    }

                    // clear the append item by client side
                    if (this.Items.Count > 1)
                    {
                        this.Items.Remove(1);
                    }
                }
            }

            if (!this.Page.IsPostBack)
            {
                this.Page.ClientScript.RegisterClientScriptResource(this.GetType(), "DotNetNuke.Web.UI.WebControls.Resources.TermsSelector.js");

                ClientResourceManager.RegisterStyleSheet(
                    this.Page,
                    this.Page.ClientScript.GetWebResourceUrl(this.GetType(), "DotNetNuke.Web.UI.WebControls.Resources.TermsSelector.css"));

                ClientAPI.RegisterClientVariable(this.Page, "TermsSelectorCallback",
                    ClientAPI.GetCallbackEventReference(this, "'[PARAMS]'", "webcontrols.termsSelector.itemDataLoaded", "this",
                        "webcontrols.termsSelector.itemDataLoadError"), true);
            }
        }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            if (this.Terms != null)
            {
                this.Attributes.Add("SelectedTerms", string.Join(",", this.Terms.Select(t => t.TermId.ToString()).ToArray()));
            }

            this.Attributes.Add("IncludeSystemVocabularies", this.IncludeSystemVocabularies.ToString().ToLowerInvariant());
            this.Attributes.Add("IncludeTags", this.IncludeTags.ToString().ToLowerInvariant());
            this.Attributes.Add("PortalId", this.PortalId.ToString());
        }

        private ArrayList GetTerms()
        {
            var vocabRep = Util.GetVocabularyController();
            var terms = new ArrayList();
            var vocabularies = from v in vocabRep.GetVocabularies() where v.ScopeType.ScopeType == "Application" || (v.ScopeType.ScopeType == "Portal" && v.ScopeId == this.PortalId) select v;

            foreach (Vocabulary v in vocabularies)
            {
                if (v.IsSystem)
                {
                    if (this.IncludeSystemVocabularies || (this.IncludeTags && v.Name == "Tags"))
                    {
                        this.AddTerms(v, terms);
                    }
                }
                else
                {
                    this.AddTerms(v, terms);
                }
            }

            return terms;
        }

        private void AddTerms(Vocabulary v, ArrayList terms)
        {
            ITermController termRep = Util.GetTermController();

            // Add a dummy parent term if simple vocabulary
            if (v.Type == VocabularyType.Simple)
            {
                terms.Add(new { termId = -v.VocabularyId, name = v.Name, parentTermId = Null.NullInteger });
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

        public class TreeViewTemplate : ITemplate
        {
            private RadComboBoxItem _container;
            private TermsSelector _termsSelector;

            private DnnTreeView _tree;

            public void InstantiateIn(Control container)
            {
                this._container = (RadComboBoxItem)container;
                this._termsSelector = (TermsSelector)container.Parent;

                this._tree = new DnnTreeView();
                this._tree.ID = string.Format("{0}_TreeView", this._termsSelector.ID);
                this._tree.CheckBoxes = true;
                this._tree.EnableViewState = false;

                // bind client-side events
                this._tree.OnClientNodeChecked = "webcontrols.termsSelector.OnClientNodeChecked";

                this._container.Controls.Add(this._tree);
            }
        }
    }
}
