// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Common.Utilities
{
    using System;

    /// <summary>
    /// Wrapper class for any object that maps string key onto the object value (like Dictionary).
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <remarks></remarks>
    public abstract class StateVariable<T>
    {
        private readonly string _key;
        private readonly Func<T> _initializer;

        /// <summary>
        /// Initializes a new instance of the <see cref="StateVariable{T}"/> class.
        /// Initializes a new item variable.
        /// </summary>
        /// <param name="key">
        /// The key to use for storing the value in the items.
        /// </param>
        protected StateVariable(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentNullException("key");
            }

            this._key = key + this.GetType().FullName;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StateVariable{T}"/> class.
        /// Initializes a new item variable with a initializer.
        /// </summary>
        /// <param name="key">The key to use for storing the value in the dictionary.</param>
        /// <param name="initializer">A function that is called in order to create a default value per dictionary.</param>
        /// <remarks></remarks>
        protected StateVariable(string key, Func<T> initializer)
            : this(key)
        {
            if (initializer == null)
            {
                throw new ArgumentNullException("initializer");
            }

            this._initializer = initializer;
        }

        /// <summary>
        /// Gets a value indicating whether indicates wether there is a value present or not.
        /// </summary>
        public bool HasValue
        {
            get
            {
                return this[this._key] != null;
            }
        }

        /// <summary>
        /// Gets the value in the current items or if none is available <c>default(T)</c>.
        /// </summary>
        public T ValueOrDefault
        {
            get
            {
                var returnedValue = this.GetInitializedInternalValue();
                if (returnedValue == null)
                {
                    return default(T);
                }

                return (T)returnedValue;
            }
        }

        /// <summary>
        /// Gets or sets or gets the value in the current items.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// If you try to get a value while none is set use <see cref="ValueOrDefault"/> for safe access.
        /// </exception>
        public T Value
        {
            get
            {
                var returnedValue = this.GetInitializedInternalValue();
                if (returnedValue == null)
                {
                    throw new InvalidOperationException("There is no value for the '" + this._key + "' key.");
                }

                return (T)returnedValue;
            }

            set
            {
                this[this._key] = value;
            }
        }

        /// <summary>
        /// Get/sets the value in associated dictionary/map.
        /// </summary>
        /// <param name="key">Value key.</param>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        protected abstract object this[string key] { get; set; }

        /// <summary>
        /// Clears the value in the current items.
        /// </summary>
        public void Clear()
        {
            this.Remove(this._key);
        }

        /// <summary>
        /// Removes the value in associated dictionary according.
        /// </summary>
        /// <param name="key">Value key.</param>
        /// <remarks></remarks>
        protected abstract void Remove(string key);

        private object GetInitializedInternalValue()
        {
            var value = this[this._key];
            if (value == null && this._initializer != null)
            {
                value = this._initializer();
                this[this._key] = value;
            }

            return value;
        }
    }
}
