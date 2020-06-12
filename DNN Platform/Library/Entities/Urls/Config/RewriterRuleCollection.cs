

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
using System;
using System.Collections;

namespace DotNetNuke.Entities.Urls.Config
{
    [Serializable]
    public class RewriterRuleCollection : CollectionBase
    {
        public virtual RewriterRule this[int index]
        {
            get
            {
                return (RewriterRule)this.List[index];
            }

            set
            {
                this.List[index] = value;
            }
        }

        public void Add(RewriterRule r)
        {
            this.InnerList.Add(r);
        }
    }
}
