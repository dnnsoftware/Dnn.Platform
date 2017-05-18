#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2017
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