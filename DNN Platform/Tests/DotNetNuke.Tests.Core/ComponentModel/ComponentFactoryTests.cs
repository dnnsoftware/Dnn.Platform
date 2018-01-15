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