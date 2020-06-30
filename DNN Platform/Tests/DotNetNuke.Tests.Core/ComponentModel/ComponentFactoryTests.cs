// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Core.ComponentModel
{
    using System;
    using System.Collections;

    using DotNetNuke.ComponentModel;
    using DotNetNuke.Tests.Utilities.Mocks;
    using Moq;
    using NUnit.Framework;

    /// <summary>
    ///   Summary description for ComponentFactoryTests.
    /// </summary>
    [TestFixture]
    public class ComponentFactoryTests
    {
        public static IContainer CreateMockContainer()
        {
            var mockContainer = new Mock<IContainer>();
            IContainer container = mockContainer.Object;
            ComponentFactory.Container = container;
            return container;
        }

        [TearDown]
        public void TearDown()
        {
            MockComponentProvider.ResetContainer();
        }

        [Test]
        public void ComponentFactory_InstallComponents_Should_Accept_Empty_InstallersArray()
        {
            IContainer container = CreateMockContainer();

            ComponentFactory.InstallComponents(new IComponentInstaller[0] { });
        }

        [Test]
        public void ComponentFactory_InstallComponents_Should_Throw_On_Null_InstallerArray()
        {
            IContainer container = CreateMockContainer();

            Assert.Throws<ArgumentNullException>(() => ComponentFactory.InstallComponents(null));
        }

        [Test]
        public void ComponentFactory_InstallComponents_Should_Throw_On_Null_Installer_In_Array()
        {
            IContainer container = CreateMockContainer();

            var mockInstaller = new Mock<IComponentInstaller>();
            mockInstaller.Setup(i => i.InstallComponents(container));

            Assert.Throws<ArgumentNullException>(() => ComponentFactory.InstallComponents(mockInstaller.Object, null));
        }

        [Test]
        public void DNNPRO_13443_ComponentFactory_DuplicatedInsert()
        {
            // Setup
            var testComp1 = new ArrayList();
            var testComp2 = new ArrayList();

            testComp1.Add("testComp1");
            testComp2.Add("testComp2");

            // Act
            ComponentFactory.RegisterComponentInstance<IList>(testComp1);
            ComponentFactory.RegisterComponentInstance<IList>(testComp2);

            // Assert
            var retreivedComponent = ComponentFactory.GetComponent<IList>();
            Assert.AreEqual(testComp1, retreivedComponent);
        }
    }
}
