// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Entities.Portals
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    using DotNetNuke.Common;
    using DotNetNuke.Internal.SourceGenerators;

    /// <summary>A collection of <see cref="PortalAliasInfo"/> instances, indexed by <see cref="PortalAliasInfo.HttpAlias"/>.</summary>
    [Serializable]
    [DnnDeprecated(9, 7, 2, "use IDictionary<string, IPortalAliasInfo> instead")]
    public partial class PortalAliasCollection : DictionaryBase, IDictionary<string, PortalAliasInfo>
    {
        /// <summary>Gets a value indicating whether the collection contains keys that are not null.</summary>
        public bool HasKeys => this.Dictionary.Keys.Count > 0;

        public ICollection Keys => this.Dictionary.Keys;

        /// <inheritdoc />
        public bool IsReadOnly => this.Dictionary.IsReadOnly;

        /// <inheritdoc />
        ICollection<PortalAliasInfo> IDictionary<string, PortalAliasInfo>.Values
            => this.Dictionary.Values.Cast<PortalAliasInfo>().ToList();

        /// <inheritdoc />
        ICollection<string> IDictionary<string, PortalAliasInfo>.Keys
            => this.Dictionary.Keys.Cast<string>().ToList();

        public ICollection Values => this.Dictionary.Values;

        /// <summary>Gets or sets the value associated with the specified key.</summary>
        /// <param name="key">The HTTP alias.</param>
        public PortalAliasInfo this[string key]
        {
            get => (PortalAliasInfo)this.Dictionary[key];
            set => this.Dictionary[key] = value;
        }

        /// <inheritdoc />
        public bool TryGetValue(string key, out PortalAliasInfo value)
        {
            if (!this.ContainsKey(key))
            {
                value = null;
                return false;
            }

            value = this[key];
            return true;
        }

        /// <inheritdoc cref="IDictionary.Contains"/>
        [DnnDeprecated(9, 7, 2, "use IDictionary<string, IPortalAliasInfo> instead")]
        public partial bool Contains(string key) => this.Dictionary.Contains(key);

        /// <inheritdoc />
        public bool ContainsKey(string key) => this.Dictionary.Contains(key);

        /// <inheritdoc />
        public bool Remove(string key)
        {
            if (!this.ContainsKey(key))
            {
                return false;
            }

            this.Dictionary.Remove(key);
            return true;
        }

        /// <inheritdoc />
        public bool Remove(KeyValuePair<string, PortalAliasInfo> item)
        {
            return this.Contains(item) && this.Remove(item.Key);
        }

        /// <summary>Adds an entry to the collection.</summary>
        /// <param name="key">The <see cref="string"/> to use as the key of the element to add.</param>
        /// <param name="value">The <see cref="PortalAliasInfo"/> to use as the value of the element to add.</param>
        [DnnDeprecated(9, 7, 2, "use IDictionary<string, IPortalAliasInfo> instead")]
        public partial void Add(string key, PortalAliasInfo value) => this.Dictionary.Add(key, value);

        /// <inheritdoc />
        public void Add(KeyValuePair<string, PortalAliasInfo> item) => this.Dictionary.Add(item.Key, item.Value);

        /// <inheritdoc />
        public bool Contains(KeyValuePair<string, PortalAliasInfo> item) =>
            this.TryGetValue(item.Key, out var value) && EqualityComparer<PortalAliasInfo>.Default.Equals(value, item.Value);

        /// <inheritdoc />
        public void CopyTo(KeyValuePair<string, PortalAliasInfo>[] array, int arrayIndex)
        {
            Requires.NotNull(nameof(array), array);
            if (arrayIndex > array.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(arrayIndex), arrayIndex, "arrayIndex must be less than the length of array");
            }

            if (array.Length - arrayIndex < this.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(array), array, "array length must be large enough to hold contents");
            }

            foreach (DictionaryEntry entry in this.Dictionary)
            {
                array[arrayIndex++] = new KeyValuePair<string, PortalAliasInfo>((string)entry.Key, (PortalAliasInfo)entry.Value);
            }
        }

        /// <inheritdoc />
        IEnumerator<KeyValuePair<string, PortalAliasInfo>> IEnumerable<KeyValuePair<string, PortalAliasInfo>>.GetEnumerator()
        {
            return new PortalAliasEnumerator(this.Dictionary.GetEnumerator());
        }

        private class PortalAliasEnumerator(IDictionaryEnumerator enumerator) : IEnumerator<KeyValuePair<string, PortalAliasInfo>>
        {
            /// <inheritdoc />
            public KeyValuePair<string, PortalAliasInfo> Current =>
                new KeyValuePair<string, PortalAliasInfo>((string)enumerator.Key, (PortalAliasInfo)enumerator.Value);

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
