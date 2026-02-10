// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Data
{
    using System;
    using System.Collections.Generic;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Common;
    using DotNetNuke.ComponentModel;
    using DotNetNuke.Data.PetaPoco;
    using DotNetNuke.Internal.SourceGenerators;

    using global::PetaPoco;

    using Microsoft.Extensions.DependencyInjection;

    /// <summary>A class for creating <see cref="IDataContext"/> instances.</summary>
    public partial class DataContext
    {
        /// <summary>Get an <see cref="IDataContext"/> instance.</summary>
        /// <returns>The data context.</returns>
        [DnnDeprecated(10, 2, 2, "Use the overload taking IHostSettings")]
        public static partial IDataContext Instance()
            => Instance(Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettings>());

        /// <summary>Get an <see cref="IDataContext"/> instance.</summary>
        /// <param name="hostSettings">The host settings.</param>
        /// <returns>The data context.</returns>
        public static IDataContext Instance(IHostSettings hostSettings)
        {
            IDataContext instance = ComponentFactory.GetComponent<IDataContext>();

            if (instance == null)
            {
                var defaultConnectionStringName = DataProvider.Instance().Settings["connectionStringName"];

                instance = new PetaPocoDataContext(hostSettings, defaultConnectionStringName, DataProvider.Instance().ObjectQualifier);
            }

            return instance;
        }

        /// <summary>Get an <see cref="IDataContext"/> instance.</summary>
        /// <param name="connectionStringName">The connection string name.</param>
        /// <returns>The data context.</returns>
        [DnnDeprecated(10, 2, 2, "Use the overload taking IHostSettings")]
        public static partial IDataContext Instance(string connectionStringName)
            => Instance(Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettings>(), connectionStringName);

        /// <summary>Get an <see cref="IDataContext"/> instance.</summary>
        /// <param name="hostSettings">The host settings.</param>
        /// <param name="connectionStringName">The connection string name.</param>
        /// <returns>The data context.</returns>
        public static IDataContext Instance(IHostSettings hostSettings, string connectionStringName)
        {
            IDataContext instance = ComponentFactory.GetComponent<IDataContext>(connectionStringName);

            if (instance == null)
            {
                instance = new PetaPocoDataContext(hostSettings, connectionStringName, DataProvider.Instance().ObjectQualifier);
            }

            return instance;
        }

        /// <summary>Get an <see cref="IDataContext"/> instance.</summary>
        /// <param name="mappers">The mappers.</param>
        /// <returns>The data context.</returns>
        [DnnDeprecated(10, 2, 2, "Use the overload taking IHostSettings")]
#pragma warning disable CS3001 // Argument type is not CLS-compliant
        public static partial IDataContext Instance(Dictionary<Type, IMapper> mappers)
            => Instance(Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettings>(), mappers);

        /// <summary>Get an <see cref="IDataContext"/> instance.</summary>
        /// <param name="hostSettings">The host settings.</param>
        /// <param name="mappers">The mappers.</param>
        /// <returns>The data context.</returns>
        public static IDataContext Instance(IHostSettings hostSettings, Dictionary<Type, IMapper> mappers)
#pragma warning restore CS3001
        {
            IDataContext instance = ComponentFactory.GetComponent<IDataContext>();

            if (instance == null)
            {
                var defaultConnectionStringName = DataProvider.Instance().Settings["connectionStringName"];

                instance = new PetaPocoDataContext(hostSettings, defaultConnectionStringName, DataProvider.Instance().ObjectQualifier, mappers);
            }

            return instance;
        }

        /// <summary>Get an <see cref="IDataContext"/> instance.</summary>
        /// <param name="connectionStringName">The connection string name.</param>
        /// <param name="mappers">The mappers.</param>
        /// <returns>The data context.</returns>
        [DnnDeprecated(10, 2, 2, "Use the overload taking IHostSettings")]
#pragma warning disable CS3001 // Argument type is not CLS-compliant
        public static partial IDataContext Instance(string connectionStringName, Dictionary<Type, IMapper> mappers)
            => Instance(Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettings>(), connectionStringName, mappers);

        /// <summary>Get an <see cref="IDataContext"/> instance.</summary>
        /// <param name="hostSettings">The host settings.</param>
        /// <param name="connectionStringName">The connection string name.</param>
        /// <param name="mappers">The mappers.</param>
        /// <returns>The data context.</returns>
        public static IDataContext Instance(IHostSettings hostSettings, string connectionStringName, Dictionary<Type, IMapper> mappers)
#pragma warning restore CS3001
        {
            IDataContext instance = ComponentFactory.GetComponent<IDataContext>(connectionStringName);

            if (instance == null)
            {
                instance = new PetaPocoDataContext(hostSettings, connectionStringName, DataProvider.Instance().ObjectQualifier, mappers);
            }

            return instance;
        }
    }
}
