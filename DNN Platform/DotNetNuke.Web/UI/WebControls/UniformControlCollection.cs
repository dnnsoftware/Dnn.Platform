// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.UI.WebControls
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Threading;
    using System.Web.UI;

    public sealed class UniformControlCollection<TOwner, TChildren> : IList<TChildren>
        where TOwner : Control
        where TChildren : Control
    {
        private readonly TOwner owner;

        /// <summary>Initializes a new instance of the <see cref="UniformControlCollection{TOwner, TChildren}"/> class.</summary>
        /// <param name="owner">The owner control.</param>
        internal UniformControlCollection(TOwner owner)
        {
            this.owner = owner;
        }

        /// <summary>Gets the number of elements contained in the <see cref="ICollection{T}" />.</summary>
        /// <returns>
        /// The number of elements contained in the <see cref="ICollection{T}" />.
        /// </returns>
        public int Count
        {
            get
            {
                return this.owner.HasControls() ? this.owner.Controls.Count : 0;
            }
        }

        /// <summary>Gets a value indicating whether the <see cref="ICollection{T}" /> is read-only.</summary>
        /// <returns>
        /// true if the <see cref="ICollection{T}" /> is read-only; otherwise, false.
        /// </returns>
        public bool IsReadOnly
        {
            get
            {
                return false;
            }
        }

        /// <summary>Gets or sets the element at the specified index.</summary>
        /// <returns>
        /// The element at the specified index.
        /// </returns>
        /// <param name="index">
        /// The zero-based index of the element to get or set.
        /// </param>
        /// <exception cref="System.ArgumentOutOfRangeException"><paramref name="index" /> is not a valid index in the <see cref="IList{T}" />.
        /// </exception>
        /// <exception cref="System.NotSupportedException">
        /// The property is set and the <see cref="IList{T}" /> is read-only.
        /// </exception>
        public TChildren this[int index]
        {
            get
            {
                return this.owner.Controls[index] as TChildren;
            }

            set
            {
                this.RemoveAt(index);
                this.AddAt(index, value);
            }
        }

        public void AddAt(int index, TChildren childControl)
        {
            this.owner.Controls.AddAt(index, childControl);
        }

        /// <summary>Determines the index of a specific item in the <see cref="IList{T}" />.</summary>
        /// <returns>
        /// The index of <paramref name="item" /> if found in the list; otherwise, -1.
        /// </returns>
        /// <param name="item">
        /// The object to locate in the <see cref="IList{T}" />.
        /// </param>
        public int IndexOf(TChildren item)
        {
            return this.owner.Controls.IndexOf(item);
        }

        /// <summary>Inserts an item to the <see cref="IList{T}" /> at the specified index.</summary>
        /// <param name="index">
        /// The zero-based index at which <paramref name="item" /> should be inserted.
        /// </param>
        /// <param name="item">
        /// The object to insert into the <see cref="IList{T}" />.
        /// </param>
        /// <exception cref="System.ArgumentOutOfRangeException"><paramref name="index" /> is not a valid index in the <see cref="IList{T}" />.
        /// </exception>
        /// <exception cref="System.NotSupportedException">
        /// The <see cref="IList{T}" /> is read-only.
        /// </exception>
        public void Insert(int index, TChildren item)
        {
            this.owner.Controls.AddAt(index, item);
        }

        /// <summary>Removes the <see cref="IList{T}" /> item at the specified index.</summary>
        /// <param name="index">
        /// The zero-based index of the item to remove.
        /// </param>
        /// <exception cref="System.ArgumentOutOfRangeException"><paramref name="index" /> is not a valid index in the <see cref="IList{T}" />.
        /// </exception>
        /// <exception cref="System.NotSupportedException">
        /// The <see cref="IList{T}" /> is read-only.
        /// </exception>
        public void RemoveAt(int index)
        {
            this.owner.Controls.RemoveAt(index);
        }

        /// <summary>Removes the first occurrence of a specific object from the <see cref="ICollection{T}" />.</summary>
        /// <returns>
        /// true if <paramref name="item" /> was successfully removed from the <see cref="ICollection{T}" />; otherwise, false. This method also returns false if <paramref name="item" /> is not found in the original <see cref="ICollection{T}" />.
        /// </returns>
        /// <param name="item">
        /// The object to remove from the <see cref="ICollection{T}" />.
        /// </param>
        /// <exception cref="System.NotSupportedException">
        /// The <see cref="ICollection{T}" /> is read-only.
        /// </exception>
        public bool Remove(TChildren item)
        {
            this.owner.Controls.Remove(item);
            return true;
        }

        /// <summary>Returns an enumerator that iterates through the collection.</summary>
        /// <returns>
        /// A <see cref="IEnumerator{T}" /> that can be used to iterate through the collection.
        /// </returns>
        /// <filterpriority>1.</filterpriority>
        public IEnumerator<TChildren> GetEnumerator()
        {
            var enumerator = this.owner.Controls.GetEnumerator();
            while (enumerator.MoveNext())
            {
                yield return enumerator.Current as TChildren;
            }
        }

        /// <summary>Removes all items from the <see cref="ICollection{T}" />.</summary>
        /// <exception cref="System.NotSupportedException">
        /// The <see cref="ICollection{T}" /> is read-only.
        /// </exception>
        public void Clear()
        {
            if (this.owner.HasControls())
            {
                this.owner.Controls.Clear();
            }
        }

        /// <summary>Adds an item to the <see cref="ICollection{T}" />.</summary>
        /// <param name="item">
        /// The object to add to the <see cref="ICollection{T}" />.
        /// </param>
        /// <exception cref="System.NotSupportedException">
        /// The <see cref="ICollection{T}" /> is read-only.
        /// </exception>
        public void Add(TChildren item)
        {
            this.owner.Controls.Add(item);
        }

        /// <summary>Copies the elements of the <see cref="ICollection{T}" /> to an <see cref="System.Array" />, starting at a particular <see cref="System.Array" /> index.</summary>
        /// <param name="array">
        /// The one-dimensional <see cref="System.Array" /> that is the destination of the elements copied from <see cref="ICollection{T}" />. The <see cref="System.Array" /> must have zero-based indexing.
        /// </param>
        /// <param name="arrayIndex">
        /// The zero-based index in <paramref name="array" /> at which copying begins.
        /// </param>
        /// <exception cref="System.ArgumentNullException"><paramref name="array" /> is null.
        /// </exception>
        /// <exception cref="System.ArgumentOutOfRangeException"><paramref name="arrayIndex" /> is less than 0.
        /// </exception>
        /// <exception cref="System.ArgumentException"><paramref name="array" /> is multidimensional.
        /// -or-
        /// <paramref name="arrayIndex" /> is equal to or greater than the length of <paramref name="array" />.
        /// -or-
        /// The number of elements in the source <see cref="ICollection{T}" /> is greater than the available space from <paramref name="arrayIndex" /> to the end of the destination <paramref name="array" />.
        /// -or-
        /// Type paramref name="T" cannot be cast automatically to the type of the destination <paramref name="array" />.
        /// </exception>
        public void CopyTo(TChildren[] array, int arrayIndex)
        {
            var enumerator = this.GetEnumerator();
            while (enumerator.MoveNext())
            {
                array.SetValue(enumerator.Current, Math.Max(Interlocked.Increment(ref arrayIndex), arrayIndex - 1));
            }
        }

        /// <summary>Determines whether the <see cref="ICollection{T}" /> contains a specific value.</summary>
        /// <returns>
        /// true if <paramref name="item" /> is found in the <see cref="ICollection{T}" />; otherwise, false.
        /// </returns>
        /// <param name="item">
        /// The object to locate in the <see cref="ICollection{T}" />.
        /// </param>
        public bool Contains(TChildren item)
        {
            return this.owner.Controls.Contains(item);
        }

        /// <inheritdoc/>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.EnumerableGetEnumerator();
        }

        /// <summary>Returns an enumerator that iterates through a collection.</summary>
        /// <returns>
        /// An <see cref="System.Collections.IEnumerator" /> object that can be used to iterate through the collection.
        /// </returns>
        /// <filterpriority>2.</filterpriority>
        private IEnumerator EnumerableGetEnumerator()
        {
            return this.owner.Controls.GetEnumerator();
        }
    }
}
