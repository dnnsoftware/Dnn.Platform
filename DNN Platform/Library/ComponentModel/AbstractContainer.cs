// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.ComponentModel
{
    using System;
    using System.Collections;

    public abstract class AbstractContainer : IContainer
    {
        public abstract string Name { get; }

        public abstract object GetComponent(string name);

        public abstract object GetComponent(string name, Type contractType);

        public abstract object GetComponent(Type contractType);

        public virtual TContract GetComponent<TContract>()
        {
            return (TContract)this.GetComponent(typeof(TContract));
        }

        public virtual TContract GetComponent<TContract>(string name)
        {
            return (TContract)this.GetComponent(name, typeof(TContract));
        }

        public abstract string[] GetComponentList(Type contractType);

        public virtual string[] GetComponentList<TContract>()
        {
            return this.GetComponentList(typeof(TContract));
        }

        public abstract IDictionary GetComponentSettings(string name);

        public virtual IDictionary GetComponentSettings(Type component)
        {
            return this.GetComponentSettings(component.FullName);
        }

        public IDictionary GetComponentSettings<TComponent>()
        {
            return this.GetComponentSettings(typeof(TComponent).FullName);
        }

        public abstract void RegisterComponent(string name, Type contractType, Type componentType, ComponentLifeStyleType lifestyle);

        public virtual void RegisterComponent(string name, Type contractType, Type componentType)
        {
            this.RegisterComponent(name, contractType, componentType, ComponentLifeStyleType.Singleton);
        }

        public virtual void RegisterComponent(string name, Type componentType)
        {
            this.RegisterComponent(name, componentType, componentType, ComponentLifeStyleType.Singleton);
        }

        public virtual void RegisterComponent(Type contractType, Type componentType)
        {
            this.RegisterComponent(componentType.FullName, contractType, componentType, ComponentLifeStyleType.Singleton);
        }

        public virtual void RegisterComponent(Type contractType, Type componentType, ComponentLifeStyleType lifestyle)
        {
            this.RegisterComponent(componentType.FullName, contractType, componentType, lifestyle);
        }

        public virtual void RegisterComponent(Type componentType)
        {
            this.RegisterComponent(componentType.FullName, componentType, componentType, ComponentLifeStyleType.Singleton);
        }

        public virtual void RegisterComponent<TComponent>()
            where TComponent : class
        {
            this.RegisterComponent(typeof(TComponent));
        }

        public virtual void RegisterComponent<TComponent>(string name)
            where TComponent : class
        {
            this.RegisterComponent(name, typeof(TComponent), typeof(TComponent), ComponentLifeStyleType.Singleton);
        }

        public virtual void RegisterComponent<TComponent>(string name, ComponentLifeStyleType lifestyle)
            where TComponent : class
        {
            this.RegisterComponent(name, typeof(TComponent), typeof(TComponent), lifestyle);
        }

        public virtual void RegisterComponent<TContract, TComponent>()
            where TComponent : class
        {
            this.RegisterComponent(typeof(TContract), typeof(TComponent));
        }

        public virtual void RegisterComponent<TContract, TComponent>(string name)
            where TComponent : class
        {
            this.RegisterComponent(name, typeof(TContract), typeof(TComponent), ComponentLifeStyleType.Singleton);
        }

        public virtual void RegisterComponent<TContract, TComponent>(string name, ComponentLifeStyleType lifestyle)
            where TComponent : class
        {
            this.RegisterComponent(name, typeof(TContract), typeof(TComponent), lifestyle);
        }

        public abstract void RegisterComponentSettings(string name, IDictionary dependencies);

        public virtual void RegisterComponentSettings(Type component, IDictionary dependencies)
        {
            this.RegisterComponentSettings(component.FullName, dependencies);
        }

        public virtual void RegisterComponentSettings<TComponent>(IDictionary dependencies)
        {
            this.RegisterComponentSettings(typeof(TComponent).FullName, dependencies);
        }

        public abstract void RegisterComponentInstance(string name, Type contractType, object instance);

        public void RegisterComponentInstance(string name, object instance)
        {
            this.RegisterComponentInstance(name, instance.GetType(), instance);
        }

        public void RegisterComponentInstance<TContract>(object instance)
        {
            this.RegisterComponentInstance(instance.GetType().FullName, typeof(TContract), instance);
        }

        public void RegisterComponentInstance<TContract>(string name, object instance)
        {
            this.RegisterComponentInstance(name, typeof(TContract), instance);
        }

        public virtual IDictionary GetCustomDependencies<TComponent>()
        {
            return this.GetComponentSettings(typeof(TComponent).FullName);
        }
    }
}
