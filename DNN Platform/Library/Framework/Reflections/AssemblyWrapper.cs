// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System;
using System.Reflection;

using DotNetNuke.Framework.Internal.Reflection;

namespace DotNetNuke.Framework.Reflections
{
    public class AssemblyWrapper : IAssembly
    {
        private readonly Assembly _assembly;

        public AssemblyWrapper(Assembly assembly)
        {
            _assembly = assembly;
        }

        public Type[] GetTypes()
        {
            return _assembly.GetTypes();
        }
    }
}
