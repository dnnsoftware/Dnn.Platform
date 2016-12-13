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

using System.Collections.Generic;
using System.Xml;

using DotNetNuke.Common.Utilities;
using Dnn.Modules.Dashboard.Data;

#endregion

namespace Dnn.Modules.Dashboard.Components.Database
{
    public class DatabaseController : IDashboardData
    {
        #region IDashboardData Members

        public void ExportData(XmlWriter writer)
        {
            DbInfo database = GetDbInfo();

            //Write start of Database 
            writer.WriteStartElement("database");

            writer.WriteElementString("productVersion", database.ProductVersion);
            writer.WriteElementString("servicePack", database.ServicePack);
            writer.WriteElementString("productEdition", database.ProductEdition);
            writer.WriteElementString("softwarePlatform", database.SoftwarePlatform);

            //Write start of Backups 
            writer.WriteStartElement("backups");

            //Iterate through Backups 
            foreach (BackupInfo backup in database.Backups)
            {
                writer.WriteStartElement("backup");
                writer.WriteElementString("name", backup.Name);
                writer.WriteElementString("backupType", backup.BackupType);
                writer.WriteElementString("size", backup.Size.ToString());
                writer.WriteElementString("startDate", backup.StartDate.ToString());
                writer.WriteElementString("finishDate", backup.FinishDate.ToString());
                writer.WriteEndElement();
            }
			
            //Write end of Backups 
            writer.WriteEndElement();

            //Write start of Files 
            writer.WriteStartElement("files");

            //Iterate through Files 
            foreach (DbFileInfo dbFile in database.Files)
            {
                writer.WriteStartElement("file");
                writer.WriteElementString("name", dbFile.Name);
                writer.WriteElementString("fileType", dbFile.FileType);
                writer.WriteElementString("shortFileName", dbFile.ShortFileName);
                writer.WriteElementString("fileName", dbFile.FileName);
                writer.WriteElementString("size", dbFile.Size.ToString());
                writer.WriteElementString("megabytes", dbFile.Megabytes.ToString());
                writer.WriteEndElement();
            }
			
            //Write end of Files 
            writer.WriteEndElement();

            //Write end of Database 
            writer.WriteEndElement();
        }

        #endregion

        public static DbInfo GetDbInfo()
        {
            return CBO.FillObject<DbInfo>(DataService.GetDbInfo());
        }

        public static List<BackupInfo> GetDbBackups()
        {
            return CBO.FillCollection<BackupInfo>(DataService.GetDbBackups());
        }

        public static List<DbFileInfo> GetDbFileInfo()
        {
            return CBO.FillCollection<DbFileInfo>(DataService.GetDbFileInfo());
        }
    }
}