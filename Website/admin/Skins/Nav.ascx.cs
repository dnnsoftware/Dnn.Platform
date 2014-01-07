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
using DotNetNuke.Modules.NavigationProvider;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.UI.WebControls;

#endregion

namespace DotNetNuke.UI.Skins.Controls
{
    public partial class Nav : NavObjectBase
    {
        private void InitializeComponent()
        {
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            try
            {
                bool blnIndicateChildren = bool.Parse(GetValue(IndicateChildren, "True"));
                string strRightArrow;
                string strDownArrow;
                var objSkins = new SkinController();
				
				//image for right facing arrow
                if (!String.IsNullOrEmpty(IndicateChildImageSub))
                {
                    strRightArrow = IndicateChildImageSub;
                }
                else
                {
                    strRightArrow = "breadcrumb.gif"; //removed APPIMAGEPATH token - http://www.dotnetnuke.com/Community/ForumsDotNetNuke/tabid/795/forumid/76/threadid/85554/scope/posts/Default.aspx
                }
				
				//image for down facing arrow
                if (!String.IsNullOrEmpty(IndicateChildImageRoot))
                {
                    strDownArrow = IndicateChildImageRoot;
                }
                else
                {
                    strDownArrow = "menu_down.gif"; //removed APPIMAGEPATH token - http://www.dotnetnuke.com/Community/ForumsDotNetNuke/tabid/795/forumid/76/threadid/85554/scope/posts/Default.aspx
                }
				
				//Set correct image path for all separator images
                if (!String.IsNullOrEmpty(SeparatorHTML))
                {
                    SeparatorHTML = FixImagePath(SeparatorHTML);
                }
				
                if (!String.IsNullOrEmpty(SeparatorLeftHTML))
                {
                    SeparatorLeftHTML = FixImagePath(SeparatorLeftHTML);
                }
				
                if (!String.IsNullOrEmpty(SeparatorRightHTML))
                {
                    SeparatorRightHTML = FixImagePath(SeparatorRightHTML);
                }
                if (!String.IsNullOrEmpty(SeparatorLeftHTMLBreadCrumb))
                {
                    SeparatorLeftHTMLBreadCrumb = FixImagePath(SeparatorLeftHTMLBreadCrumb);
                }
				
                if (!String.IsNullOrEmpty(SeparatorRightHTMLBreadCrumb))
                {
                    SeparatorRightHTMLBreadCrumb = FixImagePath(SeparatorRightHTMLBreadCrumb);
                }
				
                if (!String.IsNullOrEmpty(SeparatorLeftHTMLActive))
                {
                    SeparatorLeftHTMLActive = FixImagePath(SeparatorLeftHTMLActive);
                }
				
                if (!String.IsNullOrEmpty(SeparatorRightHTMLActive))
                {
                    SeparatorRightHTMLActive = FixImagePath(SeparatorRightHTMLActive);
                }
				
                if (!String.IsNullOrEmpty(NodeLeftHTMLBreadCrumbRoot))
                {
                    NodeLeftHTMLBreadCrumbRoot = FixImagePath(NodeLeftHTMLBreadCrumbRoot);
                }
				
                if (!String.IsNullOrEmpty(NodeRightHTMLBreadCrumbRoot))
                {
                    NodeRightHTMLBreadCrumbRoot = FixImagePath(NodeRightHTMLBreadCrumbRoot);
                }
				
                if (!String.IsNullOrEmpty(NodeLeftHTMLBreadCrumbSub))
                {
                    NodeLeftHTMLBreadCrumbSub = FixImagePath(NodeLeftHTMLBreadCrumbSub);
                }
				
                if (!String.IsNullOrEmpty(NodeRightHTMLBreadCrumbSub))
                {
                    NodeRightHTMLBreadCrumbSub = FixImagePath(NodeRightHTMLBreadCrumbSub);
                }
				
                if (!String.IsNullOrEmpty(NodeLeftHTMLRoot))
                {
                    NodeLeftHTMLRoot = FixImagePath(NodeLeftHTMLRoot);
                }
				
                if (!String.IsNullOrEmpty(NodeRightHTMLRoot))
                {
                    NodeRightHTMLRoot = FixImagePath(NodeRightHTMLRoot);
                }
				
                if (!String.IsNullOrEmpty(NodeLeftHTMLSub))
                {
                    NodeLeftHTMLSub = FixImagePath(NodeLeftHTMLSub);
                }
				
                if (!String.IsNullOrEmpty(NodeRightHTMLSub))
                {
                    NodeRightHTMLSub = FixImagePath(NodeRightHTMLSub);
                }
				
                if (String.IsNullOrEmpty(PathImage))
                {
                    PathImage = PortalSettings.HomeDirectory;
                }
				
                if (blnIndicateChildren)
                {
                    IndicateChildImageSub = strRightArrow;
                    if (ControlOrientation.ToLower() == "vertical")
                    {
                        IndicateChildImageRoot = strRightArrow;
                    }
                    else
                    {
                        IndicateChildImageRoot = strDownArrow;
                    }
                }
                else
                {
                    IndicateChildImageSub = "[APPIMAGEPATH]spacer.gif";
                }
				
                PathSystemScript = Globals.ApplicationPath + "/controls/SolpartMenu/";
                PathSystemImage = "[APPIMAGEPATH]";
                BuildNodes(null);
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        private string FixImagePath(string strPath)
        {
            if (strPath.IndexOf("src=") != -1 && strPath.IndexOf("src=\"/") < 0)
            {
                return strPath.Replace("src=\"", "src=\"[SKINPATH]");
            }
            else
            {
                return strPath;
            }
        }

        private void BuildNodes(DNNNode objNode)
        {
            DNNNodeCollection objNodes;
            objNodes = GetNavigationNodes(objNode);
            Control.ClearNodes(); //since we always bind we need to clear the nodes for providers that maintain their state
            Bind(objNodes);
        }

        protected override void OnInit(EventArgs e)
        {
            InitializeNavControl(this, "SolpartMenuNavigationProvider");
            Control.NodeClick += Control_NodeClick;
            Control.PopulateOnDemand += Control_PopulateOnDemand;

            base.OnInit(e);
            InitializeComponent();
        }

        private void Control_NodeClick(NavigationEventArgs args)
        {
            if (args.Node == null)
            {
                args.Node = Navigation.GetNavigationNode(args.ID, Control.ID);
            }
            Response.Redirect(Globals.ApplicationURL(int.Parse(args.Node.Key)), true);
        }

        private void Control_PopulateOnDemand(NavigationEventArgs args)
        {
            if (args.Node == null)
            {
                args.Node = Navigation.GetNavigationNode(args.ID, Control.ID);
            }
            BuildNodes(args.Node);
        }
    }
}