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
