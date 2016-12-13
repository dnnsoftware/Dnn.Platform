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

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Services.Localization;
using DotNetNuke.UI.Modules;
using DotNetNuke.UI.UserControls;
using DotNetNuke.UI.Utilities;
using DotNetNuke.UI.WebControls;
using DotNetNuke.Web.UI.WebControls;

using Globals = DotNetNuke.Common.Globals;

namespace DotNetNuke.Modules.UrlManagement.Components
{
    public abstract class PortAliasesModuleBase : ModuleUserControlBase
    {
        protected List<PortalAliasInfo> _Aliases;
        private int _currentPortalId = -1;

        #region Protected Properties

        protected abstract int ActionColumnIndex { get; }

        protected abstract LinkButton AddAliasButton { get; }

        protected bool AddMode
        {
            get
            {
                bool _Mode = Null.NullBoolean;
                if (ViewState["Mode"] != null)
                {
                    _Mode = Convert.ToBoolean(ViewState["Mode"]);
                }
                return _Mode;
            }
            set
            {
                ViewState["Mode"] = value;
            }
        }

        protected List<PortalAliasInfo> Aliases
        {
            get
            {
                return _Aliases ?? (_Aliases = PortalAliasController.Instance.GetPortalAliasesByPortalId(_currentPortalId).ToList());
            }
        }

        protected int CurrentPortalId
        {
            get { return _currentPortalId;  }
        }

        protected abstract Label ErrorLabel { get; }

        protected abstract DataGrid Grid { get; }

        protected PortalAliasInfo PortalAlias
        {
            get { return ModuleContext.PortalAlias; }
        }

        protected int PortalId
        {
            get { return ModuleContext.PortalId; }
        }

        protected Dictionary<string, Locale> Locales
        {
            get { return LocaleController.Instance.GetLocales(PortalId); }
        }

        #endregion

        #region Protected Methods

        protected void AddAlias(object sender, EventArgs e)
        {
            //Add a new empty rule and set the editrow to the new row
            var portalAlias = new PortalAliasInfo { PortalID = CurrentPortalId };
            Aliases.Add(portalAlias);
            Grid.EditItemIndex = Aliases.Count - 1;

            //Set the AddMode to true
            AddMode = true;

            //Rebind the collection
            BindAliases();
        }

        protected void BindAliases()
        {
            Grid.DataSource = Aliases;
            Grid.DataBind();
        }

        protected void CancelEdit(object source, CommandEventArgs e)
        {
            if (AddMode)
            {
                //Remove the temporary added row
                Aliases.RemoveAt(Aliases.Count - 1);
                AddMode = false;
            }

            //Clear editrow
            Grid.EditItemIndex = -1;
            ErrorLabel.Visible = false;

            //Rebind the collection
            BindAliases();
        }

        protected void DeleteAiasesGrid(object source, CommandEventArgs e)
        {
            //Get the index of the row to delete
            int index = Convert.ToInt32(e.CommandArgument);

            //Remove the alias from the aliases collection
            var portalAlias = Aliases[index];

            PortalAliasController.Instance.DeletePortalAlias(portalAlias);
            //should remove the portal's folder if exist
            var portalFolder = PortalController.GetPortalFolder(portalAlias.HTTPAlias);
            var serverPath = Globals.GetAbsoluteServerPath(Request);

            if (!string.IsNullOrEmpty(portalFolder) && Directory.Exists(serverPath + portalFolder))
            {
                PortalController.DeletePortalFolder(serverPath, portalFolder);
            }

            //Rebind the collection
            _Aliases = null;
            BindAliases();

        }

        protected void EditAiasesGrid(object source, CommandEventArgs e)
        {
            //Set the AddMode to false
            AddMode = false;

            //Set the editrow
            Grid.EditItemIndex = Convert.ToInt32(e.CommandArgument);

            //Rebind the collection
            BindAliases();
        }

        protected bool GetIsPrimary(int index)
        {
            bool isPrimary = false;
            var isPrimaryCheckbox = Grid.Items[index].Controls[0].FindControl("isPrimary") as CheckBox;

            if (isPrimaryCheckbox != null)
            {
                isPrimary = isPrimaryCheckbox.Checked;
            }
            return isPrimary;
        }

        protected abstract void GetPortalAliasProperties(int index, PortalAliasInfo portalAlias);

        protected virtual void GridItemDataBound(object sender, DataGridItemEventArgs e)
        {
            DataGridItem item = e.Item;

            if (item.ItemType == ListItemType.Item || item.ItemType == ListItemType.AlternatingItem || item.ItemType == ListItemType.SelectedItem || item.ItemType == ListItemType.EditItem)
            {
                bool isEditItem = (item.ItemType == ListItemType.EditItem);

                var portalAlias = (PortalAliasInfo)item.DataItem;

                if (portalAlias != null)
                {
                    Control imgColumnControl = item.Controls[ActionColumnIndex].FindControl("deleteButton");
                    var delImage = (DnnImageButton) imgColumnControl;
                    if (delImage != null)
                    {
                        delImage.Visible = (portalAlias.PortalAliasID != PortalAlias.PortalAliasID && !portalAlias.IsPrimary && !isEditItem);

                        if (delImage.Visible) ClientAPI.AddButtonConfirm(delImage, Localization.GetString("DeleteItem"));
                    }

                    imgColumnControl = item.Controls[ActionColumnIndex].FindControl("editButton");
                    var editImage = (DnnImageButton) imgColumnControl;
                    if (editImage != null)
                    {
                        editImage.Visible = (portalAlias.PortalAliasID != PortalAlias.PortalAliasID && !isEditItem);
                    }

                    imgColumnControl = item.Controls[ActionColumnIndex].FindControl("currentAliasHelpLabel");
                    var image = (LabelControl) imgColumnControl;
                    if (image != null)
                    {
                        image.Visible = (portalAlias.PortalAliasID == PortalAlias.PortalAliasID && !isEditItem);
                    }

                    if (isEditItem)
                    {
                        var cultureCodeDropDown = item.Controls[2].FindControl("cultureCodeDropDown") as DnnComboBox;
                        if (cultureCodeDropDown != null)
                        {
                            var locales = new Dictionary<string, string> {{"<" + LocalizeString("None_Specified") + ">", String.Empty}};
                            foreach (var locale in Locales)
                            {
                                locales.Add(locale.Value.NativeName, locale.Value.Code);
                            }
                            cultureCodeDropDown.DataSource = locales;
                            cultureCodeDropDown.DataBind();
                            cultureCodeDropDown.Select(portalAlias.CultureCode, false);
                        }
                    }
                }
            }
        }

        protected override void LoadViewState(object savedState)
        {
            var myState = (object[])savedState;
            if ((myState[0] != null))
            {
                base.LoadViewState(myState[0]);
            }
            if ((myState[1] != null))
            {
                var aliasCount = (int)myState[1];
                Aliases.Clear();
                for (int i = 0; i <= aliasCount - 1; i++)
                {
                    string aliasString = Convert.ToString(myState[i + 2]);
                    var sr = new StringReader(aliasString);
                    Aliases.Add(CBO.DeserializeObject<PortalAliasInfo>(sr));
                }
            }
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            AddAliasButton.Click += AddAlias;
            Grid.ItemDataBound += GridItemDataBound;
            Grid.EditCommand += EditAiasesGrid;
            Grid.DeleteCommand += DeleteAiasesGrid;
            Grid.CancelCommand += CancelEdit;

            foreach (DataGridColumn column in Grid.Columns)
            {
                if (ReferenceEquals(column.GetType(), typeof(ImageCommandColumn)))
                {
                    //Manage Delete Confirm JS
                    var imageColumn = (ImageCommandColumn)column;
                    if (imageColumn.CommandName == "Delete")
                    {
                        imageColumn.OnClickJS = Localization.GetString("DeleteItem");
                    }

                    //Localize Image Column Text
                    if (!String.IsNullOrEmpty(imageColumn.CommandName))
                    {
                        imageColumn.Text = Localization.GetString(imageColumn.CommandName, LocalResourceFile);
                    }
                }
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            _currentPortalId = (Request.QueryString["pid"] != null) ? Int32.Parse(Request.QueryString["pid"]) : PortalId;

            if (!Page.IsPostBack)
            {
                BindAliases();
            }

        }

        protected virtual void SaveAliasesGrid(object source, CommandEventArgs e)
        {
            string childPath = string.Empty;
            string message = string.Empty;

            //Get the index of the row to save
            int index = Grid.EditItemIndex;

            var portalAlias = Aliases[index];
            var ctlAlias = Grid.Items[index].Cells[1].FindControl("txtHTTPAlias") as TextBox;

            bool isPrimary = GetIsPrimary(index);

            string strAlias = String.Empty;
            if (ctlAlias != null)
            {
                strAlias = ctlAlias.Text.Trim();
            }

            if (string.IsNullOrEmpty(strAlias))
            {
                message = Localization.GetString("InvalidAlias", LocalResourceFile);
            }
            else
            {
                if (strAlias.IndexOf("://", StringComparison.Ordinal) != -1)
                {
                    strAlias = strAlias.Remove(0, strAlias.IndexOf("://", StringComparison.Ordinal) + 3);
                }
                if (strAlias.IndexOf("\\\\", StringComparison.Ordinal) != -1)
                {
                    strAlias = strAlias.Remove(0, strAlias.IndexOf("\\\\", StringComparison.Ordinal) + 2);
                }

                //Validate Alias, this needs to be done with lowercase, downstream we only check with lowercase variables
                if (!PortalAliasController.ValidateAlias(strAlias.ToLowerInvariant(), false))
                {
                    message = Localization.GetString("InvalidAlias", LocalResourceFile);
                }
            }

            if (string.IsNullOrEmpty(message) && AddMode)
            {
                var aliases = PortalAliasController.Instance.GetPortalAliases();
                if (aliases.Contains(strAlias))
                {
                    message = Localization.GetString("DuplicateAlias", LocalResourceFile);
                }
            }

            if (string.IsNullOrEmpty(message))
            {
                portalAlias.HTTPAlias = strAlias;
                portalAlias.IsPrimary = isPrimary;

                var item = Grid.Items[index];
                var cultureCodeDropDown = item.Controls[2].FindControl("cultureCodeDropDown") as DnnComboBox;
                if (cultureCodeDropDown != null)
                {
                    portalAlias.CultureCode = cultureCodeDropDown.SelectedValue;
                }

                GetPortalAliasProperties(index, portalAlias);

                if (AddMode)
                {
                    PortalAliasController.Instance.AddPortalAlias(portalAlias);
                }
                else
                {
                    PortalAliasController.Instance.UpdatePortalAlias(portalAlias);
                }

                //Reset Edit Index
                ErrorLabel.Visible = false;
                Grid.EditItemIndex = -1;
                _Aliases = null;
            }
            else
            {
                ErrorLabel.Text = message;
                ErrorLabel.Visible = true;
            }

            BindAliases();
        }

        protected override object SaveViewState()
        {
            object baseState = base.SaveViewState();
            var allStates = new object[Aliases.Count + 2];
            allStates[0] = baseState;
            allStates[1] = Aliases.Count;
            for (int i = 0; i <= Aliases.Count - 1; i++)
            {
                var portalAlias = Aliases[i];
                var sw = new StringWriter();
                CBO.SerializeObject(portalAlias, sw);
                allStates[i + 2] = sw.ToString();
            }
            return allStates;
        }

        #endregion
    }
}