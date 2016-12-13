#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2017
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
using DotNetNuke.Entities.Content.Taxonomy;
using DotNetNuke.Framework;
using DotNetNuke.Framework.JavaScriptLibraries;
using DotNetNuke.Modules.Taxonomy.Presenters;
using DotNetNuke.Modules.Taxonomy.Views.Models;
using DotNetNuke.Services.Localization;
using DotNetNuke.Web.Client.ClientResourceManagement;
using DotNetNuke.Web.Mvp;
using DotNetNuke.Web.UI.WebControls;
using WebFormsMvp;

#endregion

namespace DotNetNuke.Modules.Taxonomy.Views
{

    [PresenterBinding(typeof (EditVocabularyPresenter))]
    public partial class EditVocabulary : ModuleView<EditVocabularyModel>, IEditVocabularyView
    {

        #region IEditVocabularyView Implementation

        public event EventHandler AddTerm;
        public event EventHandler CancelTerm;
        public event EventHandler Delete;
        public event EventHandler DeleteTerm;
        public event EventHandler Save;
        public event EventHandler SaveTerm;
        public event EventHandler<TermsEventArgs> SelectTerm;


        public void BindTerm(Term term, IEnumerable<Term> terms, bool isHeirarchical, bool loadFromControl, bool editEnabled)
        {
            editTermControl.BindTerm(term, terms, isHeirarchical, loadFromControl, editEnabled);
        }

        public void BindTerms(IEnumerable<Term> terms, bool isHeirarchical, bool dataBind)
        {
            termsList.BindTerms(terms.ToList(), isHeirarchical, dataBind);
        }

        public void BindVocabulary(Vocabulary vocabulary, bool editEnabled, bool deleteEnabled, bool showScope)
        {
            editVocabularyControl.BindVocabulary(vocabulary, editEnabled, showScope);
            saveVocabulary.Enabled = editEnabled;
            deleteVocabulary.Visible = deleteEnabled;
            addTermButton.Enabled = editEnabled;
            saveTermButton.Enabled = editEnabled;
            deleteTermButton.Enabled = editEnabled;
            cancelEdit.NavigateUrl = Model.CancelUrl;
        }

        public void ClearSelectedTerm()
        {
            termsList.ClearSelectedTerm();
            pnlTermEditor.Visible = false;
            pnlVocabTerms.Visible = true;
        }

        public void SetTermEditorMode(bool isAddMode, int termId)
        {
            if (isAddMode)
            {
                termLabel.Text = Localization.GetString("NewTerm", LocalResourceFile);
                saveTermButton.Text = Localization.GetString("NewTerm", LocalResourceFile);
            }
            else
            {
                termLabel.Text = Localization.GetString("CurrentTerm", LocalResourceFile);
                saveTermButton.Text = Localization.GetString("CurrentTerm", LocalResourceFile); 
            }

            deleteVocabulary.Visible = !isAddMode;
        }

        public void ShowTermEditor(bool showEditor)
        {
            pnlTermEditor.Visible = showEditor;
            pnlVocabTerms.Visible = !showEditor;
        }

        #endregion

        #region Event Handlers

		protected override void OnInit(EventArgs e)
		{
			base.OnInit(e);
            ClientResourceManager.EnableAsyncPostBackHandler();
            editVocabularyControl.LocalResourceFile = LocalResourceFile;
		}

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            JavaScript.RequestRegistration(CommonJs.DnnPlugins);
			ServicesFramework.Instance.RequestAjaxAntiForgerySupport();

            addTermButton.Click += OnAddTermClick;
            cancelTermButton.Click += OnCancelTermClick;
            deleteTermButton.Click += OnDeleteTermClick;
            deleteVocabulary.Click += OnDeleteVocabClick;
            saveTermButton.Click += OnSaveTermClick;
            saveVocabulary.Click += OnSaveVocabClick;
            termsList.SelectedTermChanged += OnTermsListIndexChanged;
        }

        protected void OnAddTermClick(object sender, EventArgs e)
        {
            if (AddTerm != null)
            {
                deleteTermButton.Visible = false;
                AddTerm(this, e);
            }
        }

        protected void OnCancelTermClick(object sender, EventArgs e)
        {
            if (CancelTerm != null)
            {
                CancelTerm(this, e);
            }
        }

        protected void OnDeleteTermClick(object sender, EventArgs e)
        {
            if (DeleteTerm != null)
            {
                DeleteTerm(this, e);
            }
        }

        protected void OnDeleteVocabClick(object sender, EventArgs e)
        {
            if (Delete != null)
            {
                Delete(this, e);
            }
        }

        protected void OnSaveTermClick(object sender, EventArgs e)
        {
            if (SaveTerm != null)
            {
                SaveTerm(this, e);
            }
        }

        protected void OnSaveVocabClick(object sender, EventArgs e)
        {
            if (Save != null)
            {
                Save(this, e);
            }
        }

        protected void OnTermsListIndexChanged(object sender, TermsEventArgs e)
        {
            if (SelectTerm != null)
            {
                deleteTermButton.Visible = true;
                SelectTerm(this, e);
            }
        }

        #endregion

    }
}