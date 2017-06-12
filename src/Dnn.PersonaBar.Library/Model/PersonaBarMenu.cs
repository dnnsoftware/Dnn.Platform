// DotNetNuke® - http://www.dnnsoftware.com
//
// Copyright (c) 2002-2016, DNN Corp.
// All rights reserved.

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
