#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2018
// by DotNetNuke Corporation
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation 
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and 
// to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions 
// of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
// TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.
#endregion
#region Usings

using System;
using System.Collections;
using System.Collections.Generic;

#endregion

namespace DotNetNuke.ComponentModel
{
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

        private static void VerifyContainer()
        {
            if (Container == null)
            {
                Container = new SimpleContainer();
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

        public static void RegisterComponent<TComponent>() where TComponent : class
        {
            VerifyContainer();
            Container.RegisterComponent<TComponent>();
        }

        public static void RegisterComponent<TContract, TComponent>() where TComponent : class
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

        public static void RegisterComponent<TComponent>(string name) where TComponent : class
        {
            VerifyContainer();
            Container.RegisterComponent<TComponent>(name);
        }

        public static void RegisterComponent<TContract, TComponent>(string name) where TComponent : class
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
    }
}