#region Usings

using System;
using System.Collections;

#endregion

namespace DotNetNuke.HttpModules.Config
{
    [Serializable]
    public class AnalyticsEngineCollection : CollectionBase
    {
        public virtual AnalyticsEngine this[int index]
        {
            get
            {
                return (AnalyticsEngine) base.List[index];
            }
            set
            {
                base.List[index] = value;
            }
        }

        public void Add(AnalyticsEngine a)
        {
            InnerList.Add(a);
        }
    }
}
