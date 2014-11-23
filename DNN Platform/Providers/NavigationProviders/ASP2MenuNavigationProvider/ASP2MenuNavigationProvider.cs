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
using System.Web.UI.WebControls;

using DotNetNuke.Modules.NavigationProvider;
using DotNetNuke.UI.WebControls;

#endregion

namespace DotNetNuke.NavigationControl
{
    public class ASP2MenuNavigationProvider : NavigationProvider
    {
		#region "Member Variables"
		
        private Menu m_objMenu;
        private string m_strControlID;
        private string m_strNodeLeftHTMLBreadCrumbRoot = "";
        private string m_strNodeLeftHTMLBreadCrumbSub = "";
        private string m_strNodeLeftHTMLRoot = "";
        private string m_strNodeLeftHTMLSub = "";
        private string m_strNodeRightHTMLBreadCrumbRoot = "";
        private string m_strNodeRightHTMLBreadCrumbSub = "";
        private string m_strNodeRightHTMLRoot = "";
        private string m_strNodeRightHTMLSub = "";
        private string m_strPathImage = "";
        private string m_strSystemPathImage = "";
		
		#endregion
		
		#region "Properties"
		
		#region "General"
		
        public Menu Menu
        {
            get
            {
                return m_objMenu;
            }
        }

        public override Control NavigationControl
        {
            get
            {
                return Menu;
            }
        }

        public override bool SupportsPopulateOnDemand
        {
            get
            {
                return false;
            }
        }

        public override string ControlID
        {
            get
            {
                return m_strControlID;
            }
            set
            {
                m_strControlID = value;
            }
        }

		#endregion
		
		#region "Paths"
		
        public override string PathImage
        {
            get
            {
                return m_strPathImage;
            }
            set
            {
                m_strPathImage = value;
            }
        }

        public override string PathSystemImage
        {
            get
            {
                return m_strSystemPathImage;
            }
            set
            {
                m_strSystemPathImage = value;
            }
        }
		
		#endregion
		
		#region "Rendering"

        public override string ForceDownLevel
        {
            get
            {
                return (Menu.StaticDisplayLevels > 1).ToString();
            }
            set
            {
                if (Convert.ToBoolean(value))
                {
                    Menu.StaticDisplayLevels = 99;
                }
                else
                {
                    Menu.StaticDisplayLevels = 1;
                }
            }
        }

        public override Orientation ControlOrientation
        {
            get
            {
                switch (Menu.Orientation)
                {
                    case System.Web.UI.WebControls.Orientation.Horizontal:
                        return Orientation.Horizontal;
                    default:
                        return Orientation.Vertical;
                }
            }
            set
            {
                switch (value)
                {
                    case Orientation.Horizontal:
                        Menu.Orientation = System.Web.UI.WebControls.Orientation.Horizontal;
                        break;
                    case Orientation.Vertical:
                        Menu.Orientation = System.Web.UI.WebControls.Orientation.Vertical;
                        break;
                }
            }
        }
		
		#endregion

		#region "Mouse Properties"

        public override decimal MouseOutHideDelay
        {
            get
            {
                return Menu.DisappearAfter;
            }
            set
            {
                Menu.DisappearAfter = Convert.ToInt32(value);
            }
        }
		
		#endregion
		
		#region "Indicate Children"

        public override bool IndicateChildren
        {
            get
            {
                return Menu.DynamicEnableDefaultPopOutImage;
            }
            set
            {
                Menu.DynamicEnableDefaultPopOutImage = value;
            }
        }

        public override string IndicateChildImageSub
        {
            get
            {
                return Menu.DynamicPopOutImageUrl;
            }
            set
            {
                Menu.DynamicPopOutImageUrl = value;
            }
        }

        public override string IndicateChildImageRoot
        {
            get
            {
                return Menu.StaticPopOutImageUrl;
            }
            set
            {
                Menu.StaticPopOutImageUrl = value;
            }
        }
		
		#endregion
		
		#region "Custom HTML"

        public override string NodeLeftHTMLRoot
        {
            get
            {
                return m_strNodeLeftHTMLRoot;
            }
            set
            {
                m_strNodeLeftHTMLRoot = value;
            }
        }

        public override string NodeRightHTMLRoot
        {
            get
            {
                return m_strNodeRightHTMLRoot;
            }
            set
            {
                m_strNodeRightHTMLRoot = value;
            }
        }

        public override string NodeLeftHTMLSub
        {
            get
            {
                return m_strNodeLeftHTMLSub;
            }
            set
            {
                m_strNodeLeftHTMLSub = value;
            }
        }

        public override string NodeRightHTMLSub
        {
            get
            {
                return m_strNodeRightHTMLSub;
            }
            set
            {
                m_strNodeRightHTMLSub = value;
            }
        }

        public override string NodeLeftHTMLBreadCrumbSub
        {
            get
            {
                return m_strNodeLeftHTMLBreadCrumbSub;
            }
            set
            {
                m_strNodeLeftHTMLBreadCrumbSub = value;
            }
        }

        public override string NodeRightHTMLBreadCrumbSub
        {
            get
            {
                return m_strNodeRightHTMLBreadCrumbSub;
            }
            set
            {
                m_strNodeRightHTMLBreadCrumbSub = value;
            }
        }

        public override string NodeLeftHTMLBreadCrumbRoot
        {
            get
            {
                return m_strNodeLeftHTMLBreadCrumbRoot;
            }
            set
            {
                m_strNodeLeftHTMLBreadCrumbRoot = value;
            }
        }

        public override string NodeRightHTMLBreadCrumbRoot
        {
            get
            {
                return m_strNodeRightHTMLBreadCrumbRoot;
            }
            set
            {
                m_strNodeRightHTMLBreadCrumbRoot = value;
            }
        }

		#endregion
		
		#region "CSS"
		
        public override string CSSControl
        {
            get
            {
                return Menu.DynamicMenuStyle.CssClass;
            }
            set
            {
                Menu.DynamicMenuStyle.CssClass = value;
            }
        }

        public override string CSSNode
        {
            get
            {
                return Menu.DynamicMenuItemStyle.CssClass;
            }
            set
            {
                Menu.DynamicMenuItemStyle.CssClass = value;
                for (int i = 0; i <= Menu.LevelMenuItemStyles.Count - 1; i++)
                {
                    Menu.LevelMenuItemStyles[i].CssClass = value;
                }
            }
        }

        public override string CSSNodeRoot
        {
            get
            {
                return Menu.LevelMenuItemStyles[0].CssClass;
            }
            set
            {
                Menu.LevelMenuItemStyles[0].CssClass = value;
            }
        }

        public override string CSSNodeSelectedRoot
        {
            get
            {
                return Menu.LevelSelectedStyles[0].CssClass;
            }
            set
            {
                Menu.LevelSelectedStyles[0].CssClass = value;
            }
        }

        public override string CSSNodeSelectedSub
        {
            get
            {
                return Menu.LevelSelectedStyles[1].CssClass;
            }
            set
            {
                for (int i = 1; i <= Menu.LevelSelectedStyles.Count - 1; i++)
                {
                    Menu.LevelSelectedStyles[i].CssClass = value;
                }
            }
        }

        //* Same as CSSNodeHoverSub
        public override string CSSNodeHover
        {
            get
            {
                return Menu.DynamicHoverStyle.CssClass;
            }
            set
            {
                Menu.DynamicHoverStyle.CssClass = value;
                Menu.StaticHoverStyle.CssClass = value;
            }
        }

        public override string CSSNodeHoverSub
        {
            get
            {
                return Menu.DynamicHoverStyle.CssClass;
            }
            set
            {
                Menu.DynamicHoverStyle.CssClass = value;
            }
        }

        public override string CSSNodeHoverRoot
        {
            get
            {
                return Menu.StaticHoverStyle.CssClass;
            }
            set
            {
                Menu.StaticHoverStyle.CssClass = value;
            }
        }
		
		#endregion

		#endregion

		#region "Event Handlers"
		
        /// <summary>
        /// This method is called by the provider to allow for the control to default values and set up
        /// event handlers
        /// </summary>
        /// <remarks></remarks>
        public override void Initialize()
        {
            m_objMenu = new Menu();
            Menu.ID = m_strControlID;
            Menu.EnableViewState = false;	//Not sure why, but when we disable viewstate the menuitemclick does not fire...
            
			//default properties to match DNN defaults 
            Menu.DynamicPopOutImageUrl = "";
            Menu.StaticPopOutImageUrl = "";
            ControlOrientation = Orientation.Horizontal;

            //add event handlers
            Menu.MenuItemClick += Menu_NodeClick;
            Menu.PreRender += Menu_PreRender;
			
			//add how many levels worth of styles???
            for (int i = 0; i <= 6; i++)
            {
                Menu.LevelMenuItemStyles.Add(new MenuItemStyle());
                Menu.LevelSelectedStyles.Add(new MenuItemStyle());
            }
        }

        /// <summary>
        /// Responsible for the populating of the underlying navigation control 
        /// </summary>
        /// <param name="objNodes">Node hierarchy used in control population</param>
        /// <remarks></remarks>
        public override void Bind(DNNNodeCollection objNodes)
        {
            MenuItem objMenuItem = null;
            DNNNode objPrevNode;
            if (IndicateChildren == false)
            {
                IndicateChildImageSub = "";
                IndicateChildImageRoot = "";
            }
            string strLeftHTML = "";
            string strRightHTML = "";
            foreach (DNNNode objNode in objNodes)
            {
                if (objNode.IsBreak)
                {
                }
                else
                {
                    strLeftHTML = "";
                    strRightHTML = "";
                    if (objNode.Level == 0) //root menu
                    {
                        Menu.Items.Add(GetMenuItem(objNode));
                        objMenuItem = Menu.Items[Menu.Items.Count - 1];
                        if (!String.IsNullOrEmpty(NodeLeftHTMLRoot))
                        {
                            strLeftHTML = NodeLeftHTMLRoot;
                        }
                        if (objNode.BreadCrumb)
                        {
                            if (!String.IsNullOrEmpty(NodeLeftHTMLBreadCrumbRoot))
                            {
                                strLeftHTML = NodeLeftHTMLBreadCrumbRoot;
                            }
                            if (!String.IsNullOrEmpty(NodeRightHTMLBreadCrumbRoot))
                            {
                                strRightHTML = NodeRightHTMLBreadCrumbRoot;
                            }
                        }
                        if (!String.IsNullOrEmpty(NodeRightHTMLRoot))
                        {
                            strRightHTML = NodeRightHTMLRoot;
                        }
                    }
                    else
                    {
                        try
                        {
                            MenuItem objParent = Menu.FindItem(GetValuePath(objNode.ParentNode));
                            objParent.ChildItems.Add(GetMenuItem(objNode));
                            objMenuItem = objParent.ChildItems[objParent.ChildItems.Count - 1];
                            if (!String.IsNullOrEmpty(NodeLeftHTMLSub))
                            {
                                strLeftHTML = NodeLeftHTMLSub;
                            }
                            if (objNode.BreadCrumb)
                            {
                                if (!String.IsNullOrEmpty(NodeLeftHTMLBreadCrumbSub))
                                {
                                    strLeftHTML = NodeLeftHTMLBreadCrumbSub;
                                }
                                if (!String.IsNullOrEmpty(NodeRightHTMLBreadCrumbSub))
                                {
                                    strRightHTML = NodeRightHTMLBreadCrumbSub;
                                }
                            }
                            if (!String.IsNullOrEmpty(NodeRightHTMLSub))
                            {
                                strRightHTML = NodeRightHTMLSub;
                            }
                        }
                        catch
                        {
                            //throws exception if the parent tab has not been loaded ( may be related to user role security not allowing access to a parent tab )
                            objMenuItem = null;
                        }
                    }
                    if (objMenuItem != null)
                    {
						//Append LeftHTML/RightHTML to menu's text property
                        if (!String.IsNullOrEmpty(strLeftHTML))
                        {
                            objMenuItem.Text = strLeftHTML + objMenuItem.Text;
                        }
                        if (!String.IsNullOrEmpty(strRightHTML))
                        {
                            objMenuItem.Text = objMenuItem.Text + strRightHTML;
                        }
                    }
					
					//Figure out image paths
                    if (!String.IsNullOrEmpty(objNode.Image))
                    {
                        if (objNode.Image.StartsWith("~/images/"))
                        {
                            objNode.Image = objNode.Image.Replace("~/images/", PathSystemImage);
                        }
                        else if (!objNode.Image.Contains("://") && objNode.Image.StartsWith("/") == false && !String.IsNullOrEmpty(PathImage))
                        {
                            objNode.Image = PathImage + objNode.Image;
                        }
                        objMenuItem.ImageUrl = objNode.Image;
                    }
                    Bind(objNode.DNNNodes);
                    objPrevNode = objNode;
                }
            }
        }

        private void Menu_NodeClick(object source, MenuEventArgs e)
        {
            base.RaiseEvent_NodeClick(e.Item.Value);
        }

        private void Menu_PreRender(object sender, EventArgs e)
        {
            if (!String.IsNullOrEmpty(Menu.StaticPopOutImageUrl))
            {
                Menu.StaticPopOutImageUrl = PathSystemImage + Menu.StaticPopOutImageUrl;
            }
            if (!String.IsNullOrEmpty(Menu.DynamicPopOutImageUrl))
            {
                Menu.DynamicPopOutImageUrl = PathSystemImage + Menu.DynamicPopOutImageUrl;
            }
        }

		#endregion

		#region "Private Methods"

		/// <summary>
        /// Loops through each of the nodes parents and concatenates the keys to derive its valuepath
        /// </summary>
        /// <param name="objNode">DNNNode object to obtain valuepath from</param>
        /// <returns>ValuePath of node</returns>
        /// <remarks>
        /// the ASP.NET Menu creates a unique key based off of all the menuitem's parents, delimited by a string.
        /// I wish there was a way around this, for we are already guaranteeing the uniqueness of the key since is it pulled from the
        /// database.  
        /// </remarks>
        private string GetValuePath(DNNNode objNode)
        {
            DNNNode objParent = objNode.ParentNode;
            string strPath = objNode.Key;
            do
            {
                if (objParent == null || objParent.Level == -1)
                {
                    break;
                }
                strPath = objParent.Key + Menu.PathSeparator + strPath;
                objParent = objParent.ParentNode;
            } while (true);
            return strPath;
        }

        /// <summary>
        /// Create a ASP.NET Menu item for a given DNNNode
        /// </summary>
        /// <param name="objNode">Node to create item off of</param>
        /// <returns></returns>
        /// <remarks>
        /// Due to ValuePath needed for postback routine, there is a HACK to replace out the 
        /// id with the valuepath if a JSFunciton is specified
        /// </remarks>
        private MenuItem GetMenuItem(DNNNode objNode)
        {
            var objItem = new MenuItem();
            objItem.Text = objNode.Text;
            objItem.Value = objNode.Key;
            if (!String.IsNullOrEmpty(objNode.JSFunction))
            {
                //HACK...  The postback event needs to have the entire ValuePath to the menu item, not just the unique id
                //__doPostBack('dnn:ctr365:dnnACTIONS:ctldnnACTIONS','6') -> __doPostBack('dnn:ctr365:dnnACTIONS:ctldnnACTIONS','0\\6')}
                objItem.NavigateUrl = "javascript:" + objNode.JSFunction.Replace(Menu.ID + "','" + objNode.Key + "'", Menu.ID + "'.'" + GetValuePath(objNode).Replace("/", "\\\\") + "'") +
                                      objNode.NavigateURL;
            }
            else
            {
                objItem.NavigateUrl = objNode.NavigateURL;
            }
            objItem.Target = objNode.Target;
            objItem.Selectable = objNode.Enabled;
            objItem.ImageUrl = objNode.Image; //possibly fix for path
            objItem.Selected = objNode.Selected;
            objItem.ToolTip = objNode.ToolTip;
            return objItem;
		}

		#endregion
	}
}
