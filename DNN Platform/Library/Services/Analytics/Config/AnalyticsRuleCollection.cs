// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
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
