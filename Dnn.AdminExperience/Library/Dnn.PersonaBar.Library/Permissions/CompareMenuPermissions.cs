// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
#region Usings

using System.Collections;
using Dnn.PersonaBar.Library.Model;

#endregion

namespace Dnn.PersonaBar.Library.Permissions
{
    internal class CompareMenuPermissions : IComparer
    {
        #region IComparer Members

        public int Compare(object x, object y)
        {
            return ((MenuPermissionInfo) x).MenuPermissionId.CompareTo(((MenuPermissionInfo) y).MenuPermissionId);
        }

        #endregion
    }
}
