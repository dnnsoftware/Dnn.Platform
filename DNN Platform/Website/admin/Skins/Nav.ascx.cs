// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
#region Usings

using System;

using DotNetNuke.Common;
using DotNetNuke.Modules.NavigationProvider;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.UI.WebControls;

#endregion

namespace DotNetNuke.UI.Skins.Controls
{
    [Obsolete("Support was removed for SolPart & Similar Modules in DNN 8.x, this control is no-longer functional to that point.  Usage of DDRMenu is suggested.  Scheduled removal in v11.0.0.")]
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
                    strRightArrow = "breadcrumb.gif"; //removed APPIMAGEPATH token - https://www.dnnsoftware.com/Community/ForumsDotNetNuke/tabid/795/forumid/76/threadid/85554/scope/posts/Default.aspx
                }
				
				//image for down facing arrow
                if (!String.IsNullOrEmpty(IndicateChildImageRoot))
                {
                    strDownArrow = IndicateChildImageRoot;
                }
                else
                {
                    strDownArrow = "menu_down.gif"; //removed APPIMAGEPATH token - https://www.dnnsoftware.com/Community/ForumsDotNetNuke/tabid/795/forumid/76/threadid/85554/scope/posts/Default.aspx
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
                    if (ControlOrientation.ToLowerInvariant() == "vertical")
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
