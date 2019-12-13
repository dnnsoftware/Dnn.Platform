// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
#region Usings

using System.Collections;
using System.Web.UI;

#endregion

namespace DotNetNuke.UI.WebControls
{
	/// <summary>A collection of PageHierarchyData objects</summary>
    public class NavDataPageHierarchicalEnumerable : ArrayList, IHierarchicalEnumerable
    {
        #region IHierarchicalEnumerable Members

        /// <summary>
        /// Handles enumeration
        /// </summary>
        /// <param name="enumeratedItem"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public virtual IHierarchyData GetHierarchyData(object enumeratedItem)
        {
            return (IHierarchyData) enumeratedItem;
        }

        #endregion
    }
}
