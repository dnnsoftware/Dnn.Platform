#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2014
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

using System;
using System.Collections.Generic;
using System.Reflection;

using DotNetNuke.Framework.Internal.Reflection;

namespace DotNetNuke.Framework.Reflections
{
    public class TypeLocator : ITypeLocator, IAssemblyLocator
    {
        private IAssemblyLocator _assemblyLocator;

        internal IAssemblyLocator AssemblyLocator
        {
            get { return _assemblyLocator ?? (_assemblyLocator = this); }
            set { _assemblyLocator = value; }
        }

        public IEnumerable<Type> GetAllMatchingTypes(Predicate<Type> predicate)
        {
            foreach (var assembly in AssemblyLocator.Assemblies)
            {
                Type[] types;
                try
                {
                    types = assembly.GetTypes();
                }
                catch (ReflectionTypeLoadException ex)
                {
                    //some assemblies don't want to be reflected but they still 
                    //expose types in the exception
                    types = ex.Types ?? new Type[0];
                }

                foreach (var type in types)
                {
                    if(type != null)
                    {
                        if(predicate(type))
                        {
                            yield return type;
                        }
                    }
                }
            }
        }

        IEnumerable<IAssembly> IAssemblyLocator.Assemblies
        {
            //this method is not readily testable as the assemblies in the current app domain
            //will vary depending on the test runner and test configuration
            get
            {
                foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    yield return new AssemblyWrapper(assembly);
                }
            }
        }
    }
}