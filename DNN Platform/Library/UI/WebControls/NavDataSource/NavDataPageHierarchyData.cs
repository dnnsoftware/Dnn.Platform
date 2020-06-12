// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

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
            this.m_objNode = obj;
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
                if (string.IsNullOrEmpty(this.m_objNode.Image) || this.m_objNode.Image.StartsWith("/"))
                {
                    return this.m_objNode.Image;
                }
                else
                {
                    return PortalController.Instance.GetCurrentPortalSettings().HomeDirectory + this.m_objNode.Image;
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
                return this.m_objNode.HasNodes;
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
                return this.GetValuePath(this.m_objNode);
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
                return this.m_objNode;
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
            if (this.m_objNode != null)
            {
                foreach (DNNNode objNode in this.m_objNode.DNNNodes)
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
            if (this.m_objNode != null)
            {
                return new NavDataPageHierarchyData(this.m_objNode.ParentNode);
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
                return this.GetSafeValue(this.m_objNode.Text, "");
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
                return this.GetValuePath(this.m_objNode);
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
                return this.GetSafeValue(this.m_objNode.NavigateURL, "");
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
                return this.GetSafeValue(this.m_objNode.ToolTip, "");
            }
        }

        #endregion

        public override string ToString()
        {
            return this.m_objNode.Text;
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
            string strPath = this.GetSafeValue(objNode.Key, "");
            do
            {
                if (objParent == null || objParent.Level == -1)
                {
                    break;
                }
                strPath = this.GetSafeValue(objParent.Key, "") + "\\" + strPath;
                objParent = objParent.ParentNode;
            } while (true);
            return strPath;
        }
    }
}
