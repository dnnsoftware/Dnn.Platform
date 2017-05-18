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

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using DotNetNuke.Entities;
using DotNetNuke.Tests.Utilities;
using NUnit.Framework;

namespace DotNetNuke.Tests.Core.Entities.Portals
{
    public class PortalSettingsControllerTestFactory
    // ReSharper disable once InconsistentNaming
    {
        internal static string FilePath
        {
            get
            {
                var uri = new System.Uri(Assembly.GetExecutingAssembly().CodeBase);
                string path = Path.GetFullPath(uri.AbsolutePath).Replace("%20", " ");

                return Path.Combine(path.Substring(0, path.IndexOf("bin", System.StringComparison.Ordinal)), "Entities\\Portals\\TestFiles");
            }
        }

        internal static Dictionary<string, string> GetHostSettings()
        {
            var settings = new Dictionary<string, string>();

            //Read Test Settings
            Util.ReadStream(FilePath, "HostSettings", (line, header) =>
                            {
                                string[] fields = line.Split(',');
                                string key = fields[0].Trim();
                                string value = fields[1].Trim();

                                settings.Add(key, value);
                            });
            return settings;
        }

        internal static IEnumerable LoadPortalSettings_Loads_Default_Value
        {
            get
            {
                var testData = new ArrayList();

                Util.ReadStream(FilePath, "DefaultValues", (line, header) =>
                            {
                                var fieldList = new Dictionary<string, string>();
                                var testName = "Load_Default_Value";
                                string[] headers = header.Split(',');
                                string[] fields = line.Split(',');
                                for (int i = 0; i < fields.Length; i++ )
                                {
                                    string key = headers[i].Trim(new[] { '\t', '"' });
                                    string val = fields[i].Trim(new[] {'\t', '"'});

                                    fieldList[key] = val;
                                }

                                testName += "_";
 
                                testName += fields[0];

                                testData.Add(new TestCaseData(fieldList).SetName(testName));
                            });

                return testData;
            }
        }

        internal static IEnumerable LoadPortalSettings_Loads_Setting_Value
        {
            get
            {
                var testData = new ArrayList();

                Util.ReadStream(FilePath, "SettingValues", (line, header) =>
                {
                    var fieldList = new Dictionary<string, string>();
                    var testName = "Load_Setting_Value";
                    string[] headers = header.Split(',');
                    string[] fields = line.Split(',');
                    for (int i = 0; i < fields.Length; i++)
                    {
                        string key = headers[i].Trim(new[] { '\t', '"' });
                        string val = fields[i].Trim(new[] { '\t', '"' });

                        fieldList[key] = val;
                    }

                    testName += "_";

                    testName += fields[0];

                    testData.Add(new TestCaseData(fieldList).SetName(testName));
                });

                return testData;
            }
        }
    }
}
