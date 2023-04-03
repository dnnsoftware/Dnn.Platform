using Cantarus.Modules.PolyDeploy.Components.DataAccess.Models;
using DotNetNuke.Data;
using System.Collections.Generic;
using System.Linq;

namespace Cantarus.Modules.PolyDeploy.Components.DataAccess.DataControllers
{
    internal class SettingDataController
    {
        /// <summary>
        /// Retrieve a setting from the database by its group and key.
        /// </summary>
        /// <param name="group"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public Setting GetSetting(string group, string key)
        {
            using (IDataContext context = DataContext.Instance())
            {
                var repo = context.GetRepository<Setting>();

                return repo.Find("WHERE [Group] = @0 AND [Key] = @1", group, key).FirstOrDefault<Setting>();
            }
        }

        /// <summary>
        /// Return all settings belonging to a group.
        /// </summary>
        /// <param name="group"></param>
        /// <returns></returns>
        public IEnumerable<Setting> GetSettings(string group)
        {
            using (IDataContext context = DataContext.Instance())
            {
                var repo = context.GetRepository<Setting>();

                return repo.Find("WHERE [Group] = @0", group);
            }
        }

        /// <summary>
        /// Create a new setting.
        /// </summary>
        /// <param name="setting"></param>
        public void Create(Setting setting)
        {
            using (IDataContext context = DataContext.Instance())
            {
                var repo = context.GetRepository<Setting>();

                repo.Insert(setting);
            }
        }

        /// <summary>
        /// Update an existing setting.
        /// </summary>
        /// <param name="setting"></param>
        public void Update(Setting setting)
        {
            using (IDataContext context = DataContext.Instance())
            {
                var repo = context.GetRepository<Setting>();

                repo.Update(setting);
            }
        }

        /// <summary>
        /// Delete a setting.
        /// </summary>
        /// <param name="setting"></param>
        public void Delete(Setting setting)
        {
            using (IDataContext context = DataContext.Instance())
            {
                var repo = context.GetRepository<Setting>();

                repo.Delete(setting);
            }
        }
    }
}
