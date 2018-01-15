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

#endregion

namespace DotNetNuke.ComponentModel
{
    public abstract class AbstractContainer : IContainer
    {
        #region IContainer Members

        public abstract string Name { get; }

        public abstract object GetComponent(string name);

        public abstract object GetComponent(string name, Type contractType);

        public abstract object GetComponent(Type contractType);

        public virtual TContract GetComponent<TContract>()
        {
            return (TContract) GetComponent(typeof (TContract));
        }

        public virtual TContract GetComponent<TContract>(string name)
        {
            return (TContract) GetComponent(name, typeof (TContract));
        }

        public abstract string[] GetComponentList(Type contractType);

        public virtual string[] GetComponentList<TContract>()
        {
            return GetComponentList(typeof (TContract));
        }

        public abstract IDictionary GetComponentSettings(string name);

        public virtual IDictionary GetComponentSettings(Type component)
        {
            return GetComponentSettings(component.FullName);
        }

        public IDictionary GetComponentSettings<TComponent>()
        {
            return GetComponentSettings(typeof (TComponent).FullName);
        }

        public abstract void RegisterComponent(string name, Type contractType, Type componentType, ComponentLifeStyleType lifestyle);

        public virtual void RegisterComponent(string name, Type contractType, Type componentType)
        {
            RegisterComponent(name, contractType, componentType, ComponentLifeStyleType.Singleton);
        }

        public virtual void RegisterComponent(string name, Type componentType)
        {
            RegisterComponent(name, componentType, componentType, ComponentLifeStyleType.Singleton);
        }

        public virtual void RegisterComponent(Type contractType, Type componentType)
        {
            RegisterComponent(componentType.FullName, contractType, componentType, ComponentLifeStyleType.Singleton);
        }

        public virtual void RegisterComponent(Type contractType, Type componentType, ComponentLifeStyleType lifestyle)
        {
            RegisterComponent(componentType.FullName, contractType, componentType, lifestyle);
        }

        public virtual void RegisterComponent(Type componentType)
        {
            RegisterComponent(componentType.FullName, componentType, componentType, ComponentLifeStyleType.Singleton);
        }

        public virtual void RegisterComponent<TComponent>() where TComponent : class
        {
            RegisterComponent(typeof (TComponent));
        }

        public virtual void RegisterComponent<TComponent>(string name) where TComponent : class
        {
            RegisterComponent(name, typeof (TComponent), typeof (TComponent), ComponentLifeStyleType.Singleton);
        }

        public virtual void RegisterComponent<TComponent>(string name, ComponentLifeStyleType lifestyle) where TComponent : class
        {
            RegisterComponent(name, typeof (TComponent), typeof (TComponent), lifestyle);
        }

        public virtual void RegisterComponent<TContract, TComponent>() where TComponent : class
        {
            RegisterComponent(typeof (TContract), typeof (TComponent));
        }

        public virtual void RegisterComponent<TContract, TComponent>(string name) where TComponent : class
        {
            RegisterComponent(name, typeof (TContract), typeof (TComponent), ComponentLifeStyleType.Singleton);
        }

        public virtual void RegisterComponent<TContract, TComponent>(string name, ComponentLifeStyleType lifestyle) where TComponent : class
        {
            RegisterComponent(name, typeof (TContract), typeof (TComponent), lifestyle);
        }

        public abstract void RegisterComponentSettings(string name, IDictionary dependencies);

        public virtual void RegisterComponentSettings(Type component, IDictionary dependencies)
        {
            RegisterComponentSettings(component.FullName, dependencies);
        }

        public virtual void RegisterComponentSettings<TComponent>(IDictionary dependencies)
        {
            RegisterComponentSettings(typeof (TComponent).FullName, dependencies);
        }

        public abstract void RegisterComponentInstance(string name, Type contractType, object instance);

        public void RegisterComponentInstance(string name, object instance)
        {
            RegisterComponentInstance(name, instance.GetType(), instance);
        }

        public void RegisterComponentInstance<TContract>(object instance)
        {
            RegisterComponentInstance(instance.GetType().FullName, typeof (TContract), instance);
        }

        public void RegisterComponentInstance<TContract>(string name, object instance)
        {
            RegisterComponentInstance(name, typeof (TContract), instance);
        }

        #endregion

        public virtual IDictionary GetCustomDependencies<TComponent>()
        {
            return GetComponentSettings(typeof (TComponent).FullName);
        }
    }
}