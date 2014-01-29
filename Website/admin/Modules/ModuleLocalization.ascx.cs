#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2014
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
using System.Web.UI.WebControls;

using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Framework;
using DotNetNuke.Services.Localization;
using DotNetNuke.UI.Utilities;

using Telerik.Web.UI;


#endregion

namespace DotNetNuke.Admin.Modules
{
	public partial class ModuleLocalization : UserControlBase
	{
		private List<ModuleInfo> _Modules;
		private bool _ShowEditColumn = Null.NullBoolean;
		private bool _ShowFooter = true;

		private bool _ShowLanguageColumn = true;

        #region Contructors

        public ModuleLocalization()
        {
            ModuleId = Null.NullInteger;
            TabId = Null.NullInteger;
        }

        #endregion

        public event EventHandler<EventArgs> ModuleLocalizationChanged;

		protected List<ModuleInfo> Modules
		{
			get
			{
				if (_Modules == null)
				{
					_Modules = LoadTabModules();
				}
				return _Modules;
			}
		}

		#region Public Properties

		public string LocalResourceFile
		{
			get
			{
				return Localization.GetResourceFile(this, "ModuleLocalization.ascx");
			}
		}

		public int ModuleId
		{
			get
			{
				return (int)ViewState["ModuleId"];
			}
			set
			{
				ViewState["ModuleId"] = value;
			}
		}

		public bool ShowEditColumn
		{
			get
			{
				return _ShowEditColumn;
			}
			set
			{
				_ShowEditColumn = value;
			}
		}

		public bool ShowFooter
		{
			get
			{
				return _ShowFooter;
			}
			set
			{
				_ShowFooter = value;
			}
		}

		public bool ShowLanguageColumn
		{
			get
			{
				return _ShowLanguageColumn;
			}
			set
			{
				_ShowLanguageColumn = value;
			}
		}

		public int TabId
		{
			get
			{
				return (int)ViewState["TabId"];
			}
			set
			{
				ViewState["TabId"] = value;
			}
		}

		#endregion

		#region Private Methods

		private List<ModuleInfo> LoadTabModules()
		{
			var moduleCtl = new ModuleController();
			var moduleList = new List<ModuleInfo>();

			//Check if we have module scope
			if (ModuleId > Null.NullInteger)
			{
				ModuleInfo sourceModule = moduleCtl.GetModule(ModuleId, TabId);
				if (sourceModule.LocalizedModules != null)
				{
					foreach (ModuleInfo localizedModule in sourceModule.LocalizedModules.Values)
					{
						moduleList.Add(localizedModule);
					}
				}
			}
			else
			{
				foreach (ModuleInfo m in moduleCtl.GetTabModules(TabId).Values)
				{
					if (!m.IsDeleted)
					{
						moduleList.Add(m);
						if (m.LocalizedModules != null)
						{
							foreach (ModuleInfo localizedModule in m.LocalizedModules.Values)
							{
								moduleList.Add(localizedModule);
							}
						}
					}
				}
			}

			return moduleList;
		}

		private void ToggleCheckBox(GridDataItem dataItem, bool toggleValue)
		{
			var rowCheckBox = (CheckBox)dataItem.FindControl("rowCheckBox");
			if (rowCheckBox.Visible)
			{
				rowCheckBox.Checked = toggleValue;
				dataItem.Selected = toggleValue;
			}
		}

		#endregion

		#region Protected Methods

		protected bool ShowHeaderCheckBox()
		{
			bool showCheckBox = Null.NullBoolean;
			if (Modules != null)
			{
				showCheckBox = Modules.Where(m => !m.IsDefaultLanguage).Count() > 0;
			}
			return showCheckBox;
		}

		protected void OnModuleLocalizationChanged(EventArgs e)
		{
			if (ModuleLocalizationChanged != null)
			{
				ModuleLocalizationChanged(this, e);
			}
		}

		#endregion

		#region Public Methods

		public override void DataBind()
		{
			if (TabId != Null.NullInteger)
			{
				localizedModulesGrid.DataSource = Modules;
			}
			localizedModulesGrid.DataBind();
		}

		public void LocalizeSelectedItems(bool localize)
		{
			var moduleCtrl = new ModuleController();

			foreach (GridDataItem row in localizedModulesGrid.SelectedItems)
			{
				var localizedModuleId = (int)row.OwnerTableView.DataKeyValues[row.ItemIndex]["ModuleId"];
				var localizedTabId = (int)row.OwnerTableView.DataKeyValues[row.ItemIndex]["TabId"];
				ModuleInfo sourceModule = moduleCtrl.GetModule(localizedModuleId, localizedTabId, false);

				if (sourceModule != null)
				{
					if (sourceModule.DefaultLanguageModule != null)
					{
						if (localize)
						{
							//Localize
							moduleCtrl.LocalizeModule(sourceModule, LocaleController.Instance.GetLocale(sourceModule.CultureCode));
						}
						else
						{
							//Delocalize
							moduleCtrl.DeLocalizeModule(sourceModule);

							//Mark module as Not Translated
							moduleCtrl.UpdateTranslationStatus(sourceModule, false);
						}
					}
				}
			}

			moduleCtrl.ClearCache(TabId);

			//Rebind localized Modules
			DataBind();

			//Raise Changed event
			OnModuleLocalizationChanged(EventArgs.Empty);
		}

		public void MarkTranslatedSelectedItems(bool translated)
		{
			var moduleCtrl = new ModuleController();

			foreach (GridDataItem row in localizedModulesGrid.SelectedItems)
			{
				var localizedModuleId = (int)row.OwnerTableView.DataKeyValues[row.ItemIndex]["ModuleId"];
				var localizedTabId = (int)row.OwnerTableView.DataKeyValues[row.ItemIndex]["TabId"];
				ModuleInfo sourceModule = moduleCtrl.GetModule(localizedModuleId, localizedTabId, false);

				if (sourceModule.IsLocalized)
				{
					moduleCtrl.UpdateTranslationStatus(sourceModule, translated);
				}
			}

			moduleCtrl.ClearCache(TabId);

            //Raise Changed event
            OnModuleLocalizationChanged(EventArgs.Empty);
            
            //Rebind localized Modules
			DataBind();
		}

		#endregion

		#region EventHandlers

		protected override void OnInit(EventArgs e)
		{
			base.OnInit(e);

			delocalizeModuleButton.Click += delocalizeModuleButton_Click;
			localizeModuleButton.Click += localizeModuleButton_Click;
			//localizedModulesGrid.ItemDataBound += localizedModulesGrid_ItemDataBound;
			localizedModulesGrid.PreRender += localizedModulesGrid_PreRender;
			markModuleTranslatedButton.Click += markModuleTranslatedButton_Click;
			markModuleUnTranslatedButton.Click += markModuleUnTranslatedButton_Click;
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			ClientAPI.AddButtonConfirm(delocalizeModuleButton, Localization.GetString("BindConfirm", LocalResourceFile));
		}

		protected void delocalizeModuleButton_Click(object sender, EventArgs e)
		{
			LocalizeSelectedItems(false);
		}

		protected void localizeModuleButton_Click(object sender, EventArgs e)
		{
			LocalizeSelectedItems(true);
		}

        //protected void localizedModulesGrid_ItemDataBound(object sender, GridItemEventArgs e)
        //{
        //    var gridItem = e.Item as GridDataItem;
        //    if (gridItem != null)
        //    {
        //        var localizedModule = gridItem.DataItem as ModuleInfo;
        //        if (localizedModule != null)
        //        {
        //            var selectCheckBox = gridItem.FindControl("rowCheckBox") as CheckBox;
        //            if (selectCheckBox != null)
        //            {
        //                selectCheckBox.Visible = !localizedModule.IsDefaultLanguage;
        //            }
        //        }
        //    }
        //}

		protected void localizedModulesGrid_PreRender(object sender, EventArgs e)
		{
			foreach (GridColumn column in localizedModulesGrid.Columns)
			{
				if ((column.UniqueName == "Edit"))
				{
					column.Visible = ShowEditColumn;
				}
				if ((column.UniqueName == "Language"))
				{
					column.Visible = ShowLanguageColumn;
				}
			}
			localizedModulesGrid.Rebind();

			footerPlaceHolder.Visible = ShowFooter && Modules.Where(m => !m.IsDefaultLanguage).Count() > 0;
		}

		protected void markModuleTranslatedButton_Click(object sender, EventArgs e)
		{
			MarkTranslatedSelectedItems(true);
		}

		protected void markModuleUnTranslatedButton_Click(object sender, EventArgs e)
		{
			MarkTranslatedSelectedItems(false);
		}

        //protected void ToggleRowSelection(object sender, EventArgs e)
        //{
        //    ((GridItem)((CheckBox)sender).Parent.Parent).Selected = ((CheckBox)sender).Checked;
        //}

        //protected void ToggleSelectedState(object sender, EventArgs e)
        //{
        //    foreach (GridDataItem dataItem in localizedModulesGrid.MasterTableView.Items)
        //    {
        //        ToggleCheckBox(dataItem, ((CheckBox)sender).Checked);
        //    }
        //}

		#endregion

	}
}