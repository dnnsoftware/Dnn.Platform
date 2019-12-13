﻿// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
#region Usings

using System;
using System.Web.UI;
using System.Web.UI.HtmlControls;

using DotNetNuke.Framework;
using DotNetNuke.Framework.JavaScriptLibraries;
using DotNetNuke.UI.Skins;

#endregion

namespace DotNetNuke.UI.Containers.Controls
{
    /// -----------------------------------------------------------------------------
    /// <summary></summary>
    /// <remarks></remarks>
    /// -----------------------------------------------------------------------------
	[ParseChildren(false)]
	[PersistChildren(true)]
    public partial class Toggle : SkinObjectBase
	{
		#region "Private Properties"

    	private string _target;

		#endregion

		#region "Public Properties"

		public string Class { get; set; }

    	public string Target
    	{
    		get
    		{
				if(this.Parent == null || string.IsNullOrEmpty(_target))
				{
					return string.Empty;
				}

    			var targetControl = this.Parent.FindControl(_target);
				if(targetControl == null)
				{
					return string.Empty;
				}
				else
				{
					return targetControl.ClientID;
				}

    		}
			set
			{
				_target = value;
			}
    	}

		#endregion

		#region "Event Handlers"

		protected override void OnPreRender(EventArgs e)
		{
            JavaScript.RequestRegistration(CommonJs.jQuery);
            JavaScript.RequestRegistration(CommonJs.jQueryMigrate);

			var toggleScript = string.Format("<script type=\"text/javascript\">(function($){{$(\"#{0}\").find(\"a.toggleHandler\").click(function(e){{$(\"#{1}\").slideToggle();$(this).toggleClass('collapsed');e.preventDefault();}});}})(jQuery);</script>",
			                                 ClientID,
			                                 Target);
			Page.ClientScript.RegisterStartupScript(GetType(), ClientID, toggleScript);
		}

		protected override void Render(HtmlTextWriter writer)
		{
			writer.AddAttribute("id", ClientID);
			writer.AddAttribute("class", Class);
			writer.RenderBeginTag("h2");

			writer.AddAttribute("href", "#");
			writer.AddAttribute("class", "toggleHandler");
			writer.RenderBeginTag("a");

			RenderChildren(writer);

			writer.RenderEndTag();
			writer.RenderEndTag();
		}

		#endregion
	}
}
