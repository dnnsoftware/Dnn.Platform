// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
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
