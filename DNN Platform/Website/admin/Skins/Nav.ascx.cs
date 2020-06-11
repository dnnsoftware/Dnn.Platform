﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

#region Usings

using System;

using DotNetNuke.Common;
using DotNetNuke.Modules.NavigationProvider;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.UI.WebControls;

#endregion

namespace DotNetNuke.UI.Skins.Controls
{
    [Obsolete("Support was removed for SolPart & Similar Modules in DNN 8.x, this control is no-longer functional to that point.  Usage of DDRMenu is suggested.  Scheduled removal in v10.0.0.")]
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
                bool blnIndicateChildren = bool.Parse(this.GetValue(this.IndicateChildren, "True"));
                string strRightArrow;
                string strDownArrow;
                var objSkins = new SkinController();
				
				//image for right facing arrow
                if (!String.IsNullOrEmpty(this.IndicateChildImageSub))
                {
                    strRightArrow = this.IndicateChildImageSub;
                }
                else
                {
                    strRightArrow = "breadcrumb.gif"; //removed APPIMAGEPATH token - https://www.dnnsoftware.com/Community/ForumsDotNetNuke/tabid/795/forumid/76/threadid/85554/scope/posts/Default.aspx
                }
				
				//image for down facing arrow
                if (!String.IsNullOrEmpty(this.IndicateChildImageRoot))
                {
                    strDownArrow = this.IndicateChildImageRoot;
                }
                else
                {
                    strDownArrow = "menu_down.gif"; //removed APPIMAGEPATH token - https://www.dnnsoftware.com/Community/ForumsDotNetNuke/tabid/795/forumid/76/threadid/85554/scope/posts/Default.aspx
                }
				
				//Set correct image path for all separator images
                if (!String.IsNullOrEmpty(this.SeparatorHTML))
                {
                    this.SeparatorHTML = this.FixImagePath(this.SeparatorHTML);
                }
				
                if (!String.IsNullOrEmpty(this.SeparatorLeftHTML))
                {
                    this.SeparatorLeftHTML = this.FixImagePath(this.SeparatorLeftHTML);
                }
				
                if (!String.IsNullOrEmpty(this.SeparatorRightHTML))
                {
                    this.SeparatorRightHTML = this.FixImagePath(this.SeparatorRightHTML);
                }
                if (!String.IsNullOrEmpty(this.SeparatorLeftHTMLBreadCrumb))
                {
                    this.SeparatorLeftHTMLBreadCrumb = this.FixImagePath(this.SeparatorLeftHTMLBreadCrumb);
                }
				
                if (!String.IsNullOrEmpty(this.SeparatorRightHTMLBreadCrumb))
                {
                    this.SeparatorRightHTMLBreadCrumb = this.FixImagePath(this.SeparatorRightHTMLBreadCrumb);
                }
				
                if (!String.IsNullOrEmpty(this.SeparatorLeftHTMLActive))
                {
                    this.SeparatorLeftHTMLActive = this.FixImagePath(this.SeparatorLeftHTMLActive);
                }
				
                if (!String.IsNullOrEmpty(this.SeparatorRightHTMLActive))
                {
                    this.SeparatorRightHTMLActive = this.FixImagePath(this.SeparatorRightHTMLActive);
                }
				
                if (!String.IsNullOrEmpty(this.NodeLeftHTMLBreadCrumbRoot))
                {
                    this.NodeLeftHTMLBreadCrumbRoot = this.FixImagePath(this.NodeLeftHTMLBreadCrumbRoot);
                }
				
                if (!String.IsNullOrEmpty(this.NodeRightHTMLBreadCrumbRoot))
                {
                    this.NodeRightHTMLBreadCrumbRoot = this.FixImagePath(this.NodeRightHTMLBreadCrumbRoot);
                }
				
                if (!String.IsNullOrEmpty(this.NodeLeftHTMLBreadCrumbSub))
                {
                    this.NodeLeftHTMLBreadCrumbSub = this.FixImagePath(this.NodeLeftHTMLBreadCrumbSub);
                }
				
                if (!String.IsNullOrEmpty(this.NodeRightHTMLBreadCrumbSub))
                {
                    this.NodeRightHTMLBreadCrumbSub = this.FixImagePath(this.NodeRightHTMLBreadCrumbSub);
                }
				
                if (!String.IsNullOrEmpty(this.NodeLeftHTMLRoot))
                {
                    this.NodeLeftHTMLRoot = this.FixImagePath(this.NodeLeftHTMLRoot);
                }
				
                if (!String.IsNullOrEmpty(this.NodeRightHTMLRoot))
                {
                    this.NodeRightHTMLRoot = this.FixImagePath(this.NodeRightHTMLRoot);
                }
				
                if (!String.IsNullOrEmpty(this.NodeLeftHTMLSub))
                {
                    this.NodeLeftHTMLSub = this.FixImagePath(this.NodeLeftHTMLSub);
                }
				
                if (!String.IsNullOrEmpty(this.NodeRightHTMLSub))
                {
                    this.NodeRightHTMLSub = this.FixImagePath(this.NodeRightHTMLSub);
                }
				
                if (String.IsNullOrEmpty(this.PathImage))
                {
                    this.PathImage = this.PortalSettings.HomeDirectory;
                }
				
                if (blnIndicateChildren)
                {
                    this.IndicateChildImageSub = strRightArrow;
                    if (this.ControlOrientation.ToLowerInvariant() == "vertical")
                    {
                        this.IndicateChildImageRoot = strRightArrow;
                    }
                    else
                    {
                        this.IndicateChildImageRoot = strDownArrow;
                    }
                }
                else
                {
                    this.IndicateChildImageSub = "[APPIMAGEPATH]spacer.gif";
                }
				
                this.PathSystemScript = Globals.ApplicationPath + "/controls/SolpartMenu/";
                this.PathSystemImage = "[APPIMAGEPATH]";
                this.BuildNodes(null);
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
            objNodes = this.GetNavigationNodes(objNode);
            this.Control.ClearNodes(); //since we always bind we need to clear the nodes for providers that maintain their state
            this.Bind(objNodes);
        }

        protected override void OnInit(EventArgs e)
        {
            this.InitializeNavControl(this, "SolpartMenuNavigationProvider");
            this.Control.NodeClick += this.Control_NodeClick;
            this.Control.PopulateOnDemand += this.Control_PopulateOnDemand;

            base.OnInit(e);
            this.InitializeComponent();
        }

        private void Control_NodeClick(NavigationEventArgs args)
        {
            if (args.Node == null)
            {
                args.Node = Navigation.GetNavigationNode(args.ID, this.Control.ID);
            }
            this.Response.Redirect(Globals.ApplicationURL(int.Parse(args.Node.Key)), true);
        }

        private void Control_PopulateOnDemand(NavigationEventArgs args)
        {
            if (args.Node == null)
            {
                args.Node = Navigation.GetNavigationNode(args.ID, this.Control.ID);
            }
            this.BuildNodes(args.Node);
        }
    }
}
