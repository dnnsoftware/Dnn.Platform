// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Security.Components
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Security.Cryptography;
    using System.Text.RegularExpressions;
    using System.Xml;

    using DotNetNuke.Application;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Data;
    using Newtonsoft.Json;

    public class Utility
    {
        private const long MaxFileSize = 1024 * 1024 * 10; //10M

        private const int ModifiedFilesCount = 50;

        private static readonly IList<Regex> ExcludedFilePathRegexList = new List<Regex>()
        {
            new Regex(Regex.Escape("\\App_Data\\ClientDependency"), RegexOptions.Compiled | RegexOptions.IgnoreCase),
            new Regex(Regex.Escape("\\App_Data\\Search"), RegexOptions.Compiled | RegexOptions.IgnoreCase),
            new Regex(Regex.Escape("\\d+-System\\Cache\\Pages"), RegexOptions.Compiled | RegexOptions.IgnoreCase),
            new Regex(Regex.Escape("\\d+-System\\Thumbnailsy"), RegexOptions.Compiled | RegexOptions.IgnoreCase),
            new Regex(Regex.Escape("\\Portals\\_default\\Logs"), RegexOptions.Compiled | RegexOptions.IgnoreCase),
            new Regex(Regex.Escape("\\App_Data\\_imagecache"), RegexOptions.Compiled | RegexOptions.IgnoreCase),
            new Regex(Regex.Escape(AppDomain.CurrentDomain.BaseDirectory + "Default.aspx"), RegexOptions.Compiled | RegexOptions.IgnoreCase),
            new Regex(Regex.Escape(AppDomain.CurrentDomain.BaseDirectory + "Default.aspx.cs"), RegexOptions.Compiled | RegexOptions.IgnoreCase),
            new Regex(Regex.Escape(AppDomain.CurrentDomain.BaseDirectory + "web.config"), RegexOptions.Compiled | RegexOptions.IgnoreCase),
        };

        /// <summary>
        ///     delete unnedded installwizard files.
        /// </summary>
        public static void CleanUpInstallerFiles()
        {
            var files = new List<string>
            {
                "DotNetNuke.install.config",
                "DotNetNuke.install.config.resources",
                "InstallWizard.aspx",
                "InstallWizard.aspx.cs",
                "InstallWizard.aspx.designer.cs",
                "UpgradeWizard.aspx",
                "UpgradeWizard.aspx.cs",
                "UpgradeWizard.aspx.designer.cs",
                "Install.aspx",
                "Install.aspx.cs",
                "Install.aspx.designer.cs",
            };

            foreach (var file in files)
            {
                try
                {
                    FileSystemUtils.DeleteFile(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Install\\" + file));
                }
                catch (Exception)
                {
                    //do nothing.
                }
            }
        }

        /// <summary>
        ///     search all files in the website for matching text.
        /// </summary>
        /// <param name="searchText">the matching text.</param>
        /// <returns>ienumerable of file names.</returns>
        public static IEnumerable<object> SearchFiles(string searchText)
        {
            try
            {
                var fileList = GetFiles(AppDomain.CurrentDomain.BaseDirectory, "*.*", SearchOption.AllDirectories);
                return
                    from file in fileList
                    where FileContainsText(file, searchText)
                    let f = new FileInfo(file)
                    select new SearchFileInfo
                    {
                        FileName = f.FullName,
                        LastWriteTime = f.LastWriteTime.ToString(CultureInfo.InvariantCulture)
                    };
            }
            catch
            {
                //suppress any unexpected error
            }
            return null;
        }

        /// <summary>
        ///     search all website files for files with a potential dangerous extension.
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<string> FindUnexpectedExtensions(IList<string> invalidFolders)
        {
            var files = GetFiles(AppDomain.CurrentDomain.BaseDirectory, "*.*", SearchOption.AllDirectories, invalidFolders)
            .Where(s => s.EndsWith(".asp", StringComparison.OrdinalIgnoreCase) || s.EndsWith(".php", StringComparison.OrdinalIgnoreCase));
            return files;
        }

        /// <summary>
        ///     search all website files which are hidden or system.
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<string> FineHiddenSystemFiles()
        {
            var files = GetFiles(AppDomain.CurrentDomain.BaseDirectory, "*.*", SearchOption.AllDirectories)
            .Where(f =>
            {
                if (Path.GetFileName(f)?.Equals("thumbs.db", StringComparison.OrdinalIgnoreCase) == true)
                {
                    return false;
                }

                var attributes = File.GetAttributes(f);
                return attributes.HasFlag(FileAttributes.Hidden) || attributes.HasFlag(FileAttributes.System);
            });
            return files;
        }

        public static List<object> SearchDatabase(string searchText)
        {
            List<object> results = new List<object>();
            var dataProvider = DataProvider.Instance();

            try
            {
                using (var dr = dataProvider.ExecuteReader("SecurityAnalyzer_SearchAllTables", searchText))
                {
                    while (dr.Read())
                    {

                        results.Add(new
                        {
                            ColumnName = dr["ColumnName"],
                            ColumnValue = dr["ColumnValue"]
                        });
                    }
                }
            }
            catch
            {
                //ignore
            }
            return results;
        }

        public static XmlDocument LoadFileSumData()
        {
            using (
                var stream =
                    Assembly.GetExecutingAssembly()
                        .GetManifestResourceStream("Dnn.PersonaBar.Extensions.Components.Security.Resources.sums.resources"))
            {
                if (stream != null)
                {
                    var xmlDocument = new XmlDocument { XmlResolver = null };
                    xmlDocument.Load(stream);

                    return xmlDocument;
                }
                else
                {
                    return null;
                }
            }
        }

        public static string GetFileCheckSum(string fileName)
        {
            using (var cryptographyProvider = SHA256.Create(CryptoConfig.AllowOnlyFipsAlgorithms ? "System.Security.Cryptography.SHA256CryptoServiceProvider" : "System.Security.Cryptography.SHA256Cng"))
            {
                using (var stream = File.OpenRead(fileName))
                {
                    return BitConverter.ToString(cryptographyProvider.ComputeHash(stream)).Replace("-", "").ToLowerInvariant();
                }
            }
        }

        public static string GetApplicationVersion()
        {
            return DotNetNukeContext.Current.Application.Version.ToString(3);
        }

        public static string GetApplicationType()
        {
            switch (DotNetNukeContext.Current.Application.Name)
            {
                case "DNNCORP.CE":
                    return "Platform";
                case "DNNCORP.XE":
                case "DNNCORP.PE":
                    return "Content";
                case "DNNCORP.SOCIAL":
                    return "Social";
                default:
                    return "Platform";
            }
        }

        public static IList<FileInfo> GetLastModifiedFiles()
        {
            var files = GetFiles(AppDomain.CurrentDomain.BaseDirectory, "*.*", SearchOption.AllDirectories)
                .Where(f => !ExcludedFilePathRegexList.Any(r => r.IsMatch(f)))
                .Select(f => new FileInfo(f))
                .OrderByDescending(f => f.LastWriteTime)
                .Take(ModifiedFilesCount).ToList();

            return files;
        }

        public static IList<FileInfo> GetLastModifiedExecutableFiles()
        {
            var executableExtensions = new List<string>() { ".asp", ".aspx", ".php" };
            var files = GetFiles(AppDomain.CurrentDomain.BaseDirectory, "*.*", SearchOption.AllDirectories)
                .Where(f =>
                {
                    var extension = Path.GetExtension(f);
                    return extension != null && executableExtensions.Contains(extension.ToLowerInvariant());
                }).ToList();
            files.Add(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Default.aspx.cs"));
            files.Add(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "web.config"));

            var defaultPage = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Default.aspx");
            if (!files.Contains(defaultPage))
            {
                files.Add(defaultPage);
            }

            return files
            .Select(f => new FileInfo(f))
            .OrderByDescending(f => f.LastWriteTime)
            .Take(ModifiedFilesCount).ToList();

        }

        private static bool FileContainsText(string name, string searchText)
        {
            try
            {
                // If the file has been deleted since we took  
                // the snapshot, ignore it and return the empty string. 
                if (IsReadable(name))
                {
                    return File.ReadAllText(name).IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0;
                }
            }
            catch (Exception)
            {
                //might be a locking issue
            }

            return false;
        }

        private static bool IsReadable(string name)
        {
            if (!File.Exists(name))
            {
                return false;
            }

            var file = new FileInfo(name);
            if (file.Length > MaxFileSize) //when file large than 10M, then don't read it.
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Recursively finds file.
        /// </summary>
        /// <returns></returns>
        private static IEnumerable<string> GetFiles(string path, string searchPattern, SearchOption searchOption)
        {
            try
            {
                return GetFiles(path, searchPattern, searchOption, new List<string>());
            }
            catch (Exception)
            {
                return new List<string>();
            }
        }

        /// <summary>
        /// Recursively finds file.
        /// </summary>
        /// <returns></returns>
        private static IEnumerable<string> GetFiles(string path, string searchPattern, SearchOption searchOption, ICollection<string> invalidFolders)
        {
            //Looking at the root folder only. There should not be any permission issue here.
            IList<string> files;
            try
            {
                files = Directory.GetFiles(path, searchPattern, SearchOption.TopDirectoryOnly).ToList();
            }
            catch (Exception)
            {
                invalidFolders.Add(path);
                yield break;
            }

            foreach (var file in files)
            {
                yield return file;
            }

            if (searchOption == SearchOption.AllDirectories)
            {
                IList<string> folders;
                try
                {
                    folders = Directory.GetDirectories(path, "*", SearchOption.TopDirectoryOnly).ToList();
                }
                catch (Exception)
                {
                    invalidFolders.Add(path);
                    yield break;
                }

                foreach (var folder in folders)
                {
                    //recursive call to the same method
                    foreach (var f in GetFiles(folder, searchPattern, searchOption, invalidFolders))
                    {
                        yield return f;
                    }
                }
            }
        }

        // This will reduce serialization thrown exceptions of anonymous type
        [JsonObject]
        private class SearchFileInfo
        {
            public string FileName;
            public string LastWriteTime;
        }
    }
}
