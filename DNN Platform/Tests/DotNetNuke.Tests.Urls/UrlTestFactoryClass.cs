// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Urls
{
    using System;
    using System.Collections;
    using System.Collections.Generic;

    using DotNetNuke.Entities.Urls;
    using NUnit.Framework;

    internal static class UrlTestFactoryClass
    {
        internal static IEnumerable FriendlyUrl_BaseTestCases
        {
            get
            {
                var testData = new ArrayList();

                TestUtil.ReadStream("FriendlyUrl\\BaseTestList", (line, header) => GetTestsWithAliases("FriendlyUrl", line, testData));

                return testData;
            }
        }

        internal static IEnumerable FriendlyUrl_ForceLowerCaseTestCases
        {
            get
            {
                var testData = new ArrayList();

                GetTestsWithAliases("FriendlyUrl", "ForceLowerCase", testData);

                return testData;
            }
        }

        internal static IEnumerable FriendlyUrl_ImprovedTestCases
        {
            get
            {
                var testData = new ArrayList();

                TestUtil.ReadStream("FriendlyUrl\\ImprovedTestList", (line, header) => GetTestsWithAliases("FriendlyUrl", line, testData));

                return testData;
            }
        }

        internal static IEnumerable FriendlyUrl_PageExtensionTestCases
        {
            get
            {
                var testData = new ArrayList();

                GetTestsWithAliases("FriendlyUrl", "PageExtension", testData);

                return testData;
            }
        }

        internal static IEnumerable FriendlyUrl_PrimaryPortalAliasTestCases
        {
            get
            {
                var testData = new ArrayList();

                GetTestsWithAliases("FriendlyUrl", "PrimaryPortalAlias", testData);

                return testData;
            }
        }

        internal static IEnumerable FriendlyUrl_RegexTestCases
        {
            get
            {
                var testData = new ArrayList();

                GetTestsWithAliases("FriendlyUrl", "Regex", testData);

                return testData;
            }
        }

        internal static IEnumerable FriendlyUrl_ReplaceCharsTestCases
        {
            get
            {
                var testData = new ArrayList();

                TestUtil.ReadStream("FriendlyUrl\\ReplaceCharsTestList", (line, header) => GetTestsWithAliases("FriendlyUrl", line, testData));

                return testData;
            }
        }

        internal static IEnumerable FriendlyUrl_ReplaceSpaceTestCases
        {
            get
            {
                var testData = new ArrayList();

                GetTestsWithAliases("FriendlyUrl", "ReplaceSpace", testData);

                return testData;
            }
        }

        internal static IEnumerable FriendlyUrl_SpaceEncodingTestCases
        {
            get
            {
                var testData = new ArrayList();

                GetTestsWithAliases("FriendlyUrl", "SpaceEncoding", testData);

                return testData;
            }
        }

        internal static IEnumerable FriendlyUrl_VanityUrlTestCases
        {
            get
            {
                var testData = new ArrayList();

                GetTestsWithAliases("FriendlyUrl", "VanityUrl", testData);

                return testData;
            }
        }

        internal static IEnumerable UrlRewrite_BasicTestCases
        {
            get
            {
                var testData = new ArrayList();

                TestUtil.ReadStream("UrlRewrite\\TestList", (line, header) => GetTestsWithAliases("UrlRewrite", line, testData));

                return testData;
            }
        }

        internal static IEnumerable UrlRewrite_DeletedTabHandlingTestCases
        {
            get
            {
                var testData = new ArrayList();

                GetTestsWithAliases("UrlRewrite", "DeletedTabHandling", testData);

                return testData;
            }
        }

        internal static IEnumerable UrlRewrite_DoNotRedirect
        {
            get
            {
                var testData = new ArrayList();

                GetTestsWithAliases("UrlRewrite", "DoNotRedirect", testData);

                return testData;
            }
        }

        internal static IEnumerable UrlRewrite_ForceLowerCaseTestCases
        {
            get
            {
                var testData = new ArrayList();

                GetTestsWithAliases("UrlRewrite", "ForceLowerCase", testData);

                return testData;
            }
        }

        internal static IEnumerable UrlRewrite_ForwardExternalUrlTestCases
        {
            get
            {
                var testData = new ArrayList();

                GetTestsWithAliases("UrlRewrite", "ForwardExternalUrl", testData);

                return testData;
            }
        }

        internal static IEnumerable UrlRewrite_JiraTests
        {
            get
            {
                var testData = new ArrayList();

                GetTestsWithAliases("UrlRewrite", "Jira_Tests", testData);

                return testData;
            }
        }

        internal static IEnumerable UrlRewrite_PrimaryPortalAliasTestCases
        {
            get
            {
                var testData = new ArrayList();

                GetTestsWithAliases("UrlRewrite", "PrimaryPortalAlias", testData);
                GetTests(string.Empty, string.Empty, "UrlRewrite", "PrimaryPortalAlias_Default", testData);

                return testData;
            }
        }

        internal static IEnumerable UrlRewrite_RegexTestCases
        {
            get
            {
                var testData = new ArrayList();

                GetTestsWithAliases("UrlRewrite", "Regex", testData);

                return testData;
            }
        }

        internal static IEnumerable UrlRewrite_ReplaceCharsTestCases
        {
            get
            {
                var testData = new ArrayList();

                TestUtil.ReadStream("UrlRewrite\\ReplaceCharsTestList", (line, header) => GetTestsWithAliases("UrlRewrite", line, testData));

                return testData;
            }
        }

        internal static IEnumerable UrlRewrite_ReplaceSpaceTestCases
        {
            get
            {
                var testData = new ArrayList();

                GetTestsWithAliases("UrlRewrite", "ReplaceSpace", testData);

                return testData;
            }
        }

        internal static IEnumerable UrlRewrite_SecureRedirectTestCases
        {
            get
            {
                var testData = new ArrayList();

                GetTestsWithAliases("UrlRewrite", "SecureRedirect", testData);

                return testData;
            }
        }

        internal static IEnumerable UrlRewrite_SiteRootRedirectTestCases
        {
            get
            {
                var testData = new ArrayList();

                GetTestsWithAliases("UrlRewrite", "SiteRootRedirect", testData);

                return testData;
            }
        }

        internal static IEnumerable UrlRewrite_VanityUrlTestCases
        {
            get
            {
                var testData = new ArrayList();

                GetTestsWithAliases("UrlRewrite", "VanityUrl", testData);

                return testData;
            }
        }

        internal static FriendlyUrlSettings GetSettings(string testType, string testName, int portalId)
        {
            return GetSettings(testType, testName, "Settings", portalId);
        }

        internal static FriendlyUrlSettings GetSettings(string testType, string testName, string settingsFile, int portalId)
        {
            var settings = new FriendlyUrlSettings(portalId);

            // Read Test Settings
            TestUtil.ReadStream(string.Format("{0}\\{1}\\{2}", testType, testName, settingsFile), (line, header) =>
            {
                string[] fields = line.Split(',');
                string key = fields[0].Trim();
                string value = fields[1].Trim();

                var type = typeof(FriendlyUrlSettings);
                var property = type.GetProperty(key);
                if (property != null)
                {
                    if (property.PropertyType == typeof(bool))
                    {
                        property.SetValue(settings, Convert.ToBoolean(value), null);
                    }
                    else if (property.PropertyType == typeof(int))
                    {
                        property.SetValue(settings, Convert.ToInt32(value), null);
                    }
                    else if (property.PropertyType.BaseType == typeof(Enum))
                    {
                        property.SetValue(settings, Enum.Parse(property.PropertyType, value), null);
                    }
                    else
                    {
                        property.SetValue(settings, value, null);
                    }
                }
            });
            return settings;
        }

        internal static Dictionary<string, string> GetDictionary(string testType, string testName, string settingsFile)
        {
            var dictionary = new Dictionary<string, string>();

            // Read Test Settings
            TestUtil.ReadStream(string.Format("{0}\\{1}\\{2}", testType, testName, settingsFile), (line, header) =>
            {
                string[] fields = line.Split(',');
                string key = fields[0].Trim();
                string value = fields[1].Trim();

                dictionary.Add(key, value);
            });
            return dictionary;
        }

        private static void GetTestsWithAliases(string testType, string testName, ArrayList testData)
        {
            TestUtil.ReadStream(string.Format("{0}", "Aliases"), (line, header) =>
            {
                string[] fields = line.Split(',');
                GetTests(fields[1].Trim(), fields[0].Trim(), testType, testName, testData);
            });
        }

        private static void GetTests(string testPrefix, string alias, string testType, string testName, ArrayList testData)
        {
            try
            {
                // Read Test File Data
                TestUtil.ReadStream(string.Format("{0}\\{1}\\{2}", testType, testName, "TestFile"), (line, header) =>
                            {
                                var fieldList = new Dictionary<string, string>();
                                fieldList["TestName"] = testName;
                                fieldList["Alias"] = alias;
                                string[] headers = header.Split(',');
                                string[] fields = line.Split(',');
                                for (int i = 0; i < fields.Length; i++)
                                {
                                    string key = headers[i].Trim(new[] { '\t', '"' });
                                    string val = fields[i].Trim(new[] { '\t', '"' });

                                    fieldList[key] = val;
                                }

                                string name = testName + "_";
                                if (!string.IsNullOrEmpty(testPrefix))
                                {
                                    name += testPrefix + "_";
                                }

                                name += fields[0];

                                testData.Add(new TestCaseData(fieldList).SetName(name));
                            });
            }

            // ReSharper disable RedundantCatchClause
#pragma warning disable 168
            catch (Exception exc)
#pragma warning restore 168
            {
                throw;
            }

            // ReSharper restore RedundantCatchClause
        }
    }
}
