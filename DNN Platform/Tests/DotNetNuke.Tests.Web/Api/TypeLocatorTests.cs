// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
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
