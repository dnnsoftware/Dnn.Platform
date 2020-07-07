// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.HttpModules.Config
{
    using System;
    using System.Collections;

    [Serializable]
    public class AnalyticsEngineCollection : CollectionBase
    {
        public virtual AnalyticsEngine this[int index]
        {
            get
            {
                return (AnalyticsEngine)this.List[index];
            }

            set
            {
                this.List[index] = value;
            }
        }

        public void Add(AnalyticsEngine a)
        {
            this.InnerList.Add(a);
        }
    }
}
