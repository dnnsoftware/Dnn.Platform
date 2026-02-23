// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.Modules.BulkInstall.Components.DataAccess.DataControllers
{
    using System.Collections.Generic;
    using System.Linq;

    using Dnn.Modules.BulkInstall.Components.DataAccess.Models;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Data;

    /// <summary>The data controller for <see cref="IPSpec"/>.</summary>
    /// <param name="hostSettings">The host settings.</param>
    public sealed class IPSpecDataController(IHostSettings hostSettings)
    {
        private readonly IHostSettings hostSettings = hostSettings;

        /// <summary>Creates an <see cref="IPSpec"/>.</summary>
        /// <param name="ipSpec">The IP spec to create.</param>
        public void Create(IPSpec ipSpec)
        {
            using IDataContext context = DataContext.Instance(this.hostSettings);
            var repo = context.GetRepository<IPSpec>();

            repo.Insert(ipSpec);
        }

        /// <summary>Retrieve all the <see cref="IPSpec"/> objects from the database.</summary>
        /// <returns>A sequence of <see cref="IPSpec"/>.</returns>
        public IEnumerable<IPSpec> Get()
        {
            using IDataContext context = DataContext.Instance(this.hostSettings);
            var repo = context.GetRepository<IPSpec>();

            return repo.Get();
        }

        /// <summary>Gets a single <see cref="IPSpec"/> by its ID.</summary>
        /// <param name="ipSpecId">The IP spec ID.</param>
        /// <returns>The <see cref="IPSpec"/> or <see langword="null"/>.</returns>
        public IPSpec Get(int ipSpecId)
        {
            using IDataContext context = DataContext.Instance(this.hostSettings);
            var repo = context.GetRepository<IPSpec>();

            return repo.GetById<int>(ipSpecId);
        }

        /// <summary>Gets a single <see cref="IPSpec"/> by its IP address.</summary>
        /// <param name="address">The IP address.</param>
        /// <returns>The <see cref="IPSpec"/> or <see langword="null"/>.</returns>
        public IPSpec Get(string address)
        {
            using IDataContext context = DataContext.Instance(this.hostSettings);
            return context.ExecuteSingleOrDefault<IPSpec>(
                System.Data.CommandType.StoredProcedure,
                "{databaseOwner}[{objectQualifier}BulkInstall_IPSpecByAddress]",
                address);
        }

        /// <summary>Gets a single <see cref="IPSpec"/> by its name.</summary>
        /// <param name="name">The IP spec name.</param>
        /// <returns>The <see cref="IPSpec"/> or <see langword="null"/>.</returns>
        public IPSpec GetByName(string name)
        {
            using IDataContext context = DataContext.Instance(this.hostSettings);
            var repo = context.GetRepository<IPSpec>();

            return repo.Find("WHERE [Name] = @0", name).FirstOrDefault<IPSpec>();
        }

        /// <summary>Delete the passed <see cref="IPSpec"/>.</summary>
        /// <param name="ipSpec">The IP spec to delete.</param>
        public void Delete(IPSpec ipSpec)
        {
            using IDataContext context = DataContext.Instance(this.hostSettings);
            var repo = context.GetRepository<IPSpec>();

            repo.Delete(ipSpec);
        }
    }
}
