// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Installer
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Net;
    using System.Text;
    using System.Threading;
    using System.Xml.XPath;

    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Common.Utilities.Internal;
    using DotNetNuke.Entities.Host;
    using DotNetNuke.Services.Installer.Log;
    using DotNetNuke.Services.Installer.Packages;
    using DotNetNuke.UI.Modules;

    using Localization = DotNetNuke.Services.Localization.Localization;

    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The InstallerBase class is a Base Class for all Installer
    ///     classes that need to use Localized Strings.  It provides these strings
    ///     as localized Constants.
    /// </summary>
    /// <remarks>
    /// </remarks>
    public class Util
    {
        /// -----------------------------------------------------------------------------
        /// <summary>
        /// The DeleteFile method deletes a file.
        /// </summary>
        /// <param name="installFile">The file to delete.</param>
        /// <param name="basePath">The basePath to the file.</param>
        /// <param name="log">A Logger to log the result.</param>
        public static void DeleteFile(InstallFile installFile, string basePath, Logger log)
        {
            DeleteFile(installFile.FullName, basePath, log);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// The DeleteFile method deletes a file.
        /// </summary>
        /// <param name="fileName">The file to delete.</param>
        /// <param name="basePath">The basePath to the file.</param>
        /// <param name="log">A Logger to log the result.</param>
        public static void DeleteFile(string fileName, string basePath, Logger log)
        {
            string fullFileName = Path.Combine(basePath, fileName);
            if (File.Exists(fullFileName))
            {
                RetryableAction.RetryEverySecondFor30Seconds(() => FileSystemUtils.DeleteFile(fullFileName), "Delete file " + fullFileName);
                log.AddInfo(string.Format(FILE_Deleted, fileName));
                string folderName = Path.GetDirectoryName(fullFileName);
                if (folderName != null)
                {
                    var folder = new DirectoryInfo(folderName);
                    TryDeleteFolder(folder, log);
                }
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// The GetLocalizedString method provides a conveniencewrapper around the
        /// Localization of Strings.
        /// </summary>
        /// <param name="key">The localization key.</param>
        /// <returns>The localized string.</returns>
        public static string GetLocalizedString(string key)
        {
            return Localization.GetString(key, Localization.SharedResourceFile);
        }

        public static bool IsFileValid(InstallFile file, string packageWhiteList)
        {
            // Check the White List
            FileExtensionWhitelist whiteList = Host.AllowedExtensionWhitelist;

            // Check the White Lists
            string strExtension = file.Extension.ToLowerInvariant();
            if (strExtension == "dnn" || whiteList.IsAllowedExtension(strExtension) || packageWhiteList.Contains(strExtension) ||
                 (packageWhiteList.Contains("*dataprovider") && strExtension.EndsWith("dataprovider")))
            {
                // Install File is Valid
                return true;
            }

            return false;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// The InstallURL method provides a utility method to build the correct url
        /// to install a package (and return to where you came from).
        /// </summary>
        /// <param name="tabId">The id of the tab you are on.</param>
        /// <param name="type">The type of package you are installing.</param>
        /// <returns>The localized string.</returns>
        public static string InstallURL(int tabId, string type)
        {
            var parameters = new string[2];
            parameters[0] = "rtab=" + tabId;
            if (!string.IsNullOrEmpty(type))
            {
                parameters[1] = "ptype=" + type;
            }

            var context = new ModuleInstanceContext();
            return context.NavigateUrl(tabId, "Install", false, parameters);
        }

        public static string InstallURL(int tabId, string returnUrl, string type)
        {
            var parameters = new string[3];
            parameters[0] = "rtab=" + tabId;
            if (!string.IsNullOrEmpty(returnUrl))
            {
                parameters[1] = "returnUrl=" + returnUrl;
            }

            if (!string.IsNullOrEmpty(type))
            {
                parameters[2] = "ptype=" + type;
            }

            var context = new ModuleInstanceContext();
            return context.NavigateUrl(tabId, "Install", false, parameters);
        }

        public static string InstallURL(int tabId, string returnUrl, string type, string package)
        {
            var parameters = new string[4];
            parameters[0] = "rtab=" + tabId;
            if (!string.IsNullOrEmpty(returnUrl))
            {
                parameters[1] = "returnUrl=" + returnUrl;
            }

            if (!string.IsNullOrEmpty(type))
            {
                parameters[2] = "ptype=" + type;
            }

            if (!string.IsNullOrEmpty(package))
            {
                parameters[3] = "package=" + package;
            }

            var context = new ModuleInstanceContext();
            return context.NavigateUrl(tabId, "Install", false, parameters);
        }

        public static string UnInstallURL(int tabId, int packageId, string returnUrl)
        {
            var parameters = new string[3];
            parameters[0] = "rtab=" + tabId;
            parameters[1] = "returnUrl=" + returnUrl;
            parameters[2] = "packageId=" + packageId;
            var context = new ModuleInstanceContext();
            return context.NavigateUrl(tabId, "UnInstall", true, parameters);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// The PackageWriterURL method provides a utility method to build the correct url
        /// to create a package (and return to where you came from).
        /// </summary>
        /// <param name="context">The ModuleContext of the module.</param>
        /// <param name="packageId">The id of the package you are packaging.</param>
        /// <returns>The localized string.</returns>
        public static string PackageWriterURL(ModuleInstanceContext context, int packageId)
        {
            var parameters = new string[3];
            parameters[0] = "rtab=" + context.TabId;
            parameters[1] = "packageId=" + packageId;
            parameters[2] = "mid=" + context.ModuleId;

            return context.NavigateUrl(context.TabId, "PackageWriter", true, parameters);
        }

        public static string ParsePackageIconFileName(PackageInfo package)
        {
            var filename = string.Empty;
            if ((package.IconFile != null) && (package.PackageType.Equals("Module", StringComparison.OrdinalIgnoreCase) || package.PackageType.Equals("Auth_System", StringComparison.OrdinalIgnoreCase) || package.PackageType.Equals("Container", StringComparison.OrdinalIgnoreCase) || package.PackageType.Equals("Skin", StringComparison.OrdinalIgnoreCase)))
            {
                filename = package.IconFile.StartsWith("~/" + package.FolderName) ? package.IconFile.Remove(0, ("~/" + package.FolderName).Length).TrimStart('/') : package.IconFile;
            }

            return filename;
        }

        public static string ParsePackageIconFile(PackageInfo package)
        {
            var iconFile = string.Empty;
            if ((package.IconFile != null) && (package.PackageType.Equals("Module", StringComparison.OrdinalIgnoreCase) || package.PackageType.Equals("Auth_System", StringComparison.OrdinalIgnoreCase) || package.PackageType.Equals("Container", StringComparison.OrdinalIgnoreCase) || package.PackageType.Equals("Skin", StringComparison.OrdinalIgnoreCase)))
            {
                iconFile = !package.IconFile.StartsWith("~/") ? "~/" + package.FolderName + "/" + package.IconFile : package.IconFile;
            }

            return iconFile;
        }

        public static string ReadAttribute(XPathNavigator nav, string attributeName)
        {
            return ValidateNode(nav.GetAttribute(attributeName, string.Empty), false, null, string.Empty, string.Empty);
        }

        public static string ReadAttribute(XPathNavigator nav, string attributeName, Logger log, string logmessage)
        {
            return ValidateNode(nav.GetAttribute(attributeName, string.Empty), true, log, logmessage, string.Empty);
        }

        public static string ReadAttribute(XPathNavigator nav, string attributeName, bool isRequired, Logger log, string logmessage, string defaultValue)
        {
            return ValidateNode(nav.GetAttribute(attributeName, string.Empty), isRequired, log, logmessage, defaultValue);
        }

        public static string GetPackageBackupName(PackageInfo package)
        {
            var packageName = package.Name;
            var version = package.Version;
            var packageType = package.PackageType;

            var fileName = $"{packageType}_{packageName}_{version}.resources";
            if (fileName.IndexOfAny(Path.GetInvalidFileNameChars()) > Null.NullInteger)
            {
                fileName = Globals.CleanFileName(fileName);
            }

            return fileName;
        }

        public static string GetPackageBackupPath(PackageInfo package)
        {
            var fileName = GetPackageBackupName(package);
            var folderPath = Path.Combine(Globals.ApplicationMapPath, Util.BackupInstallPackageFolder);

            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            return Path.Combine(folderPath, fileName);
        }

        public static string ReadElement(XPathNavigator nav, string elementName)
        {
            return ValidateNode(XmlUtils.GetNodeValue(nav, elementName), false, null, string.Empty, string.Empty);
        }

        public static string ReadElement(XPathNavigator nav, string elementName, string defaultValue)
        {
            return ValidateNode(XmlUtils.GetNodeValue(nav, elementName), false, null, string.Empty, defaultValue);
        }

        public static string ReadElement(XPathNavigator nav, string elementName, Logger log, string logmessage)
        {
            return ValidateNode(XmlUtils.GetNodeValue(nav, elementName), true, log, logmessage, string.Empty);
        }

        public static string ReadElement(XPathNavigator nav, string elementName, bool isRequired, Logger log, string logmessage, string defaultValue)
        {
            return ValidateNode(XmlUtils.GetNodeValue(nav, elementName), isRequired, log, logmessage, defaultValue);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// The RestoreFile method restores a file from the backup folder.
        /// </summary>
        /// <param name="installFile">The file to restore.</param>
        /// <param name="basePath">The basePath to the file.</param>
        /// <param name="log">A Logger to log the result.</param>
        public static void RestoreFile(InstallFile installFile, string basePath, Logger log)
        {
            string fullFileName = Path.Combine(basePath, installFile.FullName);
            string backupFileName = Path.Combine(installFile.BackupPath, installFile.Name + ".config");

            // Copy File back over install file
            FileSystemUtils.CopyFile(backupFileName, fullFileName);

            log.AddInfo(string.Format(FILE_RestoreBackup, installFile.FullName));
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// The UnInstallURL method provides a utility method to build the correct url
        /// to uninstall a package (and return to where you came from).
        /// </summary>
        /// <param name="tabId">The id of the tab you are on.</param>
        /// <param name="packageId">The id of the package you are uninstalling.</param>
        /// <returns>The localized string.</returns>
        public static string UnInstallURL(int tabId, int packageId)
        {
            var parameters = new string[2];
            parameters[0] = "rtab=" + tabId;
            parameters[1] = "packageId=" + packageId;
            var context = new ModuleInstanceContext();
            return context.NavigateUrl(tabId, "UnInstall", true, parameters);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// The WriteStream reads a source stream and writes it to a destination file.
        /// </summary>
        /// <param name="sourceStream">The Source Stream.</param>
        /// <param name="destFileName">The Destination file.</param>
        public static void WriteStream(Stream sourceStream, string destFileName)
        {
            var file = new FileInfo(destFileName);
            if (file.Directory != null && !file.Directory.Exists)
            {
                file.Directory.Create();
            }

            // HACK: Temporary fix, upping retry limit due to locking for existing filesystem access.  This "fixes" azure, but isn't the most elegant
            TryToCreateAndExecute(destFileName, (f) => StreamToStream(sourceStream, f), 3500);
        }

        /// <summary>
        /// Try to create file and perform an action on a file until a specific amount of time.
        /// </summary>
        /// <param name="path">Path of the file.</param>
        /// <param name="action">Action to execute on file.</param>
        /// <param name="milliSecondMax">Maimum amount of time to try to do the action.</param>
        /// <returns>true if action occur and false otherwise.</returns>
        public static bool TryToCreateAndExecute(string path, Action<FileStream> action, int milliSecondMax = Timeout.Infinite)
        {
            var result = false;
            var dateTimeStart = DateTime.Now;
            Tuple<AutoResetEvent, FileSystemWatcher> tuple = null;

            while (true)
            {
                try
                {
                    // Open for create, requesting read/write access, allow others to read/write as well
                    using (var file = File.Open(path, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite))
                    {
                        action(file);
                        result = true;
                        break;
                    }
                }
                catch (IOException)
                {
                    // Init only once and only if needed. Prevent against many instantiation in case of multhreaded
                    // file access concurrency (if file is frequently accessed by someone else). Better memory usage.
                    if (tuple == null)
                    {
                        var autoResetEvent = new AutoResetEvent(true);
                        var fileSystemWatcher = new FileSystemWatcher(Path.GetDirectoryName(path))
                        {
                            EnableRaisingEvents = true,
                        };

                        fileSystemWatcher.Changed +=
                            (o, e) =>
                            {
                                if (Path.GetFullPath(e.FullPath) == Path.GetFullPath(path))
                                {
                                    autoResetEvent.Set();
                                }
                            };

                        tuple = new Tuple<AutoResetEvent, FileSystemWatcher>(autoResetEvent, fileSystemWatcher);
                    }

                    int milliSecond = Timeout.Infinite;
                    if (milliSecondMax != Timeout.Infinite)
                    {
                        milliSecond = (int)(DateTime.Now - dateTimeStart).TotalMilliseconds;
                        if (milliSecond >= milliSecondMax)
                        {
                            result = false;
                            break;
                        }
                    }

                    tuple.Item1.WaitOne(milliSecond);
                }
            }

            if (tuple != null && tuple.Item1 != null) // Dispose of resources now (don't wait the GC).
            {
                tuple.Item1.Dispose();
                tuple.Item2.Dispose();
            }

            return result;
        }

        public static WebResponse GetExternalRequest(string URL, byte[] Data, string Username, string Password, string Domain, string ProxyAddress, int ProxyPort, bool DoPOST, string UserAgent,
                                                    string Referer, out string Filename)
        {
            return GetExternalRequest(URL, Data, Username, Password, Domain, ProxyAddress, ProxyPort, DoPOST, UserAgent, Referer, out Filename, Host.WebRequestTimeout);
        }

        public static WebResponse GetExternalRequest(string URL, byte[] Data, string Username, string Password, string Domain, string ProxyAddress, int ProxyPort, bool DoPOST, string UserAgent,
                                                    string Referer, out string Filename, int requestTimeout)
        {
            return GetExternalRequest(URL, Data, Username, Password, Domain, ProxyAddress, ProxyPort, string.Empty, string.Empty, DoPOST, UserAgent, Referer, out Filename, requestTimeout);
        }

        public static WebResponse GetExternalRequest(string URL, byte[] Data, string Username, string Password, string Domain, string ProxyAddress, int ProxyPort,
                                                    string ProxyUsername, string ProxyPassword, bool DoPOST, string UserAgent, string Referer, out string Filename)
        {
            return GetExternalRequest(URL, Data, Username, Password, Domain, ProxyAddress, ProxyPort, ProxyUsername, ProxyPassword, DoPOST, UserAgent, Referer, out Filename, Host.WebRequestTimeout);
        }

        public static WebResponse GetExternalRequest(string URL, byte[] Data, string Username, string Password, string Domain, string ProxyAddress, int ProxyPort,
                                                    string ProxyUsername, string ProxyPassword, bool DoPOST, string UserAgent, string Referer, out string Filename, int requestTimeout)
        {
            if (!DoPOST && Data != null && Data.Length > 0)
            {
                string restoftheurl = Encoding.ASCII.GetString(Data);
                if (URL != null && URL.IndexOf("?") <= 0)
                {
                    URL = URL + "?";
                }

                URL = URL + restoftheurl;
            }

            var wreq = (HttpWebRequest)WebRequest.Create(URL);
            wreq.UserAgent = UserAgent;
            wreq.Referer = Referer;
            wreq.Method = "GET";
            if (DoPOST)
            {
                wreq.Method = "POST";
            }

            wreq.Timeout = requestTimeout;

            if (!string.IsNullOrEmpty(ProxyAddress))
            {
                var proxy = new WebProxy(ProxyAddress, ProxyPort);
                if (!string.IsNullOrEmpty(ProxyUsername))
                {
                    var proxyCredentials = new NetworkCredential(ProxyUsername, ProxyPassword);
                    proxy.Credentials = proxyCredentials;
                }

                wreq.Proxy = proxy;
            }

            if (Username != null && Password != null && Domain != null && Username.Trim() != string.Empty && Password.Trim() != null && Domain.Trim() != null)
            {
                wreq.Credentials = new NetworkCredential(Username, Password, Domain);
            }
            else if (Username != null && Password != null && Username.Trim() != string.Empty && Password.Trim() != null)
            {
                wreq.Credentials = new NetworkCredential(Username, Password);
            }

            if (DoPOST && Data != null && Data.Length > 0)
            {
                wreq.ContentType = "application/x-www-form-urlencoded";
                Stream request = wreq.GetRequestStream();
                request.Write(Data, 0, Data.Length);
                request.Close();
            }

            Filename = string.Empty;
            WebResponse wrsp = wreq.GetResponse();
            string cd = wrsp.Headers["Content-Disposition"];
            if (cd != null && cd.Trim() != string.Empty && cd.StartsWith("attachment"))
            {
                if (cd.IndexOf("filename") > -1 && cd.Substring(cd.IndexOf("filename")).IndexOf("=") > -1)
                {
                    string filenameParam = cd.Substring(cd.IndexOf("filename"));

                    if (filenameParam.IndexOf("\"") > -1)
                    {
                        Filename = filenameParam.Substring(filenameParam.IndexOf("\"") + 1).TrimEnd(Convert.ToChar("\"")).TrimEnd(Convert.ToChar("\\"));
                    }
                    else
                    {
                        Filename = filenameParam.Substring(filenameParam.IndexOf("=") + 1);
                    }
                }
            }

            return wrsp;
        }

        public static void DeployExtension(WebResponse wr, string myfile, string installFolder)
        {
            Stream remoteStream = null;
            Stream localStream = null;

            try
            {
                // Once the WebResponse object has been retrieved,
                // get the stream object associated with the response's data
                remoteStream = wr.GetResponseStream();

                // Create the local file with zip extension to ensure installation
                localStream = File.Create(installFolder + "/" + myfile);

                // Allocate a 1k buffer
                var buffer = new byte[1024];
                int bytesRead;

                // Simple do/while loop to read from stream until
                // no bytes are returned
                do
                {
                    // Read data (up to 1k) from the stream
                    bytesRead = remoteStream.Read(buffer, 0, buffer.Length);

                    // Write the data to the local file
                    localStream.Write(buffer, 0, bytesRead);
                }
                while (bytesRead > 0);
            }
            finally
            {
                // Close the response and streams objects here
                // to make sure they're closed even if an exception
                // is thrown at some point
                if (remoteStream != null)
                {
                    remoteStream.Close();
                }

                if (localStream != null)
                {
                    localStream.Close();
                }
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// The StreamToStream method reads a source stream and wrtites it to a destination stream.
        /// </summary>
        /// <param name="sourceStream">The Source Stream.</param>
        /// <param name="destStream">The Destination Stream.</param>
        private static void StreamToStream(Stream sourceStream, Stream destStream)
        {
            var buf = new byte[1024];
            int count;
            do
            {
                // Read the chunk from the source
                count = sourceStream.Read(buf, 0, 1024);

                // Write the chunk to the destination
                destStream.Write(buf, 0, count);
            }
            while (count > 0);
            destStream.Flush();
        }

        private static void TryDeleteFolder(DirectoryInfo folder, Logger log)
        {
            if (folder.GetFiles().Length == 0 && folder.GetDirectories().Length == 0)
            {
                folder.Delete();
                log.AddInfo(string.Format(FOLDER_Deleted, folder.Name));
                TryDeleteFolder(folder.Parent, log);
            }
        }

        private static string ValidateNode(string propValue, bool isRequired, Logger log, string logmessage, string defaultValue)
        {
            if (string.IsNullOrEmpty(propValue))
            {
                if (isRequired)
                {
                    // Log Error
                    log.AddFailure(logmessage);
                }
                else
                {
                    // Use Default
                    propValue = defaultValue;
                }
            }

            return propValue;
        }

        // ReSharper disable InconsistentNaming
        public const string DEFAULT_MANIFESTEXT = ".manifest";
        public const string BackupInstallPackageFolder = "App_Data/ExtensionPackages/";
        public static string ASSEMBLY_Added = GetLocalizedString("ASSEMBLY_Added");
        public static string ASSEMBLY_AddedBindingRedirect = GetLocalizedString("ASSEMBLY_AddedBindingRedirect");
        public static string ASSEMBLY_RemovedBindingRedirect = GetLocalizedString("ASSEMBLY_RemovedBindingRedirect");
        public static string ASSEMBLY_InUse = GetLocalizedString("ASSEMBLY_InUse");
        public static string ASSEMBLY_Registered = GetLocalizedString("ASSEMBLY_Registered");
        public static string ASSEMBLY_UnRegistered = GetLocalizedString("ASSEMBLY_UnRegistered");
        public static string ASSEMBLY_Updated = GetLocalizedString("ASSEMBLY_Updated");
        public static string AUTHENTICATION_ReadSuccess = GetLocalizedString("AUTHENTICATION_ReadSuccess");
        public static string AUTHENTICATION_LoginSrcMissing = GetLocalizedString("AUTHENTICATION_LoginSrcMissing");
        public static string AUTHENTICATION_Registered = GetLocalizedString("AUTHENTICATION_Registered");
        public static string AUTHENTICATION_SettingsSrcMissing = GetLocalizedString("AUTHENTICATION_SettingsSrcMissing");
        public static string AUTHENTICATION_TypeMissing = GetLocalizedString("AUTHENTICATION_TypeMissing");
        public static string AUTHENTICATION_UnRegistered = GetLocalizedString("AUTHENTICATION_UnRegistered");
        public static string CLEANUP_Processing = GetLocalizedString("CLEANUP_Processing");
        public static string CLEANUP_ProcessComplete = GetLocalizedString("CLEANUP_ProcessComplete");
        public static string CLEANUP_ProcessError = GetLocalizedString("CLEANUP_ProcessError");
        public static string COMPONENT_Installed = GetLocalizedString("COMPONENT_Installed");
        public static string COMPONENT_Skipped = GetLocalizedString("COMPONENT_Skipped");
        public static string COMPONENT_RolledBack = GetLocalizedString("COMPONENT_RolledBack");
        public static string COMPONENT_RollingBack = GetLocalizedString("COMPONENT_RollingBack");
        public static string COMPONENT_UnInstalled = GetLocalizedString("COMPONENT_UnInstalled");
        public static string CONFIG_Committed = GetLocalizedString("CONFIG_Committed");
        public static string CONFIG_RolledBack = GetLocalizedString("CONFIG_RolledBack");
        public static string CONFIG_Updated = GetLocalizedString("CONFIG_Updated");
        public static string DASHBOARD_ReadSuccess = GetLocalizedString("DASHBOARD_ReadSuccess");
        public static string DASHBOARD_SrcMissing = GetLocalizedString("DASHBOARD_SrcMissing");
        public static string DASHBOARD_Registered = GetLocalizedString("DASHBOARD_Registered");
        public static string DASHBOARD_KeyMissing = GetLocalizedString("DASHBOARD_KeyMissing");
        public static string DASHBOARD_LocalResourcesMissing = GetLocalizedString("DASHBOARD_LocalResourcesMissing");
        public static string DASHBOARD_UnRegistered = GetLocalizedString("DASHBOARD_UnRegistered");
        public static string DNN_Reading = GetLocalizedString("DNN_Reading");
        public static string DNN_ReadingComponent = GetLocalizedString("DNN_ReadingComponent");
        public static string DNN_ReadingPackage = GetLocalizedString("DNN_ReadingPackage");
        public static string DNN_Success = GetLocalizedString("DNN_Success");
        public static string EVENTMESSAGE_CommandMissing = GetLocalizedString("EVENTMESSAGE_CommandMissing");
        public static string EVENTMESSAGE_TypeMissing = GetLocalizedString("EVENTMESSAGE_TypeMissing");
        public static string EXCEPTION = GetLocalizedString("EXCEPTION");
        public static string EXCEPTION_FileLoad = GetLocalizedString("EXCEPTION_FileLoad");
        public static string EXCEPTION_FileRead = GetLocalizedString("EXCEPTION_FileRead");
        public static string EXCEPTION_GlobDotDotNotSupportedInCleanup = GetLocalizedString("EXCEPTION_GlobDotDotNotSupportedInCleanup");
        public static string EXCEPTION_InstallerCreate = GetLocalizedString("EXCEPTION_InstallerCreate");
        public static string EXCEPTION_MissingDnn = GetLocalizedString("EXCEPTION_MissingDnn");
        public static string EXCEPTION_MultipleDnn = GetLocalizedString("EXCEPTION_MultipleDnn");
        public static string EXCEPTION_NameMissing = GetLocalizedString("EXCEPTION_NameMissing");
        public static string EXCEPTION_Type = GetLocalizedString("EXCEPTION_Type");
        public static string EXCEPTION_TypeMissing = GetLocalizedString("EXCEPTION_TypeMissing");
        public static string EXCEPTION_VersionMissing = GetLocalizedString("EXCEPTION_VersionMissing");
        public static string FILE_CreateBackup = GetLocalizedString("FILE_CreateBackup");
        public static string FILE_Created = GetLocalizedString("FILE_Created");
        public static string FILE_Deleted = GetLocalizedString("FILE_Deleted");
        public static string FILE_Found = GetLocalizedString("FILE_Found");
        public static string FILE_Loading = GetLocalizedString("FILE_Loading");
        public static string FILE_NotAllowed = GetLocalizedString("FILE_NotAllowed");
        public static string FILE_NotFound = GetLocalizedString("FILE_NotFound");
        public static string FILE_ReadSuccess = GetLocalizedString("FILE_ReadSuccess");
        public static string FILE_RestoreBackup = GetLocalizedString("FILE_RestoreBackup");
        public static string FILES_CreatedResources = GetLocalizedString("FILES_CreatedResources");
        public static string FILES_Expanding = GetLocalizedString("FILES_Expanding");
        public static string FILES_Loading = GetLocalizedString("FILES_Loading");
        public static string FILES_Reading = GetLocalizedString("FILES_Reading");
        public static string FILES_ReadingEnd = GetLocalizedString("FILES_ReadingEnd");
        public static string FOLDER_Created = GetLocalizedString("FOLDER_Created");
        public static string FOLDER_Deleted = GetLocalizedString("FOLDER_Deleted");
        public static string FOLDER_DeletedBackup = GetLocalizedString("FOLDER_DeletedBackup");
        public static string INSTALL_Compatibility = GetLocalizedString("INSTALL_Compatibility");
        public static string INSTALL_Dependencies = GetLocalizedString("INSTALL_Dependencies");
        public static string INSTALL_Aborted = GetLocalizedString("INSTALL_Aborted");
        public static string INSTALL_Failed = GetLocalizedString("INSTALL_Failed");
        public static string INSTALL_Committed = GetLocalizedString("INSTALL_Committed");
        public static string INSTALL_Namespace = GetLocalizedString("INSTALL_Namespace");
        public static string INSTALL_Package = GetLocalizedString("INSTALL_Package");
        public static string INSTALL_Permissions = GetLocalizedString("INSTALL_Permissions");
        public static string INSTALL_Start = GetLocalizedString("INSTALL_Start");
        public static string INSTALL_Success = GetLocalizedString("INSTALL_Success");
        public static string INSTALL_Version = GetLocalizedString("INSTALL_Version");
        public static string LANGUAGE_PortalsEnabled = GetLocalizedString("LANGUAGE_PortalsEnabled");
        public static string LANGUAGE_Registered = GetLocalizedString("LANGUAGE_Registered");
        public static string LANGUAGE_UnRegistered = GetLocalizedString("LANGUAGE_UnRegistered");
        public static string LIBRARY_ReadSuccess = GetLocalizedString("LIBRARY_ReadSuccess");
        public static string LIBRARY_Registered = GetLocalizedString("LIBRARY_Registered");
        public static string LIBRARY_UnRegistered = GetLocalizedString("LIBRARY_UnRegistered");
        public static string MODULE_ControlKeyMissing = GetLocalizedString("MODULE_ControlKeyMissing");
        public static string MODULE_ControlTypeMissing = GetLocalizedString("MODULE_ControlTypeMissing");
        public static string MODULE_FriendlyNameMissing = GetLocalizedString("MODULE_FriendlyNameMissing");
        public static string MODULE_InvalidVersion = GetLocalizedString("MODULE_InvalidVersion");
        public static string MODULE_ReadSuccess = GetLocalizedString("MODULE_ReadSuccess");
        public static string MODULE_Registered = GetLocalizedString("MODULE_Registered");
        public static string MODULE_UnRegistered = GetLocalizedString("MODULE_UnRegistered");
        public static string MODULE_AdminPageAdded = GetLocalizedString("MODULE_AdminPageAdded");
        public static string MODULE_AdminPagemoduleAdded = GetLocalizedString("MODULE_AdminPagemoduleAdded");
        public static string MODULE_AdminPageRemoved = GetLocalizedString("MODULE_AdminPageRemoved");
        public static string MODULE_AdminPagemoduleRemoved = GetLocalizedString("MODULE_AdminPagemoduleRemoved");
        public static string MODULE_HostPageAdded = GetLocalizedString("MODULE_HostPageAdded");
        public static string MODULE_HostPagemoduleAdded = GetLocalizedString("MODULE_HostPagemoduleAdded");
        public static string MODULE_HostPageRemoved = GetLocalizedString("MODULE_HostPageRemoved");
        public static string MODULE_HostPagemoduleRemoved = GetLocalizedString("MODULE_HostPagemoduleRemoved");
        public static string PACKAGE_NoLicense = GetLocalizedString("PACKAGE_NoLicense");
        public static string PACKAGE_NoReleaseNotes = GetLocalizedString("PACKAGE_NoReleaseNotes");
        public static string PACKAGE_UnRecognizable = GetLocalizedString("PACKAGE_UnRecognizable");
        public static string SECURITY_Installer = GetLocalizedString("SECURITY_Installer");
        public static string SECURITY_NotRegistered = GetLocalizedString("SECURITY_NotRegistered");
        public static string SKIN_BeginProcessing = GetLocalizedString("SKIN_BeginProcessing");
        public static string SKIN_Installed = GetLocalizedString("SKIN_Installed");
        public static string SKIN_EndProcessing = GetLocalizedString("SKIN_EndProcessing");
        public static string SKIN_Registered = GetLocalizedString("SKIN_Registered");
        public static string SKIN_UnRegistered = GetLocalizedString("SKIN_UnRegistered");
        public static string SQL_Begin = GetLocalizedString("SQL_Begin");
        public static string SQL_BeginFile = GetLocalizedString("SQL_BeginFile");
        public static string SQL_BeginUnInstall = GetLocalizedString("SQL_BeginUnInstall");
        public static string SQL_Committed = GetLocalizedString("SQL_Committed");
        public static string SQL_End = GetLocalizedString("SQL_End");
        public static string SQL_EndFile = GetLocalizedString("SQL_EndFile");
        public static string SQL_EndUnInstall = GetLocalizedString("SQL_EndUnInstall");
        public static string SQL_Exceptions = GetLocalizedString("SQL_Exceptions");
        public static string SQL_Executing = GetLocalizedString("SQL_Executing");
        public static string SQL_RolledBack = GetLocalizedString("SQL_RolledBack");
        public static string UNINSTALL_Start = GetLocalizedString("UNINSTALL_Start");
        public static string UNINSTALL_StartComp = GetLocalizedString("UNINSTALL_StartComp");
        public static string UNINSTALL_Failure = GetLocalizedString("UNINSTALL_Failure");
        public static string UNINSTALL_Success = GetLocalizedString("UNINSTALL_Success");
        public static string UNINSTALL_SuccessComp = GetLocalizedString("UNINSTALL_SuccessComp");
        public static string UNINSTALL_Warnings = GetLocalizedString("UNINSTALL_Warnings");
        public static string UNINSTALL_WarningsComp = GetLocalizedString("UNINSTALL_WarningsComp");
        public static string URLPROVIDER_NameMissing = GetLocalizedString("URLPROVIDER_NameMissing");
        public static string URLPROVIDER_ReadSuccess = GetLocalizedString("URLPROVIDER_ReadSuccess");
        public static string URLPROVIDER_Registered = GetLocalizedString("URLPROVIDER_Registered");
        public static string URLPROVIDER_TypeMissing = GetLocalizedString("URLPROVIDER_TypeMissing");
        public static string URLPROVIDER_UnRegistered = GetLocalizedString("URLPROVIDER_UnRegistered");
        public static string WRITER_AddFileToManifest = GetLocalizedString("WRITER_AddFileToManifest");
        public static string WRITER_CreateArchive = GetLocalizedString("WRITER_CreateArchive");
        public static string WRITER_CreatedManifest = GetLocalizedString("WRITER_CreatedManifest");
        public static string WRITER_CreatedPackage = GetLocalizedString("WRITER_CreatedPackage");
        public static string WRITER_CreatingManifest = GetLocalizedString("WRITER_CreatingManifest");
        public static string WRITER_CreatingPackage = GetLocalizedString("WRITER_CreatingPackage");
        public static string WRITER_SavedFile = GetLocalizedString("WRITER_SavedFile");
        public static string WRITER_SaveFileError = GetLocalizedString("WRITER_SaveFileError");
        public static string REGEX_Version = "\\d{2}.\\d{2}.\\d{2}";

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// The BackupFile method backs up a file to the backup folder.
        /// </summary>
        /// <param name="installFile">The file to backup.</param>
        /// <param name="basePath">The basePath to the file.</param>
        /// <param name="log">A Logger to log the result.</param>
        public static void BackupFile(InstallFile installFile, string basePath, Logger log)
        {
            string fullFileName = Path.Combine(basePath, installFile.FullName);
            string backupFileName = Path.Combine(installFile.BackupPath, installFile.Name + ".config");

            // create the backup folder if neccessary
            if (!Directory.Exists(installFile.BackupPath))
            {
                Directory.CreateDirectory(installFile.BackupPath);
            }

            // Copy file to backup location
            RetryableAction.RetryEverySecondFor30Seconds(() => FileSystemUtils.CopyFile(fullFileName, backupFileName), "Backup file " + fullFileName);
            log.AddInfo(string.Format(FILE_CreateBackup, installFile.FullName));
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// The CopyFile method copies a file from the temporary extract location.
        /// </summary>
        /// <param name="installFile">The file to copy.</param>
        /// <param name="basePath">The basePath to the file.</param>
        /// <param name="log">A Logger to log the result.</param>
        public static void CopyFile(InstallFile installFile, string basePath, Logger log)
        {
            string filePath = Path.Combine(basePath, installFile.Path);
            string fullFileName = Path.Combine(basePath, installFile.FullName);

            // create the folder if neccessary
            if (!Directory.Exists(filePath))
            {
                log.AddInfo(string.Format(FOLDER_Created, filePath));
                Directory.CreateDirectory(filePath);
            }

            // Copy file from temp location
            RetryableAction.RetryEverySecondFor30Seconds(() => FileSystemUtils.CopyFile(installFile.TempFileName, fullFileName), "Copy file to " + fullFileName);

            log.AddInfo(string.Format(FILE_Created, installFile.FullName));
        }

        // ReSharper restore InconsistentNaming
    }
}
