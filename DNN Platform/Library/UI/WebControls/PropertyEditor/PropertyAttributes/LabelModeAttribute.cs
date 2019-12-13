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
    public sealed class LabelModeAttribute : Attribute
    {
        private readonly LabelMode _Mode;

        /// <summary>
        /// Initializes a new instance of the LabelModeAttribute class.
        /// </summary>
        /// <param name="mode">The label mode to apply to the associated property</param>
        public LabelModeAttribute(LabelMode mode)
        {
            _Mode = mode;
        }

        public LabelMode Mode
        {
            get
            {
                return _Mode;
            }
        }
    }
}
