// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.DDRMenu
{
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

    public class Actions : ActionBase
    {
        private DDRMenuNavigationProvider navProvider;
        private Dictionary<int, ModuleAction> actions;

        public string PathSystemScript { get; set; }

        public string MenuStyle { get; set; }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        [PersistenceMode(PersistenceMode.InnerProperty)]
        public List<ClientOption> ClientOptions { get; set; }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        [PersistenceMode(PersistenceMode.InnerProperty)]
        public List<TemplateArgument> TemplateArguments { get; set; }

        protected override void OnInit(EventArgs e)
        {
            using (new DNNContext(this))
            {
                base.OnInit(e);

                this.navProvider = (DDRMenuNavigationProvider)NavigationProvider.Instance("DDRMenuNavigationProvider");
                this.navProvider.ControlID = "ctl" + this.ID;
                this.navProvider.MenuStyle = this.MenuStyle;
                this.navProvider.Initialize();

                this.Controls.Add(this.navProvider.NavigationControl);
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            using (new DNNContext(this))
            {
                base.OnLoad(e);

                this.SetMenuDefaults();
            }
        }

        protected override void OnPreRender(EventArgs e)
        {
            using (new DNNContext(this))
            {
                base.OnPreRender(e);

                try
                {
                    this.navProvider.TemplateArguments = this.TemplateArguments;
                    this.BindMenu(Navigation.GetActionNodes(this.ActionRoot, this, -1));
                }
                catch (Exception exc)
                {
                    Exceptions.ProcessModuleLoadException(this, exc);
                }
            }
        }

        private void BindMenu(DNNNodeCollection objNodes)
        {
            this.Visible = this.DisplayControl(objNodes);
            if (!this.Visible)
            {
                return;
            }

            this.navProvider.ClearNodes();
            foreach (DNNNode node in objNodes)
            {
                this.ProcessNode(node);
            }

            this.navProvider.Bind(objNodes, false);
        }

        private void ActionClick(NavigationEventArgs args)
        {
            using (new DNNContext(this))
            {
                try
                {
                    this.ProcessAction(args.ID);
                }
                catch (Exception exc)
                {
                    Exceptions.ProcessModuleLoadException(this, exc);
                }
            }
        }

        private void AddActionIDs(ModuleAction action)
        {
            if (!this.actions.ContainsKey(action.ID))
            {
                this.actions.Add(action.ID, action);
            }

            if (action.HasChildren())
            {
                foreach (ModuleAction a in action.Actions)
                {
                    this.AddActionIDs(a);
                }
            }
        }

        private ModuleAction FindAction(int id)
        {
            if (this.actions == null)
            {
                this.actions = new Dictionary<int, ModuleAction>();
                this.AddActionIDs(this.ActionRoot);
            }

            ModuleAction result;
            return this.actions.TryGetValue(id, out result) ? result : null;
        }

        private void ProcessNode(DNNNode dnnNode)
        {
            if (!dnnNode.IsBreak)
            {
                var action = this.FindAction(Convert.ToInt32(dnnNode.Key));
                if (action != null)
                {
                    dnnNode.set_CustomAttribute("CommandName", action.CommandName);
                    dnnNode.set_CustomAttribute("CommandArgument", action.CommandArgument);
                }
            }

            if (!string.IsNullOrEmpty(dnnNode.JSFunction))
            {
                dnnNode.JSFunction = string.Format(
                    "if({0}){{{1}}};",
                    dnnNode.JSFunction,
                    this.Page.ClientScript.GetPostBackEventReference(this.navProvider.NavigationControl, dnnNode.ID));
            }

            foreach (DNNNode node in dnnNode.DNNNodes)
            {
                this.ProcessNode(node);
            }
        }

        private void SetMenuDefaults()
        {
            try
            {
                this.navProvider.StyleIconWidth = 15M;
                this.navProvider.MouseOutHideDelay = 500M;
                this.navProvider.MouseOverAction = NavigationProvider.HoverAction.Expand;
                this.navProvider.MouseOverDisplay = NavigationProvider.HoverDisplay.None;
                this.navProvider.CSSControl = "ModuleTitle_MenuBar";
                this.navProvider.CSSContainerRoot = "ModuleTitle_MenuContainer";
                this.navProvider.CSSNode = "ModuleTitle_MenuItem";
                this.navProvider.CSSIcon = "ModuleTitle_MenuIcon";
                this.navProvider.CSSContainerSub = "ModuleTitle_SubMenu";
                this.navProvider.CSSBreak = "ModuleTitle_MenuBreak";
                this.navProvider.CSSNodeHover = "ModuleTitle_MenuItemSel";
                this.navProvider.CSSIndicateChildSub = "ModuleTitle_MenuArrow";
                this.navProvider.CSSIndicateChildRoot = "ModuleTitle_RootMenuArrow";
                this.navProvider.PathImage = Globals.ApplicationPath + "/Images/";
                this.navProvider.PathSystemImage = Globals.ApplicationPath + "/Images/";
                this.navProvider.IndicateChildImageSub = "action_right.gif";
                this.navProvider.IndicateChildren = true;
                this.navProvider.StyleRoot = "background-color: Transparent; font-size: 1pt;";
                this.navProvider.NodeClick += this.ActionClick;
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }
    }
}
