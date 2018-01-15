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

using DotNetNuke.Collections.Internal;

#endregion

namespace DotNetNuke.ComponentModel
{
    public class SimpleContainer : AbstractContainer
    {
        private readonly string _name;
        private readonly ComponentBuilderCollection _componentBuilders = new ComponentBuilderCollection();

        private readonly SharedDictionary<string, IDictionary> _componentDependencies = new SharedDictionary<string, IDictionary>();

        private readonly ComponentTypeCollection _componentTypes = new ComponentTypeCollection();

        private readonly SharedDictionary<Type, string> _registeredComponents = new SharedDictionary<Type, string>();

        #region "Constructors"

        /// <summary>
        ///   Initializes a new instance of the SimpleContainer class.
        /// </summary>
        public SimpleContainer() : this(string.Format("Container_{0}", Guid.NewGuid()))
        {
        }

        /// <summary>
        ///   Initializes a new instance of the SimpleContainer class.
        /// </summary>
        /// <param name = "name"></param>
        public SimpleContainer(string name)
        {
            _name = name;
        }

        #endregion

        #region "Private Methods"

        private void AddBuilder(Type contractType, IComponentBuilder builder)
        {
            ComponentType componentType = GetComponentType(contractType);
            if (componentType != null)
            {
                ComponentBuilderCollection builders = componentType.ComponentBuilders;

                using (builders.GetWriteLock())
                {
                    builders.AddBuilder(builder, true);
                }

                using (_componentBuilders.GetWriteLock())
                {
                    _componentBuilders.AddBuilder(builder, false);
                }
            }
        }

        private void AddComponentType(Type contractType)
        {
            ComponentType componentType = GetComponentType(contractType);

            if (componentType == null)
            {
                componentType = new ComponentType(contractType);

                using (_componentTypes.GetWriteLock())
                {
                    _componentTypes[componentType.BaseType] = componentType;
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

            using (_componentBuilders.GetReadLock())
            {
                _componentBuilders.TryGetValue(name, out builder);
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

            using (_componentTypes.GetReadLock())
            {
                _componentTypes.TryGetValue(contractType, out componentType);
            }

            return componentType;
        }

        public override void RegisterComponent(string name, Type type)
        {
            using (_registeredComponents.GetWriteLock())
            {
                _registeredComponents[type] = name;
            }
        }

        #endregion

        public override string Name
        {
            get
            {
                return _name;
            }
        }

        public override object GetComponent(string name)
        {
            IComponentBuilder builder = GetComponentBuilder(name);

            return GetComponent(builder);
        }

        public override object GetComponent(Type contractType)
        {
            ComponentType componentType = GetComponentType(contractType);
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
                    IComponentBuilder builder = GetDefaultComponentBuilder(componentType);

                    component = GetComponent(builder);
                }
            }

            return component;
        }

        public override object GetComponent(string name, Type contractType)
        {
            ComponentType componentType = GetComponentType(contractType);
            object component = null;

            if (componentType != null)
            {
                IComponentBuilder builder = GetComponentBuilder(name);

                component = GetComponent(builder);
            }
            return component;
        }

        public override string[] GetComponentList(Type contractType)
        {
            var components = new List<string>();

            using (_registeredComponents.GetReadLock())
            {
                foreach (KeyValuePair<Type, string> kvp in _registeredComponents)
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
            using (_componentDependencies.GetReadLock())
            {
                settings = _componentDependencies[name];
            }
            return settings;
        }

        public override void RegisterComponent(string name, Type contractType, Type type, ComponentLifeStyleType lifestyle)
        {
            AddComponentType(contractType);

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
            AddBuilder(contractType, builder);

            RegisterComponent(name, type);
        }

        public override void RegisterComponentInstance(string name, Type contractType, object instance)
        {
            AddComponentType(contractType);

            AddBuilder(contractType, new InstanceComponentBuilder(name, instance));

            RegisterComponent(name, instance.GetType());
        }

        public override void RegisterComponentSettings(string name, IDictionary dependencies)
        {
            using (_componentDependencies.GetWriteLock())
            {
                _componentDependencies[name] = dependencies;
            }
        }
    }
}
