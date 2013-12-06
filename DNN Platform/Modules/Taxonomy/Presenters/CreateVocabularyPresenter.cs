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
using System.Linq;

using DotNetNuke.Common;
using DotNetNuke.Entities.Content.Data;
using DotNetNuke.Entities.Content.Taxonomy;
using DotNetNuke.Modules.Taxonomy.Validators;
using DotNetNuke.Modules.Taxonomy.Views;
using DotNetNuke.Modules.Taxonomy.Views.Models;
using DotNetNuke.UI.Skins.Controls;
using DotNetNuke.Web.Mvp;
using DotNetNuke.Web.Validators;


#endregion

namespace DotNetNuke.Modules.Taxonomy.Presenters
{

    public class CreateVocabularyPresenter : ModulePresenter<ICreateVocabularyView, CreateVocabularyModel>
    {

        private readonly IScopeTypeController _scopeTypeController;
        private readonly IVocabularyController _vocabularyController;

        #region Constructors

        public CreateVocabularyPresenter(ICreateVocabularyView createView) : this(createView, new VocabularyController(new DataService()), new ScopeTypeController(new DataService()))
        {
        }

        public CreateVocabularyPresenter(ICreateVocabularyView createView, IVocabularyController vocabularyController, IScopeTypeController scopeTypeController) : base(createView)
        {
            Requires.NotNull("vocabularyController", vocabularyController);
            Requires.NotNull("scopeTypeController", scopeTypeController);

            _vocabularyController = vocabularyController;
            _scopeTypeController = scopeTypeController;

            View.Save += Save;
            View.Model.Vocabulary = GetVocabulary();
        }

        #endregion

        #region Public Properties

        public IScopeTypeController ScopeTypeController
        {
            get
            {
                return _scopeTypeController;
            }
        }

        public IVocabularyController VocabularyController
        {
            get
            {
                return _vocabularyController;
            }
        }

        #endregion

        #region Private Methods

        private Vocabulary GetVocabulary()
        {
            var vocabulary = new Vocabulary();
            var scopeType = IsSuperUser ? ScopeTypeController.GetScopeTypes().Where(s => s.ScopeType == "Application").SingleOrDefault() : ScopeTypeController.GetScopeTypes().Where(s => s.ScopeType == "Portal").SingleOrDefault();

            if (scopeType != null)
            {
                vocabulary.ScopeTypeId = scopeType.ScopeTypeId;
            }
            vocabulary.Type = VocabularyType.Simple;

            return vocabulary;
        }

        #endregion

        protected override void OnLoad()
        {
            base.OnLoad();
            View.Model.TaxonomyHomeUrl = Globals.NavigateURL(TabId);
            View.BindVocabulary(View.Model.Vocabulary, IsSuperUser);

            Validator.Validators.Add(new VocabularyNameValidator(VocabularyController));
        }

        #region Public Methods

        public void Save(object sender, EventArgs e)
        {
            //Bind Model
            View.BindVocabulary(View.Model.Vocabulary, IsSuperUser);
            if (View.Model.Vocabulary.ScopeType.ScopeType == "Portal")
            {
                View.Model.Vocabulary.ScopeId = PortalId;
            }

            //Validate Model
            var result = Validator.ValidateObject(View.Model.Vocabulary);

            if (result.IsValid)
            {
                //Save Vocabulary
                VocabularyController.AddVocabulary(View.Model.Vocabulary);

                //Redirect to Vocabulary List
                Response.Redirect(Globals.NavigateURL(TabId));
            }
            else
            {
                foreach (var error in result.Errors)
                {
                    var message = string.Format(LocalizeString(error.ErrorMessage), View.Model.Vocabulary.Name);
                    ShowMessage(message, ModuleMessage.ModuleMessageType.RedError, false);
                }
            }
        }

        #endregion

    }
}