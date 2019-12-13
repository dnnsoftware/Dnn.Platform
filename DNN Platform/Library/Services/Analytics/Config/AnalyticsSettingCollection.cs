#region Usings

using System;
using System.Collections;

#endregion

namespace DotNetNuke.Services.Analytics.Config
{
    [Serializable]
    public class AnalyticsSettingCollection : CollectionBase
    {
        public virtual AnalyticsSetting this[int index]
        {
            get
            {
                return (AnalyticsSetting) base.List[index];
            }
            set
            {
                base.List[index] = value;
            }
        }

        public void Add(AnalyticsSetting r)
        {
            InnerList.Add(r);
        }
    }
}
