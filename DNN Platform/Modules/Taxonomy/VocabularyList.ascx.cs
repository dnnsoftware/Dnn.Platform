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
using System.Web.UI.WebControls;

using DotNetNuke.Common;
using DotNetNuke.Modules.Taxonomy.Presenters;
using DotNetNuke.Modules.Taxonomy.Views.Models;
using DotNetNuke.Services.Localization;
using DotNetNuke.Web.Mvp;
using DotNetNuke.Web.UI.WebControls;

using Telerik.Web.UI;

using WebFormsMvp;

#endregion

namespace DotNetNuke.Modules.Taxonomy.Views
{

    [PresenterBinding(typeof (VocabularyListPresenter))]
    public partial class VocabularyList : ModuleView<VocabularyListModel>, IVocabularyListView
    {

        #region Public Events

        public event GridNeedDataSourceEventHandler GridsNeedDataSource;
        public event GridItemEventHandler GridsItemDataBound;

        #endregion

        #region Protected Methods

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            vocabulariesGrid.NeedDataSource += VocabularyNeedDataSource;
            vocabulariesGrid.ItemDataBound += VocabularyGridItemDataBound;
        }

        #endregion

        #region IVocabularyListView Implementation

        public void ShowAddButton(bool showButton)
        {
            hlAddVocab.Visible = showButton;
        }

        public void Refresh()
        {
            hlAddVocab.NavigateUrl = Model.NewVocabUrl;
        }

        protected void VocabularyGridItemDataBound(object sender, GridItemEventArgs e)
        {
            GridsItemDataBound(sender, e);
        }

        protected void VocabularyNeedDataSource(object sender, GridNeedDataSourceEventArgs e)
        {
            GridsNeedDataSource(sender, e);
        }

        #endregion

    }
}