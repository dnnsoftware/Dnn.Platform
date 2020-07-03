// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.UI.Containers.Controls
{
    using System;
    using System.Web.UI;
    using System.Web.UI.HtmlControls;

    using DotNetNuke.Framework;
    using DotNetNuke.Framework.JavaScriptLibraries;
    using DotNetNuke.UI.Skins;

    /// -----------------------------------------------------------------------------
    /// <summary></summary>
    /// <remarks></remarks>
    /// -----------------------------------------------------------------------------
    [ParseChildren(false)]
    [PersistChildren(true)]
    public partial class Toggle : SkinObjectBase
    {
        private string _target;

        public string Class { get; set; }

        public string Target
        {
            get
            {
                if (this.Parent == null || string.IsNullOrEmpty(this._target))
                {
                    return string.Empty;
                }

                var targetControl = this.Parent.FindControl(this._target);
                if (targetControl == null)
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
                this._target = value;
            }
        }

        protected override void OnPreRender(EventArgs e)
        {
            JavaScript.RequestRegistration(CommonJs.jQuery);
            JavaScript.RequestRegistration(CommonJs.jQueryMigrate);

            var toggleScript = string.Format(
                "<script type=\"text/javascript\">(function($){{$(\"#{0}\").find(\"a.toggleHandler\").click(function(e){{$(\"#{1}\").slideToggle();$(this).toggleClass('collapsed');e.preventDefault();}});}})(jQuery);</script>",
                this.ClientID,
                this.Target);
            this.Page.ClientScript.RegisterStartupScript(this.GetType(), this.ClientID, toggleScript);
        }

        protected override void Render(HtmlTextWriter writer)
        {
            writer.AddAttribute("id", this.ClientID);
            writer.AddAttribute("class", this.Class);
            writer.RenderBeginTag("h2");

            writer.AddAttribute("href", "#");
            writer.AddAttribute("class", "toggleHandler");
            writer.RenderBeginTag("a");

            this.RenderChildren(writer);

            writer.RenderEndTag();
            writer.RenderEndTag();
        }
    }
}
