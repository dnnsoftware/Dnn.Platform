// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.UI.Skins.Controls
{
    using System;

    using DotNetNuke.Common;
    using DotNetNuke.Modules.NavigationProvider;
    using DotNetNuke.Services.Exceptions;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.UI.WebControls;

    /// Project  : DotNetNuke
    /// Class    : TreeViewMenu
    /// <summary>
    /// TreeViewMenu is a Skin Object that creates a Menu using the DNN Treeview Control
    /// to provide a Windows Explore like Menu.
    /// </summary>
    public partial class TreeViewMenu : NavObjectBase
    {
        private const string MyFileName = "TreeViewMenu.ascx";
        private string bodyCssClass = string.Empty;
        private string cssClass = string.Empty;
        private string headerCssClass = string.Empty;
        private string headerText = string.Empty;
        private string headerTextCssClass = "Head";
        private bool includeHeader = true;
        private string nodeChildCssClass = "Normal";
        private string nodeClosedImage = "~/images/folderclosed.gif";
        private string nodeCollapseImage = "~/images/min.gif";
        private string nodeCssClass = "Normal";
        private string nodeExpandImage = "~/images/max.gif";
        private string nodeLeafImage = "~/images/file.gif";
        private string nodeOpenImage = "~/images/folderopen.gif";
        private string nodeOverCssClass = "Normal";
        private string nodeSelectedCssClass = "Normal";
        private string resourceKey = string.Empty;
        private string treeCssClass = string.Empty;
        private string treeGoUpImage = "~/images/folderup.gif";
        private int treeIndentWidth = 10;
        private string width = "100%";

        private enum EImageType
        {
            FolderClosed = 0,
            FolderOpen = 1,
            Page = 2,
            GotoParent = 3,
        }

        public string BodyCssClass
        {
            get
            {
                return this.bodyCssClass;
            }

            set
            {
                this.bodyCssClass = value;
            }
        }

        public string CssClass
        {
            get
            {
                return this.cssClass;
            }

            set
            {
                this.cssClass = value;
            }
        }

        public string HeaderCssClass
        {
            get
            {
                return this.headerCssClass;
            }

            set
            {
                this.headerCssClass = value;
            }
        }

        public string HeaderTextCssClass
        {
            get
            {
                return this.headerTextCssClass;
            }

            set
            {
                this.headerTextCssClass = value;
            }
        }

        public string HeaderText
        {
            get
            {
                return this.headerText;
            }

            set
            {
                this.headerText = value;
            }
        }

        public bool IncludeHeader
        {
            get
            {
                return this.includeHeader;
            }

            set
            {
                this.includeHeader = value;
            }
        }

        public string NodeChildCssClass
        {
            get
            {
                return this.nodeChildCssClass;
            }

            set
            {
                this.nodeChildCssClass = value;
            }
        }

        public string NodeClosedImage
        {
            get
            {
                return this.nodeClosedImage;
            }

            set
            {
                this.nodeClosedImage = value;
            }
        }

        public string NodeCollapseImage
        {
            get
            {
                return this.nodeCollapseImage;
            }

            set
            {
                this.nodeCollapseImage = value;
            }
        }

        public string NodeCssClass
        {
            get
            {
                return this.nodeCssClass;
            }

            set
            {
                this.nodeCssClass = value;
            }
        }

        public string NodeExpandImage
        {
            get
            {
                return this.nodeExpandImage;
            }

            set
            {
                this.nodeExpandImage = value;
            }
        }

        public string NodeLeafImage
        {
            get
            {
                return this.nodeLeafImage;
            }

            set
            {
                this.nodeLeafImage = value;
            }
        }

        public string NodeOpenImage
        {
            get
            {
                return this.nodeOpenImage;
            }

            set
            {
                this.nodeOpenImage = value;
            }
        }

        public string NodeOverCssClass
        {
            get
            {
                return this.nodeOverCssClass;
            }

            set
            {
                this.nodeOverCssClass = value;
            }
        }

        public string NodeSelectedCssClass
        {
            get
            {
                return this.nodeSelectedCssClass;
            }

            set
            {
                this.nodeSelectedCssClass = value;
            }
        }

        public bool NoWrap { get; set; }

        public string ResourceKey
        {
            get
            {
                return this.resourceKey;
            }

            set
            {
                this.resourceKey = value;
            }
        }

        public string TreeCssClass
        {
            get
            {
                return this.treeCssClass;
            }

            set
            {
                this.treeCssClass = value;
            }
        }

        public string TreeGoUpImage
        {
            get
            {
                return this.treeGoUpImage;
            }

            set
            {
                this.treeGoUpImage = value;
            }
        }

        public int TreeIndentWidth
        {
            get
            {
                return this.treeIndentWidth;
            }

            set
            {
                this.treeIndentWidth = value;
            }
        }

        public string Width
        {
            get
            {
                return this.width;
            }

            set
            {
                this.width = value;
            }
        }

        /// <summary>
        /// The Page_Load server event handler on this user control is used
        /// to populate the tree with the Pages.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            try
            {
                if (this.Page.IsPostBack == false)
                {
                    this.BuildTree(null, false);

                    // Main Table Properties
                    if (!string.IsNullOrEmpty(this.Width))
                    {
                        this.tblMain.Width = this.Width;
                    }

                    if (!string.IsNullOrEmpty(this.CssClass))
                    {
                        this.tblMain.Attributes.Add("class", this.CssClass);
                    }

                    // Header Properties
                    if (!string.IsNullOrEmpty(this.HeaderCssClass))
                    {
                        this.cellHeader.Attributes.Add("class", this.HeaderCssClass);
                    }

                    if (!string.IsNullOrEmpty(this.HeaderTextCssClass))
                    {
                        this.lblHeader.CssClass = this.HeaderTextCssClass;
                    }

                    // Header Text (if set)
                    if (!string.IsNullOrEmpty(this.HeaderText))
                    {
                        this.lblHeader.Text = this.HeaderText;
                    }

                    // ResourceKey overrides if found
                    if (!string.IsNullOrEmpty(this.ResourceKey))
                    {
                        string strHeader = Localization.GetString(this.ResourceKey, Localization.GetResourceFile(this, MyFileName));
                        if (!string.IsNullOrEmpty(strHeader))
                        {
                            this.lblHeader.Text = Localization.GetString(this.ResourceKey, Localization.GetResourceFile(this, MyFileName));
                        }
                    }

                    // If still not set get default key
                    if (string.IsNullOrEmpty(this.lblHeader.Text))
                    {
                        string strHeader = Localization.GetString("Title", Localization.GetResourceFile(this, MyFileName));
                        if (!string.IsNullOrEmpty(strHeader))
                        {
                            this.lblHeader.Text = Localization.GetString("Title", Localization.GetResourceFile(this, MyFileName));
                        }
                        else
                        {
                            this.lblHeader.Text = "Site Navigation";
                        }
                    }

                    this.tblHeader.Visible = this.IncludeHeader;

                    // Main Panel Properties
                    if (!string.IsNullOrEmpty(this.BodyCssClass))
                    {
                        this.cellBody.Attributes.Add("class", this.BodyCssClass);
                    }

                    this.cellBody.NoWrap = this.NoWrap;
                }
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        /// <inheritdoc/>
        protected override void OnInit(EventArgs e)
        {
            this.InitializeTree();
            this.InitializeNavControl(this.cellBody, "DNNTreeNavigationProvider");
            this.Control.NodeClick += this.DNNTree_NodeClick;
            this.Control.PopulateOnDemand += this.DNNTree_PopulateOnDemand;
            base.OnInit(e);
            this.InitializeComponent();
        }

        private void InitializeComponent()
        {
        }

        /// <summary>The BuildTree helper method is used to build the tree.</summary>
        private void BuildTree(DNNNode objNode, bool blnPODRequest)
        {
            bool blnAddUpNode = false;
            DNNNodeCollection objNodes;
            objNodes = this.GetNavigationNodes(objNode);

            if (blnPODRequest == false)
            {
                if (!string.IsNullOrEmpty(this.Level))
                {
                    switch (this.Level.ToLowerInvariant())
                    {
                        case "root":
                            break;
                        case "child":
                            blnAddUpNode = true;
                            break;
                        default:
                            if (this.Level.ToLowerInvariant() != "root" && this.PortalSettings.ActiveTab.BreadCrumbs.Count > 1)
                            {
                                blnAddUpNode = true;
                            }

                            break;
                    }
                }
            }

            // add goto Parent node
            if (blnAddUpNode)
            {
                var objParentNode = new DNNNode();
                objParentNode.ID = this.PortalSettings.ActiveTab.ParentId.ToString();
                objParentNode.Key = objParentNode.ID;
                objParentNode.Text = Localization.GetString("Parent", Localization.GetResourceFile(this, MyFileName));
                objParentNode.ToolTip = Localization.GetString("GoUp", Localization.GetResourceFile(this, MyFileName));
                objParentNode.CSSClass = this.NodeCssClass;
                objParentNode.Image = this.ResolveUrl(this.TreeGoUpImage);
                objParentNode.ClickAction = eClickAction.PostBack;
                objNodes.InsertBefore(0, objParentNode);
            }

            foreach (DNNNode objPNode in objNodes)
            {
                // clean up to do in processnodes???
                this.ProcessNodes(objPNode);
            }

            this.Bind(objNodes);

            // technically this should always be a dnntree.  If using dynamic controls Nav.ascx should be used.  just being safe.
            if (this.Control.NavigationControl is DnnTree)
            {
                var objTree = (DnnTree)this.Control.NavigationControl;
                if (objTree.SelectedTreeNodes.Count > 0)
                {
                    var objTNode = (TreeNode)objTree.SelectedTreeNodes[1];
                    if (objTNode.DNNNodes.Count > 0)
                    {
                        // only expand it if nodes are not pending
                        objTNode.Expand();
                    }
                }
            }
        }

        private void ProcessNodes(DNNNode objParent)
        {
            if (!string.IsNullOrEmpty(objParent.Image))
            {
            }
            else if (objParent.HasNodes)
            {
                // imagepath applied in provider...
                objParent.Image = this.ResolveUrl(this.NodeClosedImage);
            }
            else
            {
                objParent.Image = this.ResolveUrl(this.NodeLeafImage);
            }

            foreach (DNNNode objNode in objParent.DNNNodes)
            {
                this.ProcessNodes(objNode);
            }
        }

        /// <summary>Sets common properties on DNNTree control.</summary>
        private void InitializeTree()
        {
            if (string.IsNullOrEmpty(this.PathImage))
            {
                this.PathImage = this.PortalSettings.HomeDirectory;
            }

            if (string.IsNullOrEmpty(this.PathSystemImage))
            {
                this.PathSystemImage = this.ResolveUrl("~/images/");
            }

            if (string.IsNullOrEmpty(this.IndicateChildImageRoot))
            {
                this.IndicateChildImageRoot = this.ResolveUrl(this.NodeExpandImage);
            }

            if (string.IsNullOrEmpty(this.IndicateChildImageSub))
            {
                this.IndicateChildImageSub = this.ResolveUrl(this.NodeExpandImage);
            }

            if (string.IsNullOrEmpty(this.IndicateChildImageExpandedRoot))
            {
                this.IndicateChildImageExpandedRoot = this.ResolveUrl(this.NodeCollapseImage);
            }

            if (string.IsNullOrEmpty(this.IndicateChildImageExpandedSub))
            {
                this.IndicateChildImageExpandedSub = this.ResolveUrl(this.NodeCollapseImage);
            }

            if (string.IsNullOrEmpty(this.CSSNode))
            {
                this.CSSNode = this.NodeChildCssClass;
            }

            if (string.IsNullOrEmpty(this.CSSNodeRoot))
            {
                this.CSSNodeRoot = this.NodeCssClass;
            }

            if (string.IsNullOrEmpty(this.CSSNodeHover))
            {
                this.CSSNodeHover = this.NodeOverCssClass;
            }

            if (string.IsNullOrEmpty(this.CSSNodeSelectedRoot))
            {
                this.CSSNodeSelectedRoot = this.NodeSelectedCssClass;
            }

            if (string.IsNullOrEmpty(this.CSSNodeSelectedSub))
            {
                this.CSSNodeSelectedSub = this.NodeSelectedCssClass;
            }

            if (string.IsNullOrEmpty(this.CSSControl))
            {
                this.CSSControl = this.TreeCssClass;
            }
        }

        /// <summary>
        /// The DNNTree_NodeClick server event handler on this user control runs when a
        /// Node (Page) in the TreeView is clicked.
        /// </summary>
        /// <remarks>The event only fires when the Node contains child nodes, as leaf nodes
        /// have their NavigateUrl Property set.
        /// </remarks>
        private void DNNTree_NodeClick(NavigationEventArgs args)
        {
            if (args.Node == null)
            {
                args.Node = Navigation.GetNavigationNode(args.ID, this.Control.ID);
            }

            this.Response.Redirect(Globals.ApplicationURL(int.Parse(args.Node.Key)), true);
        }

        private void DNNTree_PopulateOnDemand(NavigationEventArgs args)
        {
            if (args.Node == null)
            {
                args.Node = Navigation.GetNavigationNode(args.ID, this.Control.ID);
            }

            this.BuildTree(args.Node, true);
        }
    }
}
