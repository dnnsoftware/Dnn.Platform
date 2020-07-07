// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.ComponentModel
{
    using System;
    using System.Collections;
    using System.Collections.Generic;

    using DotNetNuke.Collections.Internal;

    public class SimpleContainer : AbstractContainer
    {
        private readonly string _name;
        private readonly ComponentBuilderCollection _componentBuilders = new ComponentBuilderCollection();

        private readonly SharedDictionary<string, IDictionary> _componentDependencies = new SharedDictionary<string, IDictionary>();

        private readonly ComponentTypeCollection _componentTypes = new ComponentTypeCollection();

        private readonly SharedDictionary<Type, string> _registeredComponents = new SharedDictionary<Type, string>();

        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleContainer"/> class.
        /// </summary>
        public SimpleContainer()
            : this(string.Format("Container_{0}", Guid.NewGuid()))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleContainer"/> class.
        /// </summary>
        /// <param name = "name"></param>
        public SimpleContainer(string name)
        {
            this._name = name;
        }

        public override string Name
        {
            get
            {
                return this._name;
            }
        }

        public override void RegisterComponent(string name, Type type)
        {
            using (this._registeredComponents.GetWriteLock())
            {
                this._registeredComponents[type] = name;
            }
        }

        public override object GetComponent(string name)
        {
            IComponentBuilder builder = this.GetComponentBuilder(name);

            return this.GetComponent(builder);
        }

        public override object GetComponent(Type contractType)
        {
            ComponentType componentType = this.GetComponentType(contractType);
            object component = null;

            if (componentType != null)
            {
                int builderCount;

                using (componentType.ComponentBuilders.GetReadLock())
                {
                    builderCount = componentType.ComponentBuilders.Count;
                }

                if (builderCount > 0)
                {
                    IComponentBuilder builder = this.GetDefaultComponentBuilder(componentType);

                    component = this.GetComponent(builder);
                }
            }

            return component;
        }

        public override object GetComponent(string name, Type contractType)
        {
            ComponentType componentType = this.GetComponentType(contractType);
            object component = null;

            if (componentType != null)
            {
                IComponentBuilder builder = this.GetComponentBuilder(name);

                component = this.GetComponent(builder);
            }

            return component;
        }

        public override string[] GetComponentList(Type contractType)
        {
            var components = new List<string>();

            using (this._registeredComponents.GetReadLock())
            {
                foreach (KeyValuePair<Type, string> kvp in this._registeredComponents)
                {
                    if (contractType.IsAssignableFrom(kvp.Key))
                    {
                        components.Add(kvp.Value);
                    }
                }
            }

            return components.ToArray();
        }

        public override IDictionary GetComponentSettings(string name)
        {
            IDictionary settings;
            using (this._componentDependencies.GetReadLock())
            {
                settings = this._componentDependencies[name];
            }

            return settings;
        }

        public override void RegisterComponent(string name, Type contractType, Type type, ComponentLifeStyleType lifestyle)
        {
            this.AddComponentType(contractType);

            IComponentBuilder builder = null;
            switch (lifestyle)
            {
                case ComponentLifeStyleType.Transient:
                    builder = new TransientComponentBuilder(name, type);
                    break;
                case ComponentLifeStyleType.Singleton:
                    builder = new SingletonComponentBuilder(name, type);
                    break;
            }

            this.AddBuilder(contractType, builder);

            this.RegisterComponent(name, type);
        }

        public override void RegisterComponentInstance(string name, Type contractType, object instance)
        {
            this.AddComponentType(contractType);

            this.AddBuilder(contractType, new InstanceComponentBuilder(name, instance));

            this.RegisterComponent(name, instance.GetType());
        }

        public override void RegisterComponentSettings(string name, IDictionary dependencies)
        {
            using (this._componentDependencies.GetWriteLock())
            {
                this._componentDependencies[name] = dependencies;
            }
        }

        private void AddBuilder(Type contractType, IComponentBuilder builder)
        {
            ComponentType componentType = this.GetComponentType(contractType);
            if (componentType != null)
            {
                ComponentBuilderCollection builders = componentType.ComponentBuilders;

                using (builders.GetWriteLock())
                {
                    builders.AddBuilder(builder, true);
                }

                using (this._componentBuilders.GetWriteLock())
                {
                    this._componentBuilders.AddBuilder(builder, false);
                }
            }
        }

        private void AddComponentType(Type contractType)
        {
            ComponentType componentType = this.GetComponentType(contractType);

            if (componentType == null)
            {
                componentType = new ComponentType(contractType);

                using (this._componentTypes.GetWriteLock())
                {
                    this._componentTypes[componentType.BaseType] = componentType;
                }
            }
        }

        private object GetComponent(IComponentBuilder builder)
        {
            object component;
            if (builder == null)
            {
                component = null;
            }
            else
            {
                component = builder.BuildComponent();
            }

            return component;
        }

        private IComponentBuilder GetComponentBuilder(string name)
        {
            IComponentBuilder builder;

            using (this._componentBuilders.GetReadLock())
            {
                this._componentBuilders.TryGetValue(name, out builder);
            }

            return builder;
        }

        private IComponentBuilder GetDefaultComponentBuilder(ComponentType componentType)
        {
            IComponentBuilder builder;

            using (componentType.ComponentBuilders.GetReadLock())
            {
                builder = componentType.ComponentBuilders.DefaultBuilder;
            }

            return builder;
        }

        private ComponentType GetComponentType(Type contractType)
        {
            ComponentType componentType;

            using (this._componentTypes.GetReadLock())
            {
                this._componentTypes.TryGetValue(contractType, out componentType);
            }

            return componentType;
        }
    }
}
