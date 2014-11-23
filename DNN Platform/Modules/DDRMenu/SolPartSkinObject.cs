using System;
using System.Collections.Generic;
using DotNetNuke.Web.DDRMenu.DNNCommon;
using DotNetNuke.UI.Skins;

namespace DotNetNuke.Web.DDRMenu
{
	public class SolPartSkinObject : NavObjectBase
	{
		public string SeparateCss { get; set; }
		public string MenuBarCssClass { get { return CSSControl; } set { CSSControl = value; } }
		public string MenuContainerCssClass { get { return CSSContainerRoot; } set { CSSContainerRoot = value; } }
		public string MenuItemCssClass { get { return CSSNode; } set { CSSNode = value; } }
		public string MenuIconCssClass { get { return CSSIcon; } set { CSSIcon = value; } }
		public string SubMenuCssClass { get { return CSSContainerSub; } set { CSSContainerSub = value; } }
		public string MenuItemSelCssClass { get { return CSSNodeHover; } set { CSSNodeHover = value; } }
		public string MenuBreakCssClass { get { return CSSBreak; } set { CSSBreak = value; } }
		public string MenuArrowCssClass { get { return CSSIndicateChildSub; } set { CSSIndicateChildSub = value; } }
		public string MenuRootArrowCssClass { get { return CSSIndicateChildRoot; } set { CSSIndicateChildRoot = value; } }
		public string BackColor { get { return StyleBackColor; } set { StyleBackColor = value; } }
		public string ForeColor { get { return StyleForeColor; } set { StyleForeColor = value; } }
		public string HighlightColor { get { return StyleHighlightColor; } set { StyleHighlightColor = value; } }
		public string IconBackgroundColor { get { return StyleIconBackColor; } set { StyleIconBackColor = value; } }
		public string SelectedBorderColor { get { return StyleSelectionBorderColor; } set { StyleSelectionBorderColor = value; } }
		public string SelectedColor { get { return StyleSelectionColor; } set { StyleSelectionColor = value; } }
		public string SelectedForeColor { get { return StyleSelectionForeColor; } set { StyleSelectionForeColor = value; } }
		public string Display { get { return ControlOrientation; } set { ControlOrientation = value; } }
		public string MenuBarHeight { get { return StyleControlHeight; } set { StyleControlHeight = value; } }
		public string MenuBorderWidth { get { return StyleBorderWidth; } set { StyleBorderWidth = value; } }
		public string MenuItemHeight { get { return StyleNodeHeight; } set { StyleNodeHeight = value; } }
		public string Moveable { get; set; }
		public string IconWidth { get { return StyleIconWidth; } set { StyleIconWidth = value; } }
		public string MenuEffectsShadowColor { get { return EffectsShadowColor; } set { EffectsShadowColor = value; } }
		public string MenuEffectsMouseOutHideDelay { get { return MouseOutHideDelay; } set { MouseOutHideDelay = value; } }
		public string MenuEffectsMouseOverDisplay { get { return MouseOverDisplay; } set { MouseOverDisplay = value; } }
		public string MenuEffectsMouseOverExpand { get { return MouseOverAction; } set { MouseOverAction = value; } }
		public string MenuEffectsStyle { get { return EffectsStyle; } set { EffectsStyle = value; } }
		public string FontNames { get { return StyleFontNames; } set { StyleFontNames = value; } }
		public string FontSize { get { return StyleFontSize; } set { StyleFontSize = value; } }
		public string FontBold { get { return StyleFontBold; } set { StyleFontBold = value; } }
		public string MenuEffectsShadowStrength { get { return EffectsShadowStrength; } set { EffectsShadowStrength = value; } }
		public string MenuEffectsMenuTransition { get { return EffectsTransition; } set { EffectsTransition = value; } }
		public string MenuEffectsMenuTransitionLength { get { return EffectsDuration; } set { EffectsDuration = value; } }
		public string MenuEffectsShadowDirection { get { return EffectsShadowDirection; } set { EffectsShadowDirection = value; } }
		public string ForceFullMenuList { get { return ForceCrawlerDisplay; } set { ForceCrawlerDisplay = value; } }
		public string UseSkinPathArrowImages { get; set; }
		public string UseRootBreadCrumbArrow { get; set; }
		public string UseSubMenuBreadCrumbArrow { get; set; }
		public string RootMenuItemBreadCrumbCssClass { get { return CSSBreadCrumbRoot; } set { CSSBreadCrumbRoot = value; } }
		public string SubMenuItemBreadCrumbCssClass { get { return CSSBreadCrumbSub; } set { CSSBreadCrumbSub = value; } }
		public string RootMenuItemCssClass { get { return CSSNodeRoot; } set { CSSNodeRoot = value; } }
		public string RootBreadCrumbArrow { get; set; }
		public string SubMenuBreadCrumbArrow { get; set; }
		public string UseArrows { get { return IndicateChildren; } set { IndicateChildren = value; } }
		public string DownArrow { get; set; }
		public string RightArrow { get; set; }
		public string RootMenuItemActiveCssClass { get { return CSSNodeSelectedRoot; } set { CSSNodeSelectedRoot = value; } }
		public string SubMenuItemActiveCssClass { get { return CSSNodeSelectedSub; } set { CSSNodeSelectedSub = value; } }
		public string RootMenuItemSelectedCssClass { get { return CSSNodeHoverRoot; } set { CSSNodeHoverRoot = value; } }
		public string SubMenuItemSelectedCssClass { get { return CSSNodeHoverSub; } set { CSSNodeHoverSub = value; } }
		public string Separator { get { return SeparatorHTML; } set { SeparatorHTML = value; } }
		public string SeparatorCssClass { get { return CSSSeparator; } set { CSSSeparator = value; } }
		public string RootMenuItemLeftHtml { get { return NodeLeftHTMLRoot; } set { NodeLeftHTMLRoot = value; } }
		public string RootMenuItemRightHtml { get { return NodeRightHTMLRoot; } set { NodeRightHTMLRoot = value; } }
		public string SubMenuItemLeftHtml { get { return NodeLeftHTMLSub; } set { NodeLeftHTMLSub = value; } }
		public string SubMenuItemRightHtml { get { return NodeRightHTMLSub; } set { NodeRightHTMLSub = value; } }
		public string LeftSeparator { get { return SeparatorLeftHTML; } set { SeparatorLeftHTML = value; } }
		public string RightSeparator { get { return SeparatorRightHTML; } set { SeparatorRightHTML = value; } }
		public string LeftSeparatorActive { get { return SeparatorLeftHTMLActive; } set { SeparatorLeftHTMLActive = value; } }
		public string RightSeparatorActive { get { return SeparatorRightHTMLActive; } set { SeparatorRightHTMLActive = value; } }
		public string LeftSeparatorBreadCrumb { get { return SeparatorLeftHTMLBreadCrumb; } set { SeparatorLeftHTMLBreadCrumb = value; } }
		public string RightSeparatorBreadCrumb { get { return SeparatorRightHTMLBreadCrumb; } set { SeparatorRightHTMLBreadCrumb = value; } }
		public string LeftSeparatorCssClass { get { return CSSLeftSeparator; } set { CSSLeftSeparator = value; } }
		public string RightSeparatorCssClass { get { return CSSRightSeparator; } set { CSSRightSeparator = value; } }
		public string LeftSeparatorActiveCssClass { get { return CSSLeftSeparatorSelection; } set { CSSLeftSeparatorSelection = value; } }
		public string RightSeparatorActiveCssClass { get { return CSSRightSeparatorSelection; } set { CSSRightSeparatorSelection = value; } }
		public string LeftSeparatorBreadCrumbCssClass { get { return CSSLeftSeparatorBreadCrumb; } set { CSSLeftSeparatorBreadCrumb = value; } }
		public string RightSeparatorBreadCrumbCssClass { get { return CSSRightSeparatorBreadCrumb; } set { CSSRightSeparatorBreadCrumb = value; } }
		public string MenuAlignment { get { return ControlAlignment; } set { ControlAlignment = value; } }
		public string ClearDefaults { get; set; }
		public string DelaySubmenuLoad { get; set; }
		public string RootOnly { get; set; }

		public string NodeXmlPath { get; set; }
		public string NodeSelector { get; set; }
		public string IncludeNodes { get; set; }
		public string ExcludeNodes { get; set; }
		public string NodeManipulator { get; set; }

		protected override void OnInit(EventArgs e)
		{
			using (new DNNContext(this))
			{
				InitializeNavControl(this, "DDRMenuNavigationProvider");
				var ddrControl = (DDRMenuNavigationProvider)Control;
				ddrControl.MenuStyle = "SolPart";
				ddrControl.CustomAttributes = new List<CustomAttribute>();
				if (!String.IsNullOrEmpty(NodeXmlPath))
				{
					ddrControl.CustomAttributes.Add(new CustomAttribute {Name = "NodeXmlPath", Value = NodeXmlPath});
				}
				if (!String.IsNullOrEmpty(NodeSelector))
				{
					ddrControl.CustomAttributes.Add(new CustomAttribute {Name = "NodeSelector", Value = NodeSelector});
				}
				if (!String.IsNullOrEmpty(IncludeNodes))
				{
					ddrControl.CustomAttributes.Add(new CustomAttribute {Name = "IncludeNodes", Value = IncludeNodes});
				}
				if (!String.IsNullOrEmpty(ExcludeNodes))
				{
					ddrControl.CustomAttributes.Add(new CustomAttribute {Name = "ExcludeNodes", Value = ExcludeNodes});
				}
				if (!String.IsNullOrEmpty(NodeManipulator))
				{
					ddrControl.CustomAttributes.Add(new CustomAttribute {Name = "NodeManipulator", Value = NodeManipulator});
				}

				base.OnInit(e);
			}
		}

		protected override void OnPreRender(EventArgs e)
		{
			using (new DNNContext(this))
			{
				var blnUseSkinPathArrowImages = Boolean.Parse(GetValue(UseSkinPathArrowImages, "false"));
				var blnUseRootBreadcrumbArrow = Boolean.Parse(GetValue(UseRootBreadCrumbArrow, "true"));
				var blnUseSubMenuBreadcrumbArrow = Boolean.Parse(GetValue(UseSubMenuBreadCrumbArrow, "false"));
				var blnUseArrows = Boolean.Parse(GetValue(UseArrows, "true"));
				var blnRootOnly = Boolean.Parse(GetValue(RootOnly, "false"));

				if (blnRootOnly)
				{
					blnUseArrows = false;
					PopulateNodesFromClient = false;
					StartTabId = -1;
					ExpandDepth = 1;
				}

				var strRootBreadcrumbArrow = !String.IsNullOrEmpty(RootBreadCrumbArrow)
				                             	? PortalSettings.ActiveTab.SkinPath + RootBreadCrumbArrow
				                             	: ResolveUrl("~/images/breadcrumb.gif");

				if (blnUseSubMenuBreadcrumbArrow)
				{
					var strSubMenuBreadcrumbArrow = ResolveUrl("~/images/breadcrumb.gif");
					NodeLeftHTMLBreadCrumbSub = "<img alt='*' BORDER='0' src='" + strSubMenuBreadcrumbArrow + "'>";
				}

				if (blnUseRootBreadcrumbArrow)
				{
					NodeLeftHTMLBreadCrumbRoot = "<img alt='*' BORDER='0' src='" + strRootBreadcrumbArrow + "'>";
				}

				var strRightArrow = !String.IsNullOrEmpty(RightArrow) ? RightArrow : "breadcrumb.gif";
				var strDownArrow = !String.IsNullOrEmpty(DownArrow) ? DownArrow : "menu_down.gif";

				if (!String.IsNullOrEmpty(Separator))
				{
					if (Separator.IndexOf("src=") != -1)
					{
						Separator = Separator.Replace("src=\"", "src=\"" + PortalSettings.ActiveTab.SkinPath);
					}
				}

				if (!String.IsNullOrEmpty(LeftSeparator))
				{
					if (LeftSeparator.IndexOf("src=") != -1)
					{
						LeftSeparator = LeftSeparator.Replace("src=\"", "src=\"" + PortalSettings.ActiveTab.SkinPath);
					}
				}
				if (!String.IsNullOrEmpty(RightSeparator))
				{
					if (RightSeparator.IndexOf("src=") != -1)
					{
						RightSeparator = RightSeparator.Replace("src=\"", "src=\"" + PortalSettings.ActiveTab.SkinPath);
					}
				}
				if (!String.IsNullOrEmpty(LeftSeparatorBreadCrumb))
				{
					if (LeftSeparatorBreadCrumb.IndexOf("src=") != -1)
					{
						LeftSeparatorBreadCrumb = LeftSeparatorBreadCrumb.Replace("src=\"", "src=\"" + PortalSettings.ActiveTab.SkinPath);
					}
				}
				if (!String.IsNullOrEmpty(RightSeparatorBreadCrumb))
				{
					if (RightSeparatorBreadCrumb.IndexOf("src=") != -1)
					{
						RightSeparatorBreadCrumb = RightSeparatorBreadCrumb.Replace(
							"src=\"", "src=\"" + PortalSettings.ActiveTab.SkinPath);
					}
				}
				if (!String.IsNullOrEmpty(LeftSeparatorActive))
				{
					if (LeftSeparatorActive.IndexOf("src=") != -1)
					{
						LeftSeparatorActive = LeftSeparatorActive.Replace("src=\"", "src=\"" + PortalSettings.ActiveTab.SkinPath);
					}
				}
				if (!String.IsNullOrEmpty(RightSeparatorActive))
				{
					if (RightSeparatorActive.IndexOf("src=") != -1)
					{
						RightSeparatorActive = RightSeparatorActive.Replace("src=\"", "src=\"" + PortalSettings.ActiveTab.SkinPath);
					}
				}

				PathSystemImage = blnUseSkinPathArrowImages ? PortalSettings.ActiveTab.SkinPath : ResolveUrl("~/images/");

				PathImage = PortalSettings.HomeDirectory;

				if (blnUseArrows)
				{
					IndicateChildImageSub = strRightArrow;
					IndicateChildImageRoot = (ControlOrientation.ToLower() == "vertical") ? strRightArrow : strDownArrow;
				}
				else
				{
					PathSystemImage = ResolveUrl("~/images/");
					IndicateChildImageSub = "spacer.gif";
				}

				if (String.IsNullOrEmpty(PathSystemScript))
				{
					PathSystemScript = ResolveUrl("~/controls/SolpartMenu/");
				}

				Control.Bind(GetNavigationNodes(null));
				base.OnPreRender(e);
			}
		}
	}
}