// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Library.Common
{
    using System;
    using System.Collections.Generic;

    using DotNetNuke.ComponentModel;
    using DotNetNuke.Instrumentation;

    public class IocUtil
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(IocUtil));

        /// <summary>
        /// Register a component into the IOC container for later instantiation.
        /// </summary>
        /// <typeparam name="TContract">Contract interface for the component to registr with the IOC container.</typeparam>
        /// <typeparam name="TConcrete">Concrete implementation class (must have apublic default constructor).</typeparam>
        /// <param name="name">Optional name for the contract. Useful when more than once class implements the same contract.</param>
        /// <returns>True if the component was created; false if it was already created in the system.</returns>
        /// <remarks>This helper creates a singleton instance for the contract.</remarks>
        public static bool RegisterComponent<TContract, TConcrete>(string name = null)
            where TContract : class
            where TConcrete : class, new()
        {
            try
            {
                if (!string.IsNullOrEmpty(name))
                {
                    name += "." + typeof(TContract).FullName;
                }

                var component = GetInstanceLocal<TContract>(name);

                if (component != null)
                {
                    return false;
                }

                if (string.IsNullOrEmpty(name))
                {
                    ComponentFactory.RegisterComponent<TContract, TConcrete>();
                }
                else
                {
                    ComponentFactory.RegisterComponent<TContract, TConcrete>(name);
                }

                return true;
            }
            catch (Exception e)
            {
                Logger.Error(e);
                return false;
            }
        }

        /// <summary>
        /// Register a specific instance into the IOC container for later use.
        /// </summary>
        /// <typeparam name="TContract">Contract interface for the component to registr with the IOC container.</typeparam>
        /// <param name="name">Name for the contract. Useful when more than once class implements the same contract. Pass as null when unused.</param>
        /// <param name="instance">Concrete implementation class (must have apublic default constructor).</param>
        /// <returns>True if the component was created; false if it was already created in the system.</returns>
        public static bool RegisterComponentInstance<TContract>(string name, object instance)
            where TContract : class
        {
            try
            {
                if (!string.IsNullOrEmpty(name))
                {
                    name += "." + typeof(TContract).FullName;
                }

                var component = GetInstanceLocal<TContract>(name);
                if (component != null)
                {
                    return false;
                }

                if (string.IsNullOrEmpty(name))
                {
                    ComponentFactory.RegisterComponentInstance<TContract>(instance);
                }
                else
                {
                    ComponentFactory.RegisterComponentInstance<TContract>(name, instance);
                }

                return true;
            }
            catch (Exception e)
            {
                Logger.Error(e);
                return false;
            }
        }

        public static TContract GetInstance<TContract>(string name = null)
            where TContract : class
        {
            if (!string.IsNullOrEmpty(name))
            {
                name += "." + typeof(TContract).FullName;
            }

            var instance = GetInstanceLocal<TContract>(name);
            if (instance == null)
            {
                Logger.WarnFormat(
                    "No instance of type '{0}' and name '{1}' is registered in the IOC container.",
                    typeof(TContract).FullName, name ?? "<empty>");
            }

            return instance;
        }

        /// <summary>
        /// Retrieves a concrete implementation of the given interface/contract.
        /// </summary>
        /// <typeparam name="TContract">Contract interface for the component to get a concrete implementation of.</typeparam>
        /// <returns>A concrete implementation of the given interface (or null if none is registered).</returns>
        public static IEnumerable<TContract> GetInstanceContracts<TContract>()
            where TContract : class
        {
            var instances = ComponentFactory.GetComponents<TContract>();
            return instances.Values;
        }

        private static TContract GetInstanceLocal<TContract>(string name)
            where TContract : class
        {
            return string.IsNullOrEmpty(name)
                ? ComponentFactory.GetComponent<TContract>()
                : ComponentFactory.GetComponent<TContract>(name);
        }
    }
}
