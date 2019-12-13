// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
#region Usings

using System;
using System.Web.UI;

#endregion

namespace DotNetNuke.UI.WebControls
{
	/// <summary>The NavDataSourceView class encapsulates the capabilities of the NavDataSource data source control.</summary>
    public class NavDataSourceView : HierarchicalDataSourceView
    {
        private readonly string m_sKey;
        private string m_sNamespace = "MyNS";

        public NavDataSourceView(string viewPath)
        {
            if (String.IsNullOrEmpty(viewPath))
            {
                m_sKey = "";
            }
            else if (viewPath.IndexOf("\\") > -1)
            {
                m_sKey = viewPath.Substring(viewPath.LastIndexOf("\\") + 1);
            }
            else
            {
                m_sKey = viewPath;
            }
        }

        public string Namespace
        {
            get
            {
                return m_sNamespace;
            }
            set
            {
                m_sNamespace = value;
            }
        }

        /// <summary>
        /// Starting with the rootNode, recursively build a list of
        /// PageInfo nodes, create PageHierarchyData
        /// objects, add them all to the PageHierarchicalEnumerable,
        /// and return the list.
        /// </summary>
        /// <returns></returns>
        /// <remarks></remarks>
        public override IHierarchicalEnumerable Select()
        {
            var objPages = new NavDataPageHierarchicalEnumerable();
            DNNNodeCollection objNodes;
            objNodes = Navigation.GetNavigationNodes(m_sNamespace);
            if (!String.IsNullOrEmpty(m_sKey))
            {
                objNodes = objNodes.FindNodeByKey(m_sKey).DNNNodes;
            }
            foreach (DNNNode objNode in objNodes)
            {
                objPages.Add(new NavDataPageHierarchyData(objNode));
            }
            return objPages;
        }
    }
}
