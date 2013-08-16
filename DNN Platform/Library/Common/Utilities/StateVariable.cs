using System;

namespace DotNetNuke.Common.Utilities
{

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
        /// Initializes a new item variable
        /// </summary>
        /// <param name="key">
        /// The key to use for storing the value in the items
        /// </param>
        protected StateVariable(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentNullException("key");
            }
            _key = key + GetType().FullName;
        }

        /// <summary>
        /// Initializes a new item variable with a initializer
        /// </summary>
        /// <param name="key">The key to use for storing the value in the dictionary</param>
        /// <param name="initializer">A function that is called in order to create a default value per dictionary</param>
        /// <remarks></remarks>
        protected StateVariable(string key, Func<T> initializer) : this(key)
        {
            if (initializer == null)
            {
                throw new ArgumentNullException("initializer");
            }
            this._initializer = initializer;
        }

        private object GetInitializedInternalValue()
        {
            var value = this[_key];
            if (value == null && _initializer != null)
            {
                value = _initializer();
                this[_key] = value;
            }
            return value;
        }

        /// <summary>
        /// Get/sets the value in associated dictionary/map
        /// </summary>
        /// <param name="key">Value key</param>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        protected abstract object this[string key] { get; set; }

        /// <summary>
        /// Removes the value in associated dictionary according
        /// </summary>
        /// <param name="key">Value key</param>
        /// <remarks></remarks>
        protected abstract void Remove(string key);

        /// <summary>
        /// Indicates wether there is a value present or not
        /// </summary>
        public bool HasValue
        {
            get
            {
                return this[_key] != null;
            }
        }

        /// <summary>
        /// Sets or gets the value in the current items
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// If you try to get a value while none is set use <see cref="ValueOrDefault"/> for safe access
        /// </exception>
        public T Value
        {
            get
            {
                var returnedValue = GetInitializedInternalValue();
                if (returnedValue == null)
                {
                    throw new InvalidOperationException("There is no value for the '" + _key + "' key.");
                }
                return (T)returnedValue;
            }
            set
            {
                this[_key] = value;
            }
        }

        /// <summary>
        /// Gets the value in the current items or if none is available <c>default(T)</c>
        /// </summary>
        public T ValueOrDefault
        {
            get
            {
                var returnedValue = GetInitializedInternalValue();
                if (returnedValue == null)
                {
                    return default(T);
                }
                return (T)returnedValue;
            }
        }

        /// <summary>
        /// Clears the value in the current items
        /// </summary>
        public void Clear()
        {
            Remove(_key);
        }

    }

}
