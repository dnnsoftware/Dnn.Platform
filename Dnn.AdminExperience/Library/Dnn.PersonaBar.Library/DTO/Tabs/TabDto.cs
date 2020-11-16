// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace Dnn.PersonaBar.Library.DTO.Tabs
{
    using System.Collections.Generic;

    public class TabDto
    {
        public TabDto()
        {
            this.CheckedState = NodeCheckedState.UnChecked;
            this.IsOpen = false;
            this.Selectable = true;
        }

        public string Name { get; set; }

        public string TabId { get; set; }

        public string ImageUrl { get; set; }

        public string Tooltip { get; set; }

        public int ParentTabId { get; set; }

        public bool HasChildren { get; set; }

        public bool IsOpen { get; set; }

        public bool Selectable { get; set; }

        public NodeCheckedState CheckedState { get; set; }

        public IList<TabDto> ChildTabs { get; set; }
    }
}
