#region Usings

using System;
using System.Collections;
using System.Reflection;

#endregion

namespace DotNetNuke.UI.WebControls
{
    public class PropertyNameComparer : IComparer
    {
        #region IComparer Members

        public int Compare(object x, object y)
        {
            if (x is PropertyInfo && y is PropertyInfo)
            {
                return string.Compare(((PropertyInfo) x).Name, ((PropertyInfo) y).Name);
            }
            else
            {
                throw new ArgumentException("Object is not of type PropertyInfo");
            }
        }

        #endregion
    }
}
