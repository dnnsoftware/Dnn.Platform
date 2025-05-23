﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.UI.WebControls
{
    using System;

    [AttributeUsage(AttributeTargets.Property)]
    public sealed class RequiredAttribute : Attribute
    {
        private readonly bool required;

        /// <summary>Initializes a new instance of the <see cref="RequiredAttribute"/> class.</summary>
        /// <param name="required">Whether the value is required.</param>
        public RequiredAttribute(bool required)
        {
            this.required = required;
        }

        public bool Required
        {
            get
            {
                return this.required;
            }
        }
    }
}
