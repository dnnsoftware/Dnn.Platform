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
using System.Web.UI;
using System.Web.UI.HtmlControls;

using DotNetNuke.Framework;
using DotNetNuke.UI.Skins;

#endregion

namespace DotNetNuke.UI.Containers.Controls
{
    /// -----------------------------------------------------------------------------
    /// <summary></summary>
    /// <remarks></remarks>
    /// <history>
    /// 	[cniknet]	10/15/2004	Replaced public members with properties and removed
    ///                             brackets from property names
    /// </history>
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
			jQuery.RegisterJQuery(Page);

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