// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
#region Usings

using System.Collections;

#endregion

namespace DotNetNuke.Services.EventQueue
{
    public class EventMessageCollection : CollectionBase
    {
        public virtual EventMessage this[int index]
        {
            get
            {
                return (EventMessage) base.List[index];
            }
            set
            {
                base.List[index] = value;
            }
        }

        public void Add(EventMessage message)
        {
            InnerList.Add(message);
        }
    }
}
