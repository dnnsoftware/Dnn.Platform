﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Web.UI.WebControls
{
    using System;
    using System.Web.UI;

    public class DnnTabCollection : ControlCollection
    {
        public DnnTabCollection(Control owner)
            : base(owner)
        {
        }

        public new DnnTab this[int index]
        {
            get
            {
                return (DnnTab)base[index];
            }
        }

        public override void Add(Control child)
        {
            if (child is DnnTab)
            {
                base.Add(child);
            }
            else
            {
                throw new ArgumentException("DnnTabCollection must contain controls of type DnnTab");
            }
        }

        public override void AddAt(int index, Control child)
        {
            if (child is DnnTab)
            {
                base.AddAt(index, child);
            }
            else
            {
                throw new ArgumentException("DnnTabCollection must contain controls of type DnnTab");
            }
        }
    }
}
