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
using System.Web.UI.WebControls;

using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Content.Data;
using DotNetNuke.Entities.Content.Taxonomy;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Modules.Taxonomy.Views;
using DotNetNuke.Modules.Taxonomy.Views.Models;
using DotNetNuke.Security.Permissions;
using DotNetNuke.Services.Localization;
using DotNetNuke.UI.Modules;
using DotNetNuke.Web.Mvp;

using Telerik.Web.UI;

#endregion

namespace DotNetNuke.Modules.Taxonomy.Presenters
{

    public class VocabularyListPresenter : ModulePresenter<IVocabularyListView, VocabularyListModel>
    {

        private readonly IVocabularyController _vocabularyController;

        #region Constructors

        public VocabularyListPresenter(IVocabularyListView view) : this(view, new VocabularyController(new DataService()))
        {
        }

        public VocabularyListPresenter(IVocabularyListView listView, IVocabularyController vocabularyController) : base(listView)
        {
            Requires.NotNull("vocabularyController", vocabularyController);

            _vocabularyController = vocabularyController;
        }

        #endregion

        #region Event Handlers

        protected override void OnLoad()
        {
            base.OnLoad();

            View.Model.CanEdit = ModulePermissionController.HasModulePermission(ModuleContext.Configuration.ModulePermissions, "EDIT");
            View.Model.NewVocabUrl = (ModuleContext != null)
                                         ? ModuleContext.NavigateUrl(TabId, "CreateVocabulary", false, "mid=" + ModuleId)
                                         : Globals.NavigateURL(TabId, "CreateVocabulary", "mid=" + ModuleId);
            View.ShowAddButton(View.Model.CanEdit);
            View.GridsNeedDataSource += GridNeedDataSource;
            View.GridsItemDataBound += GridItemDataBound;
            View.Refresh();
        }

        protected void GridNeedDataSource(object sender, GridNeedDataSourceEventArgs e)
        {
            var objGrid = (RadGrid)sender;

            objGrid.DataSource = (from v in _vocabularyController.GetVocabularies() where v.ScopeType.ScopeType == "Application" || (v.ScopeType.ScopeType == "Portal" && v.ScopeId == PortalId) select v).ToList();
        }

        protected void GridItemDataBound(object sender, GridItemEventArgs e)
        {
            if (!(e.Item is GridDataItem)) return;
            var vocabKey = (int)e.Item.OwnerTableView.DataKeyValues[e.Item.ItemIndex]["VocabularyId"];

            var dataItem = (GridDataItem)e.Item;

            var hlEdit = ((HyperLink)(dataItem)["EditItem"].FindControl("hlEdit"));
            hlEdit.NavigateUrl = ModuleContext.NavigateUrl(ModuleContext.TabId, "EditVocabulary", false,"mid=" + ModuleContext.ModuleId, "VocabularyId=" + vocabKey);
            hlEdit.Visible = View.Model.CanEdit;

            var imgEdit = ((Image)(dataItem)["EditItem"].FindControl("imgEdit"));
            imgEdit.AlternateText = Localization.GetString("Edit", LocalResourceFile);
            imgEdit.ToolTip = Localization.GetString("Edit", LocalResourceFile);
            imgEdit.Visible = View.Model.CanEdit;
        }

        #endregion

    }
}