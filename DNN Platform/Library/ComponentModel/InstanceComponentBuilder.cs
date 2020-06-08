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
        /// Initializes a new instance of the InstanceComponentBuilder class.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="instance"></param>
        public InstanceComponentBuilder(string name, object instance)
        {
            _Name = name;
            _Instance = instance;
        }

        #region IComponentBuilder Members

        public object BuildComponent()
        {
            return _Instance;
        }

        public string Name
        {
            get
            {
                return _Name;
            }
        }

        #endregion
    }
}
