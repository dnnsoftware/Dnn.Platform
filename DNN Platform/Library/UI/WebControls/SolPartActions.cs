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

using DotNetNuke.Common;
using DotNetNuke.Entities.Modules.Actions;
using DotNetNuke.Modules.NavigationProvider;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.UI.Containers;

#endregion

namespace DotNetNuke.UI.WebControls
{
    public class SolPartActions : ActionBase
    {
        private int m_intExpandDepth = -1;
        private NavigationProvider m_objControl;
        private string m_strProviderName = "SolpartMenuNavigationProvider";

        public string ProviderName
        {
            get
            {
                return m_strProviderName;
            }
            set
            {
                m_strProviderName = value;
            }
        }

        public bool PopulateNodesFromClient { get; set; }

        public int ExpandDepth //JH - POD
        {
            get
            {
                if (PopulateNodesFromClient == false || Control.SupportsPopulateOnDemand == false)
                {
                    return -1;
                }
                return m_intExpandDepth;
            }
            set
            {
                m_intExpandDepth = value;
            }
        }

        public NavigationProvider Control //Modules.ActionProvider.ActionProvider
        {
            get
            {
                return m_objControl;
            }
        }

        public string PathSystemScript { get; set; }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            SetMenuDefaults();
        }

        private void SetMenuDefaults()
        {
            try
            {
				//--- original page set attributes ---
                Control.StyleIconWidth = 15;
                Control.MouseOutHideDelay = 500;
                Control.MouseOverAction = NavigationProvider.HoverAction.Expand;
                Control.MouseOverDisplay = NavigationProvider.HoverDisplay.None;

                //style sheet settings
                Control.CSSControl = "ModuleTitle_MenuBar"; //ctlActions.MenuCSS.MenuBar
                Control.CSSContainerRoot = "ModuleTitle_MenuContainer"; //ctlActions.MenuCSS.MenuContainer
                Control.CSSNode = "ModuleTitle_MenuItem"; //ctlActions.MenuCSS.MenuItem
                Control.CSSIcon = "ModuleTitle_MenuIcon"; //ctlActions.MenuCSS.MenuIcon
                Control.CSSContainerSub = "ModuleTitle_SubMenu"; //ctlActions.MenuCSS.SubMenu
                Control.CSSBreak = "ModuleTitle_MenuBreak"; //ctlActions.MenuCSS.MenuBreak
                Control.CSSNodeHover = "ModuleTitle_MenuItemSel"; //ctlActions.MenuCSS.MenuItemSel
                Control.CSSIndicateChildSub = "ModuleTitle_MenuArrow"; //ctlActions.MenuCSS.MenuArrow
                Control.CSSIndicateChildRoot = "ModuleTitle_RootMenuArrow"; //ctlActions.MenuCSS.RootMenuArrow
                
				//generate dynamic menu
				if (String.IsNullOrEmpty(Control.PathSystemScript))
                {
                    Control.PathSystemScript = Globals.ApplicationPath + "/Controls/SolpartMenu/";
                }
                Control.PathImage = Globals.ApplicationPath + "/Images/";
                Control.PathSystemImage = Globals.ApplicationPath + "/Images/";
                Control.IndicateChildImageSub = "action_right.gif";
                Control.IndicateChildren = true;
                Control.StyleRoot = "background-color: Transparent; font-size: 1pt;"; //backwards compatibility HACK
                Control.NodeClick += ctlActions_MenuClick;
            }
            catch (Exception exc) //Module failed to load
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        private void ctlActions_MenuClick(NavigationEventArgs args) //Handles ctlActions.MenuClick
        {
            try
            {
                ProcessAction(args.ID);
            }
            catch (Exception exc) //Module failed to load
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        public void BindMenu()
        {
            BindMenu(Navigation.GetActionNodes(ActionRoot, this, ExpandDepth));
        }

        private void BindMenu(DNNNodeCollection objNodes)
        {
            Visible = DisplayControl(objNodes);
            if (Visible)
            {
                Control.ClearNodes(); //since we always bind we need to clear the nodes for providers that maintain their state
                foreach (DNNNode objNode in objNodes)
                {
                    ProcessNodes(objNode);
                }
                Control.Bind(objNodes);
            }
        }

        private void ProcessNodes(DNNNode objParent)
        {
            if (!String.IsNullOrEmpty(objParent.JSFunction))
            {
                objParent.JSFunction = string.Format("if({0}){{{1}}};", objParent.JSFunction, Page.ClientScript.GetPostBackEventReference(Control.NavigationControl, objParent.ID));
            }
            foreach (DNNNode objNode in objParent.DNNNodes)
            {
                ProcessNodes(objNode);
            }
        }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            try
            {
                BindMenu();
            }
            catch (Exception exc) //Module failed to load
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        protected override void OnInit(EventArgs e)
        {
            m_objControl = NavigationProvider.Instance(ProviderName);
            Control.PopulateOnDemand += Control_PopulateOnDemand;
            base.OnInit(e);
            Control.ControlID = "ctl" + ID;
            Control.Initialize();
            Controls.Add(Control.NavigationControl);
        }

        private void Control_PopulateOnDemand(NavigationEventArgs args)
        {
            SetMenuDefaults();
            ActionRoot.Actions.AddRange(Actions); //Modules how add custom actions in control lifecycle will not have those actions populated... 

            ModuleAction objAction = ActionRoot;
            if (ActionRoot.ID != Convert.ToInt32(args.ID))
            {
                objAction = ActionManager.GetAction(Convert.ToInt32(args.ID));
            }
            if (args.Node == null)
            {
                args.Node = Navigation.GetActionNode(args.ID, Control.ID, objAction, this);
            }
            Control.ClearNodes(); //since we always bind we need to clear the nodes for providers that maintain their state
            BindMenu(Navigation.GetActionNodes(objAction, args.Node, this, ExpandDepth));
        }
    }
}