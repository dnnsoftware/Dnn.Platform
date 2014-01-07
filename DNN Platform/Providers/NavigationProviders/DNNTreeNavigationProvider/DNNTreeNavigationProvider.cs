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

using DotNetNuke.Modules.NavigationProvider;
using DotNetNuke.UI.WebControls;

#endregion

namespace DotNetNuke.NavigationControl
{
    public class DNNTreeNavigationProvider : NavigationProvider
    {
        private string _controlID;

        public DnnTree Tree { get; private set; }

        public override Control NavigationControl
        {
            get
            {
                return Tree;
            }
        }

        public override bool SupportsPopulateOnDemand
        {
            get
            {
                return true;
            }
        }

        public override string IndicateChildImageSub
        {
            get
            {
                return Tree.CollapsedNodeImage;
            }
            set
            {
                Tree.CollapsedNodeImage = value;
            }
        }

        public override string IndicateChildImageRoot
        {
            get
            {
                return Tree.CollapsedNodeImage;
            }
            set
            {
                Tree.CollapsedNodeImage = value;
            }
        }

        public override string WorkImage
        {
            get
            {
                return Tree.WorkImage;
            }
            set
            {
                Tree.WorkImage = value;
            }
        }

        public override string IndicateChildImageExpandedRoot
        {
            get
            {
                return Tree.ExpandedNodeImage;
            }
            set
            {
                Tree.ExpandedNodeImage = value;
            }
        }

        public override string IndicateChildImageExpandedSub
        {
            get
            {
                return Tree.ExpandedNodeImage;
            }
            set
            {
                Tree.ExpandedNodeImage = value;
            }
        }

        public override string ControlID
        {
            get
            {
                return _controlID;
            }
            set
            {
                _controlID = value;
            }
        }

        public override string CSSBreadCrumbSub { get; set; }

        public override string CSSBreadCrumbRoot { get; set; }

        public override string CSSControl
        {
            get
            {
                return Tree.CssClass;
            }
            set
            {
                Tree.CssClass = value;
            }
        }

        public override string CSSIcon
        {
            get
            {
                return Tree.DefaultIconCssClass;
            }
            set
            {
                Tree.DefaultIconCssClass = value;
            }
        }

        public override string CSSNode
        {
            get
            {
                return Tree.DefaultNodeCssClass;
            }
            set
            {
                Tree.DefaultNodeCssClass = value;
            }
        }

        public override string CSSNodeSelectedSub { get; set; }

        public override string CSSNodeSelectedRoot { get; set; }

        public override string CSSNodeHover
        {
            get
            {
                return Tree.DefaultNodeCssClassOver;
            }
            set
            {
                Tree.DefaultNodeCssClassOver = value;
            }
        }

        public override string CSSNodeRoot { get; set; }

        public override string CSSNodeHoverSub { get; set; }

        public override string CSSNodeHoverRoot { get; set; }

        public override string ForceDownLevel
        {
            get
            {
                return Tree.ForceDownLevel.ToString();
            }
            set
            {
                Tree.ForceDownLevel = Convert.ToBoolean(value);
            }
        }

        public override bool IndicateChildren { get; set; }

        public override bool PopulateNodesFromClient
        {
            get
            {
                return Tree.PopulateNodesFromClient;
            }
            set
            {
                Tree.PopulateNodesFromClient = value;
            }
        }

        public override string PathSystemImage
        {
            get
            {
                return Tree.SystemImagesPath;
            }
            set
            {
                Tree.SystemImagesPath = value;
            }
        }

        public override string PathImage { get; set; }

        public override string PathSystemScript
        {
            get
            {
                return Tree.TreeScriptPath;
            }
            set
            {
            }
        }

        public override void Initialize()
        {
            Tree = new DnnTree();
            Tree.ID = _controlID;
            Tree.NodeClick += DNNTree_NodeClick;
            Tree.PopulateOnDemand += DNNTree_PopulateOnDemand;
        }

        public override void Bind(DNNNodeCollection objNodes)
        {
            TreeNode objTreeItem;
            if (IndicateChildren == false)
            {
                IndicateChildImageSub = "";
                IndicateChildImageRoot = "";
                IndicateChildImageExpandedSub = "";
                IndicateChildImageExpandedRoot = "";
            }
            if (!String.IsNullOrEmpty(CSSNodeSelectedRoot) && CSSNodeSelectedRoot == CSSNodeSelectedSub)
            {
                Tree.DefaultNodeCssClassSelected = CSSNodeSelectedRoot; //set on parent, thus decreasing overall payload
            }
            foreach (DNNNode objNode in objNodes)
            {
                if (objNode.Level == 0) //root Tree
                {
                    int intIndex = Tree.TreeNodes.Import(objNode, true);
                    objTreeItem = Tree.TreeNodes[intIndex];
                    if (objNode.Enabled == false)
                    {
                        objTreeItem.ClickAction = eClickAction.Expand;
                    }
                    if (!String.IsNullOrEmpty(CSSNodeRoot))
                    {
                        objTreeItem.CssClass = CSSNodeRoot;
                    }
                    if (!String.IsNullOrEmpty(CSSNodeHoverRoot))
                    {
                        objTreeItem.CSSClassHover = CSSNodeHoverRoot;
                    }
                    if (String.IsNullOrEmpty(Tree.DefaultNodeCssClassSelected) && !String.IsNullOrEmpty(CSSNodeSelectedRoot))
                    {
                        objTreeItem.CSSClassSelected = CSSNodeSelectedRoot;
                    }
                    objTreeItem.CSSIcon = " "; //< ignore for root...???
                    if (objNode.BreadCrumb)
                    {
                        objTreeItem.CssClass = CSSBreadCrumbRoot;
                    }
                }
                else
                {
                    try
                    {
                        TreeNode objParent = Tree.TreeNodes.FindNode(objNode.ParentNode.ID);
                        if (objParent == null) //POD
                        {
                            objParent = Tree.TreeNodes[Tree.TreeNodes.Import(objNode.ParentNode.Clone(), true)];
                        }
                        objTreeItem = objParent.TreeNodes.FindNode(objNode.ID);
                        if (objTreeItem == null) //POD
                        {
                            objTreeItem = objParent.TreeNodes[objParent.TreeNodes.Import(objNode.Clone(), true)];
                        }
                        if (objNode.Enabled == false)
                        {
                            objTreeItem.ClickAction = eClickAction.Expand;
                        }
                        if (!String.IsNullOrEmpty(CSSNodeHover))
                        {
                            objTreeItem.CSSClassHover = CSSNodeHover;
                        }
                        if (String.IsNullOrEmpty(Tree.DefaultNodeCssClassSelected) && !String.IsNullOrEmpty(CSSNodeSelectedSub))
                        {
                            objTreeItem.CSSClassSelected = CSSNodeSelectedSub;
                        }
                        if (objNode.BreadCrumb)
                        {
                            objTreeItem.CssClass = CSSBreadCrumbSub;
                        }
                    }
                    catch
                    {
                        //throws exception if the parent tab has not been loaded ( may be related to user role security not allowing access to a parent tab )
                        objTreeItem = null;
                    }
                }
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
                    objTreeItem.Image = objNode.Image;
                }
                objTreeItem.ToolTip = objNode.ToolTip;

                //End Select
                if (objNode.Selected)
                {
                    Tree.SelectNode(objNode.ID);
                }
                Bind(objNode.DNNNodes);
            }
        }

        private void DNNTree_NodeClick(object source, DNNTreeNodeClickEventArgs e)
        {
            RaiseEvent_NodeClick(e.Node);
        }

        private void DNNTree_PopulateOnDemand(object source, DNNTreeEventArgs e)
        {
            RaiseEvent_PopulateOnDemand(e.Node);
        }

        public override void ClearNodes()
        {
            Tree.TreeNodes.Clear();
        }
    }
}