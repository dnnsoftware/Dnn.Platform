#region Usings

using System;
using System.Web.UI;


#endregion

namespace DotNetNuke.Web.UI.WebControls
{
    public class DnnTabCollection : ControlCollection
    {
        public DnnTabCollection(Control owner) : base(owner)
        {
        }

        public new DnnTab this[int index]
        {
            get
            {
                return (DnnTab) base[index];
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
