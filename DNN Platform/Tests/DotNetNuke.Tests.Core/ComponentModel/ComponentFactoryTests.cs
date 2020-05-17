﻿// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System;
using System.Collections;

using DotNetNuke.ComponentModel;
using DotNetNuke.Tests.Utilities.Mocks;

using Moq;

using NUnit.Framework;

namespace DotNetNuke.Tests.Core.ComponentModel
{
    /// <summary>
    ///   Summary description for ComponentFactoryTests
    /// </summary>
    [TestFixture]
    public class ComponentFactoryTests
    {
        #region InstallComponents

        [TearDown]
        public void TearDown()
        {
            MockComponentProvider.ResetContainer();
        }

        [Test]
        public void ComponentFactory_InstallComponents_Should_Accept_Empty_InstallersArray()
        {
            IContainer container = CreateMockContainer();

            ComponentFactory.InstallComponents(new IComponentInstaller[0] {});
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
            //Setup
            var testComp1 = new ArrayList();
            var testComp2 = new ArrayList();

            testComp1.Add("testComp1");
            testComp2.Add("testComp2");

            //Act
            ComponentFactory.RegisterComponentInstance<IList>(testComp1);
            ComponentFactory.RegisterComponentInstance<IList>(testComp2);

            //Assert
            var retreivedComponent = ComponentFactory.GetComponent<IList>();
            Assert.AreEqual(testComp1, retreivedComponent);
        }

        #endregion

        public static IContainer CreateMockContainer()
        {
            var mockContainer = new Mock<IContainer>();
            IContainer container = mockContainer.Object;
            ComponentFactory.Container = container;
            return container;
        }
    }
}
