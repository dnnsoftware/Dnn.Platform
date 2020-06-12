// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

using System;
using System.Web.UI;

namespace DotNetNuke.Web.UI.WebControls
{
    /// <summary>
    /// Restricts the client to add only controls of specific type into the control collection
    /// </summary>
    /// <remarks></remarks>
    public sealed class TypedControlCollection<T> : ControlCollection
        where T : Control
    {
        public TypedControlCollection(Control owner)
            : base(owner)
        {
        }

        public override void Add(Control child)
        {
            if (!(child is T))
            {
                throw new InvalidOperationException("Not supported");
            }

            base.Add(child);
        }

        public override void AddAt(int index, Control child)
        {
            if (!(child is T))
            {
                throw new InvalidOperationException("Not supported");
            }

            base.AddAt(index, child);
        }
    }
}
