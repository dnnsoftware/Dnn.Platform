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

using DotNetNuke.Entities.Content.Taxonomy;
using DotNetNuke.Modules.Taxonomy.Views.Models;
using DotNetNuke.Web.Mvp;
using DotNetNuke.Web.UI.WebControls;


#endregion

namespace DotNetNuke.Modules.Taxonomy.Views
{
    public interface IEditVocabularyView : IModuleView<EditVocabularyModel>
    {
        void BindVocabulary(Vocabulary vocabulary, bool editEnabled, bool deleteEnabled, bool showScope);

        void BindTerms(IEnumerable<Term> terms, bool isHeirarchical, bool dataBind);

        void BindTerm(Term term, IEnumerable<Term> terms, bool isHeirarchical, bool loadFromControl, bool editEnabled);

        void ClearSelectedTerm();

        void SetTermEditorMode(bool isAddMode, int termId);

        void ShowTermEditor(bool showEditor);

        event EventHandler AddTerm;
        event EventHandler CancelTerm;
        event EventHandler Delete;
        event EventHandler DeleteTerm;
        event EventHandler Save;
        event EventHandler SaveTerm;

        event EventHandler<TermsEventArgs> SelectTerm;
    }
}