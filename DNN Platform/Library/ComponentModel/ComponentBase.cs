﻿// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
#region Usings

using System;

#endregion

namespace DotNetNuke.ComponentModel
{
    public abstract class ComponentBase<TContract, TType> where TType : class, TContract
    {
        private static TContract _testableInstance;
        private static bool _useTestable = false;

        public static TContract Instance
        {
            get
            {
                if (_useTestable && _testableInstance != null)
                    return _testableInstance;

                var component = ComponentFactory.GetComponent<TContract>();

                if (component == null)
                {
                    component = (TContract) Activator.CreateInstance(typeof (TType), true);
                    ComponentFactory.RegisterComponentInstance<TContract>(component);
                }

                return component;
            }
        }

        /// <summary>
        /// Registers an instance to use for the Singleton
        /// </summary>
        /// <remarks>Intended for unit testing purposes, not thread safe</remarks>
        /// <param name="instance"></param>
        internal static void SetTestableInstance(TContract instance)
        {
            _testableInstance = instance;
            _useTestable = true;
        }

        /// <summary>
        /// Clears the current instance, a new instance will be initialized when next requested
        /// </summary>
        /// <remarks>Intended for unit testing purposes, not thread safe</remarks>
        internal static void ClearInstance()
        {
            _useTestable = false;
            _testableInstance = default(TContract);
        }

        public static void RegisterInstance(TContract instance)
        {
            if ((ComponentFactory.GetComponent<TContract>() == null))
            {
                ComponentFactory.RegisterComponentInstance<TContract>(instance);
            }
        }
    }
}
