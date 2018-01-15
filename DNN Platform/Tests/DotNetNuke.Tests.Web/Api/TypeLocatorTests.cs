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
using System;
using System.Linq;

using DotNetNuke.Framework.Internal.Reflection;
using DotNetNuke.Framework.Reflections;

using Moq;
using NUnit.Framework;

namespace DotNetNuke.Tests.Web.Api
{
    public class TypeLocatorTests
    {
        [Test]
        public void LocateAllMatchingTypes()
        {
            var assembly = new Mock<IAssembly>();
            assembly.Setup(x => x.GetTypes()).Returns(new[] {typeof(TypeLocatorTests), typeof(ServiceRoutingManagerTests)});
            var assemblyLocator = new Mock<IAssemblyLocator>();
            assemblyLocator.Setup(x => x.Assemblies).Returns(new[] {assembly.Object});

            var typeLocator = new TypeLocator {AssemblyLocator = assemblyLocator.Object};

            var types = typeLocator.GetAllMatchingTypes(x => true).ToList();

            CollectionAssert.AreEquivalent(new[]{typeof(TypeLocatorTests), typeof(ServiceRoutingManagerTests)}, types);
            assembly.Verify(x => x.GetTypes(), Times.Once());
            assemblyLocator.Verify(x => x.Assemblies, Times.Once());
        }
    }
}