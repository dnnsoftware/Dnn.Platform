// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.UI.WebControls
{
    using System;
    using System.Web.UI;

    /// <summary>The NavDataSourceView class encapsulates the capabilities of the NavDataSource data source control.</summary>
    public class NavDataSourceView : HierarchicalDataSourceView
    {
        private readonly string m_sKey;
        private string m_sNamespace = "MyNS";

        public NavDataSourceView(string viewPath)
        {
            if (string.IsNullOrEmpty(viewPath))
            {
                this.m_sKey = string.Empty;
            }
            else if (viewPath.IndexOf("\\") > -1)
            {
                this.m_sKey = viewPath.Substring(viewPath.LastIndexOf("\\") + 1);
            }
            else
            {
                this.m_sKey = viewPath;
            }
        }

        public string Namespace
        {
            get
            {
                return this.m_sNamespace;
            }

            set
            {
                this.m_sNamespace = value;
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
            objNodes = Navigation.GetNavigationNodes(this.m_sNamespace);
            if (!string.IsNullOrEmpty(this.m_sKey))
            {
                objNodes = objNodes.FindNodeByKey(this.m_sKey).DNNNodes;
            }

            foreach (DNNNode objNode in objNodes)
            {
                objPages.Add(new NavDataPageHierarchyData(objNode));
            }

            return objPages;
        }
    }
}
