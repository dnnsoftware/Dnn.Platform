// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
#region Usings

using System.Collections.Generic;

#endregion

namespace Dnn.PersonaBar.Library.DTO.Tabs
{
    public class TabDto
    {
        public TabDto()
        {
            CheckedState= NodeCheckedState.UnChecked;
            IsOpen = false;
            Selectable = true;
        }
        public string Name { get; set; }

        public string TabId { get; set; }

        public string ImageUrl { get; set; }

        public string Tooltip { get; set; }

        public int ParentTabId { get; set; }

        public  bool HasChildren { get; set; }

        public bool IsOpen { get; set; }

        public bool Selectable { get; set; }

        public NodeCheckedState CheckedState { get; set; }

        public IList<TabDto> ChildTabs { get; set; }
    }
}
