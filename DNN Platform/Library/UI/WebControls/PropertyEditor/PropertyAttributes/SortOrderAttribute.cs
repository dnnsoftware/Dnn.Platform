// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
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
