using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Telerik.Web.UI;

namespace DotNetNuke.RadEditorProvider.Components
{
	internal class DnnEditor : RadEditor
	{
		public bool PreventDefaultStylesheet { get; set; }

		protected override void RegisterCssReferences()
		{
			if (!PreventDefaultStylesheet)
			{
				base.RegisterCssReferences();
			}
		}
	}
}
