// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Core.Entities.Portals
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;

    using DotNetNuke.Entities;
    using DotNetNuke.Tests.Utilities;
    using NUnit.Framework;

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

        internal static Dictionary<string, string> GetHostSettings()
        {
            var settings = new Dictionary<string, string>();

            // Read Test Settings
            Util.ReadStream(FilePath, "HostSettings", (line, header) =>
                            {
                                string[] fields = line.Split(',');
                                string key = fields[0].Trim();
                                string value = fields[1].Trim();

                                settings.Add(key, value);
                            });
            return settings;
        }
    }
}
