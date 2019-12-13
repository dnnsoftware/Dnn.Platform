﻿// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System.Collections;
using System.Collections.Generic;

using DotNetNuke.ComponentModel;

using NUnit.Framework;

namespace DotNetNuke.Tests.Core.ComponentModel
{
    [TestFixture]
    public class SimpleContainerTests
    {
        [Test]
        //DNN-17622  http://support.dotnetnuke.com/issue/ViewIssue.aspx?id=17622&PROJID=2
        public void GetComponenetListSupportsInterfaces()
        {
            var container = new SimpleContainer();
            container.RegisterComponent<IList>("payload", ComponentLifeStyleType.Singleton);

            var retrieved = container.GetComponentList(typeof(IList));

            CollectionAssert.AreEqual(new List<string> {"payload"}, retrieved);
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
