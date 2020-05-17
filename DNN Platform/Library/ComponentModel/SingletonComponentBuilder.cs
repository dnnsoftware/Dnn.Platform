﻿// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
#region Usings

using System;

using DotNetNuke.Framework;

#endregion

namespace DotNetNuke.ComponentModel
{
    internal class SingletonComponentBuilder : IComponentBuilder
    {
        private readonly string _Name;
        private readonly Type _Type;
        private object _Instance;

        /// <summary>
        /// Initializes a new instance of the SingletonComponentBuilder class.
        /// </summary>
        /// <param name="name">The name of the component</param>
        /// <param name="type">The type of the component</param>
        public SingletonComponentBuilder(string name, Type type)
        {
            _Name = name;
            _Type = type;
        }

        #region IComponentBuilder Members

        public object BuildComponent()
        {
            if (_Instance == null)
            {
                CreateInstance();
            }
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

        private void CreateInstance()
        {
            _Instance = Reflection.CreateObject(_Type);
        }
    }
}
