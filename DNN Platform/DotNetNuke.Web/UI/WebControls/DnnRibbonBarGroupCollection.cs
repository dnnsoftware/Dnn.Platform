// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
#region Usings

using System;
using System.Web.UI;


#endregion

namespace DotNetNuke.Web.UI.WebControls
{
    public class DnnRibbonBarGroupCollection : ControlCollection
    {
        public DnnRibbonBarGroupCollection(Control owner) : base(owner)
        {
        }

        public new DnnRibbonBarGroup this[int index]
        {
            get
            {
                return (DnnRibbonBarGroup) base[index];
            }
        }

        public override void Add(Control child)
        {
            if (child is DnnRibbonBarGroup)
            {
                base.Add(child);
            }
            else
            {
                throw new ArgumentException("DnnRibbonBarGroupCollection must contain controls of type DnnRibbonBarGroup");
            }
        }

        public override void AddAt(int index, Control child)
        {
            if (child is DnnRibbonBarGroup)
            {
                base.AddAt(index, child);
            }
            else
            {
                throw new ArgumentException("DnnRibbonBarGroupCollection must contain controls of type DnnRibbonBarGroup");
            }
        }
    }
}
