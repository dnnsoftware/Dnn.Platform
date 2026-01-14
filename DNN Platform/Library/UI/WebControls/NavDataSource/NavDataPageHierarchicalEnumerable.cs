// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.UI.WebControls
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Web.UI;

    /// <summary>A collection of PageHierarchyData objects.</summary>
    public class NavDataPageHierarchicalEnumerable : ArrayList, IHierarchicalEnumerable, IList<NavDataPageHierarchyData>
    {
        /// <inheritdoc />
        NavDataPageHierarchyData IList<NavDataPageHierarchyData>.this[int index]
        {
            get => (NavDataPageHierarchyData)this[index];
            set => this[index] = value;
        }

        /// <summary>Handles enumeration.</summary>
        /// <param name="enumeratedItem">THe <see cref="IHierarchyData"/> item.</param>
        /// <returns><paramref name="enumeratedItem"/>.</returns>
        public virtual IHierarchyData GetHierarchyData(object enumeratedItem)
        {
            return (IHierarchyData)enumeratedItem;
        }

        /// <inheritdoc />
        public void Add(NavDataPageHierarchyData item) => base.Add(item);

        /// <inheritdoc />
        public bool Contains(NavDataPageHierarchyData item) => base.Contains(item);

        /// <inheritdoc />
        public void CopyTo(NavDataPageHierarchyData[] array, int arrayIndex) => base.CopyTo(array, arrayIndex);

        /// <inheritdoc />
        public bool Remove(NavDataPageHierarchyData item)
        {
            if (!this.Contains(item))
            {
                return false;
            }

            base.Remove(item);
            return true;
        }

        /// <inheritdoc />
        public int IndexOf(NavDataPageHierarchyData item) => base.IndexOf(item);

        /// <inheritdoc />
        public void Insert(int index, NavDataPageHierarchyData item) => base.Insert(index, item);

        /// <inheritdoc />
        IEnumerator<NavDataPageHierarchyData> IEnumerable<NavDataPageHierarchyData>.GetEnumerator()
        {
            return new NavDataPageHierarchyDataEnumerator(this.GetEnumerator());
        }

        private class NavDataPageHierarchyDataEnumerator(IEnumerator enumerator) : IEnumerator<NavDataPageHierarchyData>
        {
            /// <inheritdoc />
            public NavDataPageHierarchyData Current => (NavDataPageHierarchyData)enumerator.Current;

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
