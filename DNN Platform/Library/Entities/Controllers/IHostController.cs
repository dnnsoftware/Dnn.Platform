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

using System.Collections.Generic;

#endregion

namespace DotNetNuke.Entities.Controllers
{
	/// <summary>
	/// Interface of HostController.
	/// </summary>
	/// <seealso cref="HostController"/>
    public interface IHostController
    {
        bool GetBoolean(string key);

        bool GetBoolean(string key, bool defaultValue);

        double GetDouble(string key, double defaultValue);

        double GetDouble(string key);

        int GetInteger(string key);

        int GetInteger(string key, int defaultValue);

        Dictionary<string, ConfigurationSetting> GetSettings();

        Dictionary<string, string> GetSettingsDictionary();

        string GetEncryptedString(string key,string passPhrase);

        string GetString(string key);

        string GetString(string key, string defaultValue);

        void Update(Dictionary<string, string> settings);

        void Update(ConfigurationSetting config);

        void Update(ConfigurationSetting config, bool clearCache);

        void Update(string key, string value, bool clearCache);

        void Update(string key, string value);

        void UpdateEncryptedString(string key, string value, string passPhrase);

        void IncrementCrmVersion(bool includeOverridingPortals);
    }
}