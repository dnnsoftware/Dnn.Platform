// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.UI
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Web.UI;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Modules.Actions;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.Security;
    using DotNetNuke.Security.Permissions;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.UI.Containers;
    using DotNetNuke.UI.Modules;
    using DotNetNuke.UI.WebControls;

    public class Navigation
    {
        public enum NavNodeOptions
        {
            IncludeSelf = 1,
            IncludeParent = 2,
            IncludeSiblings = 4,
            MarkPendingNodes = 8,
            IncludeHiddenNodes = 16,
        }

        public enum ToolTipSource
        {
            TabName,
            Title,
            Description,
            None,
        }

        public static bool CanShowTab(TabInfo objTab, bool isAdminMode, bool showDisabled)
        {
            return CanShowTab(objTab, isAdminMode, showDisabled, false);
        }

        public static bool CanShowTab(TabInfo tab, bool isAdminMode, bool showDisabled, bool showHidden)
        {
            // if tab is visible, not deleted, not expired (or admin), and user has permission to see it...
            return (tab.IsVisible || showHidden) && tab.HasAVisibleVersion && !tab.IsDeleted &&
                    (!tab.DisableLink || showDisabled) &&
                    (((tab.StartDate < DateTime.Now || tab.StartDate == Null.NullDate) &&
                      (tab.EndDate > DateTime.Now || tab.EndDate == Null.NullDate)) || isAdminMode) &&
                    TabPermissionController.CanNavigateToPage(tab);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Allows for DNNNode object to be easily obtained based off of passed in ID.
        /// </summary>
        /// <param name="strID">NodeID to retrieve.</param>
        /// <param name="strNamespace">Namespace for node collection (usually control's ClientID).</param>
        /// <param name="objActionRoot">Root Action object used in searching.</param>
        /// <param name="objControl">ActionControl to base actions off of.</param>
        /// <returns>DNNNode.</returns>
        /// <remarks>
        /// Primary purpose of this is to obtain the DNNNode needed for the events exposed by the NavigationProvider.
        /// </remarks>
        /// -----------------------------------------------------------------------------
        public static DNNNode GetActionNode(string strID, string strNamespace, ModuleAction objActionRoot, Control objControl)
        {
            DNNNodeCollection objNodes = GetActionNodes(objActionRoot, objControl, -1);
            DNNNode objNode = objNodes.FindNode(strID);
            var objReturnNodes = new DNNNodeCollection(strNamespace);
            objReturnNodes.Import(objNode);
            objReturnNodes[0].ID = strID;
            return objReturnNodes[0];
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// This function provides a central location to obtain a generic node collection of the actions associated
        /// to a module based off of the current user's context.
        /// </summary>
        /// <param name="objActionRoot">Root module action.</param>
        /// <param name="objControl">ActionControl to base actions off of.</param>
        /// <returns></returns>
        /// <remarks>
        /// </remarks>
        /// -----------------------------------------------------------------------------
        public static DNNNodeCollection GetActionNodes(ModuleAction objActionRoot, Control objControl)
        {
            return GetActionNodes(objActionRoot, objControl, -1);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// This function provides a central location to obtain a generic node collection of the actions associated
        /// to a module based off of the current user's context.
        /// </summary>
        /// <param name="objActionRoot">Root module action.</param>
        /// <param name="objControl">ActionControl to base actions off of.</param>
        /// <param name="intDepth">How many levels deep should be populated.</param>
        /// <returns></returns>
        /// <remarks>
        /// </remarks>
        /// -----------------------------------------------------------------------------
        public static DNNNodeCollection GetActionNodes(ModuleAction objActionRoot, Control objControl, int intDepth)
        {
            var objCol = new DNNNodeCollection(objControl.ClientID);

            var objActionControl = objControl as IActionControl;
            if (objActionControl != null)
            {
                if (objActionRoot.Visible)
                {
                    objCol.Add();
                    DNNNode objRoot = objCol[0];
                    objRoot.ID = objActionRoot.ID.ToString();
                    objRoot.Key = objActionRoot.ID.ToString();
                    objRoot.Text = objActionRoot.Title;
                    objRoot.NavigateURL = objActionRoot.Url;
                    objRoot.Image = objActionRoot.Icon;
                    objRoot.Enabled = false;
                    AddChildActions(objActionRoot, objRoot, objRoot.ParentNode, objActionControl, intDepth);
                }
            }

            return objCol;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// This function provides a central location to obtain a generic node collection of the actions associated
        /// to a module based off of the current user's context.
        /// </summary>
        /// <param name="objActionRoot">Root module action.</param>
        /// <param name="objRootNode">Root node on which to populate children.</param>
        /// <param name="objControl">ActionControl to base actions off of.</param>
        /// <param name="intDepth">How many levels deep should be populated.</param>
        /// <returns></returns>
        /// <remarks>
        /// </remarks>
        /// -----------------------------------------------------------------------------
        public static DNNNodeCollection GetActionNodes(ModuleAction objActionRoot, DNNNode objRootNode, Control objControl, int intDepth)
        {
            DNNNodeCollection objCol = objRootNode.ParentNode.DNNNodes;
            var objActionControl = objControl as IActionControl;
            if (objActionControl != null)
            {
                AddChildActions(objActionRoot, objRootNode, objRootNode, objActionControl, intDepth);
            }

            return objCol;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Allows for DNNNode object to be easily obtained based off of passed in ID.
        /// </summary>
        /// <param name="strID">NodeID to retrieve.</param>
        /// <param name="strNamespace">Namespace for node collection (usually control's ClientID).</param>
        /// <returns>DNNNode.</returns>
        /// <remarks>
        /// Primary purpose of this is to obtain the DNNNode needed for the events exposed by the NavigationProvider.
        /// </remarks>
        /// -----------------------------------------------------------------------------
        public static DNNNode GetNavigationNode(string strID, string strNamespace)
        {
            DNNNodeCollection objNodes = GetNavigationNodes(strNamespace);
            DNNNode objNode = objNodes.FindNode(strID);
            var objReturnNodes = new DNNNodeCollection(strNamespace);
            objReturnNodes.Import(objNode);
            objReturnNodes[0].ID = strID;
            return objReturnNodes[0];
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// This function provides a central location to obtain a generic node collection of the pages/tabs included in
        /// the current context's (user) navigation hierarchy.
        /// </summary>
        /// <param name="strNamespace">Namespace (typically control's ClientID) of node collection to create.</param>
        /// <returns>Collection of DNNNodes.</returns>
        /// <remarks>
        /// Returns all navigation nodes for a given user.
        /// </remarks>
        /// -----------------------------------------------------------------------------
        public static DNNNodeCollection GetNavigationNodes(string strNamespace)
        {
            return GetNavigationNodes(strNamespace, ToolTipSource.None, -1, -1, 0);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// This function provides a central location to obtain a generic node collection of the pages/tabs included in
        /// the current context's (user) navigation hierarchy.
        /// </summary>
        /// <param name="strNamespace">Namespace (typically control's ClientID) of node collection to create.</param>
        /// <param name="eToolTips">Enumerator to determine what text to display in the tooltips.</param>
        /// <param name="intStartTabId">If using Populate On Demand, then this is the tab id of the root element to retrieve (-1 for no POD).</param>
        /// <param name="intDepth">If Populate On Demand is enabled, then this parameter determines the number of nodes to retrieve beneath the starting tab passed in (intStartTabId) (-1 for no POD).</param>
        /// <param name="intNavNodeOptions">Bitwise integer containing values to determine what nodes to display (self, siblings, parent).</param>
        /// <returns>Collection of DNNNodes.</returns>
        /// <remarks>
        /// Returns a subset of navigation nodes based off of passed in starting node id and depth.
        /// </remarks>
        /// -----------------------------------------------------------------------------
        public static DNNNodeCollection GetNavigationNodes(string strNamespace, ToolTipSource eToolTips, int intStartTabId, int intDepth, int intNavNodeOptions)
        {
            var objCol = new DNNNodeCollection(strNamespace);
            return GetNavigationNodes(new DNNNode(objCol.XMLNode), eToolTips, intStartTabId, intDepth, intNavNodeOptions);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// This function provides a central location to obtain a generic node collection of the pages/tabs included in
        /// the current context's (user) navigation hierarchy.
        /// </summary>
        /// <param name="objRootNode">Node in which to add children to.</param>
        /// <param name="eToolTips">Enumerator to determine what text to display in the tooltips.</param>
        /// <param name="intStartTabId">If using Populate On Demand, then this is the tab id of the root element to retrieve (-1 for no POD).</param>
        /// <param name="intDepth">If Populate On Demand is enabled, then this parameter determines the number of nodes to retrieve beneath the starting tab passed in (intStartTabId) (-1 for no POD).</param>
        /// <param name="intNavNodeOptions">Bitwise integer containing values to determine what nodes to display (self, siblings, parent).</param>
        /// <returns>Collection of DNNNodes.</returns>
        /// <remarks>
        /// Returns a subset of navigation nodes based off of passed in starting node id and depth.
        /// </remarks>
        /// -----------------------------------------------------------------------------
        public static DNNNodeCollection GetNavigationNodes(DNNNode objRootNode, ToolTipSource eToolTips, int intStartTabId, int intDepth, int intNavNodeOptions)
        {
            var objPortalSettings = PortalController.Instance.GetCurrentPortalSettings();
            var objBreadCrumbs = new Hashtable();
            var objTabLookup = new Hashtable();
            var objRootNodes = objRootNode.DNNNodes;
            var intLastBreadCrumbId = 0;

            //--- cache breadcrumbs in hashtable so we can easily set flag on node denoting it as a breadcrumb node (without looping multiple times) ---
            foreach (TabInfo tabInfo in objPortalSettings.ActiveTab.BreadCrumbs)
            {
                objBreadCrumbs.Add(tabInfo.TabID, 1);
                intLastBreadCrumbId = tabInfo.TabID;
            }

            var portalTabs = TabController.GetTabsBySortOrder(objPortalSettings.PortalId, objPortalSettings.CultureCode, true);
            var hostTabs = TabController.GetTabsBySortOrder(Null.NullInteger, Localization.SystemLocale, true);

            var cachedPortalTabs = new List<TabInfo>(portalTabs);
            foreach (var objTab in cachedPortalTabs)
            {
                objTabLookup.Add(objTab.TabID, objTab);
            }

            var cachedHostTabs = new List<TabInfo>(hostTabs);
            foreach (var objTab in cachedHostTabs)
            {
                objTabLookup.Add(objTab.TabID, objTab);
            }

            // convert dnn nodes to dictionary.
            var nodesDict = new Dictionary<string, DNNNode>();
            SaveDnnNodesToDictionary(nodesDict, objRootNodes);

            foreach (var objTab in cachedPortalTabs)
            {
                ProcessTab(objRootNode, objTab, objTabLookup, objBreadCrumbs, intLastBreadCrumbId, eToolTips, intStartTabId, intDepth, intNavNodeOptions, nodesDict);
            }

            foreach (var objTab in cachedHostTabs)
            {
                ProcessTab(objRootNode, objTab, objTabLookup, objBreadCrumbs, intLastBreadCrumbId, eToolTips, intStartTabId, intDepth, intNavNodeOptions, nodesDict);
            }

            return objRootNodes;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Recursive function to add module's actions to the DNNNodeCollection based off of passed in ModuleActions.
        /// </summary>
        /// <param name="parentAction">Parent action.</param>
        /// <param name="parentNode">Parent node.</param>
        /// <param name="rootNode">Root Node.</param>
        /// <param name="actionControl">ActionControl to base actions off of.</param>
        /// <param name="intDepth">How many levels deep should be populated.</param>
        /// <remarks>
        /// </remarks>
        /// -----------------------------------------------------------------------------
        private static void AddChildActions(ModuleAction parentAction, DNNNode parentNode, DNNNode rootNode, IActionControl actionControl, int intDepth)
        {
            // Add Menu Items
            foreach (ModuleAction action in parentAction.Actions)
            {
                bool isActionPending = IsActionPending(parentNode, rootNode, intDepth);
                if (action.Title == "~")
                {
                    if (isActionPending == false)
                    {
                        // A title (text) of ~ denotes a break
                        parentNode.DNNNodes.AddBreak();
                    }
                }
                else
                {
                    // if action is visible and user has permission
                    if (action.Visible &&
                        (action.Secure != SecurityAccessLevel.Anonymous ||
                            ((!ModuleHost.IsViewMode(actionControl.ModuleControl.ModuleContext.Configuration, PortalSettings.Current))
                                && ModulePermissionController.HasModuleAccess(action.Secure, Null.NullString, actionControl.ModuleControl.ModuleContext.Configuration))))
                    {
                        if (isActionPending)
                        {
                            parentNode.HasNodes = true;
                        }
                        else
                        {
                            int i = parentNode.DNNNodes.Add();
                            DNNNode node = parentNode.DNNNodes[i];
                            node.ID = action.ID.ToString();
                            node.Key = action.ID.ToString();
                            node.Text = action.Title; // no longer including SPACE in generic node collection, each control must handle how they want to display
                            if (string.IsNullOrEmpty(action.ClientScript) && string.IsNullOrEmpty(action.Url) && string.IsNullOrEmpty(action.CommandArgument))
                            {
                                node.Enabled = false;
                            }
                            else if (!string.IsNullOrEmpty(action.ClientScript))
                            {
                                node.JSFunction = action.ClientScript;
                                node.ClickAction = eClickAction.None;
                            }
                            else
                            {
                                node.NavigateURL = action.Url;
                                if (action.UseActionEvent == false && !string.IsNullOrEmpty(node.NavigateURL))
                                {
                                    node.ClickAction = eClickAction.Navigate;
                                    if (action.NewWindow)
                                    {
                                        node.Target = "_blank";
                                    }
                                }
                                else
                                {
                                    node.ClickAction = eClickAction.PostBack;
                                }
                            }

                            node.Image = action.Icon;
                            if (action.HasChildren()) // if action has children then call function recursively
                            {
                                AddChildActions(action, node, rootNode, actionControl, intDepth);
                            }
                        }
                    }
                }
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Assigns common properties from passed in tab to newly created DNNNode that is added to the passed in DNNNodeCollection.
        /// </summary>
        /// <param name="objTab">Tab to base DNNNode off of.</param>
        /// <param name="objNodes">Node collection to append new node to.</param>
        /// <param name="objBreadCrumbs">Hashtable of breadcrumb IDs to efficiently determine node's BreadCrumb property.</param>
        /// <param name="objPortalSettings">Portal settings object to determine if node is selected.</param>
        /// <param name="eToolTips"></param>
        /// <remarks>
        /// Logic moved to separate sub to make GetNavigationNodes cleaner.
        /// </remarks>
        /// -----------------------------------------------------------------------------
        private static void AddNode(TabInfo objTab, DNNNodeCollection objNodes, Hashtable objBreadCrumbs, PortalSettings objPortalSettings, ToolTipSource eToolTips, IDictionary<string, DNNNode> nodesLookup)
        {
            var objNode = new DNNNode();
            if (objTab.Title == "~") // NEW!
            {
                // A title (text) of ~ denotes a break
                objNodes.AddBreak();
            }
            else
            {
                // assign breadcrumb and selected properties
                if (objBreadCrumbs.Contains(objTab.TabID))
                {
                    objNode.BreadCrumb = true;
                    if (objTab.TabID == objPortalSettings.ActiveTab.TabID)
                    {
                        objNode.Selected = true;
                    }
                }

                if (objTab.DisableLink)
                {
                    objNode.Enabled = false;
                }

                objNode.ID = objTab.TabID.ToString();
                objNode.Key = objNode.ID;
                objNode.Text = objTab.LocalizedTabName;
                objNode.NavigateURL = objTab.FullUrl;
                objNode.ClickAction = eClickAction.Navigate;
                objNode.Image = objTab.IconFile;
                objNode.LargeImage = objTab.IconFileLarge;
                switch (eToolTips)
                {
                    case ToolTipSource.TabName:
                        objNode.ToolTip = objTab.LocalizedTabName;
                        break;
                    case ToolTipSource.Title:
                        objNode.ToolTip = objTab.Title;
                        break;
                    case ToolTipSource.Description:
                        objNode.ToolTip = objTab.Description;
                        break;
                }

                bool newWindow = false;
                if (objTab.TabSettings["LinkNewWindow"] != null && bool.TryParse((string)objTab.TabSettings["LinkNewWindow"], out newWindow) && newWindow)
                {
                    objNode.Target = "_blank";
                }

                objNodes.Add(objNode);
                if (!nodesLookup.ContainsKey(objNode.ID))
                {
                    nodesLookup.Add(objNode.ID, objNode);
                }
            }
        }

        private static bool IsActionPending(DNNNode objParentNode, DNNNode objRootNode, int intDepth)
        {
            // if we aren't restricting depth then its never pending
            if (intDepth == -1)
            {
                return false;
            }

            // parents level + 1 = current node level
            // if current node level - (roots node level) <= the desired depth then not pending
            if (objParentNode.Level + 1 - objRootNode.Level <= intDepth)
            {
                return false;
            }

            return true;
        }

        private static bool IsTabPending(TabInfo objTab, DNNNode objParentNode, DNNNode objRootNode, int intDepth, Hashtable objBreadCrumbs, int intLastBreadCrumbId, bool blnPOD)
        {
            // A
            // |
            //--B
            // | |
            // | --B-1
            // | | |
            // | | --B-1-1
            // | | |
            // | | --B-1-2
            // | |
            // | --B-2
            // |   |
            // |   --B-2-1
            // |   |
            // |   --B-2-2
            // |
            //--C
            //  |
            //  --C-1
            //  | |
            //  | --C-1-1
            //  | |
            //  | --C-1-2
            //  |
            //  --C-2
            //    |
            //    --C-2-1
            //    |
            //    --C-2-2

            // if we aren't restricting depth then its never pending
            if (intDepth == -1)
            {
                return false;
            }

            // parents level + 1 = current node level
            // if current node level - (roots node level) <= the desired depth then not pending
            if (objParentNode.Level + 1 - objRootNode.Level <= intDepth)
            {
                return false;
            }

            //--- These checks below are here so tree becomes expands to selected node ---
            if (blnPOD)
            {
                // really only applies to controls with POD enabled, since the root passed in may be some node buried down in the chain
                // and the depth something like 1.  We need to include the appropriate parent's and parent siblings
                // Why is the check for POD required?  Well to allow for functionality like RootOnly requests.  We do not want any children
                // regardless if they are a breadcrumb

                // if tab is in the breadcrumbs then obviously not pending
                if (objBreadCrumbs.Contains(objTab.TabID))
                {
                    return false;
                }

                // if parent is in the breadcrumb and it is not the last breadcrumb then not pending
                // in tree above say we our breadcrumb is (A, B, B-2) we want our tree containing A, B, B-2 AND B-1 AND C since A and B are expanded
                // we do NOT want B-2-1 and B-2-2, thus the check for Last Bread Crumb
                if (objBreadCrumbs.Contains(objTab.ParentId) && intLastBreadCrumbId != objTab.ParentId)
                {
                    return false;
                }
            }

            return true;
        }

        private static bool IsTabSibling(TabInfo objTab, int intStartTabId, Hashtable objTabLookup)
        {
            if (intStartTabId == -1)
            {
                return objTab.ParentId == -1;
            }

            return objTab.ParentId == ((TabInfo)objTabLookup[intStartTabId]).ParentId;
        }

        private static void ProcessTab(DNNNode objRootNode, TabInfo objTab, Hashtable objTabLookup, Hashtable objBreadCrumbs, int intLastBreadCrumbId, ToolTipSource eToolTips, int intStartTabId,
                                       int intDepth, int intNavNodeOptions, IDictionary<string, DNNNode> nodesLookup)
        {
            PortalSettings objPortalSettings = PortalController.Instance.GetCurrentPortalSettings();
            bool showHidden = (intNavNodeOptions & (int)NavNodeOptions.IncludeHiddenNodes) == (int)NavNodeOptions.IncludeHiddenNodes;

            if (CanShowTab(objTab, TabPermissionController.CanAdminPage(), true, showHidden)) // based off of tab properties, is it shown
            {
                DNNNodeCollection objParentNodes;
                DNNNode objParentNode;
                bool blnParentFound = nodesLookup.TryGetValue(objTab.ParentId.ToString(), out objParentNode);
                if (!blnParentFound)
                {
                    objParentNode = objRootNode;
                }

                objParentNodes = objParentNode.DNNNodes;
                if (objTab.TabID == intStartTabId)
                {
                    // is this the starting tab
                    if ((intNavNodeOptions & (int)NavNodeOptions.IncludeParent) != 0)
                    {
                        // if we are including parent, make sure there is one, then add
                        if (objTabLookup[objTab.ParentId] != null)
                        {
                            AddNode((TabInfo)objTabLookup[objTab.ParentId], objParentNodes, objBreadCrumbs, objPortalSettings, eToolTips, nodesLookup);
                            if (nodesLookup.TryGetValue(objTab.ParentId.ToString(), out objParentNode))
                            {
                                objParentNodes = objParentNode.DNNNodes;
                            }
                        }
                    }

                    if ((intNavNodeOptions & (int)NavNodeOptions.IncludeSelf) != 0)
                    {
                        // if we are including our self (starting tab) then add
                        AddNode(objTab, objParentNodes, objBreadCrumbs, objPortalSettings, eToolTips, nodesLookup);
                    }
                }
                else if (((intNavNodeOptions & (int)NavNodeOptions.IncludeSiblings) != 0) && IsTabSibling(objTab, intStartTabId, objTabLookup))
                {
                    // is this a sibling of the starting node, and we are including siblings, then add it
                    AddNode(objTab, objParentNodes, objBreadCrumbs, objPortalSettings, eToolTips, nodesLookup);
                }
                else
                {
                    if (blnParentFound) // if tabs parent already in hierarchy (as is the case when we are sending down more than 1 level)
                    {
                        // parent will be found for siblings.  Check to see if we want them, if we don't make sure tab is not a sibling
                        if (((intNavNodeOptions & (int)NavNodeOptions.IncludeSiblings) != 0) || IsTabSibling(objTab, intStartTabId, objTabLookup) == false)
                        {
                            // determine if tab should be included or marked as pending
                            bool blnPOD = (intNavNodeOptions & (int)NavNodeOptions.MarkPendingNodes) != 0;
                            if (IsTabPending(objTab, objParentNode, objRootNode, intDepth, objBreadCrumbs, intLastBreadCrumbId, blnPOD))
                            {
                                if (blnPOD)
                                {
                                    objParentNode.HasNodes = true; // mark it as a pending node
                                }
                            }
                            else
                            {
                                AddNode(objTab, objParentNodes, objBreadCrumbs, objPortalSettings, eToolTips, nodesLookup);
                            }
                        }
                    }
                    else if ((intNavNodeOptions & (int)NavNodeOptions.IncludeSelf) == 0 && objTab.ParentId == intStartTabId)
                    {
                        // if not including self and parent is the start id then add
                        AddNode(objTab, objParentNodes, objBreadCrumbs, objPortalSettings, eToolTips, nodesLookup);
                    }
                }
            }
        }

        private static void SaveDnnNodesToDictionary(IDictionary<string, DNNNode> nodesDict, DNNNodeCollection nodes)
        {
            foreach (DNNNode node in nodes)
            {
                if (!nodesDict.ContainsKey(node.ID))
                {
                    nodesDict.Add(node.ID, node);
                }

                if (node.HasNodes)
                {
                    SaveDnnNodesToDictionary(nodesDict, node.DNNNodes);
                }
            }
        }
    }
}
