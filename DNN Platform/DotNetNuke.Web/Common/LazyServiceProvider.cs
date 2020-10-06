// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.Common
{
    using System;
    using System.ComponentModel;

    public class LazyServiceProvider : IServiceProvider, INotifyPropertyChanged
    {
        private IServiceProvider serviceProvider;

        /// <inheritdoc/>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <inheritdoc/>
        public object GetService(Type serviceType)
        {
            if (this.serviceProvider is null)
            {
                throw new Exception("Cannot resolve services until the service provider is built.");
            }

            return this.serviceProvider.GetService(serviceType);
        }

        internal void SetProvider(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(serviceProvider)));
        }
    }
}
