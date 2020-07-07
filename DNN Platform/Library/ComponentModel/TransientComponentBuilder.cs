// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.ComponentModel
{
    using System;

    using DotNetNuke.Framework;

    internal class TransientComponentBuilder : IComponentBuilder
    {
        private readonly string _Name;
        private readonly Type _Type;

        /// <summary>
        /// Initializes a new instance of the <see cref="TransientComponentBuilder"/> class.
        /// </summary>
        /// <param name="name">The name of the component.</param>
        /// <param name="type">The type of the component.</param>
        public TransientComponentBuilder(string name, Type type)
        {
            this._Name = name;
            this._Type = type;
        }

        public string Name
        {
            get
            {
                return this._Name;
            }
        }

        public object BuildComponent()
        {
            return Reflection.CreateObject(this._Type);
        }
    }
}
