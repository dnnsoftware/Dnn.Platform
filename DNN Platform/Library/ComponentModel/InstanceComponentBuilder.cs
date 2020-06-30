// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.ComponentModel
{
    internal class InstanceComponentBuilder : IComponentBuilder
    {
        private readonly object _Instance;
        private readonly string _Name;

        /// <summary>
        /// Initializes a new instance of the <see cref="InstanceComponentBuilder"/> class.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="instance"></param>
        public InstanceComponentBuilder(string name, object instance)
        {
            this._Name = name;
            this._Instance = instance;
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
            return this._Instance;
        }
    }
}
