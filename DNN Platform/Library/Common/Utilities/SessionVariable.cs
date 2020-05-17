﻿// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System;
using System.Web;
using System.Web.SessionState;

namespace DotNetNuke.Common.Utilities
{

    /// <summary>
    /// Wrapper class for <see cref="HttpContext.Session"/>
    /// </summary>
    /// <typeparam name="T">The type of the value to be stored</typeparam>
    public class SessionVariable<T> : StateVariable<T>
    {

        /// <summary>
        /// Initializes a new HttpContext.Session item variable
        /// </summary>
        /// <param name="key">
        /// The key to use for storing the value in the items
        /// </param>
        public SessionVariable(string key) : base(key)
        {
        }

        /// <summary>
        /// Initializes a new HttpContext.Session item variable with a initializer
        /// </summary>
        /// <param name="key">The key to use for storing the value in the HttpContext.Current.Session</param>
        /// <param name="initializer">A function that is called in order to create a default value per HttpContext.Current.Session</param>
        /// <remarks></remarks>
        public SessionVariable(string key, Func<T> initializer) : base(key, initializer)
        {
        }

        private static HttpSessionState CurrentItems
        {
            get
            {
                var current = HttpContext.Current;
                if (current == null)
                {
                    throw new InvalidOperationException("No HttpContext is not available.");
                }
                var items = current.Session;
                if (items == null)
                {
                    throw new InvalidOperationException("No Session State available on current HttpContext.");
                }
                return items;
            }
        }

        protected override object this[string key]
        {
            get
            {
                return CurrentItems[key];
            }
            set
            {
                CurrentItems[key] = value;
            }
        }

        protected override void Remove(string key)
        {
            CurrentItems.Remove(key);
        }

    }

}
