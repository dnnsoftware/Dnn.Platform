// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.ComponentModel
{
    using System;

    public abstract class ComponentBase<TContract, TType>
        where TType : class, TContract
    {
        private static TContract _testableInstance;
        private static bool _useTestable = false;

        public static TContract Instance
        {
            get
            {
                if (_useTestable && _testableInstance != null)
                {
                    return _testableInstance;
                }

                var component = ComponentFactory.GetComponent<TContract>();

                if (component == null)
                {
                    component = (TContract)Activator.CreateInstance(typeof(TType), true);
                    ComponentFactory.RegisterComponentInstance<TContract>(component);
                }

                return component;
            }
        }

        public static void RegisterInstance(TContract instance)
        {
            if (ComponentFactory.GetComponent<TContract>() == null)
            {
                ComponentFactory.RegisterComponentInstance<TContract>(instance);
            }
        }

        /// <summary>
        /// Registers an instance to use for the Singleton.
        /// </summary>
        /// <remarks>Intended for unit testing purposes, not thread safe.</remarks>
        /// <param name="instance"></param>
        internal static void SetTestableInstance(TContract instance)
        {
            _testableInstance = instance;
            _useTestable = true;
        }

        /// <summary>
        /// Clears the current instance, a new instance will be initialized when next requested.
        /// </summary>
        /// <remarks>Intended for unit testing purposes, not thread safe.</remarks>
        internal static void ClearInstance()
        {
            _useTestable = false;
            _testableInstance = default(TContract);
        }
    }
}
