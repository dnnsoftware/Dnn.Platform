using System;
using System.Collections.Generic;
using System.Web.UI.WebControls;

namespace DNNConnect.CKEditorProvider.Helper
{
    internal class TreeViewHelper<K>
    {
        private readonly Dictionary<K, TreeNodeWithParentId<K>> _nodes = new Dictionary<K, TreeNodeWithParentId<K>>();

        /// <summary>
        /// To iterate passed in data collection(treeViewData), with the passed in TreeNodesCollection(treeViewNodes) using Dictionary collection 
        /// to get performance for large number of data for treeview control
        /// </summary>
        /// <typeparam name="T">Any object type to use for data to be used in TreeView</typeparam>
        /// <param name="treeViewData">Data collection based on T</param>
        /// <param name="treeViewNodes">Nodes need to refresh using Dictionary data structure</param>
        /// <param name="getNodeId">Method to return Text used for NodeId from T based Object</param>
        /// <param name="getParentId">Method to return Text used for ParentId from T based Object</param>
        /// <param name="getNodeText">Method to return Text used for TreeNode from T based Object</param>
        /// <param name="getNodeImageURL">Method to return Image used for TreeNode from T based Object</param>
        /// <param name="parentIdCheck">Validate the parentId for T based object if parent should not be -1</param>       
        /// <param name="allLeaves"> if passed would be added as leaves to each node</param>     
        internal void LoadNodes<T>(IEnumerable<T> treeViewData, TreeNodeCollection treeViewNodes, Func<T, K> getNodeId, Func<T, K> getParentId, Func<T, string> getNodeText, Func<T, string> getNodeValue, Func<T, string> getNodeImageURL, Func<K, bool> parentIdCheck, Dictionary<K, HashSet<TreeNode>> allLeaves = null)
        {
            FillNodes(treeViewData, getNodeId, getParentId, getNodeText, getNodeValue, getNodeImageURL);
            RefreshTreeViewNodes(treeViewNodes, parentIdCheck, allLeaves);
        }

        private void RefreshTreeViewNodes(TreeNodeCollection treeViewNodes, Func<K, bool> parentIdCheck, Dictionary<K, HashSet<TreeNode>> allLeaves = null)
        {
            treeViewNodes.Clear();
            foreach (var id in _nodes.Keys)
            {
                var data = GetTreeNodeWithParentId(id);
                var node = data.NodeData;
                var parentId = data.ParentId;
                if (parentIdCheck(parentId))
                {
                    var parentNode = GetTreeNodeWithParentId(parentId).NodeData;
                    parentNode.ChildNodes.Add(node);
                }
                else
                {
                    treeViewNodes.Add(node);
                }

                if (allLeaves != null && allLeaves.ContainsKey(id))
                {
                    foreach (var item in allLeaves[id])
                    {
                        node.ChildNodes.Add(item);
                    }
                }
            }
        }
       
        private void FillNodes<T>(
            IEnumerable<T> treeViewData, 
            Func<T, K> getNodeId, 
            Func<T, K> getParentId, 
            Func<T, string> getNodeText, 
            Func<T, string> getNodeValue, 
            Func<T, string> getNodeImageURL
            )
        {
            _nodes.Clear();
            foreach (var item in treeViewData)
            {
                var idTab = getNodeId(item);
                var displayName = getNodeText(item);
                var imageUrl = getNodeImageURL(item);
                var nodeValue = getNodeValue(item);

                var nodeData = new TreeNodeWithParentId<K>
                {
                    ParentId = getParentId(item),
                    NodeData = new TreeNode
                    {
                        Text = displayName,
                        Value = nodeValue,
                        ImageUrl = imageUrl
                    }
                };
                _nodes.Add(idTab, nodeData);
            }
        }

        private TreeNodeWithParentId<K> GetTreeNodeWithParentId(K key)
        {
            return _nodes[key];
        }        
    }

    internal class TreeNodeWithParentId<K>
    {
        public K ParentId { get; set; }
        public TreeNode NodeData { get; set; }
    }
}