// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Framework.Reflections
{
    using System;
    using System.Reflection;

    using DotNetNuke.Framework.Internal.Reflection;

    public class AssemblyWrapper : IAssembly
    {
        private readonly Assembly _assembly;

        public AssemblyWrapper(Assembly assembly)
        {
            this._assembly = assembly;
        }

        public Type[] GetTypes()
        {
            return this._assembly.GetTypes();
        }
    }
}
