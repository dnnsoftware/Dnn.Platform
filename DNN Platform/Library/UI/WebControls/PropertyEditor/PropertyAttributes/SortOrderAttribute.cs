// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

#region Usings

using System;

#endregion

namespace DotNetNuke.UI.WebControls
{
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class SortOrderAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the SortOrderAttribute class.
        /// </summary>
        /// <param name="order"></param>
        public SortOrderAttribute(int order)
        {
            Order = order;
        }

        public int Order { get; set; }

        public static int DefaultOrder
        {
            get
            {
                return Int32.MaxValue;
            }
        }
    }
}
