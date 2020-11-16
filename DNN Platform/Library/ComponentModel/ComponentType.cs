// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.ComponentModel
{
    using System;

    internal class ComponentType
    {
        private readonly Type _BaseType;
        private readonly ComponentBuilderCollection _ComponentBuilders = new ComponentBuilderCollection();

        /// <summary>
        /// Initializes a new instance of the <see cref="ComponentType"/> class.
        /// </summary>
        /// <param name="baseType">The base type of Components of this ComponentType.</param>
        public ComponentType(Type baseType)
        {
            this._BaseType = baseType;
        }

        public Type BaseType
        {
            get
            {
                return this._BaseType;
            }
        }

        public ComponentBuilderCollection ComponentBuilders
        {
            get
            {
                return this._ComponentBuilders;
            }
        }
    }
}
