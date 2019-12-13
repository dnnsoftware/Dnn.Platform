// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
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
