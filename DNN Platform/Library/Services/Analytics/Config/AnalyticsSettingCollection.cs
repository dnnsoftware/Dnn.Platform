// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
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
