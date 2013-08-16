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
using System.Web.UI;
using System.Web.UI.WebControls;

using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Instrumentation;
using DotNetNuke.Security.Permissions;

using Telerik.Web.UI;

#endregion

namespace DotNetNuke.UI.Skins.Controls
{
    /// <summary>
	/// LeftMenu skinobject
    /// </summary>
    /// <remarks></remarks>
    public partial class LeftMenu : SkinObjectBase
    {
    	private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof (LeftMenu));
		#region "Private Variables"

        private ArrayList AuthPages;
		
		//variables and structures
        private Queue PagesQueue;
        private bool _EnableAdminMenus = true;
        private bool _EnablePageIcons = true;
        private bool _EnableToolTips = true;
        private bool _EnableUserMenus = true;
        private String _ItemClickedCssClass = string.Empty;

		//panel item properties
        private String _ItemCssClass = string.Empty;
        private String _ItemDisabledCssClass = string.Empty;
        private String _ItemExpandedCssClass = string.Empty;
        private String _ItemFocusedCssClass = string.Empty;
        private Unit _ItemHeight = Unit.Empty;
        private String _ItemHoveredImageUrl = string.Empty;
        private String _ItemImageUrl = string.Empty;
        private Unit _ItemWidth = Unit.Empty;
        private int _MaxItemNumber = 20;
        private int _MaxLevel = -1;
        private int _MaxLevelNumber = 10;
        private string _PagesToExclude = string.Empty;
        private RadPanelBar _RadPanel1;
		
		//panel ROOT item properties
        private String _RootItemClickedCssClass = string.Empty;
        private String _RootItemCssClass = string.Empty;
        private String _RootItemDisabledCssClass = string.Empty;
        private String _RootItemExpandedCssClass = string.Empty;
        private String _RootItemFocusedCssClass = string.Empty;
        private Unit _RootItemHeight = Unit.Empty;
        private String _RootItemHoveredImageUrl = string.Empty;
        private String _RootItemImageUrl = string.Empty;
        private Unit _RootItemWidth = Unit.Empty;
        private string _ShowOnlyCurrent = string.Empty;
        
		//other properties
		private bool _ShowPath = true;
        private string _Skin = string.Empty;
        private String _Style = string.Empty;
        private ArrayList arrayShowPath;
        private string dnnSkinPath;
        private string dnnSkinSrc;

        protected RadPanelBar RadPanel1
        {
            get
            {
                if (_RadPanel1 == null)
                {
                    _RadPanel1 = new RadPanelBar();
                    _RadPanel1.ID = "RadPanel1";
                    LeftMenu1.Controls.Add(_RadPanel1);
                }
                return _RadPanel1;
            }
        }

		#endregion

		#region "Public Properties"

        public bool AllowCollapseAllItems
        {
            get
            {
                return RadPanel1.AllowCollapseAllItems;
            }
            set
            {
                RadPanel1.AllowCollapseAllItems = value;
            }
        }

        public AnimationType ExpandAnimationType
        {
            get
            {
                return RadPanel1.ExpandAnimation.Type;
            }
            set
            {
                RadPanel1.ExpandAnimation.Type = value;
            }
        }

        public int ExpandAnimationDuration
        {
            get
            {
                return RadPanel1.ExpandAnimation.Duration;
            }
            set
            {
                RadPanel1.ExpandAnimation.Duration = value;
            }
        }

        public int ExpandDelay
        {
            get
            {
                return RadPanel1.ExpandDelay;
            }
            set
            {
                RadPanel1.ExpandDelay = value;
            }
        }

        public AnimationType CollapseAnimationType
        {
            get
            {
                return RadPanel1.CollapseAnimation.Type;
            }
            set
            {
                RadPanel1.CollapseAnimation.Type = value;
            }
        }

        public int CollapseAnimationDuration
        {
            get
            {
                return RadPanel1.CollapseAnimation.Duration;
            }
            set
            {
                RadPanel1.CollapseAnimation.Duration = value;
            }
        }

        public int CollapseDelay
        {
            get
            {
                return RadPanel1.CollapseDelay;
            }
            set
            {
                RadPanel1.CollapseDelay = value;
            }
        }

        public PanelBarExpandMode ExpandMode
        {
            get
            {
                return RadPanel1.ExpandMode;
            }
            set
            {
                RadPanel1.ExpandMode = value;
            }
        }

        public String Skin
        {
            get
            {
                return _Skin;
            }
            set
            {
                _Skin = value;
            }
        }

        public bool EnableEmbeddedSkins
        {
            get
            {
                return RadPanel1.EnableEmbeddedSkins;
            }
            set
            {
                RadPanel1.EnableEmbeddedSkins = value;
            }
        }

        public bool EnableEmbeddedBaseStylesheet
        {
            get
            {
                return RadPanel1.EnableEmbeddedBaseStylesheet;
            }
            set
            {
                RadPanel1.EnableEmbeddedBaseStylesheet = value;
            }
        }

        public bool EnableEmbeddedScripts
        {
            get
            {
                return RadPanel1.EnableEmbeddedScripts;
            }
            set
            {
                RadPanel1.EnableEmbeddedScripts = value;
            }
        }

        public string Dir
        {
            get
            {
                return RadPanel1.Attributes["dir"];
            }
            set
            {
                RadPanel1.Attributes["dir"] = value;
            }
        }

        public Boolean CausesValidation
        {
            get
            {
                return RadPanel1.CausesValidation;
            }
            set
            {
                RadPanel1.CausesValidation = value;
            }
        }

        public String OnClientContextMenu
        {
            get
            {
                return RadPanel1.OnClientContextMenu;
            }
            set
            {
                RadPanel1.OnClientContextMenu = value;
            }
        }

        public String OnClientMouseOver
        {
            get
            {
                return RadPanel1.OnClientMouseOver;
            }
            set
            {
                RadPanel1.OnClientMouseOver = value;
            }
        }

        public String OnClientMouseOut
        {
            get
            {
                return RadPanel1.OnClientMouseOut;
            }
            set
            {
                RadPanel1.OnClientMouseOut = value;
            }
        }

        public String OnClientItemFocus
        {
            get
            {
                return RadPanel1.OnClientItemFocus;
            }
            set
            {
                RadPanel1.OnClientItemFocus = value;
            }
        }

        public String OnClientItemBlur
        {
            get
            {
                return RadPanel1.OnClientItemBlur;
            }
            set
            {
                RadPanel1.OnClientItemBlur = value;
            }
        }

        public String OnClientItemClicking
        {
            get
            {
                return RadPanel1.OnClientItemClicking;
            }
            set
            {
                RadPanel1.OnClientItemClicking = value;
            }
        }

        public String OnClientItemClicked
        {
            get
            {
                return RadPanel1.OnClientItemClicked;
            }
            set
            {
                RadPanel1.OnClientItemClicked = value;
            }
        }

        public String OnClientItemExpand
        {
            get
            {
                return RadPanel1.OnClientItemExpand;
            }
            set
            {
                RadPanel1.OnClientItemExpand = value;
            }
        }

        public String OnClientItemCollapse
        {
            get
            {
                return RadPanel1.OnClientItemCollapse;
            }
            set
            {
                RadPanel1.OnClientItemCollapse = value;
            }
        }

        public String OnClientLoad
        {
            get
            {
                return RadPanel1.OnClientLoad;
            }
            set
            {
                RadPanel1.OnClientLoad = value;
            }
        }

        public String CssClass
        {
            get
            {
                return RadPanel1.CssClass;
            }
            set
            {
                RadPanel1.CssClass = value;
            }
        }

        public Unit Height
        {
            get
            {
                return RadPanel1.Height;
            }
            set
            {
                RadPanel1.Height = value;
            }
        }

        public Unit Width
        {
            get
            {
                return RadPanel1.Width;
            }
            set
            {
                RadPanel1.Width = value;
            }
        }

        public bool PersistStateInCookie
        {
            get
            {
                return RadPanel1.PersistStateInCookie;
            }
            set
            {
                RadPanel1.PersistStateInCookie = value;
            }
        }

        public string CookieName
        {
            get
            {
                return RadPanel1.CookieName;
            }
            set
            {
                RadPanel1.CookieName = value;
            }
        }

        public string PagesToExclude
        {
            get
            {
                return _PagesToExclude;
            }
            set
            {
                _PagesToExclude = value;
            }
        }

        public String ItemCssClass
        {
            get
            {
                return _ItemCssClass;
            }
            set
            {
                _ItemCssClass = value;
            }
        }

        public String ItemDisabledCssClass
        {
            get
            {
                return _ItemDisabledCssClass;
            }
            set
            {
                _ItemDisabledCssClass = value;
            }
        }

        public String ItemExpandedCssClass
        {
            get
            {
                return _ItemExpandedCssClass;
            }
            set
            {
                _ItemExpandedCssClass = value;
            }
        }

        public String ItemFocusedCssClass
        {
            get
            {
                return _ItemFocusedCssClass;
            }
            set
            {
                _ItemFocusedCssClass = value;
            }
        }

        public String ItemClickedCssClass
        {
            get
            {
                return _ItemClickedCssClass;
            }
            set
            {
                _ItemClickedCssClass = value;
            }
        }

        public String ItemImageUrl
        {
            get
            {
                return _ItemImageUrl;
            }
            set
            {
                _ItemImageUrl = value;
            }
        }

        public String ItemHoveredImageUrl
        {
            get
            {
                return _ItemHoveredImageUrl;
            }
            set
            {
                _ItemHoveredImageUrl = value;
            }
        }

        public Unit ItemHeight
        {
            get
            {
                return _ItemHeight;
            }
            set
            {
                _ItemHeight = value;
            }
        }

        public Unit ItemWidth
        {
            get
            {
                return _ItemWidth;
            }
            set
            {
                _ItemWidth = value;
            }
        }

        public String RootItemCssClass
        {
            get
            {
                return _RootItemCssClass;
            }
            set
            {
                _RootItemCssClass = value;
            }
        }

        public String RootItemDisabledCssClass
        {
            get
            {
                return _RootItemDisabledCssClass;
            }
            set
            {
                _RootItemDisabledCssClass = value;
            }
        }

        public String RootItemExpandedCssClass
        {
            get
            {
                return _RootItemExpandedCssClass;
            }
            set
            {
                _RootItemExpandedCssClass = value;
            }
        }

        public String RootItemFocusedCssClass
        {
            get
            {
                return _RootItemFocusedCssClass;
            }
            set
            {
                _RootItemFocusedCssClass = value;
            }
        }

        public String RootItemClickedCssClass
        {
            get
            {
                return _RootItemClickedCssClass;
            }
            set
            {
                _RootItemClickedCssClass = value;
            }
        }

        public String RootItemImageUrl
        {
            get
            {
                return _RootItemImageUrl;
            }
            set
            {
                _RootItemImageUrl = value;
            }
        }

        public String RootItemHoveredImageUrl
        {
            get
            {
                return _RootItemHoveredImageUrl;
            }
            set
            {
                _RootItemHoveredImageUrl = value;
            }
        }

        public Unit RootItemHeight
        {
            get
            {
                return _RootItemHeight;
            }
            set
            {
                _RootItemHeight = value;
            }
        }

        public Unit RootItemWidth
        {
            get
            {
                return _RootItemWidth;
            }
            set
            {
                _RootItemWidth = value;
            }
        }

        public string SelectedPathHeaderItemCss { get; set; }

        public string SelectedPathItemCss { get; set; }

        public string SelectedPathItemImage { get; set; }

        public string SelectedPathHeaderItemImage { get; set; }

        public bool EnableToolTips
        {
            get
            {
                return _EnableToolTips;
            }
            set
            {
                _EnableToolTips = value;
            }
        }

        public bool ImagesOnlyPanel { get; set; }

        public bool EnableLevelCss { get; set; }

        public bool EnableItemCss { get; set; }

        public bool EnableRootItemCss { get; set; }

        public int MaxLevelNumber
        {
            get
            {
                return _MaxLevelNumber;
            }
            set
            {
                _MaxLevelNumber = value;
            }
        }

        public int MaxItemNumber
        {
            get
            {
                return _MaxItemNumber;
            }
            set
            {
                _MaxItemNumber = value;
            }
        }

        public int MaxLevel
        {
            get
            {
                return _MaxLevel;
            }
            set
            {
                _MaxLevel = value;
            }
        }

        public string ShowOnlyCurrent
        {
            get
            {
                return _ShowOnlyCurrent;
            }
            set
            {
                _ShowOnlyCurrent = value;
            }
        }

        public bool EnableItemId { get; set; }

        public bool EnablePageIcons
        {
            get
            {
                return _EnablePageIcons;
            }
            set
            {
                _EnablePageIcons = value;
            }
        }

        public bool EnableUserMenus
        {
            get
            {
                return _EnableUserMenus;
            }
            set
            {
                _EnableUserMenus = value;
            }
        }

        public bool EnableAdminMenus
        {
            get
            {
                return _EnableAdminMenus;
            }
            set
            {
                _EnableAdminMenus = value;
            }
        }

        public bool CopyChildItemLink { get; set; }

        public Boolean ShowPath
        {
            get
            {
                return _ShowPath;
            }
            set
            {
                _ShowPath = value;
            }
        }

        public String Style
        {
            get
            {
                return _Style;
            }
            set
            {
                _Style = value;
            }
        }
		
		#endregion
		
		#region "Event Handlers"

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            dnnSkinSrc = PortalSettings.ActiveTab.SkinSrc.Replace('\\', '/').Replace("//", "/");
            dnnSkinPath = dnnSkinSrc.Substring(0, dnnSkinSrc.LastIndexOf('/'));
            ApplySkin();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            var objTabController = new TabController();
            int i;
            int iItemIndex;
            int iRootGroupId = 0;
            qElement temp;
            int StartingItemId = 0;

            AuthPages = new ArrayList();
            PagesQueue = new Queue();
            arrayShowPath = new ArrayList();
            iItemIndex = 0;
			//---------------------------------------------------

            SetPanelbarProperties();

            if (!Page.IsPostBack)
            {
				//optional code to support displaying a specific branch of the page tree
                GetShowOnlyCurrent(objTabController, ref StartingItemId, ref iRootGroupId);
                //Fixed: For i = 0 To Me.PortalSettings.DesktopTabs.Count - 1
                int portalID = PortalSettings.ActiveTab.IsSuperTab ? -1 : PortalSettings.PortalId;
                IList<TabInfo> desktopTabs = TabController.GetTabsBySortOrder(portalID, PortalController.GetActivePortalLanguage(portalID), true);
                for (i = 0; i <= desktopTabs.Count - 1; i++)
                {
	                var tab = desktopTabs[i];
					if (tab.TabID == PortalSettings.ActiveTab.TabID)
                    {
						FillShowPathArray(ref arrayShowPath, tab.TabID, objTabController);
                    }
                    if (tab.IsVisible && !tab.IsDeleted &&
						(AdminMode || ((Null.IsNull(tab.StartDate) || tab.StartDate < DateTime.Now) &&
						(Null.IsNull(tab.EndDate) || tab.EndDate > DateTime.Now))) &&
                        (TabPermissionController.CanViewPage(tab) && !CheckToExclude(tab.TabName, tab.TabID)))
                    {
                        temp = new qElement();
                        temp.page = desktopTabs[i];
                        temp.radPanelItem = new RadPanelItem();
                        if (CheckShowOnlyCurrent(tab.TabID, tab.ParentId, StartingItemId, iRootGroupId) && CheckPanelVisibility(tab))
                        {
                            iItemIndex = iItemIndex + 1;
                            temp.item = iItemIndex;
                            PagesQueue.Enqueue(AuthPages.Count);
                            RadPanel1.Items.Add(temp.radPanelItem);
                        }
                        AuthPages.Add(temp);
                    }
                }
                BuildPanelbar(RadPanel1.Items);
                if ((0 == RadPanel1.Items.Count))
                {
                    RadPanel1.Visible = false;
                }
            }
        }
		
		#endregion
		
		#region "Private Helper Functions"

        private void ApplySkin()
        {
            if ((EnableEmbeddedSkins == false && !string.IsNullOrEmpty(Skin)))
            {
                string cssLink = "<link href=\"{0}/WebControlSkin/{1}/PanelBar.{1}.css\" rel=\"stylesheet\" type=\"text/css\" />";
                cssLink = string.Format(cssLink, dnnSkinPath, Skin);
                Page.Header.Controls.Add(new LiteralControl(cssLink));
                RadPanel1.Skin = Skin;
            }
        }

        private bool CheckToExclude(string tabName, Int32 tabId)
        {
            if (string.IsNullOrEmpty(PagesToExclude))
            {
                return false;
            }
            string[] temp = PagesToExclude.Split(new[] {','});
            if (temp.Length == 0)
            {
                return false;
            }
            foreach (string item in temp)
            {
                try
                {
                    if (tabId == Int32.Parse(item.Trim()))
                    {
                        return true;
                    }
                }
                catch
                {
                    if (tabName == item.Trim())
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private void GetShowOnlyCurrent(TabController objTabController, ref int StartingItemId, ref int iRootGroupId)
        {
            StartingItemId = 0;
            iRootGroupId = 0;
			//check if we have a value to work with
            if (string.IsNullOrEmpty(ShowOnlyCurrent))
            {
                return;
            }
			
			//check if user specified an ID
            if ((char.IsDigit(ShowOnlyCurrent.ToCharArray()[0])))
            {
                int output;
                if(int.TryParse(ShowOnlyCurrent, out output))
                {
                    StartingItemId = output;
                }
            }
			
			//check if user specified a page name
            if ((ShowOnlyCurrent.StartsWith("PageItem:")))
            {
                TabInfo temptab = objTabController.GetTabByName(ShowOnlyCurrent.Substring(("PageItem:").Length), PortalSettings.PortalId);
                if ((temptab != null))
                {
                    StartingItemId = temptab.TabID;
                }
            }
			
			//RootItem
            if (("RootItem" == ShowOnlyCurrent))
            {
                iRootGroupId = PortalSettings.ActiveTab.TabID;
                while (((objTabController.GetTab(iRootGroupId, PortalSettings.PortalId, true)).ParentId != -1))
                {
                    iRootGroupId = (objTabController.GetTab(iRootGroupId, PortalSettings.PortalId, true)).ParentId;
                }
            }
        }

        private bool CheckShowOnlyCurrent(int tabId, int parentId, int startingItemId, int iRootGroupId)
        {
            if ((string.IsNullOrEmpty(ShowOnlyCurrent) && parentId == -1) || ("ChildItems" == ShowOnlyCurrent && parentId == PortalSettings.ActiveTab.TabID) ||
                ("CurrentItem" == ShowOnlyCurrent && tabId == PortalSettings.ActiveTab.TabID) || ("RootItem" == ShowOnlyCurrent && iRootGroupId == parentId) ||
                (startingItemId > 0 && parentId == startingItemId))
            {
                return true;
            }
            
            return false;
        }

        private bool CheckPanelVisibility(TabInfo tab)
        {
			//Fixed: If (Not EnableAdminMenus AndAlso (tab.IsAdminTab Or tab.IsSuperTab)) Then
            if (!EnableAdminMenus && tab.IsSuperTab)
            {
                return false;
            }
			//Fixed: If (Not EnableUserMenus AndAlso Not (tab.IsAdminTab Or tab.IsSuperTab)) Then
            if (!EnableUserMenus && !tab.IsSuperTab)
            {
                return false;
            }
            return true;
        }

        private void FillShowPathArray(ref ArrayList arrayShowPath, int selectedTabID, TabController objTabController)
        {
            TabInfo tempTab = objTabController.GetTab(selectedTabID, PortalSettings.PortalId, true);
            while ((tempTab.ParentId != -1))
            {
                arrayShowPath.Add(tempTab.TabID);
                tempTab = objTabController.GetTab(tempTab.ParentId, PortalSettings.PortalId, true);
            }
            arrayShowPath.Add(tempTab.TabID);
        }

        private void CheckShowPath(int tabId, RadPanelItem panelItemToCheck, string pageName)
        {
            if ((int) arrayShowPath[0] == tabId)
            {
                panelItemToCheck.Expanded = true;
            }
            if ((arrayShowPath.Contains(tabId)))
            {
                if ((panelItemToCheck.Level > 0))
                {
                    panelItemToCheck.Selected = true;
                    var parent = (RadPanelItem) panelItemToCheck.Owner;
                    while ((parent != null && parent.Items.Count > 0))
                    {
                        try
                        {
                            parent.Expanded = true;
                            parent = (RadPanelItem) parent.Owner;
                        }
                        catch
                        {
                            parent = null;
                        }
                    }
                    if (!string.IsNullOrEmpty(SelectedPathItemCss))
                    {
                        panelItemToCheck.CssClass = SelectedPathItemCss;
                    }
                    if (!string.IsNullOrEmpty(SelectedPathItemImage))
                    {
                        panelItemToCheck.ImageUrl = SelectedPathItemImage.Replace("*SkinPath*", dnnSkinPath).Replace("*PageName*", pageName);
                    }
                }
                else
                {
                    if (!string.IsNullOrEmpty(SelectedPathHeaderItemCss))
                    {
                        panelItemToCheck.CssClass = SelectedPathHeaderItemCss;
                    }
                    if (!string.IsNullOrEmpty(SelectedPathHeaderItemImage))
                    {
                        panelItemToCheck.ImageUrl = SelectedPathHeaderItemImage.Replace("*SkinPath*", dnnSkinPath).Replace("*PageName*", pageName);
                    }
                }
            }
        }

        private void SetPanelbarProperties()
        {
            if (!string.IsNullOrEmpty(Style))
            {
                Style += "; ";
                try
                {
                    foreach (string cStyle in Style.Split(';'))
                    {
                        if ((cStyle.Trim().Length > 0))
                        {
                            RadPanel1.Style.Add(cStyle.Split(':')[0], cStyle.Split(':')[1]);
                        }
                    }
                }
				catch (Exception ex)
				{
					Logger.Error(ex);
				}
            }
        }

        private void SetItemProperties(RadPanelItem currentPanelItem, int iLevel, int iItem, string pageName)
        {
            string sLevel = EnableLevelCss && iLevel < MaxLevelNumber ? "Level" + iLevel : string.Empty;
            string sItem = iItem <= MaxItemNumber && ((EnableItemCss && iLevel > 0) || (EnableRootItemCss && iLevel == 0)) ? iItem.ToString() : string.Empty;
            if (!string.IsNullOrEmpty(ItemCssClass))
            {
                currentPanelItem.CssClass = sLevel + ItemCssClass + sItem;
            }
            if (!string.IsNullOrEmpty(ItemDisabledCssClass))
            {
                currentPanelItem.DisabledCssClass = sLevel + ItemDisabledCssClass + sItem;
            }
            if (!string.IsNullOrEmpty(ItemExpandedCssClass))
            {
                currentPanelItem.ExpandedCssClass = sLevel + ItemExpandedCssClass + sItem;
            }
            if (!string.IsNullOrEmpty(ItemFocusedCssClass))
            {
                currentPanelItem.FocusedCssClass = sLevel + ItemFocusedCssClass + sItem;
            }
            if (!string.IsNullOrEmpty(ItemClickedCssClass))
            {
                currentPanelItem.ClickedCssClass = sLevel + ItemClickedCssClass + sItem;
            }
            if (!string.IsNullOrEmpty(ItemImageUrl))
            {
                currentPanelItem.ImageUrl = ItemImageUrl.Replace("*SkinPath*", dnnSkinPath).Replace("*PageName*", pageName);
            }
            if (!string.IsNullOrEmpty(ItemHoveredImageUrl))
            {
                currentPanelItem.HoveredImageUrl = ItemHoveredImageUrl.Replace("*SkinPath*", dnnSkinPath).Replace("*PageName*", pageName);
            }
            if (!ItemHeight.IsEmpty)
            {
                currentPanelItem.Height = ItemHeight;
            }
            if (!ItemWidth.IsEmpty)
            {
                currentPanelItem.Width = ItemWidth;
            }
        }

        private void SetRootItemProperties(RadPanelItem currentPanelItem, int iLevel, int iItem, string pageName)
        {
            string sLevel = EnableLevelCss && iLevel < MaxLevelNumber ? "Level" + iLevel : string.Empty;
            string sItem = iItem <= MaxItemNumber && ((EnableItemCss && iLevel > 0) || (EnableRootItemCss && iLevel == 0)) ? iItem.ToString() : string.Empty;

            if (!string.IsNullOrEmpty(RootItemCssClass))
            {
                currentPanelItem.CssClass = sLevel + RootItemCssClass + sItem;
            }
            if (!string.IsNullOrEmpty(RootItemDisabledCssClass))
            {
                currentPanelItem.DisabledCssClass = sLevel + RootItemDisabledCssClass + sItem;
            }
            if (!string.IsNullOrEmpty(RootItemExpandedCssClass))
            {
                currentPanelItem.ExpandedCssClass = sLevel + RootItemExpandedCssClass + sItem;
            }
            if (!string.IsNullOrEmpty(RootItemFocusedCssClass))
            {
                currentPanelItem.FocusedCssClass = sLevel + RootItemFocusedCssClass + sItem;
            }
            if (!string.IsNullOrEmpty(RootItemClickedCssClass))
            {
                currentPanelItem.ClickedCssClass = sLevel + RootItemClickedCssClass + sItem;
            }
            if (!string.IsNullOrEmpty(RootItemImageUrl))
            {
                currentPanelItem.ImageUrl = RootItemImageUrl.Replace("*SkinPath*", dnnSkinPath).Replace("*PageName*", pageName);
            }
            if (!string.IsNullOrEmpty(RootItemHoveredImageUrl))
            {
                currentPanelItem.HoveredImageUrl = RootItemHoveredImageUrl.Replace("*SkinPath*", dnnSkinPath).Replace("*PageName*", pageName);
            }
            if (!RootItemHeight.IsEmpty)
            {
                currentPanelItem.Height = RootItemHeight;
            }
            if (!RootItemWidth.IsEmpty)
            {
                currentPanelItem.Width = RootItemWidth;
            }
        }

        private void BuildPanelbar(RadPanelItemCollection rootCollection)
        {
            qElement temp;
            qElement temp2;
            TabInfo page;
            int pageID;
            int j;
            int iItemIndex;
            while (PagesQueue.Count != 0)
            {
                pageID = Convert.ToInt32(PagesQueue.Dequeue());
                temp = (qElement) AuthPages[pageID];
                page = temp.page;
                temp.radPanelItem.Text = page.TabName;
                if ((!String.IsNullOrEmpty(page.IconFile) && EnablePageIcons))
                {
                    if ((page.IconFile.StartsWith("~")))
                    {
                        temp.radPanelItem.ImageUrl = Page.ResolveUrl(page.IconFile);
                    }
                    else
                    {
                        temp.radPanelItem.ImageUrl = PortalSettings.HomeDirectory + page.IconFile;
                    }
                }
                if ((!page.DisableLink))
                {
                    if ((page.FullUrl.StartsWith("*") && page.FullUrl.IndexOf("*", 1) != -1))
                    {
                        temp.radPanelItem.NavigateUrl = page.FullUrl.Substring(page.FullUrl.IndexOf("*", 1) + 1);
                        temp.radPanelItem.Target = page.FullUrl.Substring(1, page.FullUrl.IndexOf("*", 1) - 1);
                    }
                    else
                    {
                        temp.radPanelItem.NavigateUrl = page.FullUrl;
                    }
                }
                else if ((CopyChildItemLink && page.Level >= MaxLevel))
                {
                    j = 0;
					//check if there are any child items and use a href from the first one
                    while ((j < AuthPages.Count && (((qElement) AuthPages[j]).page.ParentId != page.TabID || ((qElement) AuthPages[j]).page.DisableLink)))
                    {
                        j = j + 1;
                    }
                    if ((j < AuthPages.Count))
                    {
						//child item found. use its link
                        temp.radPanelItem.NavigateUrl = ((qElement) AuthPages[j]).page.FullUrl;
                    }
                }
                if ((EnableToolTips && !String.IsNullOrEmpty(page.Description)))
                {
                    temp.radPanelItem.ToolTip = page.Description;
                }
				
				//set all other item properties
                if ((temp.radPanelItem.Level == 0))
                {
                    SetRootItemProperties(temp.radPanelItem, page.Level, temp.item, page.TabName);
                }
                else
                {
                    SetItemProperties(temp.radPanelItem, page.Level, temp.item, page.TabName);
                }
				
				//check showpath
                if ((ShowPath))
                {
                    CheckShowPath(page.TabID, temp.radPanelItem, page.TabName);
                }
				
				//image-only panel check
                if ((ImagesOnlyPanel && temp.radPanelItem.ImageUrl != string.Empty))
                {
                    temp.radPanelItem.Text = string.Empty;
                }
				
				//attach child items the current one
                if ((page.Level < MaxLevel || MaxLevel < 0))
                {
                    iItemIndex = 0;
                    for (j = 0; j <= AuthPages.Count - 1; j++)
                    {
                        temp2 = (qElement) AuthPages[j];
                        if ((temp2.page.ParentId == page.TabID))
                        {
                            temp.radPanelItem.Items.Add(temp2.radPanelItem);
                            PagesQueue.Enqueue(j);
                            iItemIndex = iItemIndex + 1;
                            temp2.item = iItemIndex;
                            AuthPages[j] = temp2;
                        }
                    }
                }
            }
        }
		
		#endregion

        #region Nested type: qElement

        private struct qElement
        {
            public int item;
            public TabInfo page;
            public RadPanelItem radPanelItem;
        }

        #endregion
    }
}