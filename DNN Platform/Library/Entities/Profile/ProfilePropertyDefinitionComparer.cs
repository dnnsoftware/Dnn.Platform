// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
#region Usings

using System.Collections;

#endregion

namespace DotNetNuke.Entities.Profile
{
    /// -----------------------------------------------------------------------------
    /// Project:    DotNetNuke
    /// Namespace:  DotNetNuke.Entities.Profile
    /// Class:      ProfilePropertyDefinitionComparer
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The ProfilePropertyDefinitionComparer class provides an implementation of
    /// IComparer to sort the ProfilePropertyDefinitionCollection by ViewOrder
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// -----------------------------------------------------------------------------
    public class ProfilePropertyDefinitionComparer : IComparer
    {
        #region IComparer Members

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Compares two ProfilePropertyDefinition objects
        /// </summary>
        /// <param name="x">A ProfilePropertyDefinition object</param>
        /// <param name="y">A ProfilePropertyDefinition object</param>
        /// <returns>An integer indicating whether x greater than y, x=y or x less than y</returns>
        /// -----------------------------------------------------------------------------
        public int Compare(object x, object y)
        {
            return ((ProfilePropertyDefinition) x).ViewOrder.CompareTo(((ProfilePropertyDefinition) y).ViewOrder);
        }

        #endregion
    }
}
