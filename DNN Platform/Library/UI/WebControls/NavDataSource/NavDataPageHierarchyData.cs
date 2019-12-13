// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
#region Usings

using System;
using System.Web.UI;

using DotNetNuke.Entities.Portals;

#endregion

namespace DotNetNuke.UI.WebControls
{
    public class NavDataPageHierarchyData : IHierarchyData, INavigateUIData
    {
        private readonly DNNNode m_objNode;

        public NavDataPageHierarchyData(DNNNode obj)
        {
            m_objNode = obj;
        }

        /// <summary>
        /// Returns nodes image
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public virtual string ImageUrl
        {
            get
            {
                if (String.IsNullOrEmpty(m_objNode.Image) || m_objNode.Image.StartsWith("/"))
                {
                    return m_objNode.Image;
                }
                else
                {
                    return PortalController.Instance.GetCurrentPortalSettings().HomeDirectory + m_objNode.Image;
                }
            }
        }

        #region IHierarchyData Members

        /// <summary>
        /// Indicates whether the hierarchical data node that the IHierarchyData object represents has any child nodes.
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public virtual bool HasChildren
        {
            get
            {
                return m_objNode.HasNodes;
            }
        }

        /// <summary>
        /// Gets the hierarchical path of the node.
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public virtual string Path
        {
            get
            {
                return GetValuePath(m_objNode);
            }
        }

        /// <summary>
        /// Gets the hierarchical data node that the IHierarchyData object represents.
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public virtual object Item
        {
            get
            {
                return m_objNode;
            }
        }

        /// <summary>
        /// Gets the name of the type of Object contained in the Item property.
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public virtual string Type
        {
            get
            {
                return "NavDataPageHierarchyData";
            }
        }

        /// <summary>
        /// Gets an enumeration object that represents all the child nodes of the current hierarchical node.
        /// </summary>
        /// <returns></returns>
        /// <remarks></remarks>
        public virtual IHierarchicalEnumerable GetChildren()
        {
            var objNodes = new NavDataPageHierarchicalEnumerable();
            if (m_objNode != null)
            {
                foreach (DNNNode objNode in m_objNode.DNNNodes)
                {
                    objNodes.Add(new NavDataPageHierarchyData(objNode));
                }
            }
            return objNodes;
        }

        /// <summary>
        /// Gets an enumeration object that represents the parent node of the current hierarchical node.
        /// </summary>
        /// <returns></returns>
        /// <remarks></remarks>
        public virtual IHierarchyData GetParent()
        {
            if (m_objNode != null)
            {
                return new NavDataPageHierarchyData(m_objNode.ParentNode);
            }
            else
            {
                return null;
            }
        }

        #endregion

        #region INavigateUIData Members

        /// <summary>
        /// Returns node name
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public virtual string Name
        {
            get
            {
                return GetSafeValue(m_objNode.Text, "");
            }
        }

        /// <summary>
        /// Returns value path of node
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public virtual string Value
        {
            get
            {
                return GetValuePath(m_objNode);
            }
        }

        /// <summary>
        /// Returns node navigation url
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public virtual string NavigateUrl
        {
            get
            {
                return GetSafeValue(m_objNode.NavigateURL, "");
            }
        }

        /// <summary>
        /// Returns Node description
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public virtual string Description
        {
            get
            {
                return GetSafeValue(m_objNode.ToolTip, "");
            }
        }

        #endregion

        public override string ToString()
        {
            return m_objNode.Text;
        }

        /// <summary>
        /// Helper function to handle cases where property is null (Nothing)
        /// </summary>
        /// <param name="Value">Value to evaluate for null</param>
        /// <param name="Def">If null, return this default</param>
        /// <returns></returns>
        /// <remarks></remarks>
        private string GetSafeValue(string Value, string Def)
        {
            if (Value != null)
            {
                return Value;
            }
            else
            {
                return Def;
            }
        }

        /// <summary>
        /// Computes valuepath necessary for ASP.NET controls to guarantee uniqueness
        /// </summary>
        /// <param name="objNode"></param>
        /// <returns>ValuePath</returns>
        /// <remarks>Not sure if it is ok to hardcode the "\" separator, but also not sure where I would get it from</remarks>
        private string GetValuePath(DNNNode objNode)
        {
            DNNNode objParent = objNode.ParentNode;
            string strPath = GetSafeValue(objNode.Key, "");
            do
            {
                if (objParent == null || objParent.Level == -1)
                {
                    break;
                }
                strPath = GetSafeValue(objParent.Key, "") + "\\" + strPath;
                objParent = objParent.ParentNode;
            } while (true);
            return strPath;
        }
    }
}
