#region Usings

using System.Collections;

#endregion

namespace DotNetNuke.Services.EventQueue
{
    public class EventMessageCollection : CollectionBase
    {
        public virtual EventMessage this[int index]
        {
            get
            {
                return (EventMessage) base.List[index];
            }
            set
            {
                base.List[index] = value;
            }
        }

        public void Add(EventMessage message)
        {
            InnerList.Add(message);
        }
    }
}
