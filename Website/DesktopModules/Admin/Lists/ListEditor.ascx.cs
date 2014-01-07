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
using System.Collections;

using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Icons;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Web.UI.WebControls;

using Telerik.Web.UI;

#endregion

// ReSharper disable CheckNamespace
namespace DotNetNuke.Common.Lists
// ReSharper restore CheckNamespace
{
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// Manages Entry List
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// -----------------------------------------------------------------------------
    public partial class ListEditor : PortalModuleBase
    {

        #region Protected Properties
        
        protected string Mode
        {
            get
            {
                return lstEntries.Mode;
            }
            set
            {
                lstEntries.Mode = value;
            }
        }

        #endregion

        #region Private Methods

        private void BindList(string key)
        {
            lstEntries.SelectedKey = key;
            lstEntries.ListPortalID = PortalSettings.ActiveTab.IsSuperTab ? Null.NullInteger : PortalId;
            lstEntries.ShowDelete = true;
            lstEntries.DataBind();
        }

        private void BindTree()
        {
            var ctlLists = new ListController();
            var colLists = ctlLists.GetListInfoCollection(string.Empty, string.Empty, PortalSettings.ActiveTab.PortalID);
            var indexLookup = new Hashtable();

            listTree.Nodes.Clear();

            foreach (ListInfo list in colLists)
            {
                String filteredNode;
                if (list.DisplayName.Contains(":"))
                {
                    var finalPeriod = list.DisplayName.LastIndexOf(".", StringComparison.InvariantCulture);
                    filteredNode = list.DisplayName.Substring(finalPeriod + 1);

                }
                else
                {
                    filteredNode = list.DisplayName;
                }
				var node = new DnnTreeNode { Text = filteredNode };
                {
                    node.Value = list.Key;
                    node.ToolTip = String.Format(LocalizeString("NoEntries"), list.EntryCount);
					node.ImageUrl = IconController.IconURL("Folder");
                }
                if (list.Level == 0)
                {
					listTree.Nodes.Add(node);
                }
                else
                {
                    if (indexLookup[list.ParentList] != null)
                    {
                        
                        var parentNode = (DnnTreeNode) indexLookup[list.ParentList];
  
                        parentNode.Nodes.Add(node);
                    }
                }
                
                //Add index key here to find it later, should suggest with Joe to add it to DNNTree
                if (indexLookup[list.Key] == null)
                {
                    indexLookup.Add(list.Key, node);
                }
            }
        }

        #endregion

        #region Event Handlers

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Page_Init runs when the control is initialised
        /// </summary>
        /// -----------------------------------------------------------------------------
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            //Set the List Entries Control Properties
            lstEntries.ID = "ListEntries";
            
            //ensure that module context is forwarded from parent module to child module
            lstEntries.ModuleContext.Configuration = ModuleContext.Configuration;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///     Page load, bind tree and enable controls
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <remarks>
        /// </remarks>
        /// -----------------------------------------------------------------------------
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            listTree.NodeClick += DNNTree_NodeClick;
            cmdAddList.Click += cmdAddList_Click;

            try
            {
                if (!Page.IsPostBack)
                {
                    //configure tree
                    if (Request.QueryString["Key"] != null)
                    {
                        Mode = "ListEntries";
                        BindList(Request.QueryString["Key"]);
                    }
                    else
                    {
                        Mode = "NoList";
                    }
                    BindTree();
                }
            }
            catch (Exception exc) //Module failed to load
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Page_PreRender runs just prior to the control being rendered
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <remarks>
        /// </remarks>
        /// -----------------------------------------------------------------------------
        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            var listSelected = Mode != "NoList";
            divNoList.Visible = !listSelected;
            divDetails.Visible = listSelected;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///     Populate list entries based on value selected in DNNTree
        /// </summary>
        /// <param name="source"></param>
        /// <param name="e"></param>
        /// <remarks>
        /// </remarks>
        /// -----------------------------------------------------------------------------
        private void DNNTree_NodeClick(object source, RadTreeNodeEventArgs e)
        {
            Mode = "ListEntries";
            BindList(e.Node.Value);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///     Handles Add New List command
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <remarks>
        ///     Using "CommandName" property of cmdSaveEntry to determine this is a new list
        /// </remarks>
        /// -----------------------------------------------------------------------------
        private void cmdAddList_Click(object sender, EventArgs e)
        {
            Mode = "AddList";
            BindList("");
        }
        
        #endregion

        #region Nested type: eImageType

        private enum eImageType
        {
            Folder = 0,
            Page = 1
        }

        #endregion
    }
}