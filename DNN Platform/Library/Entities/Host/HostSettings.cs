#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2017
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
using System.Collections;

using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Controllers;

#endregion

namespace DotNetNuke.Entities.Host
{
    [Obsolete("Replaced in DotNetNuke 5.0 by Host class because of Namespace clashes with Obsolete Globals.HostSetting Property")]
    [Serializable]
    public class HostSettings : BaseEntityInfo
    {
        public static string GetHostSetting(string key)
        {
            string setting = Null.NullString;
            HostController.Instance.GetSettingsDictionary().TryGetValue(key, out setting);
            return setting;
        }

        [Obsolete("Replaced in DNN 5.0 by Host.GetHostSettingsDictionary")]
    	public static Hashtable GetHostSettings()
	    {
            var h = new Hashtable();
            foreach (var kvp in Host.GetHostSettingsDictionary())
            {
                h.Add(kvp.Key, kvp.Value);
            }

            return h;
    	}

        [Obsolete("Replaced in DNN 5.0 by Host.GetSecureHostSettingsDictionary")]
        public static Hashtable GetSecureHostSettings()
        {
            var h = new Hashtable();
            foreach (var kvp in Host.GetSecureHostSettingsDictionary())
            {
                h.Add(kvp.Key, kvp.Value);
            }

            return h;
        }
    }
}