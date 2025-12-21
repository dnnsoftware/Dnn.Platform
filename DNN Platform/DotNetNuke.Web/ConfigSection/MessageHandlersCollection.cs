// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.ConfigSection
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Configuration;

    public class MessageHandlersCollection : ConfigurationElementCollection, ICollection<ConfigurationElement>
    {
        /// <inheritdoc />
        bool ICollection<ConfigurationElement>.IsReadOnly => this.IsReadOnly();

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

        /// <inheritdoc />
        public void Add(ConfigurationElement item) => this.BaseAdd(item);

        /// <inheritdoc />
        public void Clear() => this.BaseClear();

        /// <inheritdoc />
        public bool Contains(ConfigurationElement item) => this.BaseGet(this.GetElementKey(item)) is not null;

        /// <inheritdoc />
        public bool Remove(ConfigurationElement item)
        {
            if (!this.Contains(item))
            {
                return false;
            }

            this.BaseRemove(this.GetElementKey(item));
            return true;
        }

        /// <inheritdoc />
        IEnumerator<ConfigurationElement> IEnumerable<ConfigurationElement>.GetEnumerator()
        {
            return new ConfigurationElementEnumerator(this.GetEnumerator());
        }

        /// <inheritdoc/>
        protected override ConfigurationElement CreateNewElement()
        {
            return new MessageHandlerEntry();
        }

        /// <inheritdoc/>
        protected override object GetElementKey(ConfigurationElement element)
        {
            return (element as MessageHandlerEntry ?? new MessageHandlerEntry()).Name;
        }

        private class ConfigurationElementEnumerator(IEnumerator enumerator) : IEnumerator<ConfigurationElement>
        {
            /// <inheritdoc />
            public ConfigurationElement Current => (ConfigurationElement)enumerator.Current;

            /// <inheritdoc />
            object IEnumerator.Current => this.Current;

            /// <inheritdoc />
            public void Dispose() => (enumerator as IDisposable)?.Dispose();

            /// <inheritdoc />
            public bool MoveNext() => enumerator.MoveNext();

            /// <inheritdoc />
            public void Reset() => enumerator.Reset();
        }
    }
}
