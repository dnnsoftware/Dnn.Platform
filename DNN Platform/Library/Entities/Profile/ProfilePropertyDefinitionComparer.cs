// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Entities.Profile
{
    using System.Collections;

    /// -----------------------------------------------------------------------------
    /// Project:    DotNetNuke
    /// Namespace:  DotNetNuke.Entities.Profile
    /// Class:      ProfilePropertyDefinitionComparer
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The ProfilePropertyDefinitionComparer class provides an implementation of
    /// IComparer to sort the ProfilePropertyDefinitionCollection by ViewOrder.
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// -----------------------------------------------------------------------------
    public class ProfilePropertyDefinitionComparer : IComparer
    {
        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Compares two ProfilePropertyDefinition objects.
        /// </summary>
        /// <param name="x">A ProfilePropertyDefinition object.</param>
        /// <param name="y">A ProfilePropertyDefinition object.</param>
        /// <returns>An integer indicating whether x greater than y, x=y or x less than y.</returns>
        /// -----------------------------------------------------------------------------
        public int Compare(object x, object y)
        {
            return ((ProfilePropertyDefinition)x).ViewOrder.CompareTo(((ProfilePropertyDefinition)y).ViewOrder);
        }
    }
}
