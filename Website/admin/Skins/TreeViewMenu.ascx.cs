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
using DotNetNuke.Services.Localization;
using DotNetNuke.UI.WebControls;

#endregion

namespace DotNetNuke.UI.Skins.Controls
{
	/// -----------------------------------------------------------------------------
	/// Project	 : DotNetNuke
	/// Class	 : TreeViewMenu
	/// -----------------------------------------------------------------------------
	/// <summary>
	/// TreeViewMenu is a Skin Object that creates a Menu using the DNN Treeview Control
	/// to provide a Windows Explore like Menu.
	/// </summary>
	/// <remarks></remarks>
	/// <history>
	/// 	[cnurse]	12/8/2004	created
	/// </history>
	/// -----------------------------------------------------------------------------
    public partial class TreeViewMenu : NavObjectBase
    {
		#region "Private Members"
		
        private const string MyFileName = "TreeViewMenu.ascx";
        private string _bodyCssClass = "";
        private string _cssClass = "";
        private string _headerCssClass = "";
        private string _headerText = "";
        private string _headerTextCssClass = "Head";
        private bool _includeHeader = true;
        private string _nodeChildCssClass = "Normal";
        private string _nodeClosedImage = "~/images/folderclosed.gif";
        private string _nodeCollapseImage = "~/images/min.gif";
        private string _nodeCssClass = "Normal";
        private string _nodeExpandImage = "~/images/max.gif";
        private string _nodeLeafImage = "~/images/file.gif";
        private string _nodeOpenImage = "~/images/folderopen.gif";
        private string _nodeOverCssClass = "Normal";
        private string _nodeSelectedCssClass = "Normal";
        private string _resourceKey = "";
        private string _treeCssClass = "";
        private string _treeGoUpImage = "~/images/folderup.gif";
        private int _treeIndentWidth = 10;
        private string _width = "100%";

		#endregion

		#region "Public Properties"

        public string BodyCssClass
        {
            get
            {
                return _bodyCssClass;
            }
            set
            {
                _bodyCssClass = value;
            }
        }

        public string CssClass
        {
            get
            {
                return _cssClass;
            }
            set
            {
                _cssClass = value;
            }
        }

        public string HeaderCssClass
        {
            get
            {
                return _headerCssClass;
            }
            set
            {
                _headerCssClass = value;
            }
        }

        public string HeaderTextCssClass
        {
            get
            {
                return _headerTextCssClass;
            }
            set
            {
                _headerTextCssClass = value;
            }
        }

        public string HeaderText
        {
            get
            {
                return _headerText;
            }
            set
            {
                _headerText = value;
            }
        }

        public bool IncludeHeader
        {
            get
            {
                return _includeHeader;
            }
            set
            {
                _includeHeader = value;
            }
        }

        public string NodeChildCssClass
        {
            get
            {
                return _nodeChildCssClass;
            }
            set
            {
                _nodeChildCssClass = value;
            }
        }

        public string NodeClosedImage
        {
            get
            {
                return _nodeClosedImage;
            }
            set
            {
                _nodeClosedImage = value;
            }
        }

        public string NodeCollapseImage
        {
            get
            {
                return _nodeCollapseImage;
            }
            set
            {
                _nodeCollapseImage = value;
            }
        }

        public string NodeCssClass
        {
            get
            {
                return _nodeCssClass;
            }
            set
            {
                _nodeCssClass = value;
            }
        }

        public string NodeExpandImage
        {
            get
            {
                return _nodeExpandImage;
            }
            set
            {
                _nodeExpandImage = value;
            }
        }

        public string NodeLeafImage
        {
            get
            {
                return _nodeLeafImage;
            }
            set
            {
                _nodeLeafImage = value;
            }
        }

        public string NodeOpenImage
        {
            get
            {
                return _nodeOpenImage;
            }
            set
            {
                _nodeOpenImage = value;
            }
        }

        public string NodeOverCssClass
        {
            get
            {
                return _nodeOverCssClass;
            }
            set
            {
                _nodeOverCssClass = value;
            }
        }

        public string NodeSelectedCssClass
        {
            get
            {
                return _nodeSelectedCssClass;
            }
            set
            {
                _nodeSelectedCssClass = value;
            }
        }

        public bool NoWrap { get; set; }

        public string ResourceKey
        {
            get
            {
                return _resourceKey;
            }
            set
            {
                _resourceKey = value;
            }
        }

        public string TreeCssClass
        {
            get
            {
                return _treeCssClass;
            }
            set
            {
                _treeCssClass = value;
            }
        }

        public string TreeGoUpImage
        {
            get
            {
                return _treeGoUpImage;
            }
            set
            {
                _treeGoUpImage = value;
            }
        }

        public int TreeIndentWidth
        {
            get
            {
                return _treeIndentWidth;
            }
            set
            {
                _treeIndentWidth = value;
            }
        }

        public string Width
        {
            get
            {
                return _width;
            }
            set
            {
                _width = value;
            }
        }

        private void InitializeComponent()
        {
        }

		#endregion

		#region "Private Methods"
		
        /// -----------------------------------------------------------------------------
        /// <summary>
        /// The BuildTree helper method is used to build the tree
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <history>
        ///     [cnurse]        12/8/2004   Created
        ///		[Jon Henning]	3/21/06		Updated to handle Auto-expand and AddUpNode	
        /// </history>
        /// -----------------------------------------------------------------------------
        private void BuildTree(DNNNode objNode, bool blnPODRequest)
        {
            bool blnAddUpNode = false;
            DNNNodeCollection objNodes;
            objNodes = GetNavigationNodes(objNode);

            if (blnPODRequest == false)
            {
                if (!string.IsNullOrEmpty(Level))
                {
                    switch (Level.ToLower())
                    {
                        case "root":
                            break;
                        case "child":
                            blnAddUpNode = true;
                            break;
                        default:
                            if (Level.ToLower() != "root" && PortalSettings.ActiveTab.BreadCrumbs.Count > 1)
                            {
                                blnAddUpNode = true;
                            }
                            break;
                    }
                }
            }
			
			//add goto Parent node
            if (blnAddUpNode)
            {
                var objParentNode = new DNNNode();
                objParentNode.ID = PortalSettings.ActiveTab.ParentId.ToString();
                objParentNode.Key = objParentNode.ID;
                objParentNode.Text = Localization.GetString("Parent", Localization.GetResourceFile(this, MyFileName));
                objParentNode.ToolTip = Localization.GetString("GoUp", Localization.GetResourceFile(this, MyFileName));
                objParentNode.CSSClass = NodeCssClass;
                objParentNode.Image = ResolveUrl(TreeGoUpImage);
                objParentNode.ClickAction = eClickAction.PostBack;
                objNodes.InsertBefore(0, objParentNode);
            }
            foreach (DNNNode objPNode in objNodes) //clean up to do in processnodes???
            {
                ProcessNodes(objPNode);
            }
            Bind(objNodes);

            //technically this should always be a dnntree.  If using dynamic controls Nav.ascx should be used.  just being safe.
            if (Control.NavigationControl is DnnTree)
            {
                var objTree = (DnnTree) Control.NavigationControl;
                if (objTree.SelectedTreeNodes.Count > 0)
                {
                    var objTNode = (TreeNode) objTree.SelectedTreeNodes[1];
                    if (objTNode.DNNNodes.Count > 0) //only expand it if nodes are not pending
                    {
                        objTNode.Expand();
                    }
                }
            }
        }

        private void ProcessNodes(DNNNode objParent)
        {
            if (!String.IsNullOrEmpty(objParent.Image))
            {
            }
            else if (objParent.HasNodes) //imagepath applied in provider...
            {
                objParent.Image = ResolveUrl(NodeClosedImage);
            }
            else
            {
                objParent.Image = ResolveUrl(NodeLeafImage);
            }
            foreach (DNNNode objNode in objParent.DNNNodes)
            {
                ProcessNodes(objNode);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Sets common properties on DNNTree control
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <history>
        ///     [cnurse]        12/8/2004   Created
        /// </history>
        /// -----------------------------------------------------------------------------
        private void InitializeTree()
        {
            if (String.IsNullOrEmpty(PathImage))
            {
                PathImage = PortalSettings.HomeDirectory;
            }
            if (String.IsNullOrEmpty(PathSystemImage))
            {
                PathSystemImage = ResolveUrl("~/images/");
            }
            if (String.IsNullOrEmpty(IndicateChildImageRoot))
            {
                IndicateChildImageRoot = ResolveUrl(NodeExpandImage);
            }
            if (String.IsNullOrEmpty(IndicateChildImageSub))
            {
                IndicateChildImageSub = ResolveUrl(NodeExpandImage);
            }
            if (String.IsNullOrEmpty(IndicateChildImageExpandedRoot))
            {
                IndicateChildImageExpandedRoot = ResolveUrl(NodeCollapseImage);
            }
            if (String.IsNullOrEmpty(IndicateChildImageExpandedSub))
            {
                IndicateChildImageExpandedSub = ResolveUrl(NodeCollapseImage);
            }
            if (String.IsNullOrEmpty(CSSNode))
            {
                CSSNode = NodeChildCssClass;
            }
            if (String.IsNullOrEmpty(CSSNodeRoot))
            {
                CSSNodeRoot = NodeCssClass;
            }
            if (String.IsNullOrEmpty(CSSNodeHover))
            {
                CSSNodeHover = NodeOverCssClass;
            }
            if (String.IsNullOrEmpty(CSSNodeSelectedRoot))
            {
                CSSNodeSelectedRoot = NodeSelectedCssClass;
            }
            if (String.IsNullOrEmpty(CSSNodeSelectedSub))
            {
                CSSNodeSelectedSub = NodeSelectedCssClass;
            }
            if (String.IsNullOrEmpty(CSSControl))
            {
                CSSControl = TreeCssClass;
            }
        }

		#endregion

		#region "Event Handlers"

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// The Page_Load server event handler on this user control is used
        /// to populate the tree with the Pages.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <remarks>
        /// </remarks>
        /// <history>
        /// 	[cnurse]	12/9/2004	Created
        /// </history>
        /// -----------------------------------------------------------------------------
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            try
            {
                if (Page.IsPostBack == false)
                {
                    BuildTree(null, false);
					
					//Main Table Properties
                    if (!String.IsNullOrEmpty(Width))
                    {
                        tblMain.Width = Width;
                    }
					
                    if (!String.IsNullOrEmpty(CssClass))
                    {
                        tblMain.Attributes.Add("class", CssClass);
                    }
					
					//Header Properties
                    if (!String.IsNullOrEmpty(HeaderCssClass))
                    {
                        cellHeader.Attributes.Add("class", HeaderCssClass);
                    }
					
                    if (!String.IsNullOrEmpty(HeaderTextCssClass))
                    {
                        lblHeader.CssClass = HeaderTextCssClass;
                    }
					
					//Header Text (if set)
                    if (!String.IsNullOrEmpty(HeaderText))
                    {
                        lblHeader.Text = HeaderText;
                    }
					
					//ResourceKey overrides if found
                    if (!String.IsNullOrEmpty(ResourceKey))
                    {
                        string strHeader = Localization.GetString(ResourceKey, Localization.GetResourceFile(this, MyFileName));
                        if (!String.IsNullOrEmpty(strHeader))
                        {
                            lblHeader.Text = Localization.GetString(ResourceKey, Localization.GetResourceFile(this, MyFileName));
                        }
                    }
					
					//If still not set get default key
                    if (String.IsNullOrEmpty(lblHeader.Text))
                    {
                        string strHeader = Localization.GetString("Title", Localization.GetResourceFile(this, MyFileName));
                        if (!String.IsNullOrEmpty(strHeader))
                        {
                            lblHeader.Text = Localization.GetString("Title", Localization.GetResourceFile(this, MyFileName));
                        }
                        else
                        {
                            lblHeader.Text = "Site Navigation";
                        }
                    }
                    tblHeader.Visible = IncludeHeader;
					
					//Main Panel Properties
                    if (!String.IsNullOrEmpty(BodyCssClass))
                    {
                        cellBody.Attributes.Add("class", BodyCssClass);
                    }
                    cellBody.NoWrap = NoWrap;
                }
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// The DNNTree_NodeClick server event handler on this user control runs when a
        /// Node (Page) in the TreeView is clicked
        /// </summary>
        /// <remarks>The event only fires when the Node contains child nodes, as leaf nodes
        /// have their NavigateUrl Property set.
        /// </remarks>
        /// <history>
        /// 	[cnurse]	12/9/2004	Created
        /// </history>
        /// -----------------------------------------------------------------------------
        private void DNNTree_NodeClick(NavigationEventArgs args)
        {
            if (args.Node == null)
            {
                args.Node = Navigation.GetNavigationNode(args.ID, Control.ID);
            }
            Response.Redirect(Globals.ApplicationURL(int.Parse(args.Node.Key)), true);
        }

        private void DNNTree_PopulateOnDemand(NavigationEventArgs args)
        {
            if (args.Node == null)
            {
                args.Node = Navigation.GetNavigationNode(args.ID, Control.ID);
            }
            BuildTree(args.Node, true);
        }

        protected override void OnInit(EventArgs e)
        {
            InitializeTree();
            InitializeNavControl(cellBody, "DNNTreeNavigationProvider");
            Control.NodeClick += DNNTree_NodeClick;
            Control.PopulateOnDemand += DNNTree_PopulateOnDemand;
            base.OnInit(e);
            InitializeComponent();
        }
		
		#endregion

        #region Nested type: eImageType

        private enum eImageType
        {
            FolderClosed = 0,
            FolderOpen = 1,
            Page = 2,
            GotoParent = 3
        }

        #endregion
    }
}