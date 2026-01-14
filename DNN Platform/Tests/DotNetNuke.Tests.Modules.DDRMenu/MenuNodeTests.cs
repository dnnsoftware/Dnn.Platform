// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Modules.DDRMenu
{
    using System.Collections.Generic;
    using System.Linq;
    using DotNetNuke.Web.DDRMenu;
    using NUnit.Framework;

    [TestFixture]
    public class MenuNodeTests
    {
        [Test]
        public void FindAllByNameOrId_WhenDuplicatePageNamesExist_ReturnsAllInstances()
        {
            // arrange
            const string dupeName = "DDRMenu Test";

            var root = CreateMenuNode("root", -1);
            var home = CreateMenuNode("Home", 1, parent: root);
            var dupe1 = CreateMenuNode(dupeName, 2, parent: home);
            var dupe2 = CreateMenuNode(dupeName, 3, parent: root);
            var other1 = CreateMenuNode("Some Other Page", 4, parent: root);
            var other2 = CreateMenuNode("And Yet Another Page", 5, parent: root);

            // act
            var found = root.FindAllByNameOrId(dupeName).ToArray();

            // assert
            Assert.That(found, Is.Not.Null);
            Assert.That(found.Length, Is.EqualTo(2));
            Assert.That(found[0], Is.SameAs(dupe1));
            Assert.That(found[1], Is.SameAs(dupe2));
        }

        private static MenuNode CreateMenuNode(string text, int tabId, MenuNode parent = null)
        {
            var node = new MenuNode
            {
                Parent = parent,
                TabId = tabId,
                Text = text,
            };

            if (parent != null)
            {
                if (parent.Children == null)
                {
                    parent.Children = new List<MenuNode>();
                }

                parent.Children.Add(node);
            }

            return node;
        }
    }
}
