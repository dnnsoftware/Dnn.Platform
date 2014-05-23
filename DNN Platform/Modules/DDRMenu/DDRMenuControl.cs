using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using DotNetNuke.Web.DDRMenu.DNNCommon;

namespace DotNetNuke.Web.DDRMenu
{
	internal class DDRMenuControl : WebControl, IPostBackEventHandler
	{
		public override bool EnableViewState { get { return false; } set { } }

		internal MenuNode RootNode { get; set; }
		internal Boolean SkipLocalisation { get; set; }
		internal Settings MenuSettings { get; set; }

		public delegate void MenuClickEventHandler(string id);

		public event MenuClickEventHandler NodeClick;

		private MenuBase menu;

		protected override void OnPreRender(EventArgs e)
		{
			using (new DNNContext(this))
			{
				base.OnPreRender(e);

				MenuSettings.MenuStyle = MenuSettings.MenuStyle ?? "DNNMenu";
				menu = MenuBase.Instantiate(MenuSettings.MenuStyle);
				menu.RootNode = RootNode ?? new MenuNode();
				menu.SkipLocalisation = SkipLocalisation;
				menu.ApplySettings(MenuSettings);

				menu.PreRender();
			}
		}

		protected override void Render(HtmlTextWriter htmlWriter)
		{
			using (new DNNContext(this))
				menu.Render(htmlWriter);
		}

		public void RaisePostBackEvent(string eventArgument)
		{
			using (new DNNContext(this))
			{
				if (NodeClick != null)
				{
					NodeClick(eventArgument);
				}
			}
		}
	}
}