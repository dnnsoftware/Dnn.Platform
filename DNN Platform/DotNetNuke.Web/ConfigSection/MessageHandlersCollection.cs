// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.ConfigSection
{
    using System.Configuration;

    public class MessageHandlersCollection : ConfigurationElementCollection
    {
        public MessageHandlerEntry this[int index]
        {
            get
            {
                return this.BaseGet(index) as MessageHandlerEntry;
            }

            set
            {
                if (this.BaseGet(index) != null)
                {
                    this.BaseRemoveAt(index);
                }

                this.BaseAdd(index, value);
            }
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new MessageHandlerEntry();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return (element as MessageHandlerEntry ?? new MessageHandlerEntry()).Name;
        }
    }
}
