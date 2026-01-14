// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Tests.Utilities.Fakes
{
    using System;

    using DotNetNuke.Common;
    using DotNetNuke.Common.Extensions;

    using Microsoft.Extensions.DependencyInjection;

    using Moq;

    public class FakeServiceProvider : IServiceProvider, IServiceScopeFactory, IServiceScope
    {
        private readonly IServiceProvider provider;

        private FakeServiceProvider(IServiceProvider provider)
        {
            this.provider = provider;
        }

        public IServiceProvider ServiceProvider => this;

        public static FakeServiceProvider Create() => Create(new ServiceCollection());

        public static FakeServiceProvider Create(Action<IServiceCollection> setupServices)
        {
            var services = new ServiceCollection();
            setupServices(services);
            return Create(services);
        }

        public static FakeServiceProvider Create(IServiceCollection services) => new FakeServiceProvider(services.BuildServiceProvider());

        public static FakeServiceProvider Setup(bool setupHttpContextScope = true) => Setup(Create(), setupHttpContextScope);

        public static FakeServiceProvider Setup(Action<IServiceCollection> setupServices, bool setupHttpContextScope = true) => Setup(Create(setupServices), setupHttpContextScope);

        public static FakeServiceProvider Setup(IServiceCollection services, bool setupHttpContextScope = true) => Setup(Create(services), setupHttpContextScope);

        public static T Setup<T>(T provider, bool setupHttpContextScope = true)
            where T : IServiceProvider
        {
            Globals.DependencyProvider = provider;
            if (setupHttpContextScope)
            {
                HttpContextHelper.RegisterMockHttpContext().Object.SetScope(provider.CreateScope());
            }

            return provider;
        }

        public static void Reset()
        {
            Globals.DependencyProvider = null;
            HttpContextSource.RegisterInstance(null);
        }

        public object GetService(Type serviceType)
        {
            if (serviceType.IsInstanceOfType(this))
            {
                return this;
            }

            var registeredService = this.provider.GetService(serviceType);
            if (registeredService is not null)
            {
                return registeredService;
            }

            if (serviceType.IsInterface || serviceType.IsAbstract)
            {
                var mock = Activator.CreateInstance(typeof(Mock<>).MakeGenericType(serviceType));
                return ((Mock)mock).Object;
            }

            return Activator.CreateInstance(serviceType);
        }

        public IServiceScope CreateScope()
        {
            return this;
        }

        public void Dispose()
        {
            Reset();
            if (this.provider is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }
    }
}
