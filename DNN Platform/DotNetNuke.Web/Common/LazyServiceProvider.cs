﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNetNuke.Web.Common
{
    public class LazyServiceProvider : IServiceProvider
    {
        private IServiceProvider _serviceProvider;

        public object GetService(Type serviceType)
        {
            if (_serviceProvider is null)
                throw new Exception("Cannot resolve services until the service provider is built.");

            return _serviceProvider.GetService(serviceType);
        }

        internal void SetProvider(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }
    }
}
