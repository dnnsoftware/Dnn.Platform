// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.HttpModules.Config
{
    using System;
    using System.Collections;

    /// <summary>A collection of <see cref="AnalyticsEngine"/> instances.</summary>
    [Serializable]
    public class AnalyticsEngineCollection : CollectionBase
    {
        /// <summary>Gets or sets the element at the specified index.</summary>
        /// <param name="index">The zero-based index of the element to get or set.</param>
        /// <returns>The element at the specified index.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is less than zero. -or- <paramref name="index"/> is equal to or greater than <see cref="CollectionBase.Count"/>.</exception>
        public virtual AnalyticsEngine this[int index]
        {
            get => (AnalyticsEngine)this.List[index];
            set => this.List[index] = value;
        }

        /// <inheritdoc cref="ArrayList.Add"/>
        public void Add(AnalyticsEngine a)
        {
            this.InnerList.Add(a);
        }
    }
}
