using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Web.UI;
using DotNetNuke.Common;
using DotNetNuke.Entities.Modules.Actions;
using DotNetNuke.Modules.NavigationProvider;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.UI;
using DotNetNuke.UI.Containers;
using DotNetNuke.UI.WebControls;
using DotNetNuke.Web.DDRMenu.DNNCommon;
using DotNetNuke.Web.DDRMenu.TemplateEngine;

namespace DotNetNuke.Web.DDRMenu
{
	public class Actions : ActionBase
	{
		public string PathSystemScript { get; set; }

		public string MenuStyle { get; set; }

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[PersistenceMode(PersistenceMode.InnerProperty)]
		public List<ClientOption> ClientOptions { get; set; }

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[PersistenceMode(PersistenceMode.InnerProperty)]
		public List<TemplateArgument> TemplateArguments { get; set; }

		private DDRMenuNavigationProvider navProvider;
		private Dictionary<int, ModuleAction> actions;

		protected override void OnInit(EventArgs e)
		{
			using (new DNNContext(this))
			{
				base.OnInit(e);

				navProvider = (DDRMenuNavigationProvider)NavigationProvider.Instance("DDRMenuNavigationProvider");
				navProvider.ControlID = "ctl" + ID;
				navProvider.MenuStyle = MenuStyle;
				navProvider.Initialize();

				Controls.Add(navProvider.NavigationControl);
			}
		}

		protected override void OnLoad(EventArgs e)
		{
			using (new DNNContext(this))
			{
				base.OnLoad(e);

				SetMenuDefaults();
			}
		}

		protected override void OnPreRender(EventArgs e)
		{
			using (new DNNContext(this))
			{
				base.OnPreRender(e);

				try
				{
					navProvider.TemplateArguments = TemplateArguments;
					BindMenu(Navigation.GetActionNodes(ActionRoot, this, -1));
				}
				catch (Exception exc)
				{
					Exceptions.ProcessModuleLoadException(this, exc);
				}
			}
		}

		private void BindMenu(DNNNodeCollection objNodes)
		{
			Visible = DisplayControl(objNodes);
			if (!Visible)
			{
				return;
			}

			navProvider.ClearNodes();
			foreach (DNNNode node in objNodes)
			{
				ProcessNode(node);
			}
			navProvider.Bind(objNodes, false);
		}

		private void ActionClick(NavigationEventArgs args)
		{
			using (new DNNContext(this))
			{
				try
				{
					ProcessAction(args.ID);
				}
				catch (Exception exc)
				{
					Exceptions.ProcessModuleLoadException(this, exc);
				}
			}
		}

		private void AddActionIDs(ModuleAction action)
		{
			if (!actions.ContainsKey(action.ID))
			{
				actions.Add(action.ID, action);
			}
			if (action.HasChildren())
			{
				foreach (ModuleAction a in action.Actions)
				{
					AddActionIDs(a);
				}
			}
		}

		private ModuleAction FindAction(int id)
		{
			if (actions == null)
			{
				actions = new Dictionary<int, ModuleAction>();
				AddActionIDs(ActionRoot);
			}

			ModuleAction result;
			return actions.TryGetValue(id, out result) ? result : null;
		}

		private void ProcessNode(DNNNode dnnNode)
		{
			if (!dnnNode.IsBreak)
			{
				var action = FindAction(Convert.ToInt32(dnnNode.Key));
				if (action != null)
				{
					dnnNode.set_CustomAttribute("CommandName", action.CommandName);
					dnnNode.set_CustomAttribute("CommandArgument", action.CommandArgument);
				}
			}

			if (!String.IsNullOrEmpty(dnnNode.JSFunction))
			{
				dnnNode.JSFunction = string.Format(
					"if({0}){{{1}}};",
					dnnNode.JSFunction,
					Page.ClientScript.GetPostBackEventReference(navProvider.NavigationControl, dnnNode.ID));
			}

			foreach (DNNNode node in dnnNode.DNNNodes)
			{
				ProcessNode(node);
			}
		}

		private void SetMenuDefaults()
		{
			try
			{
				navProvider.StyleIconWidth = 15M;
				navProvider.MouseOutHideDelay = 500M;
				navProvider.MouseOverAction = NavigationProvider.HoverAction.Expand;
				navProvider.MouseOverDisplay = NavigationProvider.HoverDisplay.None;
				navProvider.CSSControl = "ModuleTitle_MenuBar";
				navProvider.CSSContainerRoot = "ModuleTitle_MenuContainer";
				navProvider.CSSNode = "ModuleTitle_MenuItem";
				navProvider.CSSIcon = "ModuleTitle_MenuIcon";
				navProvider.CSSContainerSub = "ModuleTitle_SubMenu";
				navProvider.CSSBreak = "ModuleTitle_MenuBreak";
				navProvider.CSSNodeHover = "ModuleTitle_MenuItemSel";
				navProvider.CSSIndicateChildSub = "ModuleTitle_MenuArrow";
				navProvider.CSSIndicateChildRoot = "ModuleTitle_RootMenuArrow";
				navProvider.PathImage = Globals.ApplicationPath + "/Images/";
				navProvider.PathSystemImage = Globals.ApplicationPath + "/Images/";
				navProvider.IndicateChildImageSub = "action_right.gif";
				navProvider.IndicateChildren = true;
				navProvider.StyleRoot = "background-color: Transparent; font-size: 1pt;";
				navProvider.NodeClick += ActionClick;
			}
			catch (Exception exc)
			{
				Exceptions.ProcessModuleLoadException(this, exc);
			}
		}
	}
}