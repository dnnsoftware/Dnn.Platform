// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System;
using System.Web;

namespace DotNetNuke.Common
{
    /// <summary>
    /// A unit testable alternative to HttpContext.Current
    /// </summary>
    public class HttpContextSource
    {
        private static HttpContextBase _fakeContext;

        /// <summary>
        /// Gets the current HttpContext
        /// </summary>
        public static HttpContextBase Current
        {
            get
            {
                if (_fakeContext != null)
                {
                    return _fakeContext;
                }

                if (HttpContext.Current != null)
                {
                    return new HttpContextWrapper(HttpContext.Current);
                }
                return null;
            }
        }

        /// <summary>
        /// Injects a fake/mock context for unit testing
        /// </summary>
        /// <param name="instance">The fake context to inject</param>
        public static void RegisterInstance(HttpContextBase instance)
        {
            _fakeContext = instance;
        }
    }
}
