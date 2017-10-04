#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2017
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
