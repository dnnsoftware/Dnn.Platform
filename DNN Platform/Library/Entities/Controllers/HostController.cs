#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2013
// by DotNetNuke Corporation
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation 
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and 
// to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions 
// of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
// TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.
#endregion

#region Usings

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.ComponentModel;
using DotNetNuke.Data;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Instrumentation;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Services.Log.EventLog;

using System.Globalization;
using DotNetNuke.Web.Client;
#endregion

namespace DotNetNuke.Entities.Controllers
{
    /// <summary>
	/// HostController provides business layer of host settings.
	/// </summary>
	/// <example>
	/// <code lang="C#">
	/// public static bool CheckUpgrade
    /// {
    ///     get
    ///     {
    ///         return HostController.Instance.GetBoolean("CheckUpgrade", true);
    ///     }
    /// }
	/// </code>
	/// </example>
    public class HostController : ComponentBase<IHostController, HostController>, IHostController
    {
    	private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof (HostController));
        internal HostController()
        {
        }

        #region IHostController Members

		/// <summary>
		/// Gets the setting value by the specific key.
		/// </summary>
		/// <param name="key">The key.</param>
		/// <returns>host setting's value.</returns>
		/// <exception cref="System.ArgumentException">key is empty.</exception>
        public bool GetBoolean(string key)
        {
            return GetBoolean(key, Null.NullBoolean);
        }

		/// <summary>
		/// Gets the setting value by the specific key.
		/// </summary>
		/// <param name="key">The key.</param>
		/// <param name="defaultValue">this value will be return if setting's value is empty.</param>
		/// <returns>host setting's value.</returns>
		/// <exception cref="System.ArgumentException">key is empty.</exception>
        public bool GetBoolean(string key, bool defaultValue)
        {
            Requires.NotNullOrEmpty("key", key);

            bool retValue = false;
            try
            {
                string setting = string.Empty;
                if ((GetSettings().ContainsKey(key)))
                {
                    setting = GetSettings()[key].Value;
                }

                if (string.IsNullOrEmpty(setting))
                {
                    retValue = defaultValue;
                }
                else
                {
                    retValue = (setting.ToUpperInvariant().StartsWith("Y") || setting.ToUpperInvariant() == "TRUE");
                }
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                //we just want to trap the error as we may not be installed so there will be no Settings
            }
            return retValue;
        }

		/// <summary>
		/// Gets the setting value by the specific key.
		/// </summary>
		/// <param name="key">The key.</param>
		/// <returns>host setting's value.</returns>
		/// <exception cref="System.ArgumentException">key is empty.</exception>
        public double GetDouble(string key)
        {
            return GetDouble(key, Null.NullDouble);
        }

		/// <summary>
		/// Gets the setting value by the specific key.
		/// </summary>
		/// <param name="key">The key.</param>
		/// <param name="defaultValue">this value will be return if setting's value is empty.</param>
		/// <returns>host setting's value.</returns>
		/// <exception cref="System.ArgumentException">key is empty.</exception>
        public double GetDouble(string key, double defaultValue)
        {
            Requires.NotNullOrEmpty("key", key);

            double retValue;

            if ((!GetSettings().ContainsKey(key) || !double.TryParse(GetSettings()[key].Value, out retValue)))
            {
                retValue = defaultValue;
            }

            return retValue;
        }

		/// <summary>
		/// Gets the setting value by the specific key.
		/// </summary>
		/// <param name="key">The key.</param>
		/// <returns>host setting's value.</returns>
		/// <exception cref="System.ArgumentException">key is empty.</exception>
        public int GetInteger(string key)
        {
            return GetInteger(key, Null.NullInteger);
        }

		/// <summary>
		/// Gets the setting value by the specific key.
		/// </summary>
		/// <param name="key">The key.</param>
		/// <param name="defaultValue">this value will be return if setting's value is empty.</param>
		/// <returns>host setting's value.</returns>
		/// <exception cref="System.ArgumentException">key is empty.</exception>
        public int GetInteger(string key, int defaultValue)
        {
            Requires.NotNullOrEmpty("key", key);

            int retValue;

            if ((!GetSettings().ContainsKey(key) || !int.TryParse(GetSettings()[key].Value, out retValue)))
            {
                retValue = defaultValue;
            }

            return retValue;
        }

		/// <summary>
		/// Gets all host settings.
		/// </summary>
		/// <returns>host setting.</returns>
        public Dictionary<string, ConfigurationSetting> GetSettings()
        {
            return CBO.GetCachedObject<Dictionary<string, ConfigurationSetting>>(
                                            new CacheItemArgs(DataCache.HostSettingsCacheKey, 
                                                    DataCache.HostSettingsCacheTimeOut, 
                                                    DataCache.HostSettingsCachePriority), 
                                            GetSettingsDictionaryCallBack, 
                                            true);
        }

		/// <summary>
		/// Gets all host settings as dictionary.
		/// </summary>
		/// <returns>host setting's value.</returns>
        public Dictionary<string, string> GetSettingsDictionary()
        {
            return GetSettings().ToDictionary(c => c.Key, c => c.Value.Value);
        }

        /// <summary>
        /// takes in a text value, decrypts it with a FIPS compliant algorithm and returns the value
        /// </summary>
        /// <param name="key">the host setting to read</param>
        /// <param name="passPhrase">the pass phrase used for encryption/decryption</param>
        /// <returns></returns>
        public string GetEncryptedString(string key, string passPhrase)
        {
            Requires.NotNullOrEmpty("key", key);
            Requires.NotNullOrEmpty("passPhrase", passPhrase);
            var cipherText = GetString(key);
            return Security.FIPSCompliant.DecryptAES(cipherText, passPhrase, Entities.Host.Host.GUID);
        }


        /// <summary>
		/// Gets the setting value by the specific key.
		/// </summary>
		/// <param name="key">The key.</param>
		/// <returns>host setting's value.</returns>
		/// <exception cref="System.ArgumentException">key is empty.</exception>
        public string GetString(string key)
        {
            return GetString(key, string.Empty);
        }

		/// <summary>
		/// Gets the setting value by the specific key.
		/// </summary>
		/// <param name="key">The key.</param>
		/// <param name="defaultValue">this value will be return if setting's value is empty.</param>
		/// <returns>host setting's value.</returns>
		/// <exception cref="System.ArgumentException">key is empty.</exception>
        public string GetString(string key, string defaultValue)
        {
            Requires.NotNullOrEmpty("key", key);

            if (!GetSettings().ContainsKey(key) || GetSettings()[key].Value == null)
            {
                return defaultValue;
            }

            return GetSettings()[key].Value;
        }

		/// <summary>
		/// Updates the specified settings.
		/// </summary>
		/// <param name="settings">The settings.</param>
        public void Update(Dictionary<string, string> settings)
        {
            foreach (KeyValuePair<string, string> settingKvp in settings)
            {
                Update(settingKvp.Key, settingKvp.Value, false);
            }

            DataCache.ClearHostCache(false);
        }

		/// <summary>
		/// Updates the specified config.
		/// </summary>
		/// <param name="config">The config.</param>
        public void Update(ConfigurationSetting config)
        {
            Update(config, true);
        }

		/// <summary>
		/// Updates the specified config.
		/// </summary>
		/// <param name="config">The config.</param>
		/// <param name="clearCache">if set to <c>true</c> will clear cache after update the setting.</param>
        public void Update(ConfigurationSetting config, bool clearCache)
        {
            var objEventLog = new EventLogController();
            try
            {
                var settings = GetSettingsFromDatabase();
                if (settings.ContainsKey(config.Key))
                {
                    ConfigurationSetting currentconfig;
                    settings.TryGetValue(config.Key, out currentconfig);
                    if (currentconfig != null && currentconfig.Value != config.Value)
                    {
                        DataProvider.Instance().UpdateHostSetting(config.Key, config.Value, config.IsSecure, UserController.GetCurrentUserInfo().UserID);
                        objEventLog.AddLog(config.Key,
                                           config.Value,
                                           PortalController.GetCurrentPortalSettings(),
                                           UserController.GetCurrentUserInfo().UserID,
                                           EventLogController.EventLogType.HOST_SETTING_UPDATED);
                    }
                }
                else
                {
                    DataProvider.Instance().AddHostSetting(config.Key, config.Value, config.IsSecure, UserController.GetCurrentUserInfo().UserID);
                    objEventLog.AddLog(config.Key,
                                       config.Value,
                                       PortalController.GetCurrentPortalSettings(),
                                       UserController.GetCurrentUserInfo().UserID,
                                       EventLogController.EventLogType.HOST_SETTING_CREATED);
                }
            }
            catch (Exception ex)
            {
                Exceptions.LogException(ex);
            }

            if ((clearCache))
            {
                DataCache.ClearHostCache(false);
            }
        }

		/// <summary>
		/// Updates the specified key.
		/// </summary>
		/// <param name="key">The key.</param>
		/// <param name="value">The value.</param>
		/// <param name="clearCache">if set to <c>true</c> will clear cache after update settings.</param>
        public void Update(string key, string value, bool clearCache)
        {
            Update(new ConfigurationSetting { Key = key, Value = value }, clearCache);
        }

		/// <summary>
		/// Updates the specified key.
		/// </summary>
		/// <param name="key">The key.</param>
		/// <param name="value">The value.</param>
        public void Update(string key, string value)
        {
            Update(key, value, true);
        }

        /// <summary>
        /// takes in a text value, encrypts it with a FIPS compliant algorithm and stores
        /// </summary>
        /// <param name="key">host settings key</param>
        /// <param name="value">host settings value</param>
        /// <param name="passPhrase">pass phrase to allow encryption/decryption</param>
        public void UpdateEncryptedString(string key, string value, string passPhrase)
        {
            Requires.NotNullOrEmpty("key", key);
            Requires.NotNull("value", value);
            Requires.NotNullOrEmpty("passPhrase", passPhrase);
            var cipherText = Security.FIPSCompliant.EncryptAES(value, passPhrase, Entities.Host.Host.GUID);
            Update(key, cipherText);
        }



        public void IncrementCrmVersion(bool includeOverridingPortals)
        {
            var currentVersion = Host.Host.CrmVersion;
            var newVersion = currentVersion + 1;
            Update(ClientResourceSettings.VersionKey, newVersion.ToString(CultureInfo.InvariantCulture), true);

            if (includeOverridingPortals)
            {
                PortalController.IncrementOverridingPortalsCrmVersion();
            }
        }

        #endregion

        private static Dictionary<string, ConfigurationSetting> GetSettingsFromDatabase()
        {
            var dicSettings = new Dictionary<string, ConfigurationSetting>();
            IDataReader dr = null;
            try
            {
                dr = DataProvider.Instance().GetHostSettings();
                while (dr.Read())
                {
                    string key = dr.GetString(0);
                    var config = new ConfigurationSetting
                                     {
                                         Key = key, 
                                         IsSecure = Convert.ToBoolean(dr[2]), 
                                         Value = dr.IsDBNull(1) ? string.Empty : dr.GetString(1)
                                     };

                    dicSettings.Add(key, config);
                }
            }
            catch (Exception ex)
            {
                Exceptions.LogException(ex);
            }
            finally
            {
                CBO.CloseDataReader(dr, true);
            }
            return dicSettings;
        }

        private static object GetSettingsDictionaryCallBack(CacheItemArgs cacheItemArgs)
        {
            return GetSettingsFromDatabase();
        }
    }
}