// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Dnn.PersonaBar.Library.Model
{
    [DataContract]
    [Serializable]
    public class PersonaBarMenu
    {
        private IList<MenuItem> _allItems;

        [DataMember]
        public IList<MenuItem> MenuItems { get; set; } = new List<MenuItem>();

        [IgnoreDataMember]
        public IList<MenuItem> AllItems
        {
            get
            {
                if (_allItems == null)
                {
                    _allItems = new List<MenuItem>();
                    FillAllItems(_allItems, MenuItems);
                }

                return _allItems;
            }
        }

        private void FillAllItems(IList<MenuItem> allItems, IList<MenuItem> menuItems)
        {
            foreach (var menu in menuItems)
            {
                allItems.Add(menu);
                FillAllItems(allItems, menu.Children);
            }
        }
    }
}
