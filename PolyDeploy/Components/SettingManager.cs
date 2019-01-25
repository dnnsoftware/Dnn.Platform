using Cantarus.Modules.PolyDeploy.Components.DataAccess.DataControllers;
using Cantarus.Modules.PolyDeploy.Components.DataAccess.Models;
using Cantarus.Modules.PolyDeploy.Components.Exceptions;

namespace Cantarus.Modules.PolyDeploy.Components
{
    internal class SettingManager
    {
        private static SettingDataController SettingDC = new SettingDataController();

        public static Setting GetSetting(string group, string key)
        {
            Setting setting = SettingDC.GetSetting(group, key);

            if (setting == null)
            {
                throw SettingNotFoundException.Create(group, key);
            }

            return setting;
        }

        public static void SetSetting(string group, string key, string value)
        {
            // Retrieve setting.
            Setting setting = SettingDC.GetSetting(group, key);

            // Does it already exist?
            if (setting == null)
            {
                // No, create it.
                setting = new Setting()
                {
                    Group = group,
                    Key = key,
                    Value = value
                };

                SettingDC.Create(setting);
            }
            else
            {
                // Yes, Update it.
                setting.Value = value;

                SettingDC.Update(setting);
            }
        }
    }
}
