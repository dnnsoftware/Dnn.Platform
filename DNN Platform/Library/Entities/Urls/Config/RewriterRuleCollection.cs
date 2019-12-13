// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
#region Usings

using System;
using System.Collections;

#endregion

namespace DotNetNuke.Entities.Urls.Config
{
    [Serializable]
    public class RewriterRuleCollection : CollectionBase
    {
        public virtual RewriterRule this[int index]
        {
            get
            {
                return (RewriterRule) List[index];
            }
            set
            {
                List[index] = value;
            }
        }

        public void Add(RewriterRule r)
        {
            InnerList.Add(r);
        }
    }
}
