#region Copyright
// 
// DotNetNuke® - http://www.dnnsoftware.com
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
using System.Collections.Generic;
using System.Reflection;
using Dnn.ExportImport.Components.Entities;
using Dnn.ExportImport.Components.Services;
using DotNetNuke.Entities.Profile;
using DotNetNuke.Entities.Users;
using DotNetNuke.Framework.Reflections;
using DotNetNuke.Instrumentation;
using DotNetNuke.Security.Roles;
using System.Linq;
using System.Xml.Linq;
using DotNetNuke.Entities.Portals.Internal;
using System.Xml;
using System.Text;
using System.IO;
using System.Threading;
using Dnn.ExportImport.Components.Controllers;
using DotNetNuke.Entities.Modules.Definitions;

namespace Dnn.ExportImport.Components.Common
{
    public static class Util
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Util));

        public static IEnumerable<BasePortableService> GetPortableImplementors()
        {
            var typeLocator = new TypeLocator();
            var types = typeLocator.GetAllMatchingTypes(
                t => t != null && t.IsClass && !t.IsAbstract && t.IsVisible &&
                     typeof(BasePortableService).IsAssignableFrom(t));

            foreach (var type in types)
            {
                BasePortableService portable2Type;
                try
                {
                    portable2Type = Activator.CreateInstance(type) as BasePortableService;
                }
                catch (Exception e)
                {
                    Logger.ErrorFormat("Unable to create {0} while calling BasePortableService implementors. {1}",
                        type.FullName, e.Message);
                    portable2Type = null;
                }

                if (portable2Type != null)
                {
                    yield return portable2Type;
                }
            }
        }

        public static string FormatSize(long bytes, byte decimals=1)
        {
            const long kb = 1024;
            const long mb = kb * kb;
            const long gb = mb * kb;

            if (bytes < kb) return bytes + " B";
            if (bytes < mb) return (1.0 * bytes / kb).ToString("F" + decimals) + " KB";
            if (bytes < gb) return (1.0 * bytes / mb).ToString("F" + decimals) + " MB";
            return (1.0 * bytes / gb).ToString("F" + decimals) + " GB";
        }

        public static string GetExpImpJobCacheKey(ExportImportJob job)
        {
            return string.Join(":", "ExpImpKey", job.PortalId.ToString(), job.JobId.ToString());
        }

        public static int GetUserIdByName(ExportImportJob importJob, int? exportedUserId, string exportUsername)
        {
            if (!exportedUserId.HasValue || exportedUserId <= 0)
                return -1;

            if (exportedUserId == 1)
                return 1; // default HOST user

            if (string.IsNullOrEmpty(exportUsername))
                return -1;

            var user = UserController.GetUserByName(importJob.PortalId, exportUsername);
            return user.UserID < 0 ? importJob.CreatedByUserId : user.UserID;
        }

        public static int? GetRoleIdByName(int portalId, string exportRolename)
        {
            if (string.IsNullOrEmpty(exportRolename)) return -1;

            var role = RoleController.Instance.GetRoleByName(portalId, exportRolename);
            return role?.RoleID;
        }

        public static int? GeModuleDefIdByFriendltName(string friendlyName)
        {
            if (string.IsNullOrEmpty(friendlyName)) return null;

            var moduleDefInfo = ModuleDefinitionController.GetModuleDefinitionByFriendlyName(friendlyName);
            return moduleDefInfo?.ModuleDefID;
        }

        public static int? GePermissionIdByName(string permissionCode, string permissionKey, string permissionName)
        {
            if (string.IsNullOrEmpty(permissionName) ||
                string.IsNullOrEmpty(permissionKey) ||
                string.IsNullOrEmpty(permissionName))
                return null;

            var permission = EntitiesController.Instance.GetPermissionInfo(permissionName, permissionKey, permissionName);
            return permission?.PermissionID;
        }

        public static int? GetProfilePropertyId(int portalId, int? exportedProfilePropertyId,
            string exportProfilePropertyname)
        {
            if (!exportedProfilePropertyId.HasValue || exportedProfilePropertyId <= 0)
                return -1;

            var property = ProfileController.GetPropertyDefinitionByName(portalId, exportProfilePropertyname);
            return property?.PropertyDefinitionId;
        }

        public static int CalculateTotalPages(int totalRecords, int pageSize)
        {
            return totalRecords % pageSize == 0 ? totalRecords / pageSize : totalRecords / pageSize + 1;
        }

        /// <summary>
        /// Write dictionary items to an xml file
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="itemsToWrite"></param>
        /// <param name="rootTag">Root tag to create in xml file.</param>
        public static void WriteXml(string filePath, Dictionary<string, string> itemsToWrite,
            string rootTag)
        {
            var xmlSettings = new XmlWriterSettings
            {
                ConformanceLevel = ConformanceLevel.Fragment,
                OmitXmlDeclaration = true,
                Indent = true,
                IndentChars = "  ",
                Encoding = Encoding.UTF8,
                WriteEndDocumentOnClose = true
            };
            if (File.Exists(filePath)) File.Delete(filePath);
            using (var writer = XmlWriter.Create(filePath, xmlSettings))
            {
                writer.WriteStartElement(rootTag);
                foreach (var item in itemsToWrite)
                {
                    writer.WriteElementString(item.Key, item.Value);
                }
                writer.WriteEndElement();
                writer.Close();
            }
        }

        /// <summary>
        /// Read an xml file and retun value of all the tags provided in the request, if found.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="rootTag">Root that to look in xml file</param>
        /// <param name="tagsToLook"></param>
        /// <returns>Directionary of the items.</returns>
        public static Dictionary<string, string> ReadXml(string path, string rootTag, params string[] tagsToLook)
        {
            var items = new Dictionary<string, string>();
            if (File.Exists(path))
            {
                using (var reader = PortalTemplateIO.Instance.OpenTextReader(path))
                {
                    var xmlDoc = XDocument.Load(reader);
                    foreach (var tag in tagsToLook)
                    {
                        items.Add(tag, GetTagValue(xmlDoc, tag, rootTag));
                    }
                }
            }
            return items;
        }

        //TODO: We should implement some base serializer to fix dates for all the entities.
        public static void FixDateTime<T>(T item)
        {
            var properties = item.GetType().GetRuntimeProperties();
            foreach (var property in properties)
            {
                if ((property.PropertyType == typeof(DateTime) || property.PropertyType == typeof(DateTime?)) &&
                    (property.GetValue(item) as DateTime?) == DateTime.MinValue)
                {
                    property.SetValue(item, Constants.EpochTime);
                }
            }
        }

        public static DateTime ToLocalDateTime(DateTime dateTime, UserInfo userInfo)
        {
            if (dateTime.Kind != DateTimeKind.Local)
            {
                dateTime = new DateTime(
                    dateTime.Year, dateTime.Month, dateTime.Day,
                    dateTime.Hour, dateTime.Minute, dateTime.Second,
                    DateTimeKind.Utc);
                return userInfo.LocalTime(dateTime);
            }
            return dateTime;
        }

        public static DateTime? ToLocalDateTime(DateTime? dateTime, UserInfo userInfo)
        {
            if (dateTime != null && dateTime.Value.Kind == DateTimeKind.Utc)
                return userInfo.LocalTime(dateTime.Value);
            return dateTime;
        }

        public static string GetDateTimeString(DateTime? dateTime)
        {
            return dateTime?.ToString(Thread.CurrentThread.CurrentUICulture) ?? "";
        }

        private static string GetTagValue(XDocument xmlDoc, string name, string rootTag)
        {
            return (from f in xmlDoc.Descendants(rootTag)
                    select f.Element(name)?.Value).SingleOrDefault();
        }
    }
}