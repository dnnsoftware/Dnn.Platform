﻿// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
#region Usings

using System;
using System.Collections;

#endregion

namespace DotNetNuke.ComponentModel
{
    public interface IContainer
    {
        string Name { get; }

        void RegisterComponent<TComponent>() where TComponent : class;

        void RegisterComponent<TComponent>(string name) where TComponent : class;

        void RegisterComponent<TComponent>(string name, ComponentLifeStyleType lifestyle) where TComponent : class;

        void RegisterComponent<TContract, TComponent>() where TComponent : class;

        void RegisterComponent<TContract, TComponent>(string name) where TComponent : class;

        void RegisterComponent<TContract, TComponent>(string name, ComponentLifeStyleType lifestyle) where TComponent : class;

        void RegisterComponent(Type componentType);

        void RegisterComponent(Type contractType, Type componentType);

        void RegisterComponent(Type contractType, Type componentType, ComponentLifeStyleType lifestyle);

        void RegisterComponent(string name, Type componentType);

        void RegisterComponent(string name, Type contractType, Type componentType);

        void RegisterComponent(string name, Type contractType, Type componentType, ComponentLifeStyleType lifestyle);

        void RegisterComponentInstance(string name, object instance);

        void RegisterComponentInstance(string name, Type contractType, object instance);

        void RegisterComponentInstance<TContract>(object instance);

        void RegisterComponentInstance<TContract>(string name, object instance);

        void RegisterComponentSettings(string name, IDictionary dependencies);

        void RegisterComponentSettings(Type component, IDictionary dependencies);

        void RegisterComponentSettings<TComponent>(IDictionary dependencies);

        object GetComponent(string name);

        TContract GetComponent<TContract>();

        object GetComponent(Type contractType);

        TContract GetComponent<TContract>(string name);

        object GetComponent(string name, Type contractType);

        string[] GetComponentList<TContract>();

        string[] GetComponentList(Type contractType);

        IDictionary GetComponentSettings(string name);

        IDictionary GetComponentSettings(Type component);

        IDictionary GetComponentSettings<TComponent>();
    }
}
