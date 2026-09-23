// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Extensions.Components.BulkInstall.DataAccess.DataControllers
{
    using System.Collections.Generic;
    using System.Linq;

    using Dnn.PersonaBar.Extensions.Components.BulkInstall.DataAccess.Models;
    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Data;

    /// <summary>The data controller for <see cref="Setting"/>.</summary>
    /// <param name="hostSettings">The host settings.</param>
    public sealed class SettingDataController(IHostSettings hostSettings)
    {
        private readonly IHostSettings hostSettings = hostSettings;

        /// <summary>Retrieve a <see cref="Setting"/> from the database by its group and key.</summary>
        /// <param name="group">The group.</param>
        /// <param name="key">The key.</param>
        /// <returns>The setting or <see langword="null"/>.</returns>
        public Setting GetSetting(string group, string key)
        {
            using IDataContext context = DataContext.Instance(this.hostSettings);
            var repo = context.GetRepository<Setting>();

            return repo.Find("WHERE [Group] = @0 AND [Key] = @1", group, key).FirstOrDefault<Setting>();
        }

        /// <summary>Return all <see cref="Setting"/> rows belonging to a group.</summary>
        /// <param name="group">The group.</param>
        /// <returns>A sequence of <see cref="Setting"/>.</returns>
        public IEnumerable<Setting> GetSettings(string group)
        {
            using IDataContext context = DataContext.Instance(this.hostSettings);
            var repo = context.GetRepository<Setting>();

            return repo.Find("WHERE [Group] = @0", group);
        }

        /// <summary>Create a new <see cref="Setting"/>.</summary>
        /// <param name="setting">The setting to create.</param>
        public void Create(Setting setting)
        {
            using IDataContext context = DataContext.Instance(this.hostSettings);
            var repo = context.GetRepository<Setting>();

            repo.Insert(setting);
        }

        /// <summary>Update an existing <see cref="Setting"/>.</summary>
        /// <param name="setting">The new setting information.</param>
        public void Update(Setting setting)
        {
            using IDataContext context = DataContext.Instance(this.hostSettings);
            var repo = context.GetRepository<Setting>();

            repo.Update(setting);
        }

        /// <summary>Delete a <see cref="Setting"/>.</summary>
        /// <param name="setting">The setting to delete.</param>
        public void Delete(Setting setting)
        {
            using IDataContext context = DataContext.Instance(this.hostSettings);
            var repo = context.GetRepository<Setting>();

            repo.Delete(setting);
        }
    }
}
