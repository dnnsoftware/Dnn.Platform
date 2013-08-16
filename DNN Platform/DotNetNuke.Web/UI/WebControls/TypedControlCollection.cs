using System;
using System.Web.UI;

namespace DotNetNuke.Web.UI.WebControls
{
    /// <summary>
    /// Restricts the client to add only controls of specific type into the control collection
    /// </summary>
    /// <remarks></remarks>
    public sealed class TypedControlCollection<T> : ControlCollection where T : Control
    {

        public TypedControlCollection(Control owner) : base(owner)
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
