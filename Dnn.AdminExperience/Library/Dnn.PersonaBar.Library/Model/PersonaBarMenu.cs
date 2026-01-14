// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Library.Model
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;

    [DataContract]
    [Serializable]
    public class PersonaBarMenu
    {
        private IList<MenuItem> allItems;

        [IgnoreDataMember]
        public IList<MenuItem> AllItems
        {
            get
            {
                if (this.allItems == null)
                {
                    this.allItems = new List<MenuItem>();
                    FillAllItems(this.allItems, this.MenuItems);
                }

                return this.allItems;
            }
        }

        [DataMember]
        public IList<MenuItem> MenuItems { get; set; } = new List<MenuItem>();

        private static void FillAllItems(IList<MenuItem> allItems, IList<MenuItem> menuItems)
        {
            foreach (var menu in menuItems)
            {
                allItems.Add(menu);
                FillAllItems(allItems, menu.Children);
            }
        }
    }
}
