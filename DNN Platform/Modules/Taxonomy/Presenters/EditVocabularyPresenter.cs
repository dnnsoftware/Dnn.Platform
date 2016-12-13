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
using System.Linq;

using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Content.Data;
using DotNetNuke.Entities.Content.Taxonomy;
using DotNetNuke.Modules.Taxonomy.Views;
using DotNetNuke.Modules.Taxonomy.Views.Models;
using DotNetNuke.UI.Skins.Controls;
using DotNetNuke.Web.Mvp;
using DotNetNuke.Web.UI.WebControls;
using DotNetNuke.Web.Validators;


#endregion

namespace DotNetNuke.Modules.Taxonomy.Presenters
{

    public class EditVocabularyPresenter : ModulePresenter<IEditVocabularyView, EditVocabularyModel>
    {

        private readonly ITermController _termController;
        private readonly IVocabularyController _vocabularyController;

        #region Constructors

        public EditVocabularyPresenter(IEditVocabularyView editView) : this(editView, new VocabularyController(new DataService()), new TermController(new DataService()))
        {
        }

        public EditVocabularyPresenter(IEditVocabularyView editView, IVocabularyController vocabularyController, ITermController termController) : base(editView)
        {
            Requires.NotNull("vocabularyController", vocabularyController);
            Requires.NotNull("termController", termController);

            _vocabularyController = vocabularyController;
            _termController = termController;

            View.AddTerm += AddTerm;
            View.CancelTerm += CancelTerm;
            View.Delete += DeleteVocabulary;
            View.DeleteTerm += DeleteTerm;
            View.Save += SaveVocabulary;
            View.SaveTerm += SaveTerm;
            View.SelectTerm += SelectTerm;
        }

        #endregion

        #region Public Properties

        public bool IsDeleteEnabled
        {
            get
            {
                var isEnabled = IsEditEnabled;
                if (isEnabled)
                {
                    if (View.Model != null && View.Model.Vocabulary != null && View.Model.Vocabulary.IsSystem)
                    {
                        isEnabled = Null.NullBoolean;
                    }
                }
                return isEnabled;
            }
        }

        public bool IsEditEnabled
        {
            get
            {
                var isEnabled = IsSuperUser;
                if (!isEnabled)
                {
                    //Check Portal Scope
                    if (View.Model != null && View.Model.Vocabulary != null && View.Model.Vocabulary.ScopeType != null)
                    {
                        isEnabled = String.Compare(View.Model.Vocabulary.ScopeType.ScopeType, "Portal", false) == 0;
                    }
                }
                return isEnabled;
            }
        }

        public bool IsHeirarchical
        {
            get
            {
                var isHeirarchical = Null.NullBoolean;
                if (View.Model.Vocabulary != null)
                {
                    isHeirarchical = (View.Model.Vocabulary.Type == VocabularyType.Hierarchy);
                }
                return isHeirarchical;
            }
        }

        public ITermController TermController
        {
            get
            {
                return _termController;
            }
        }

        public IVocabularyController VocabularyController
        {
            get
            {
                return _vocabularyController;
            }
        }

        public int VocabularyId
        {
            get
            {
                var vocabularyId = Null.NullInteger;
                if (!string.IsNullOrEmpty(Request.Params["VocabularyId"]))
                {
                    vocabularyId = Int32.Parse(Request.Params["VocabularyId"]);
                }
                return vocabularyId;
            }
        }

        #endregion

        #region Private Methods

        private void RefreshTerms()
        {
            View.Model.Terms = TermController.GetTermsByVocabulary(VocabularyId).ToList();
            View.BindTerms(View.Model.Terms, IsHeirarchical, true);
            View.Model.Term = null;
            View.ClearSelectedTerm();
            View.ShowTermEditor(false);
        }

        #endregion

        #region Protected Methods

        protected override void OnInit()
        {
            base.OnInit();

            if (View.Model.Vocabulary == null)
            {
                View.Model.Vocabulary = VocabularyController.GetVocabularies().Where(v => v.VocabularyId == VocabularyId).SingleOrDefault();
                View.Model.Terms = TermController.GetTermsByVocabulary(VocabularyId).ToList();
            }
        }

        protected override void OnLoad()
        {
            base.OnLoad();

            View.Model.CancelUrl = Globals.NavigateURL(TabId);
            View.BindVocabulary(View.Model.Vocabulary, IsEditEnabled, IsDeleteEnabled, IsSuperUser);
            View.BindTerms(View.Model.Terms, IsHeirarchical, !IsPostBack);
        }

        #endregion

        #region Public Methods

        public void AddTerm(object sender, EventArgs e)
        {
            View.Model.Term = new Term(View.Model.Vocabulary.VocabularyId);
            View.BindTerm(View.Model.Term, View.Model.Terms, IsHeirarchical, false, IsEditEnabled);
            View.ShowTermEditor(true);
            View.SetTermEditorMode(true, Null.NullInteger);
        }

        public void CancelTerm(object sender, EventArgs e)
        {
            View.Model.Term = null;
            View.ClearSelectedTerm();
            View.ShowTermEditor(false);
        }

        public void DeleteTerm(object sender, EventArgs e)
        {
            TermController.DeleteTerm(View.Model.Term);
            RefreshTerms();
        }

        public void DeleteVocabulary(object sender, EventArgs e)
        {
            VocabularyController.DeleteVocabulary(View.Model.Vocabulary);
            Response.Redirect(Globals.NavigateURL(TabId));
        }

        public void SaveTerm(object sender, EventArgs e)
        {
            //First Bind the term so we can get the current values from the View
            View.BindTerm(View.Model.Term, View.Model.Terms, IsHeirarchical, true, IsEditEnabled);

            var result = Validator.ValidateObject(View.Model.Term);
            if (result.IsValid)
            {
                if (View.Model.Term.TermId == Null.NullInteger)
                {
                    TermController.AddTerm(View.Model.Term);
                }
                else
                {
                    TermController.UpdateTerm(View.Model.Term);
                }

                RefreshTerms();
            }
            else
            {
                ShowMessage("TermValidationError", ModuleMessage.ModuleMessageType.RedError);
            }
        }

        public void SaveVocabulary(object sender, EventArgs e)
        {
            View.BindVocabulary(View.Model.Vocabulary, IsEditEnabled, IsDeleteEnabled, IsSuperUser);

            var result = Validator.ValidateObject(View.Model.Vocabulary);
            if (result.IsValid)
            {
                VocabularyController.UpdateVocabulary(View.Model.Vocabulary);
                Response.Redirect(Globals.NavigateURL(TabId));
            }
            else
            {
                ShowMessage("VocabularyValidationError", ModuleMessage.ModuleMessageType.RedError);
            }
        }

        public void SelectTerm(object sender, TermsEventArgs e)
        {
            View.Model.Term = e.SelectedTerm;
            View.BindTerm(View.Model.Term, View.Model.Terms, IsHeirarchical, false, IsEditEnabled);
            View.ShowTermEditor(true);
            View.SetTermEditorMode(false, View.Model.Term.TermId);
        }

        #endregion

    }
}