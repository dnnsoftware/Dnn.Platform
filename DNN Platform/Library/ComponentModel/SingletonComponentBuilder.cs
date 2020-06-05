﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

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
            this._Name = name;
            this._Type = type;
        }

        #region IComponentBuilder Members

        public object BuildComponent()
        {
            if (this._Instance == null)
            {
                this.CreateInstance();
            }
            return this._Instance;
        }

        public string Name
        {
            get
            {
                return this._Name;
            }
        }

        #endregion

        private void CreateInstance()
        {
            this._Instance = Reflection.CreateObject(this._Type);
        }
    }
}
