using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Web.UI;
using DotNetNuke.Web.DDRMenu.Localisation;
using DotNetNuke.Web.DDRMenu.DNNCommon;
using DotNetNuke.Web.DDRMenu.TemplateEngine;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.UI;
using DotNetNuke.UI.Skins;

namespace DotNetNuke.Web.DDRMenu
{
	public class SkinObject : SkinObjectBase
	{
		public string MenuStyle { get; set; }
		public string NodeXmlPath { get; set; }
		public string NodeSelector { get; set; }
		public bool IncludeContext { get; set; }
		public bool IncludeHidden { get; set; }
		public string IncludeNodes { get; set; }
		public string ExcludeNodes { get; set; }
		public string NodeManipulator { get; set; }

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[PersistenceMode(PersistenceMode.InnerProperty)]
		public List<ClientOption> ClientOptions { get; set; }

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[PersistenceMode(PersistenceMode.InnerProperty)]
		public List<TemplateArgument> TemplateArguments { get; set; }

		private MenuBase menu;

		protected override void OnPreRender(EventArgs e)
		{
			using (new DNNContext(this))
			{
				try
				{
					base.OnPreRender(e);

					menu = MenuBase.Instantiate(MenuStyle);
					menu.ApplySettings(
						new Settings
						{
							MenuStyle = MenuStyle,
							NodeXmlPath = NodeXmlPath,
							NodeSelector = NodeSelector,
							IncludeContext = IncludeContext,
							IncludeHidden = IncludeHidden,
							IncludeNodes = IncludeNodes,
							ExcludeNodes = ExcludeNodes,
							NodeManipulator = NodeManipulator,
							ClientOptions = ClientOptions,
							TemplateArguments = TemplateArguments
						});

					if (String.IsNullOrEmpty(NodeXmlPath))
					{
						menu.RootNode =
							new MenuNode(
								Localiser.LocaliseDNNNodeCollection(
									Navigation.GetNavigationNodes(
										ClientID,
										Navigation.ToolTipSource.None,
										-1,
										-1,
										DNNAbstract.GetNavNodeOptions(true))));
					}

					menu.PreRender();
				}
				catch (Exception exc)
				{
					Exceptions.ProcessModuleLoadException(this, exc);
				}
			}
		}

		protected override void Render(HtmlTextWriter writer)
		{
			using (new DNNContext(this))
			{
				try
				{
					base.Render(writer);
					menu.Render(writer);
				}
				catch (Exception exc)
				{
					Exceptions.ProcessModuleLoadException(this, exc);
				}
			}
		}
	}
}