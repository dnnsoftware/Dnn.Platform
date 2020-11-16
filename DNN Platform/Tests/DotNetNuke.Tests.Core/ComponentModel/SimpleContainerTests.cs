// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Core.ComponentModel
{
    using System.Collections;
    using System.Collections.Generic;

    using DotNetNuke.ComponentModel;
    using NUnit.Framework;

    [TestFixture]
    public class SimpleContainerTests
    {
        [Test]

        // DNN-17622  http://support.dotnetnuke.com/issue/ViewIssue.aspx?id=17622&PROJID=2
        public void GetComponenetListSupportsInterfaces()
        {
            var container = new SimpleContainer();
            container.RegisterComponent<IList>("payload", ComponentLifeStyleType.Singleton);

            var retrieved = container.GetComponentList(typeof(IList));

            CollectionAssert.AreEqual(new List<string> { "payload" }, retrieved);
        }

        [Test]
        public void RegisterComponentInstance_Must_Register_In_ComponentsList()
        {
            var container = new SimpleContainer();
            ComponentFactory.Container = container;

            container.RegisterComponentInstance("test", typeof(IList<string>), new List<string>());

            Assert.Contains("test", ComponentFactory.GetComponents<IList<string>>().Keys);
        }
    }
}
