// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

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
            this.InnerList.Add(a);
        }
    }
}
