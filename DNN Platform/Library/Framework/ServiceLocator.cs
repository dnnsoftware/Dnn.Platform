// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

using System;

namespace DotNetNuke.Framework
{
    /// <summary>
    /// Provides a readily testable way to manage a Singleton
    /// </summary>
    /// <typeparam name="TContract">The interface that the controller provides</typeparam>
    /// <typeparam name="TSelf">The type of the controller itself, used to call the GetFactory override</typeparam>
    public abstract class ServiceLocator<TContract, TSelf>
        where TSelf : ServiceLocator<TContract, TSelf>, new()
    {
// ReSharper disable StaticFieldInGenericType
// ReSharper disable InconsistentNaming
        private static Lazy<TContract> _instance = new Lazy<TContract>(InitInstance, true);
// ReSharper restore InconsistentNaming
        private static TContract _testableInstance;
        private static bool _useTestable;
// ReSharper restore StaticFieldInGenericType

        protected static Func<TContract> Factory { get; set; }

        private static TContract InitInstance()
        {
            if (Factory == null)
            {
                var controllerInstance = new TSelf();
                Factory = controllerInstance.GetFactory();
            }

            return Factory();
        }

        /// <summary>
        /// Returns a singleton of T
        /// </summary>
        public static TContract Instance
        {
            get
            {
                if (_useTestable)
                {
                    return _testableInstance;
                }

                return _instance.Value;
            }
        }

        /// <summary>
        /// Registers an instance to use for the Singleton
        /// </summary>
        /// <remarks>Intended for unit testing purposes, not thread safe</remarks>
        /// <param name="instance"></param>
        public static void SetTestableInstance(TContract instance)
        {
            _testableInstance = instance;
            _useTestable = true;
        }

        /// <summary>
        /// Clears the current instance, a new instance will be initialized when next requested
        /// </summary>
        /// <remarks>Intended for unit testing purposes, not thread safe</remarks>
        public static void ClearInstance()
        {
            _useTestable = false;
            _testableInstance = default(TContract);
            _instance = new Lazy<TContract>(InitInstance, true);
        }

        protected abstract Func<TContract> GetFactory();
    }
}
