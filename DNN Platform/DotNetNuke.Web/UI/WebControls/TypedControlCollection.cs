// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.UI.WebControls
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Web.UI;

    /// <summary>Restricts the client to add only controls of specific type into the control collection.</summary>
    /// <typeparam name="T">The type of control.</typeparam>
    public class TypedControlCollection<T> : ControlCollection, IList<T>
        where T : Control
    {
        /// <summary>Initializes a new instance of the <see cref="TypedControlCollection{T}"/> class.</summary>
        /// <param name="owner">The owner control.</param>
        public TypedControlCollection(Control owner)
            : base(owner)
        {
        }

        /// <inheritdoc />
        public new T this[int index]
        {
            get => (T)base[index];
            set => throw new NotSupportedException();
        }

        /// <inheritdoc />
        public int IndexOf(T item)
            => base.IndexOf(item);

        /// <inheritdoc />
        public void Insert(int index, T item)
            => throw new NotSupportedException();

        /// <inheritdoc/>
        public override void Add(Control child)
        {
            if (child is not T item)
            {
                this.ThrowOnInvalidControlType();
            }

            base.Add(child);
        }

        /// <inheritdoc/>
        public override void AddAt(int index, Control child)
        {
            if (child is not T)
            {
                this.ThrowOnInvalidControlType();
            }

            base.AddAt(index, child);
        }

        /// <inheritdoc />
        public void Add(T item)
            => base.Add(item);

        /// <inheritdoc />
        public bool Contains(T item)
            => base.Contains(item);

        /// <inheritdoc />
        public void CopyTo(T[] array, int arrayIndex)
            => base.CopyTo(array, arrayIndex);

        /// <inheritdoc />
        public bool Remove(T item)
        {
            if (!this.Contains(item))
            {
                return false;
            }

            base.Remove(item);
            return true;
        }

        /// <inheritdoc />
        IEnumerator<T> IEnumerable<T>.GetEnumerator()
            => new TypedEnumerator(this.GetEnumerator());

        protected virtual void ThrowOnInvalidControlType()
        {
            throw new InvalidOperationException("Not supported");
        }

        private sealed class TypedEnumerator(IEnumerator enumerator) : IEnumerator<T>
        {
            /// <inheritdoc />
            public T Current => (T)enumerator.Current;

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
