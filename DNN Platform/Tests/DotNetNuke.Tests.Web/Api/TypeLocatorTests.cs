// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Web.Api
{
    using System;
    using System.Linq;

    using DotNetNuke.Framework.Internal.Reflection;
    using DotNetNuke.Framework.Reflections;
    using Moq;
    using NUnit.Framework;

    public class TypeLocatorTests
    {
        [Test]
        public void LocateAllMatchingTypes()
        {
            var assembly = new Mock<IAssembly>();
            assembly.Setup(x => x.GetTypes()).Returns(new[] { typeof(TypeLocatorTests), typeof(ServiceRoutingManagerTests) });
            var assemblyLocator = new Mock<IAssemblyLocator>();
            assemblyLocator.Setup(x => x.Assemblies).Returns(new[] { assembly.Object });

            var typeLocator = new TypeLocator { AssemblyLocator = assemblyLocator.Object };

            var types = typeLocator.GetAllMatchingTypes(x => true).ToList();

            CollectionAssert.AreEquivalent(new[] { typeof(TypeLocatorTests), typeof(ServiceRoutingManagerTests) }, types);
            assembly.Verify(x => x.GetTypes(), Times.Once());
            assemblyLocator.Verify(x => x.Assemblies, Times.Once());
        }
    }
}
