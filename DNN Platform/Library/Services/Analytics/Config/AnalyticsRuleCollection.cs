#region Usings

using System;
using System.Collections;

#endregion

namespace DotNetNuke.Services.Analytics.Config
{
    [Serializable]
    public class AnalyticsRuleCollection : CollectionBase
    {
        public virtual AnalyticsRule this[int index]
        {
            get
            {
                return (AnalyticsRule) base.List[index];
            }
            set
            {
                base.List[index] = value;
            }
        }

        public void Add(AnalyticsRule r)
        {
            InnerList.Add(r);
        }
    }
}
