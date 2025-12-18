// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.ComponentModel
{
    using System;
    using System.Collections;
    using System.Collections.Generic;

    public static class ComponentFactory
    {
        public static IContainer Container { get; set; }

        public static void InstallComponents(params IComponentInstaller[] installers)
        {
            if (installers == null)
            {
                throw new ArgumentNullException("installers");
            }

            VerifyContainer();
            foreach (IComponentInstaller installer in installers)
            {
                if (installer == null)
                {
                    throw new ArgumentNullException("installers");
                }

                installer.InstallComponents(Container);
            }
        }

        public static object GetComponent(string name)
        {
            VerifyContainer();
            return Container.GetComponent(name);
        }

        public static TContract GetComponent<TContract>()
        {
            VerifyContainer();
            return Container.GetComponent<TContract>();
        }

        public static object GetComponent(Type contractType)
        {
            VerifyContainer();
            return Container.GetComponent(contractType);
        }

        public static TContract GetComponent<TContract>(string name)
        {
            VerifyContainer();
            return Container.GetComponent<TContract>(name);
        }

        public static object GetComponent(string name, Type contractType)
        {
            VerifyContainer();
            return Container.GetComponent(name, contractType);
        }

        public static string[] GetComponentList(Type contractType)
        {
            VerifyContainer();
            return Container.GetComponentList(contractType);
        }

        public static string[] GetComponentList<TContract>()
        {
            VerifyContainer();
            return Container.GetComponentList<TContract>();
        }

        public static Dictionary<string, TContract> GetComponents<TContract>()
        {
            VerifyContainer();
            var components = new Dictionary<string, TContract>();
            foreach (string componentName in GetComponentList<TContract>())
            {
                components[componentName] = GetComponent<TContract>(componentName);
            }

            return components;
        }

        public static IDictionary GetComponentSettings(string name)
        {
            VerifyContainer();
            return Container.GetComponentSettings(name);
        }

        public static IDictionary GetComponentSettings(Type component)
        {
            VerifyContainer();
            return Container.GetComponentSettings(component);
        }

        public static IDictionary GetComponentSettings<TComponent>()
        {
            VerifyContainer();
            return Container.GetComponentSettings<TComponent>();
        }

        public static void RegisterComponent<TComponent>()
            where TComponent : class
        {
            VerifyContainer();
            Container.RegisterComponent<TComponent>();
        }

        public static void RegisterComponent<TContract, TComponent>()
            where TComponent : class
        {
            VerifyContainer();
            Container.RegisterComponent<TContract, TComponent>();
        }

        public static void RegisterComponent(Type componentType)
        {
            VerifyContainer();
            Container.RegisterComponent(componentType);
        }

        public static void RegisterComponent(Type contractType, Type componentType)
        {
            VerifyContainer();
            Container.RegisterComponent(contractType, componentType);
        }

        public static void RegisterComponent<TComponent>(string name)
            where TComponent : class
        {
            VerifyContainer();
            Container.RegisterComponent<TComponent>(name);
        }

        public static void RegisterComponent<TContract, TComponent>(string name)
            where TComponent : class
        {
            VerifyContainer();
            Container.RegisterComponent<TContract, TComponent>(name);
        }

        public static void RegisterComponent(string name, Type componentType)
        {
            VerifyContainer();
            Container.RegisterComponent(name, componentType);
        }

        public static void RegisterComponent(string name, Type contractType, Type componentType)
        {
            VerifyContainer();
            Container.RegisterComponent(name, contractType, componentType);
        }

        public static void RegisterComponentInstance(string name, Type contractType, object instance)
        {
            VerifyContainer();
            Container.RegisterComponentInstance(name, contractType, instance);
        }

        public static void RegisterComponentInstance(string name, object instance)
        {
            VerifyContainer();
            Container.RegisterComponentInstance(name, instance);
        }

        public static void RegisterComponentInstance<TContract>(string name, object instance)
        {
            VerifyContainer();
            Container.RegisterComponentInstance<TContract>(name, instance);
        }

        public static void RegisterComponentInstance<TContract>(object instance)
        {
            VerifyContainer();
            Container.RegisterComponentInstance<TContract>(instance);
        }

        public static void RegisterComponentSettings(string name, IDictionary dependencies)
        {
            VerifyContainer();
            Container.RegisterComponentSettings(name, dependencies);
        }

        public static void RegisterComponentSettings(Type component, IDictionary dependencies)
        {
            VerifyContainer();
            Container.RegisterComponentSettings(component, dependencies);
        }

        public static void RegisterComponentSettings<TComponent>(IDictionary dependencies)
        {
            VerifyContainer();
            Container.RegisterComponentSettings<TComponent>(dependencies);
        }

        private static void VerifyContainer()
        {
            if (Container == null)
            {
                Container = new SimpleContainer();
            }
        }
    }
}
