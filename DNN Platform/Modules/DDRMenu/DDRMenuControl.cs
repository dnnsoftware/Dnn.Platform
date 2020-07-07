// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.DDRMenu
{
    using System;
    using System.Web.UI;
    using System.Web.UI.WebControls;

    using DotNetNuke.Web.DDRMenu.DNNCommon;

    internal class DDRMenuControl : WebControl, IPostBackEventHandler
    {
        private MenuBase menu;

        public delegate void MenuClickEventHandler(string id);

        public event MenuClickEventHandler NodeClick;

        public override bool EnableViewState
        {
            get { return false; }
            set { }
        }

        internal MenuNode RootNode { get; set; }

        internal bool SkipLocalisation { get; set; }

        internal Settings MenuSettings { get; set; }

        public void RaisePostBackEvent(string eventArgument)
        {
            using (new DNNContext(this))
            {
                if (this.NodeClick != null)
                {
                    this.NodeClick(eventArgument);
                }
            }
        }

        protected override void OnPreRender(EventArgs e)
        {
            using (new DNNContext(this))
            {
                base.OnPreRender(e);

                this.MenuSettings.MenuStyle = this.MenuSettings.MenuStyle ?? "DNNMenu";
                this.menu = MenuBase.Instantiate(this.MenuSettings.MenuStyle);
                this.menu.RootNode = this.RootNode ?? new MenuNode();
                this.menu.SkipLocalisation = this.SkipLocalisation;
                this.menu.ApplySettings(this.MenuSettings);

                this.menu.PreRender();
            }
        }

        protected override void Render(HtmlTextWriter htmlWriter)
        {
            using (new DNNContext(this))
            {
                this.menu.Render(htmlWriter);
            }
        }
    }
}
