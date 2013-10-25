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
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;
using System.Web.Caching;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Xml;

using DotNetNuke.Application;
using DotNetNuke.Collections.Internal;
using DotNetNuke.Common.Internal;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Data;
using DotNetNuke.Entities;
using DotNetNuke.Entities.Controllers;
using DotNetNuke.Entities.Host;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Modules.Actions;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Entities.Users;
using DotNetNuke.Framework.Providers;
using DotNetNuke.Instrumentation;
using DotNetNuke.Security;
using DotNetNuke.Security.Permissions;
using DotNetNuke.Security.Roles;
using DotNetNuke.Security.Roles.Internal;
using DotNetNuke.Services.Cache;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Services.FileSystem;
using DotNetNuke.Services.Localization;
using DotNetNuke.Services.Url.FriendlyUrl;
using DotNetNuke.UI.Skins;
using DotNetNuke.UI.Utilities;

using Microsoft.VisualBasic.CompilerServices;

using DataCache = DotNetNuke.UI.Utilities.DataCache;
using FileInfo = DotNetNuke.Services.FileSystem.FileInfo;

#endregion

namespace DotNetNuke.Common
{
    /// <summary>
    /// The global instance of DotNetNuke. all basic functions and properties are defined in this instance.
    /// </summary>
    [StandardModule]
    public sealed class Globals
    {
    	private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof (Globals));
        #region PerformanceSettings enum

        /// <summary>
        /// Enumeration of site performance setting, say by another way that means how to set the cache.
        /// </summary>
        /// <remarks>
        /// <para>Using cache will speed up the application to a great degree, we recommend to use cache for whole modules,
        /// but sometimes cache also make confuse for user, if we didn't take care of how to make cache expired when needed,
        /// such as if a data has already been deleted but the cache arn't clear, it will cause un expected errors.
        /// so you should choose a correct performance setting type when you trying to cache some stuff, and always remember
        /// update cache immediately after the data changed.</para>
        /// <para>default cache policy in core api will use cache timeout muitple Host Performance setting's value as cache time(unit: minutes):</para>
        /// <list type="bullet">
        ///	<item>HostSettingsCacheTimeOut: 20</item>
        /// <item>PortalAliasCacheTimeOut: 200</item>
        /// <item>PortalSettingsCacheTimeOut: 20</item>
        /// <item>More cache timeout definitions see<see cref="DotNetNuke.Common.Utilities.DataCache"/></item>
        /// </list>
        /// </remarks>
        public enum PerformanceSettings
        {
            /// <summary>
            /// No Caching
            /// </summary>
            NoCaching = 0,
            /// <summary>
            /// Caching for a short time
            /// </summary>
            LightCaching = 1,
            /// <summary>
            /// Caching for moderate
            /// </summary>
            ModerateCaching = 3,
            /// <summary>
            /// Caching for a long time
            /// </summary>
            HeavyCaching = 6
        }

        #endregion

        #region PortalRegistrationType enum

        /// <summary>
        /// Enumeration Of Registration Type for portal.
        /// </summary>
        /// <remarks>
        /// <para>NoRegistration: Disabled registration in portal.</para>
        /// <para>PrivateRegistration: Once user's account information has been submitted, 
        /// the portal Administrator will be notified and user's application will be subjected to a screening procedure. 
        /// If user's application is authorized, the user will receive notification of access to the portal environment.</para>
        /// <para>PublicRegistration: Once user's account information has been submitted, 
        /// user will be immediately granted access to the portal environment.</para>
        /// <para>VerifiedRegistration: Once user's account information has been submitted, 
        /// user will receive an email containing unique Verification Code. 
        /// The Verification Code will be required the first time when user attempt to sign in to the portal environment.</para>
        /// </remarks>
        public enum PortalRegistrationType
        {
            /// <summary>
            /// Disabled Registration
            /// </summary>
            NoRegistration = 0,
            /// <summary>
            /// Account need be approved by portal's administrator.
            /// </summary>
            PrivateRegistration = 1,
            /// <summary>
            /// Account will be available after post registration data successful.
            /// </summary>
            PublicRegistration = 2,
            /// <summary>
            /// Account will be available by verify code.
            /// </summary>
            VerifiedRegistration = 3
        }

        #endregion

        #region UpgradeStatus enum

        /// <summary>
        /// Enumeration Of Application upgrade status.
        /// </summary>
        public enum UpgradeStatus
        {
            /// <summary>
            /// The application need update to a higher version.
            /// </summary>
            Upgrade,
            /// <summary>
            /// The application need to install itself.
            /// </summary>
            Install,
            /// <summary>
            /// The application is normal running.
            /// </summary>
            None,
            /// <summary>
            /// The application occur error when running.
            /// </summary>
            Error,
            /// <summary>
            /// The application status is unknown,
            /// </summary>
            /// <remarks>This status should never be returned. its is only used as a flag that Status hasn't been determined.</remarks>
            Unknown
        }

        #endregion

        /// <summary>
        /// Global role id for all users
        /// </summary>
        /// <value>-1</value>
        public const string glbRoleAllUsers = "-1";

        /// <summary>
        /// Global role id for super user
        /// </summary>
        /// <value>-2</value>
        public const string glbRoleSuperUser = "-2";

        /// <summary>
        /// Global role id for unauthenticated users
        /// </summary>
        /// <value>-3</value>
        public const string glbRoleUnauthUser = "-3";

        /// <summary>
        /// Global role id by default
        /// </summary>
        /// <value>-4</value>
        public const string glbRoleNothing = "-4";

        /// <summary>
        /// Global role name for all users
        /// </summary>
        /// <value>All Users</value>
        public const string glbRoleAllUsersName = "All Users";

        /// <summary>
        /// Global ro name for super user
        /// </summary>
        /// <value>Superuser</value>
        public const string glbRoleSuperUserName = "Superuser";

        /// <summary>
        /// Global role name for unauthenticated users
        /// </summary>
        /// <value>Unauthenticated Users</value>
        public const string glbRoleUnauthUserName = "Unauthenticated Users";

        /// <summary>
        /// Default page name
        /// </summary>
        /// <value>Default.aspx</value>
        public const string glbDefaultPage = "Default.aspx";

        /// <summary>
        /// Default host skin folder
        /// </summary>
        /// <value>_default</value>
        public const string glbHostSkinFolder = "_default";

        /// <summary>
        /// Default control panel
        /// </summary>
        /// <value>Admin/ControlPanel/IconBar.ascx</value>
        public const string glbDefaultControlPanel = "Admin/ControlPanel/IconBar.ascx";

        /// <summary>
        /// Default pane name
        /// </summary>
        /// <value>ContentPane</value>
        public const string glbDefaultPane = "ContentPane";

        /// <summary>
        /// Image file types
        /// </summary>
        /// <value>jpg,jpeg,jpe,gif,bmp,png,swf</value>
        public const string glbImageFileTypes = "jpg,jpeg,jpe,gif,bmp,png";

        /// <summary>
        /// Config files folder
        /// </summary>
        /// <value>\Config\</value>
        public const string glbConfigFolder = "\\Config\\";

        /// <summary>
        /// About page name
        /// </summary>
        /// <value>about.htm</value>
        public const string glbAboutPage = "about.htm";

        /// <summary>
        /// DotNetNuke config file
        /// </summary>
        /// <value>DotNetNuke.config</value>
        public const string glbDotNetNukeConfig = "DotNetNuke.config";

        /// <summary>
        /// Default portal id for super user
        /// </summary>
        /// <value>-1</value>
        public const int glbSuperUserAppName = -1;

        /// <summary>
        /// extension of protected files
        /// </summary>
        /// <value>.resources</value>
        public const string glbProtectedExtension = ".resources";

        /// <summary>
        /// Default container folder
        /// </summary>
        /// <value>Portals/_default/Containers/</value>
        public const string glbContainersPath = "Portals/_default/Containers/";

        /// <summary>
        /// Default skin folder
        /// </summary>
        /// <value>Portals/_default/Skins/</value>
        public const string glbSkinsPath = "Portals/_default/Skins/";

        /// <summary>
        /// Email address regex pattern
        /// </summary>
        /// <value><![CDATA[^[a-zA-Z0-9_%+#&'*/=^`{|}~-](?:\.?[a-zA-Z0-9_%+#&'*/=^`{|}~-])*@(?:[a-zA-Z0-9_](?:(?:\.?|-*)[a-zA-Z0-9_])*\.[a-zA-Z]{2,9}|\[(?:2[0-4]\d|25[0-5]|[01]?\d\d?)\.(?:2[0-4]\d|25[0-5]|[01]?\d\d?)\.(?:2[0-4]\d|25[0-5]|[01]?\d\d?)\.(?:2[0-4]\d|25[0-5]|[01]?\d\d?)])$]]></value>
        public const string glbEmailRegEx =
            @"^\s*[a-zA-Z0-9_%+#&'*/=^`{|}~-](?:\.?[a-zA-Z0-9_%+#&'*/=^`{|}~-])*@(?:[a-zA-Z0-9_](?:(?:\.?|-*)[a-zA-Z0-9_])*\.[a-zA-Z]{2,9}|\[(?:2[0-4]\d|25[0-5]|[01]?\d\d?)\.(?:2[0-4]\d|25[0-5]|[01]?\d\d?)\.(?:2[0-4]\d|25[0-5]|[01]?\d\d?)\.(?:2[0-4]\d|25[0-5]|[01]?\d\d?)])\s*$";

        /// <summary>
        /// User Name regex pattern
        /// </summary>
        /// <value></value>
        public const string glbUserNameRegEx = @"";

        /// <summary>
        /// format of a script tag
        /// </summary>
        /// <value><![CDATA[<script type=\"text/javascript\" src=\"{0}\" ></script>]]></value>
        public const string glbScriptFormat = "<script type=\"text/javascript\" src=\"{0}\" ></script>";

        // global constants for the life of the application ( set in Application_Start )
        private static string _applicationPath;
        private static string _applicationMapPath;
        private static string _desktopModulePath;
        private static string _imagePath;
        private static string _hostMapPath;
        private static string _hostPath;
        private static string _installMapPath;
        private static string _installPath;
        private static Version _dataBaseVersion;
        private static UpgradeStatus _status = UpgradeStatus.Unknown;

        /// <summary>
        /// Gets the application path.
        /// </summary>
        public static string ApplicationPath
        {
            get
            {
                if (_applicationPath == null && (HttpContext.Current != null))
                {
                    if (HttpContext.Current.Request.ApplicationPath == "/")
                    {
                        _applicationPath = string.IsNullOrEmpty(Config.GetSetting("InstallationSubfolder")) ? "" : (Config.GetSetting("InstallationSubfolder") + "/").ToLowerInvariant();
                    }
                    else
                    {
                        _applicationPath = HttpContext.Current.Request.ApplicationPath.ToLowerInvariant();
                    }
                }

                return _applicationPath;
            }
        }

        /// <summary>
        /// Gets or sets the application map path.
        /// </summary>
        /// <value>
        /// The application map path.
        /// </value>
        public static string ApplicationMapPath
        {
            get
            {
                return _applicationMapPath ?? (_applicationMapPath = GetCurrentDomainDirectory());
            }
        }

        private static string GetCurrentDomainDirectory()
        {
            var dir = AppDomain.CurrentDomain.BaseDirectory.Replace("/", "\\");
            if (dir.Length > 3 &&  dir.EndsWith("\\"))
            {
                dir = dir.Substring(0, dir.Length - 1);
            }
            return dir;
        }

        /// <summary>
        /// Gets the desktop module path.
        /// </summary>
        /// <value>ApplicationPath + "/DesktopModules/"</value>
        public static string DesktopModulePath
        {
            get
            {
                if (_desktopModulePath == null)
                {
                    _desktopModulePath = ApplicationPath + "/DesktopModules/";
                }
                return _desktopModulePath;
            }
        }

        /// <summary>
        /// Gets the image path.
        /// </summary>
        /// <value>ApplicationPath + "/Images/"</value>
        public static string ImagePath
        {
            get
            {
                if (_imagePath == null)
                {
                    _imagePath = ApplicationPath + "/Images/";
                }
                return _imagePath;
            }
        }

        /// <summary>
        /// Gets the database version.
        /// </summary>
        public static Version DataBaseVersion
        {
            get
            {
                return _dataBaseVersion;
            }
        }

        /// <summary>
        /// Gets or sets the host map path.
        /// </summary>
        /// <value>ApplicationMapPath + "Portals\_default\"</value>
        public static string HostMapPath
        {
            get
            {
                if (_hostMapPath == null)
                {
                    _hostMapPath = Path.Combine(ApplicationMapPath, @"Portals\_default\");
                }
                return _hostMapPath;
            }
        }

        /// <summary>
        /// Gets or sets the host path.
        /// </summary>
        /// <value>ApplicationPath + "/Portals/_default/"</value>
        public static string HostPath
        {
            get
            {
                if (_hostPath == null)
                {
                    _hostPath = ApplicationPath + "/Portals/_default/";
                }
                return _hostPath;
            }
        }

        /// <summary>
        /// Gets or sets the install map path.
        /// </summary>
        /// <value>server map path of InstallPath.</value>
        public static string InstallMapPath
        {
            get
            {
                if (_installMapPath == null)
                {
                    _installMapPath = HttpContext.Current.Server.MapPath(InstallPath);
                }
                return _installMapPath;
            }
        }

        /// <summary>
        /// Gets or sets the install path.
        /// </summary>
        /// <value>ApplicationPath + "/Install/"</value>
        public static string InstallPath
        {
            get
            {
                if (_installPath == null)
                {
                    _installPath = ApplicationPath + "/Install/";
                }
                return _installPath;
            }
        }

        /// <summary>
        /// Gets or sets the name of the IIS app.
        /// </summary>
        /// <value>
        /// request.ServerVariables["APPL_MD_PATH"]
        /// </value>
        public static string IISAppName { get; set; }

        /// <summary>
        /// Gets or sets the name of the server.
        /// </summary>
        /// <value>
        /// server name in config file or the server's marchine name.
        /// </value>
        public static string ServerName { get; set; }

        /// <summary>
        /// Gets or sets the operating system version.
        /// </summary>
        /// <value>
        /// The operating system version.
        /// </value>
        public static Version OperatingSystemVersion { get; set; }

        /// <summary>
        /// Gets or sets the NET framework version.
        /// </summary>
        /// <value>
        /// The NET framework version.
        /// </value>
        public static Version NETFrameworkVersion { get; set; }

        /// <summary>
        /// Gets or sets the database engine version.
        /// </summary>
        /// <value>
        /// The database engine version.
        /// </value>
        public static Version DatabaseEngineVersion { get; set; }

        /// <summary>
        /// Redirects the specified URL.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <param name="endResponse">if set to <c>true</c> [end response].</param>
        public static void Redirect(string url, bool endResponse)
        {
            try
            {
                HttpContext.Current.Response.Redirect(url, endResponse);
            }
            catch (ThreadAbortException)
            {
                //we are ignoreing this error simply because there is no graceful way to redirect the user, wihtout the threadabort exception.
                //RobC
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
        }

        /// <summary>
        /// Gets the status of application.
        /// </summary>
        /// <seealso cref="GetStatus"/>
        public static UpgradeStatus Status
        {
            get
            {
                if (_status == UpgradeStatus.Unknown)
                {
                    var tempStatus = UpgradeStatus.Unknown;

                    Logger.Trace("Getting application status");
                    tempStatus = UpgradeStatus.None;
                    //first call GetProviderPath - this insures that the Database is Initialised correctly
                    //and also generates the appropriate error message if it cannot be initialised correctly
                    string strMessage = DataProvider.Instance().GetProviderPath();
                    //get current database version from DB
                    if (!strMessage.StartsWith("ERROR:"))
                    {
                        try
                        {
                            _dataBaseVersion = DataProvider.Instance().GetVersion();
                        }
                        catch (Exception ex)
                        {
                            Logger.Error(ex);
                            strMessage = "ERROR:" + ex.Message;
                        }
                    }
                    if (strMessage.StartsWith("ERROR"))
                    {
                        if (IsInstalled())
                        {
                            //Errors connecting to the database after an initial installation should be treated as errors.				
                            tempStatus = UpgradeStatus.Error;
                        }
                        else
                        {
                            //An error that occurs before the database has been installed should be treated as a new install				
                           tempStatus = UpgradeStatus.Install;
                        }
                    }
                    else if (DataBaseVersion == null)
                    {
                        //No Db Version so Install
                        tempStatus = UpgradeStatus.Install;
                    }
                    else
                    {
                        if (DotNetNukeContext.Current.Application.Version.Major > DataBaseVersion.Major)
                        {
                            //Upgrade Required (Major Version Upgrade)
                            tempStatus = UpgradeStatus.Upgrade;
                        }
                        else if (DotNetNukeContext.Current.Application.Version.Major == DataBaseVersion.Major && DotNetNukeContext.Current.Application.Version.Minor > DataBaseVersion.Minor)
                        {
                            //Upgrade Required (Minor Version Upgrade)
                            tempStatus = UpgradeStatus.Upgrade;
                        }
                        else if (DotNetNukeContext.Current.Application.Version.Major == DataBaseVersion.Major && DotNetNukeContext.Current.Application.Version.Minor == DataBaseVersion.Minor &&
                                 DotNetNukeContext.Current.Application.Version.Build > DataBaseVersion.Build)
                        {
                            //Upgrade Required (Build Version Upgrade)
                            tempStatus = UpgradeStatus.Upgrade;
                        }
                    }

                    _status = tempStatus;

                    Logger.Trace(string.Format("result of getting providerpath: {0}",strMessage));
                    Logger.Trace("Application status is " + _status);
                }
                return _status;
            }

        }


        /// <summary>
        /// IsInstalled looks at various file artifacts to determine if DotNetNuke has already been installed.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// If DotNetNuke has been installed, then we should treat database connection errors as real errors.  
        /// If DotNetNuke has not been installed, then we should expect to have database connection problems
        /// since the connection string may not have been configured yet, which can occur during the installation
        /// wizard.
        /// </remarks>
        internal static bool IsInstalled()
        {
            const int c_PassingScore = 4;
            int installationdatefactor = Convert.ToInt32(HasInstallationDate() ? 1 : 0);
            int dataproviderfactor = Convert.ToInt32(HasDataProviderLogFiles() ? 3 : 0);
            int htmlmodulefactor = Convert.ToInt32(ModuleDirectoryExists("html") ? 2 : 0);
            int portaldirectoryfactor = Convert.ToInt32(HasNonDefaultPortalDirectory() ? 2 : 0);
            int localexecutionfactor = Convert.ToInt32(HttpContext.Current.Request.IsLocal ? c_PassingScore - 1 : 0);
            //This calculation ensures that you have a more than one item that indicates you have already installed DNN.
            //While it is possible that you might not have an installation date or that you have deleted log files
            //it is unlikely that you have removed every trace of an installation and yet still have a working install

            bool isInstalled =  (!IsInstallationURL()) && ((installationdatefactor + dataproviderfactor + htmlmodulefactor + portaldirectoryfactor + localexecutionfactor) >= c_PassingScore);

            // we need to tighten this check. We now are enforcing the existence of the InstallVersion value in web.config. If 
            // this value exists, then DNN was previously installed, and we should never try to re-install it
            return isInstalled || HasInstallVersion();
        }

        /// <summary>
        /// Determines whether has data provider log files.
        /// </summary>
        /// <returns>
        ///   <c>true</c> if has data provider log files; otherwise, <c>false</c>.
        /// </returns>
        private static bool HasDataProviderLogFiles()
        {
            Provider currentdataprovider = Config.GetDefaultProvider("data");
            string providerpath = currentdataprovider.Attributes["providerPath"];
            //If the provider path does not exist, then there can't be any log files
            if (!string.IsNullOrEmpty(providerpath))
            {
                providerpath = HttpContext.Current.Server.MapPath(providerpath);
                if (Directory.Exists(providerpath))
                {
                    return Directory.GetFiles(providerpath, "*.log.resources").Length > 0;
                }
            }
            return false;
        }

        /// <summary>
        /// Determines whether has installation date.
        /// </summary>
        /// <returns>
        ///   <c>true</c> if has installation date; otherwise, <c>false</c>.
        /// </returns>
        private static bool HasInstallationDate()
        {
            return Config.GetSetting("InstallationDate") != null;
        }

        /// <summary>
        /// Determines whether has InstallVersion set.
        /// </summary>
        /// <returns>
        ///   <c>true</c> if has installation date; otherwise, <c>false</c>.
        /// </returns>
        private static bool HasInstallVersion()
        {
            return Config.GetSetting("InstallVersion") != null;
        }

        /// <summary>
        /// Check whether the modules directory is exists.
        /// </summary>
        /// <param name="moduleName">Name of the module.</param>
        /// <returns>
        /// <c>true</c> if the module directory exist, otherwise, <c>false</c>.
        /// </returns>
        private static bool ModuleDirectoryExists(string moduleName)
        {
            string dir = ApplicationMapPath + "\\desktopmodules\\" + moduleName;
            return Directory.Exists(dir);
        }

        /// <summary>
        /// Determines whether has portal directory except default portal directory in portal path.
        /// </summary>
        /// <returns>
        ///   <c>true</c> if has portal directory except default portal directory in portal path; otherwise, <c>false</c>.
        /// </returns>
        private static bool HasNonDefaultPortalDirectory()
        {
            string dir = ApplicationMapPath + "\\portals";
            if (Directory.Exists(dir))
            {
                return Directory.GetDirectories(dir).Length > 1;
            }
            return false;
        }

        /// <summary>
        /// Determines whether current request is for install.
        /// </summary>
        /// <returns>
        ///   <c>true</c> if current request is for install; otherwise, <c>false</c>.
        /// </returns>
        private static bool IsInstallationURL()
        {
            string requestURL = HttpContext.Current.Request.RawUrl.ToLowerInvariant();
            return requestURL.Contains("\\install.aspx") || requestURL.Contains("\\installwizard.aspx");
        }

        /// <summary>
        /// Gets the culture code of the tab.
        /// </summary>
        /// <param name="TabID">The tab ID.</param>
        /// <param name="IsSuperTab">if set to <c>true</c> [is super tab].</param>
        /// <param name="settings">The settings.</param>
        /// <returns>return the tab's culture code, if ths tab doesn't exist, it will return current culture name.</returns>
        internal static string GetCultureCode(int TabID, bool IsSuperTab, PortalSettings settings)
        {
            string cultureCode = Null.NullString;
            if (settings != null)
            {
                TabInfo linkTab = default(TabInfo);
                var controller = new TabController();
                if (IsSuperTab)
                {
                    linkTab = controller.GetTab(TabID, Null.NullInteger, false);
                }
                else
                {
                    linkTab = controller.GetTab(TabID, settings.PortalId, false);
                }
                if (linkTab != null)
                {
                    cultureCode = linkTab.CultureCode;
                }
                if (string.IsNullOrEmpty(cultureCode))
                {
                    cultureCode = Thread.CurrentThread.CurrentCulture.Name;
                }
            }

            return cultureCode;
        }

        /// <summary>
        /// Builds the cross tab dataset.
        /// </summary>
        /// <param name="DataSetName">Name of the data set.</param>
        /// <param name="result">The result.</param>
        /// <param name="FixedColumns">The fixed columns.</param>
        /// <param name="VariableColumns">The variable columns.</param>
        /// <param name="KeyColumn">The key column.</param>
        /// <param name="FieldColumn">The field column.</param>
        /// <param name="FieldTypeColumn">The field type column.</param>
        /// <param name="StringValueColumn">The string value column.</param>
        /// <param name="NumericValueColumn">The numeric value column.</param>
        /// <returns>the dataset instance</returns>
        public static DataSet BuildCrossTabDataSet(string DataSetName, IDataReader result, string FixedColumns, string VariableColumns, string KeyColumn, string FieldColumn, string FieldTypeColumn,
                                                   string StringValueColumn, string NumericValueColumn)
        {
            return BuildCrossTabDataSet(DataSetName, result, FixedColumns, VariableColumns, KeyColumn, FieldColumn, FieldTypeColumn, StringValueColumn, NumericValueColumn, CultureInfo.CurrentCulture);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// converts a data reader with serialized fields into a typed data set
        /// </summary>
        /// <param name="DataSetName">Name of the dataset to be created</param>
        /// <param name="result">Data reader that contains all field values serialized</param>
        /// <param name="FixedColumns">List of fixed columns, delimited by commas. Columns must be contained in DataReader</param>
        /// <param name="VariableColumns">List of variable columns, delimited by commas. Columns must be contained in DataReader</param>
        /// <param name="KeyColumn">Name of the column, that contains the row ID. Column must be contained in DataReader</param>
        /// <param name="FieldColumn">Name of the column, that contains the field name. Column must be contained in DataReader</param>
        /// <param name="FieldTypeColumn">Name of the column, that contains the field type name. Column must be contained in DataReader</param>
        /// <param name="StringValueColumn">Name of the column, that contains the field value, if stored as string. Column must be contained in DataReader</param>
        /// <param name="NumericValueColumn">Name of the column, that contains the field value, if stored as number. Column must be contained in DataReader</param>
        /// <param name="Culture">culture of the field values in data reader's string value column</param>
        /// <returns>The generated DataSet</returns>
        /// <history>
        /// 	[sleupold]     08/24/2006	Created temporary clone of core function and added support for culture based parsing of numeric values
        /// </history>
        /// -----------------------------------------------------------------------------
        public static DataSet BuildCrossTabDataSet(string DataSetName, IDataReader result, string FixedColumns, string VariableColumns, string KeyColumn, string FieldColumn, string FieldTypeColumn,
                                                   string StringValueColumn, string NumericValueColumn, CultureInfo Culture)
        {
            string[] arrFixedColumns = null;
            string[] arrVariableColumns = null;
            string[] arrField;
            string FieldType;
            int intColumn;
            int intKeyColumn;
            // create dataset
            var crosstab = new DataSet(DataSetName);
            crosstab.Namespace = "NetFrameWork";
            // create table
            var tab = new DataTable(DataSetName);
            // split fixed columns
            arrFixedColumns = FixedColumns.Split(',');
            // add fixed columns to table
            for (intColumn = 0; intColumn < arrFixedColumns.Length; intColumn++)
            {
                arrField = arrFixedColumns[intColumn].Split('|');
                var col = new DataColumn(arrField[0], Type.GetType("System." + arrField[1]));
                tab.Columns.Add(col);
            }

            // split variable columns
            if (!String.IsNullOrEmpty(VariableColumns))
            {
                arrVariableColumns = VariableColumns.Split(',');
                //add varible columns to table
                for (intColumn = 0; intColumn < arrVariableColumns.Length; intColumn++)
                {
                    arrField = arrVariableColumns[intColumn].Split('|');
                    var col = new DataColumn(arrField[0], Type.GetType("System." + arrField[1]));
                    col.AllowDBNull = true;
                    tab.Columns.Add(col);
                }
            }
            // add table to dataset
            crosstab.Tables.Add(tab);
            // add rows to table
            intKeyColumn = -1;
            DataRow row = null;
            while (result.Read())
            {
                //loop using KeyColumn as control break
                if (Convert.ToInt32(result[KeyColumn]) != intKeyColumn)
                {
                    //add row
                    if (intKeyColumn != -1)
                    {
                        tab.Rows.Add(row);
                    }
                    // create new row
                    row = tab.NewRow();
                    // assign fixed column values
                    for (intColumn = 0; intColumn < arrFixedColumns.Length; intColumn++)
                    {
                        arrField = arrFixedColumns[intColumn].Split('|');
                        row[arrField[0]] = result[arrField[0]];
                    }
                    //initialize variable column values
                    if (!String.IsNullOrEmpty(VariableColumns))
                    {
                        for (intColumn = 0; intColumn < arrVariableColumns.Length; intColumn++)
                        {
                            arrField = arrVariableColumns[intColumn].Split('|');
                            switch (arrField[1])
                            {
                                case "Decimal":
                                    row[arrField[0]] = 0;
                                    break;
                                case "String":
                                    row[arrField[0]] = "";
                                    break;
                            }
                        }
                    }
                    intKeyColumn = Convert.ToInt32(result[KeyColumn]);
                }
                //assign pivot column value
                if (!String.IsNullOrEmpty(FieldTypeColumn))
                {
                    FieldType = result[FieldTypeColumn].ToString();
                }
                else
                {
                    FieldType = "String";
                }
                switch (FieldType)
                {
                    case "Decimal":
                        row[Convert.ToInt32(result[FieldColumn])] = result[NumericValueColumn];
                        break;
                    case "String":
                        if (ReferenceEquals(Culture, CultureInfo.CurrentCulture))
                        {
                            row[result[FieldColumn].ToString()] = result[StringValueColumn];
                        }
                        else
                        {
                            switch (tab.Columns[result[FieldColumn].ToString()].DataType.ToString())
                            {
                                case "System.Decimal":
                                case "System.Currency":
                                    row[result[FieldColumn].ToString()] = decimal.Parse(result[StringValueColumn].ToString(), Culture);
                                    break;
                                case "System.Int32":
                                    row[result[FieldColumn].ToString()] = Int32.Parse(result[StringValueColumn].ToString(), Culture);
                                    break;
                                default:
                                    row[result[FieldColumn].ToString()] = result[StringValueColumn];
                                    break;
                            }
                        }
                        break;
                }
            }
            result.Close();
            // add row
            if (intKeyColumn != -1)
            {
                tab.Rows.Add(row);
            }
            // finalize dataset
            crosstab.AcceptChanges();
            // return the dataset
            return crosstab;
        }

        /// <summary>
        /// Converts the datareader to dataset.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <returns>the dataset instance</returns>
        public static DataSet ConvertDataReaderToDataSet(IDataReader reader)
        {
            // add datatable to dataset
            var objDataSet = new DataSet();
	        
			do
	        {
		        objDataSet.Tables.Add(ConvertDataReaderToDataTable(reader, false));
	        } while (reader.NextResult());
            
			reader.Close();

            return objDataSet;
        }

        /// <summary>
        /// Converts the datareader to datatable.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <returns>the datatable instance</returns>
		public static DataTable ConvertDataReaderToDataTable(IDataReader reader)
        {
	        return ConvertDataReaderToDataTable(reader, true);
        }

		/// <summary>
		/// Converts the datareader to datatable.
		/// </summary>
		/// <param name="reader">The reader.</param>
		/// <param name="closeReader">Whether close reader.</param>
		/// <returns>the datatable instance</returns>
        public static DataTable ConvertDataReaderToDataTable(IDataReader reader, bool closeReader)
        {
            // create datatable from datareader
            var objDataTable = new DataTable();
            int intFieldCount = reader.FieldCount;
            int intCounter;
            for (intCounter = 0; intCounter <= intFieldCount - 1; intCounter++)
            {
                objDataTable.Columns.Add(reader.GetName(intCounter), reader.GetFieldType(intCounter));
            }
            // populate datatable
            objDataTable.BeginLoadData();
            var objValues = new object[intFieldCount];
            while (reader.Read())
            {
                reader.GetValues(objValues);
                objDataTable.LoadDataRow(objValues, true);
            }

	        if (closeReader)
	        {
		        reader.Close();
	        }
	        objDataTable.EndLoadData();
            return objDataTable;
        }

        /// <summary>
        /// Gets the absolute server path.
        /// </summary>
        /// <param name="Request">The request.</param>
        /// <returns>absolute server path</returns>
        public static string GetAbsoluteServerPath(HttpRequest Request)
        {
            string strServerPath;
            strServerPath = Request.MapPath(Request.ApplicationPath);
            if (!strServerPath.EndsWith("\\"))
            {
                strServerPath += "\\";
            }
            return strServerPath;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the ApplicationName for the MemberRole API.
        /// </summary>
        /// <remarks>
        /// This overload is used to get the current ApplcationName.  The Application
        /// Name is in the form Prefix_Id, where Prefix is the object qualifier
        /// for this instance of DotNetNuke, and Id is the current PortalId for normal
        /// users or glbSuperUserAppName for SuperUsers.
        /// </remarks>
        /// <history>
        ///		[cnurse]	01/18/2005	documented and modifeid to handle a Prefix
        /// </history>
        /// -----------------------------------------------------------------------------
        public static string GetApplicationName()
        {
            string appName;
            if (HttpContext.Current.Items["ApplicationName"] == null || String.IsNullOrEmpty(HttpContext.Current.Items["ApplicationName"].ToString()))
            {
                PortalSettings _PortalSettings = PortalController.GetCurrentPortalSettings();
                if (_PortalSettings == null)
                {
                    appName = "/";
                }
                else
                {
                    appName = GetApplicationName(_PortalSettings.PortalId);
                }
            }
            else
            {
                appName = Convert.ToString(HttpContext.Current.Items["ApplicationName"]);
            }
            return appName;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the ApplicationName for the MemberRole API.
        /// </summary>
        /// <remarks>
        /// This overload is used to build the Application Name from the Portal Id
        /// </remarks>
        /// <history>
        ///		[cnurse]	01/18/2005	documented and modifeid to handle a Prefix
        /// </history>
        /// -----------------------------------------------------------------------------
        public static string GetApplicationName(int PortalID)
        {
            string appName;
            //Get the Data Provider Configuration
            ProviderConfiguration _providerConfiguration = ProviderConfiguration.GetProviderConfiguration("data");
            //Read the configuration specific information for the current Provider
            var objProvider = (Provider)_providerConfiguration.Providers[_providerConfiguration.DefaultProvider];
            //Get the Object Qualifier frm the Provider Configuration
            string _objectQualifier = objProvider.Attributes["objectQualifier"];
            if (!String.IsNullOrEmpty(_objectQualifier) && _objectQualifier.EndsWith("_") == false)
            {
                _objectQualifier += "_";
            }
            appName = _objectQualifier + Convert.ToString(PortalID);
            return appName;
        }

        /// <summary>
        /// Finds the database version.
        /// </summary>
        /// <param name="Major">The major.</param>
        /// <param name="Minor">The minor.</param>
        /// <param name="Build">The build.</param>
        /// <returns>return <c>true</c> if can find the specific version, otherwise will retur <c>false</c>.</returns>
        public static bool FindDatabaseVersion(int Major, int Minor, int Build)
        {
            bool version = false;
            IDataReader dr = null;
            try
            {
                dr = DataProvider.Instance().FindDatabaseVersion(Major, Minor, Build);
                if (dr.Read())
                {
                    version = true;
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
            return version;
        }

        /// <summary>
        /// Updates the database version.
        /// </summary>
        /// <param name="version">The version.</param>
        public static void UpdateDataBaseVersion(Version version)
        {
            //update the version
            DataProvider.Instance().UpdateDatabaseVersion(version.Major, version.Minor, version.Build, DotNetNukeContext.Current.Application.Name);
            _dataBaseVersion = version;
        }

        /// <summary>
        /// Adds the port.
        /// </summary>
        /// <param name="httpAlias">The HTTP alias.</param>
        /// <param name="originalUrl">The original URL.</param>
        /// <returns>url with port if the post number is not 80</returns>
        public static string AddPort(string httpAlias, string originalUrl)
        {
            var uri = new Uri(originalUrl);
            var aliasUri = new Uri(httpAlias);

            if (!uri.IsDefaultPort)
            {
                httpAlias = AddHTTP(aliasUri.Host + ":" + uri.Port + aliasUri.LocalPath);
            }

            return httpAlias;
        }

        /// <summary>
        /// Gets the name of the domain.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns>domain name</returns>
        public static string GetDomainName(HttpRequest request)
        {
            return GetDomainName(new HttpRequestWrapper(request), false);
        }

        /// <summary>
        /// Gets the name of the domain.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns>domain name</returns>
        public static string GetDomainName(HttpRequestBase request)
        {
            return GetDomainName(request, false);
        }

        /// <summary>
        /// returns the domain name of the current request ( ie. www.domain.com or 207.132.12.123 or www.domain.com/directory if subhost )
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="parsePortNumber">if set to <c>true</c> [parse port number].</param>
        /// <returns>domain name</returns>
        public static string GetDomainName(HttpRequest request, bool parsePortNumber)
        {
            return GetDomainName(new HttpRequestWrapper(request), parsePortNumber);
        }

        /// <summary>
        /// returns the domain name of the current request ( ie. www.domain.com or 207.132.12.123 or www.domain.com/directory if subhost )
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="parsePortNumber">if set to <c>true</c> [parse port number].</param>
        /// <returns>domain name</returns>
        public static string GetDomainName(HttpRequestBase request, bool parsePortNumber)
        {
            return TestableGlobals.Instance.GetDomainName(request.Url, parsePortNumber);
        }

        /// <summary>
        /// Determin whether use port number by the value in config file.
        /// </summary>
        /// <returns>
        /// <c>true</c> if use port number, otherwise, return <c>false</c>.
        /// </returns>
        public static bool UsePortNumber()
        {
            bool usePort = true;
            if ((Config.GetSetting("UsePortNumber") != null))
            {
                usePort = bool.Parse(Config.GetSetting("UsePortNumber"));
            }
            return usePort;
        }

        /// <summary>
        /// Gets the file list.
        /// </summary>
        /// <returns>file list.</returns>
        public static ArrayList GetFileList()
        {
            return GetFileList(-1, "", true, "", false);
        }

        /// <summary>
        /// Gets the file list.
        /// </summary>
        /// <param name="portalId">The portal id.</param>
        /// <returns>file list</returns>
        public static ArrayList GetFileList(int portalId)
        {
            return GetFileList(portalId, "", true, "", false);
        }

        /// <summary>
        /// Gets the file list.
        /// </summary>
        /// <param name="portalId">The portal id.</param>
        /// <param name="strExtensions">The STR extensions.</param>
        /// <returns>file list</returns>
        public static ArrayList GetFileList(int portalId, string strExtensions)
        {
            return GetFileList(portalId, strExtensions, true, "", false);
        }

        /// <summary>
        /// Gets the file list.
        /// </summary>
        /// <param name="portalId">The portal id.</param>
        /// <param name="strExtensions">The STR extensions.</param>
        /// <param name="noneSpecified">if set to <c>true</c> [none specified].</param>
        /// <returns>file list</returns>
        public static ArrayList GetFileList(int portalId, string strExtensions, bool noneSpecified)
        {
            return GetFileList(portalId, strExtensions, noneSpecified, "", false);
        }

        /// <summary>
        /// Gets the file list.
        /// </summary>
        /// <param name="PortalId">The portal id.</param>
        /// <param name="strExtensions">The STR extensions.</param>
        /// <param name="NoneSpecified">if set to <c>true</c> [none specified].</param>
        /// <param name="Folder">The folder.</param>
        /// <returns>file list</returns>
        public static ArrayList GetFileList(int PortalId, string strExtensions, bool NoneSpecified, string Folder)
        {
            return GetFileList(PortalId, strExtensions, NoneSpecified, Folder, false);
        }

        /// <summary>
        /// Gets the file list.
        /// </summary>
        /// <param name="PortalId">The portal id.</param>
        /// <param name="strExtensions">The STR extensions.</param>
        /// <param name="NoneSpecified">if set to <c>true</c> [none specified].</param>
        /// <param name="Folder">The folder.</param>
        /// <param name="includeHidden">if set to <c>true</c> [include hidden].</param>
        /// <returns>file list</returns>
        public static ArrayList GetFileList(int PortalId, string strExtensions, bool NoneSpecified, string Folder, bool includeHidden)
        {
            var arrFileList = new ArrayList();
            if (NoneSpecified)
            {
                arrFileList.Add(new FileItem("", "<" + Localization.GetString("None_Specified") + ">"));
            }

            var objFolder = FolderManager.Instance.GetFolder(PortalId, Folder);

            if (objFolder != null)
            {
                try
                {
                    var files = FolderManager.Instance.GetFiles(objFolder);
                    var fileManager = FileManager.Instance;

                    foreach (var file in files)
                    {
                        if (FilenameMatchesExtensions(file.FileName, strExtensions))
                        {
                            if (file.SupportsFileAttributes)
                            {
                                if (fileManager.FileExists(objFolder, file.FileName))
                                {
                                    if (includeHidden)
                                    {
                                        arrFileList.Add(new FileItem(file.FileId.ToString(), file.FileName));
                                    }
                                    else
                                    {
                                        var attributes = file.FileAttributes;

                                        if ((attributes & FileAttributes.Hidden) != FileAttributes.Hidden)
                                        {
                                            arrFileList.Add(new FileItem(file.FileId.ToString(), file.FileName));
                                        }
                                    }
                                }
                            }
                            else
                            {
                                //File is stored in DB - Just add to arraylist
                                arrFileList.Add(new FileItem(file.FileId.ToString(), file.FileName));
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Exceptions.LogException(ex);
                }
            }
            return arrFileList;
        }

        /// <summary>
        /// Gets the host portal settings.
        /// </summary>
        /// <returns>Host portal settings</returns>
        public static PortalSettings GetHostPortalSettings()
        {
            int TabId = -1;
            int PortalId = -1;
            PortalAliasInfo objPortalAliasInfo = null;
            //if the portal alias exists
            if (Host.HostPortalID > Null.NullInteger)
            {
                PortalId = Host.HostPortalID;
                // use the host portal
                objPortalAliasInfo = new PortalAliasInfo();
                objPortalAliasInfo.PortalID = PortalId;
            }
            // load the PortalSettings into current context
            return new PortalSettings(TabId, objPortalAliasInfo);
        }

        /// <summary>
        /// Gets the portal domain name.
        /// </summary>
        /// <param name="strPortalAlias">The STR portal alias.</param>
        /// <param name="Request">The request.</param>
        /// <param name="blnAddHTTP">if set to <c>true</c> [BLN add HTTP].</param>
        /// <returns>domain name</returns>
        public static string GetPortalDomainName(string strPortalAlias, HttpRequest Request, bool blnAddHTTP)
        {
            string strDomainName = "";
            string strURL = "";
            int intAlias;
            if (Request != null)
            {
                strURL = GetDomainName(Request);
            }
            string[] arrPortalAlias = strPortalAlias.Split(',');
            for (intAlias = 0; intAlias <= arrPortalAlias.Length - 1; intAlias++)
            {
                if (arrPortalAlias[intAlias] == strURL)
                {
                    strDomainName = arrPortalAlias[intAlias];
                }
            }
            if (String.IsNullOrEmpty(strDomainName))
            {
                strDomainName = arrPortalAlias[0];
            }
            if (blnAddHTTP)
            {
                strDomainName = AddHTTP(strDomainName);
            }
            return strDomainName;
        }

        /// <summary>
        /// Gets the portal settings.
        /// </summary>
        /// <returns>Portal settings</returns>
        public static PortalSettings GetPortalSettings()
        {
            PortalSettings portalSettings = null;
            //Try getting the settings from the Context
            if (HttpContext.Current != null)
            {
                portalSettings = (PortalSettings)HttpContext.Current.Items["PortalSettings"];
            }
            //If nothing then try getting the Host Settings
            if (portalSettings == null)
            {
                portalSettings = GetHostPortalSettings();
            }
            return portalSettings;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Returns the folder path under the root for the portal 
        /// </summary>
        /// <param name="strFileNamePath">The folder the absolute path</param>
        /// <param name="portalId">Portal Id.</param>
        /// <remarks>
        /// </remarks>
        /// <history>
        ///   [cnurse] 16/9/2004  Updated for localization, Help and 508
        ///   [Philip Beadle] 6 October 2004 Moved to Globals from WebUpload.ascx.vb so can be accessed by URLControl.ascx
        /// </history>
        /// -----------------------------------------------------------------------------
        public static string GetSubFolderPath(string strFileNamePath, int portalId)
        {
            string ParentFolderName;
            if (portalId == Null.NullInteger)
            {
                ParentFolderName = HostMapPath.Replace("/", "\\");
            }
            else
            {
                var objPortals = new PortalController();
                PortalInfo objPortal = objPortals.GetPortal(portalId);
                ParentFolderName = objPortal.HomeDirectoryMapPath.Replace("/", "\\");
            }
            string strFolderpath = strFileNamePath.Substring(0, strFileNamePath.LastIndexOf("\\") + 1);
            return strFolderpath.Substring(ParentFolderName.Length).Replace("\\", "/");
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// The GetTotalRecords method gets the number of Records returned.
        /// </summary>
        /// <param name="dr">An <see cref="IDataReader"/> containing the Total no of records</param>
        /// <returns>An Integer</returns>
        /// <history>
        /// 	[cnurse]	02/01/2006	Created
        /// </history>
        /// -----------------------------------------------------------------------------
        public static int GetTotalRecords(ref IDataReader dr)
        {
            int total = 0;
            if (dr.Read())
            {
                try
                {
                    total = Convert.ToInt32(dr["TotalRecords"]);
                }
                catch (Exception exc)
                {
                    Logger.Error(exc);
                    total = -1;
                }
            }
            return total;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// GetStatus - determines whether an upgrade/install is required and sest the
        /// Database Version and Status accordingly
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <history>
        /// 	[cnurse]	02/12/2008	created
        /// </history>
        /// -----------------------------------------------------------------------------
        [Obsolete("Replaced in DotNetNuke 6.0 by Globals.Status Property")]
        public static void GetStatus()
        {
            //There is no need to do anything here since the backing propery (Globals.Status) is now lazy loaded.
        }

        /// <summary>
        /// Sets the status.
        /// </summary>
        /// <param name="status">The status.</param>
        public static void SetStatus(UpgradeStatus status)
        {
            _status = status;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// ImportFile - converts a file url (/Portals/0/somefile.gif) to the appropriate 
        /// FileID=xx identification for use in importing portals, tabs and modules
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <returns>An UpgradeStatus enum Upgrade/Install/None</returns>
        /// <history>
        /// 	[cnurse]	10/11/2007	moved from PortalController so the same 
        ///                             logic can be used by Module and Tab templates
        /// </history>
        /// -----------------------------------------------------------------------------
        public static string ImportFile(int PortalId, string url)
        {
            string strUrl = url;
            if (GetURLType(url) == TabType.File)
            {
                var fileName = Path.GetFileName(url);

                var folderPath = url.Substring(0, url.LastIndexOf(fileName));
                var folder = FolderManager.Instance.GetFolder(PortalId, folderPath);

                if (folder != null)
                {
                    var file = FileManager.Instance.GetFile(folder, fileName);
                    if (file != null)
                    {
                        strUrl = "FileID=" + file.FileId;
                    }
                }
            }

            return strUrl;
        }

        /// <summary>
        /// Encode the post url
        /// </summary>
        /// <param name="strPost">The post url.</param>
        /// <returns>encoded value</returns>
        public static string HTTPPOSTEncode(string strPost)
        {
            strPost = strPost.Replace("\\", "");
            strPost = HttpUtility.UrlEncode(strPost);
            strPost = strPost.Replace("%2f", "/");
            return strPost;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Sets the ApplicationName for the MemberRole API
        /// </summary>
        /// <remarks>
        /// This overload takes a the PortalId
        /// </remarks>
        ///	<param name="PortalID">The Portal Id</param>
        /// <history>
        ///		[cnurse]	01/18/2005	documented
        /// </history>
        /// -----------------------------------------------------------------------------
        public static void SetApplicationName(int PortalID)
        {
            HttpContext.Current.Items["ApplicationName"] = GetApplicationName(PortalID);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Sets the ApplicationName for the MemberRole API
        /// </summary>
        /// <remarks>
        /// This overload takes a the PortalId
        /// </remarks>
        ///	<param name="ApplicationName">The Application Name to set</param>
        /// <history>
        ///		[cnurse]	01/18/2005	documented
        /// </history>
        /// -----------------------------------------------------------------------------
        public static void SetApplicationName(string ApplicationName)
        {
            HttpContext.Current.Items["ApplicationName"] = ApplicationName;
        }

        /// <summary>
        /// Formats the address on a single line ( ie. Unit, Street, City, Region, Country, PostalCode )
        /// </summary>
        /// <param name="Unit">The unit.</param>
        /// <param name="Street">The street.</param>
        /// <param name="City">The city.</param>
        /// <param name="Region">The region.</param>
        /// <param name="Country">The country.</param>
        /// <param name="PostalCode">The postal code.</param>
        /// <returns></returns>
        public static string FormatAddress(object Unit, object Street, object City, object Region, object Country, object PostalCode)
        {
            string strAddress = "";
            if (Unit != null)
            {
                if (!String.IsNullOrEmpty(Unit.ToString().Trim()))
                {
                    strAddress += ", " + Unit;
                }
            }
            if (Street != null)
            {
                if (!String.IsNullOrEmpty(Street.ToString().Trim()))
                {
                    strAddress += ", " + Street;
                }
            }
            if (City != null)
            {
                if (!String.IsNullOrEmpty(City.ToString().Trim()))
                {
                    strAddress += ", " + City;
                }
            }
            if (Region != null)
            {
                if (!String.IsNullOrEmpty(Region.ToString().Trim()))
                {
                    strAddress += ", " + Region;
                }
            }
            if (Country != null)
            {
                if (!String.IsNullOrEmpty(Country.ToString().Trim()))
                {
                    strAddress += ", " + Country;
                }
            }
            if (PostalCode != null)
            {
                if (!String.IsNullOrEmpty(PostalCode.ToString().Trim()))
                {
                    strAddress += ", " + PostalCode;
                }
            }
            if (!String.IsNullOrEmpty(strAddress.Trim()))
            {
                strAddress = strAddress.Substring(2);
            }
            return strAddress;
        }

        /// <summary>
        /// Formats the system.version into the standard format nn.nn.nn
        /// </summary>
        /// <param name="version">The version.</param>
        /// <returns>Formatted  version as string</returns>
        public static string FormatVersion(Version version)
        {
            return FormatVersion(version, false);
        }

        /// <summary>
        /// Formats the version.
        /// </summary>
        /// <param name="version">The version.</param>
        /// <param name="includeBuild">if set to <c>true</c> [include build].</param>
        /// <returns>Formatted version as string</returns>
        /// <example>
        /// <code lang="C#">
        /// var version = new Version(6, 0, 0, 147);
        /// string formattedVersion = FormatVersion(version, true); // formattedVersion's value will be: 06.00.00(147)
        /// </code>
        /// </example>
        public static string FormatVersion(Version version, bool includeBuild)
        {
            string strVersion = version.Major.ToString("00") + "." + version.Minor.ToString("00") + "." + version.Build.ToString("00");
            if (includeBuild)
            {
                strVersion += " (" + version.Revision + ")";
            }
            return strVersion;
        }

        /// <summary>
        /// Formats  a version into the standard format nn.nn.nn
        /// </summary>
        /// <param name="version">The version to be formatted.</param>
        /// <param name="fieldFormat">The field format.</param>
        /// <param name="fieldCount">The field count.</param>
        /// <param name="delimiterCharacter">The delimiter character.</param>
        /// <returns>Formatted version as a string</returns>
        public static string FormatVersion(Version version, string fieldFormat, int fieldCount, string delimiterCharacter)
        {
            string strVersion = "";
            int intZero = 0;
            if (version != null)
            {
                if (fieldCount > 0)
                {
                    if (version.Major >= 0)
                    {
                        strVersion += version.Major.ToString(fieldFormat);
                    }
                    else
                    {
                        strVersion += intZero.ToString(fieldFormat);
                    }
                }
                if (fieldCount > 1)
                {
                    strVersion += delimiterCharacter;
                    if (version.Minor >= 0)
                    {
                        strVersion += version.Minor.ToString(fieldFormat);
                    }
                    else
                    {
                        strVersion += intZero.ToString(fieldFormat);
                    }
                }
                if (fieldCount > 2)
                {
                    strVersion += delimiterCharacter;
                    if (version.Build >= 0)
                    {
                        strVersion += version.Build.ToString(fieldFormat);
                    }
                    else
                    {
                        strVersion += intZero.ToString(fieldFormat);
                    }
                }
                if (fieldCount > 3)
                {
                    strVersion += delimiterCharacter;
                    if (version.Revision >= 0)
                    {
                        strVersion += version.Revision.ToString(fieldFormat);
                    }
                    else
                    {
                        strVersion += intZero.ToString(fieldFormat);
                    }
                }
            }
            return strVersion;
        }

        /// <summary>
        /// Cloaks the text, obfuscate sensitive data to prevent collection by robots and spiders and crawlers
        /// </summary>
        /// <param name="PersonalInfo">The personal info.</param>
        /// <returns>obfuscated sensitive data by hustling ASCII characters</returns>
        public static string CloakText(string PersonalInfo)
        {
            if (PersonalInfo == null)
            {
                return Null.NullString;
            }

            const string Script = @"
                <script type=""text/javascript"">
                    //<![CDATA[
                        document.write(String.fromCharCode({0}))
                    //]]>
                </script>
            ";

            var characterCodes = PersonalInfo.Select(ch => ((int)ch).ToString(CultureInfo.InvariantCulture));
            return string.Format(Script, string.Join(",", characterCodes.ToArray()));
        }

        /// <summary>
        /// Gets the medium date by current culture.
        /// </summary>
        /// <param name="strDate">The date.</param>
        /// <returns>return formatted content of the date if paramter isn't empty, else return the parameter.</returns>
        /// <example>
        /// <code lang="C#">
        /// var mediumDate = GetMediumDate("6/1/2011");
        /// </code>
        /// </example>
        public static string GetMediumDate(string strDate)
        {
            if (!String.IsNullOrEmpty(strDate))
            {
                DateTime datDate = Convert.ToDateTime(strDate);
                string strYear = datDate.Year.ToString();
                string strMonth = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(datDate.Month);
                string strDay = datDate.Day.ToString();
                strDate = strDay + "-" + strMonth + "-" + strYear;
            }
            return strDate;
        }

        /// <summary>
        /// Gets the short date.
        /// </summary>
        /// <param name="strDate">The date.</param>
        /// <returns>short date content of the input.</returns>
        public static string GetShortDate(string strDate)
        {
            if (!String.IsNullOrEmpty(strDate))
            {
                DateTime datDate = Convert.ToDateTime(strDate);
                string strYear = datDate.Year.ToString();
                string strMonth = datDate.Month.ToString();
                string strDay = datDate.Day.ToString();
                strDate = strMonth + "/" + strDay + "/" + strYear;
            }
            return strDate;
        }

        /// <summary>
        /// Determines whether current request contains admin control information.
        /// </summary>
        /// <returns>
        ///   <c>true</c> if current request contains admin control information; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsAdminControl()
        {
            // This is needed to avoid an exception if there is no Context.  This will occur if code is called from the Scheduler
            if (HttpContext.Current == null)
            {
                return false;
            }
            return (!string.IsNullOrEmpty(HttpContext.Current.Request.QueryString["mid"])) || (!string.IsNullOrEmpty(HttpContext.Current.Request.QueryString["ctl"]));
        }

        /// <summary>
        /// Determines whether current request use admin skin.
        /// </summary>
        /// <returns>
        ///   <c>true</c> if current request use admin skin; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsAdminSkin()
        {
            bool _IsAdminSkin = Null.NullBoolean;
            if (HttpContext.Current != null)
            {
                string AdminKeys = "tab,module,importmodule,exportmodule,help";
                string ControlKey = "";
                if (HttpContext.Current.Request.QueryString["ctl"] != null)
                {
                    ControlKey = HttpContext.Current.Request.QueryString["ctl"].ToLower();
                }
                int ModuleID = -1;
                if (HttpContext.Current.Request.QueryString["mid"] != null)
                {
                    Int32.TryParse(HttpContext.Current.Request.QueryString["mid"], out ModuleID);
                }
                _IsAdminSkin = (!String.IsNullOrEmpty(ControlKey) && ControlKey != "view" && ModuleID != -1) ||
                               (!String.IsNullOrEmpty(ControlKey) && AdminKeys.IndexOf(ControlKey) != -1 && ModuleID == -1);
            }
            return _IsAdminSkin;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Returns whether the current tab is in EditMode
        /// </summary>
        /// <returns><c>true</c> if the tab is in Edit mode; otherwise <c>false</c></returns>
        /// <remarks>
        /// </remarks>
        /// <history>
        /// 	[cnurse]	06/04/2009	Created
        /// </history>
        /// -----------------------------------------------------------------------------
        public static bool IsEditMode()
        {
            return (TabPermissionController.CanAddContentToPage() && PortalController.GetCurrentPortalSettings().UserMode == PortalSettings.Mode.Edit);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Returns whether the current tab is in LayoutMode
        /// </summary>
        /// <returns><c>true</c> if the current tab is in layout mode; otherwise <c>false</c></returns>
        /// <remarks>
        /// </remarks>
        /// <history>
        /// 	[Jon Henning]	9/16/2004	Created
        /// </history>
        /// -----------------------------------------------------------------------------
        public static bool IsLayoutMode()
        {
            return (TabPermissionController.CanAddContentToPage() && PortalController.GetCurrentPortalSettings().UserMode == PortalSettings.Mode.Layout);
        }

        /// <summary>
        /// Creates the RSS.
        /// </summary>
        /// <param name="dr">The dr.</param>
        /// <param name="TitleField">The title field.</param>
        /// <param name="URLField">The URL field.</param>
        /// <param name="CreatedDateField">The created date field.</param>
        /// <param name="SyndicateField">The syndicate field.</param>
        /// <param name="DomainName">Name of the domain.</param>
        /// <param name="FileName">Name of the file.</param>
        public static void CreateRSS(IDataReader dr, string TitleField, string URLField, string CreatedDateField, string SyndicateField, string DomainName, string FileName)
        {
            // Obtain PortalSettings from Current Context
            PortalSettings _portalSettings = PortalController.GetCurrentPortalSettings();
            string strRSS = "";
            string strRelativePath = DomainName + FileName.Substring(FileName.IndexOf("\\Portals")).Replace("\\", "/");
            strRelativePath = strRelativePath.Substring(0, strRelativePath.LastIndexOf("/"));
            try
            {
                while (dr.Read())
                {
                    if (Convert.ToInt32(dr[SyndicateField]) > 0)
                    {
                        strRSS += "      <item>" + Environment.NewLine;
                        strRSS += "         <title>" + dr[TitleField] + "</title>" + Environment.NewLine;
                        if (dr["URL"].ToString().IndexOf("://") == -1)
                        {
                            if (Regex.IsMatch(dr["URL"].ToString(), "^\\d+$"))
                            {
                                strRSS += "         <link>" + DomainName + "/" + glbDefaultPage + "?tabid=" + dr[URLField] + "</link>" + Environment.NewLine;
                            }
                            else
                            {
                                strRSS += "         <link>" + strRelativePath + dr[URLField] + "</link>" + Environment.NewLine;
                            }
                        }
                        else
                        {
                            strRSS += "         <link>" + dr[URLField] + "</link>" + Environment.NewLine;
                        }
                        strRSS += "         <description>" + _portalSettings.PortalName + " " + GetMediumDate(dr[CreatedDateField].ToString()) + "</description>" + Environment.NewLine;
                        strRSS += "     </item>" + Environment.NewLine;
                    }
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
            if (!String.IsNullOrEmpty(strRSS))
            {
                strRSS = "<?xml version=\"1.0\" encoding=\"iso-8859-1\"?>" + Environment.NewLine + "<rss version=\"0.91\">" + Environment.NewLine + "  <channel>" + Environment.NewLine + "     <title>" +
                         _portalSettings.PortalName + "</title>" + Environment.NewLine + "     <link>" + DomainName + "</link>" + Environment.NewLine + "     <description>" +
                         _portalSettings.PortalName + "</description>" + Environment.NewLine + "     <language>en-us</language>" + Environment.NewLine + "     <copyright>" +
                         (!string.IsNullOrEmpty(_portalSettings.FooterText) ? _portalSettings.FooterText.Replace("[year]", DateTime.Now.Year.ToString()) : string.Empty) +
                         "</copyright>" + Environment.NewLine + "     <webMaster>" + _portalSettings.Email + "</webMaster>" + Environment.NewLine + strRSS + "   </channel>" + Environment.NewLine +
                         "</rss>";
                StreamWriter objStream;
                objStream = File.CreateText(FileName);
                objStream.WriteLine(strRSS);
                objStream.Close();
            }
            else
            {
                if (File.Exists(FileName))
                {
                    File.Delete(FileName);
                }
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// injects the upload directory into raw HTML for src and background tags
        /// </summary>
        /// <param name="strHTML">raw HTML text</param>
        /// <param name="strUploadDirectory">path of portal image directory</param>
        /// <returns>HTML with paths for images and background corrected</returns>
        /// <remarks>
        /// </remarks>
        /// <history>
        /// 	[sleupold]	8/18/2007	corrected and refactored
        /// </history>
        /// -----------------------------------------------------------------------------
        public static string ManageUploadDirectory(string strHTML, string strUploadDirectory)
        {
            strHTML = ManageTokenUploadDirectory(strHTML, strUploadDirectory, "src");
            return ManageTokenUploadDirectory(strHTML, strUploadDirectory, "background");
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// injects the upload directory into raw HTML for a single token
        /// </summary>
        /// <param name="strHTML">raw HTML text</param>
        /// <param name="strUploadDirectory">path of portal image directory</param>
        /// <param name="strToken">token to be replaced</param>
        /// <returns>HTML with paths for images and background corrected</returns>
        /// <remarks>
        /// called by ManageUploadDirectory for each token.
        /// </remarks>
        /// <history>
        /// 	[sleupold]	8/18/2007	created as refactoring of ManageUploadDirectory
        ///                             added proper handling of subdirectory installations.
        ///     [sleupold] 11/03/2007   case insensitivity added
        /// </history>
        /// -----------------------------------------------------------------------------
        public static string ManageTokenUploadDirectory(string strHTML, string strUploadDirectory, string strToken)
        {
            int P;
            int R;
            int S = 0;
            int tLen;
            string strURL;
            var sbBuff = new StringBuilder("");
            if (!String.IsNullOrEmpty(strHTML))
            {
                tLen = strToken.Length + 2;
                string _UploadDirectory = strUploadDirectory.ToLower();
                //find position of first occurrance:
                P = strHTML.IndexOf(strToken + "=\"", StringComparison.InvariantCultureIgnoreCase);
                while (P != -1)
                {
                    sbBuff.Append(strHTML.Substring(S, P - S + tLen)); //keep charactes left of URL
                    S = P + tLen; //save startpos of URL
                    R = strHTML.IndexOf("\"", S); //end of URL
                    if (R >= 0)
                    {
                        strURL = strHTML.Substring(S, R - S).ToLower();
                    }
                    else
                    {
                        strURL = strHTML.Substring(S).ToLower();
                    }
                    // add uploaddirectory if we are linking internally and the uploaddirectory is not already included
                    if (!strURL.Contains("://") && !strURL.StartsWith("/") && !strURL.StartsWith(_UploadDirectory))
                    {
                        sbBuff.Append(strUploadDirectory);
                    }
                    //find position of next occurrance:
                    P = strHTML.IndexOf(strToken + "=\"", S + strURL.Length + 2, StringComparison.InvariantCultureIgnoreCase);
                }
                if (S > -1)
                {
                    sbBuff.Append(strHTML.Substring(S));
                }
            }
            return sbBuff.ToString();
        }

        /// <summary>
        /// Finds the control recursive from child to parent.
        /// </summary>
        /// <param name="objControl">current control.</param>
        /// <param name="strControlName">the control name which want to find out.</param>
        /// <returns>control which'name is strControlName, or else return null if didn't match any control.</returns>
        public static Control FindControlRecursive(Control objControl, string strControlName)
        {
            if (objControl.Parent == null)
            {
                return null;
            }
            else
            {
                if (objControl.Parent.FindControl(strControlName) != null)
                {
                    return objControl.Parent.FindControl(strControlName);
                }
                else
                {
                    return FindControlRecursive(objControl.Parent, strControlName);
                }
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Searches control hierarchy from top down to find a control matching the passed in name
        /// </summary>
        /// <param name="objParent">Root control to begin searching</param>
        /// <param name="strControlName">Name of control to look for</param>
        /// <returns></returns>
        /// <remarks>
        /// This differs from FindControlRecursive in that it looks down the control hierarchy, whereas, the 
        /// FindControlRecursive starts at the passed in control and walks the tree up.  Therefore, this function is 
        /// more a expensive task.
        /// </remarks>
        /// <history>
        /// 	[Jon Henning]	9/17/2004	Created
        /// </history>
        /// -----------------------------------------------------------------------------
        public static Control FindControlRecursiveDown(Control objParent, string strControlName)
        {
            Control objCtl;
            objCtl = objParent.FindControl(strControlName);
            if (objCtl == null)
            {
                foreach (Control objChild in objParent.Controls)
                {
                    objCtl = FindControlRecursiveDown(objChild, strControlName);
                    if (objCtl != null)
                    {
                        break;
                    }
                }
            }
            return objCtl;
        }

        /// <summary>
        /// Sets the form focus.
        /// </summary>
        /// <param name="control">The control.</param>
        public static void SetFormFocus(Control control)
        {
            if (control.Page != null && control.Visible)
            {
                if (control.Page.Request.Browser.EcmaScriptVersion.Major >= 1)
                {
                    //JH dnn.js mod
                    if (ClientAPI.ClientAPIDisabled() == false)
                    {
                        ClientAPI.RegisterClientReference(control.Page, ClientAPI.ClientNamespaceReferences.dnn);
                        DNNClientAPI.SetInitialFocus(control.Page, control);
                    }
                    else
                    {
                        //Create JavaScript 
                        var sb = new StringBuilder();
                        sb.Append("<script type=\"text/javascript\">");
                        sb.Append("<!--");
                        sb.Append(Environment.NewLine);
                        sb.Append("function SetInitialFocus() {");
                        sb.Append(Environment.NewLine);
                        sb.Append(" document.");
                        //Find the Form
                        Control objParent = control.Parent;
                        while (!(objParent is HtmlForm))
                        {
                            objParent = objParent.Parent;
                        }
                        sb.Append(objParent.ClientID);
                        sb.Append("['");
                        sb.Append(control.UniqueID);
                        sb.Append("'].focus(); }");
                        sb.Append("window.onload = SetInitialFocus;");
                        sb.Append(Environment.NewLine);
                        sb.Append("// -->");
                        sb.Append(Environment.NewLine);
                        sb.Append("</script>");
                        // Register Client Script 
                        ClientAPI.RegisterClientScriptBlock(control.Page, "InitialFocus", sb.ToString());
                    }
                }
            }
        }

        /// <summary>
        /// Gets the external request.
        /// </summary>
        /// <param name="Address">The address.</param>
        /// <returns>Web request</returns>
        public static HttpWebRequest GetExternalRequest(string Address)
        {
            //Obtain PortalSettings from Current Context
            PortalSettings _portalSettings = PortalController.GetCurrentPortalSettings();
            //Create the request object
            var objRequest = (HttpWebRequest)WebRequest.Create(Address);
            //Set a time out to the request ... 10 seconds
            objRequest.Timeout = Host.WebRequestTimeout;
            //Attach a User Agent to the request
            objRequest.UserAgent = "DotNetNuke";
            //If there is Proxy info, apply it to the request
            if (!string.IsNullOrEmpty(Host.ProxyServer))
            {
                //Create a new Proxy
                WebProxy Proxy;
                //Create a new Network Credentials item
                NetworkCredential ProxyCredentials;
                //Fill Proxy info from host settings
                Proxy = new WebProxy(Host.ProxyServer, Host.ProxyPort);
                if (!string.IsNullOrEmpty(Host.ProxyUsername))
                {
                    //Fill the credential info from host settings
                    ProxyCredentials = new NetworkCredential(Host.ProxyUsername, Host.ProxyPassword);
                    //Apply credentials to proxy
                    Proxy.Credentials = ProxyCredentials;
                }
                //Apply Proxy to request
                objRequest.Proxy = Proxy;
            }
            return objRequest;
        }

        /// <summary>
        /// Gets the external request.
        /// </summary>
        /// <param name="Address">The address.</param>
        /// <param name="Credentials">The credentials.</param>
        /// <returns>Web request</returns>
        public static HttpWebRequest GetExternalRequest(string Address, NetworkCredential Credentials)
        {
            //Create the request object
            var objRequest = (HttpWebRequest)WebRequest.Create(Address);
            // Set a time out to the request ... 10 seconds
            objRequest.Timeout = Host.WebRequestTimeout;
            // Attach a User Agent to the request
            objRequest.UserAgent = "DotNetNuke";
            // Attach supplied credentials
            if (Credentials.UserName != null)
            {
                objRequest.Credentials = Credentials;
            }
            // If there is Proxy info, apply it to the request
            if (!string.IsNullOrEmpty(Host.ProxyServer))
            {
                // Create a new Proxy
                WebProxy Proxy;
                // Create a new Network Credentials item
                NetworkCredential ProxyCredentials;
                // Fill Proxy info from host settings
                Proxy = new WebProxy(Host.ProxyServer, Host.ProxyPort);
                if (!string.IsNullOrEmpty(Host.ProxyUsername))
                {
                    // Fill the credential info from host settings
                    ProxyCredentials = new NetworkCredential(Host.ProxyUsername, Host.ProxyPassword);
                    //Apply credentials to proxy
                    Proxy.Credentials = ProxyCredentials;
                }
                objRequest.Proxy = Proxy;
            }
            return objRequest;
        }

        /// <summary>
        /// Deletes the folder recursive, include the folder itself will be deleted.
        /// </summary>
        /// <param name="strRoot">The root.</param>
        public static void DeleteFolderRecursive(string strRoot)
        {
            if (!String.IsNullOrEmpty(strRoot))
            {
                if (Directory.Exists(strRoot))
                {
                    foreach (string strFolder in Directory.GetDirectories(strRoot))
                    {
                        DeleteFolderRecursive(strFolder);
                    }
                    foreach (string strFile in Directory.GetFiles(strRoot))
                    {
                        try
                        {
                            FileSystemUtils.DeleteFile(strFile);
                        }
                        catch (Exception ex)
                        {
                            Logger.Error(ex);
                        }
                    }
                    try
                    {
                        Directory.Delete(strRoot);
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(ex);
                    }
                }
            }
        }

        /// <summary>
        /// Deletes the files recursive which match the filter, will not delete folders and will ignore folder which is hidden or system.
        /// </summary>
        /// <param name="strRoot">The root.</param>
        /// <param name="filter">The filter.</param>
        public static void DeleteFilesRecursive(string strRoot, string filter)
        {
            if (!String.IsNullOrEmpty(strRoot))
            {
                if (Directory.Exists(strRoot))
                {
                    foreach (string strFolder in Directory.GetDirectories(strRoot))
                    {
                        var directory = new DirectoryInfo(strFolder);
                        if ((directory.Attributes & FileAttributes.Hidden) == 0 && (directory.Attributes & FileAttributes.System) == 0)
                        {
                            DeleteFilesRecursive(strFolder, filter);
                        }
                    }
                    foreach (string strFile in Directory.GetFiles(strRoot, "*" + filter))
                    {
                        try
                        {
                            FileSystemUtils.DeleteFile(strFile);
                        }
                        catch (Exception ex)
                        {
                            Logger.Error(ex);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Cleans the name of the file.
        /// </summary>
        /// <param name="FileName">Name of the file.</param>
        /// <returns>clean name</returns>
        public static string CleanFileName(string FileName)
        {
            return CleanFileName(FileName, "", "");
        }

        /// <summary>
        /// Cleans the name of the file.
        /// </summary>
        /// <param name="FileName">Name of the file.</param>
        /// <param name="BadChars">The bad chars.</param>
        /// <returns>clean name</returns>
        public static string CleanFileName(string FileName, string BadChars)
        {
            return CleanFileName(FileName, BadChars, "");
        }

        /// <summary>
        /// Cleans the name of the file.
        /// </summary>
        /// <param name="FileName">Name of the file.</param>
        /// <param name="BadChars">The bad chars.</param>
        /// <param name="ReplaceChar">The replace char.</param>
        /// <returns>clean name</returns>
        public static string CleanFileName(string FileName, string BadChars, string ReplaceChar)
        {
            string strFileName = FileName;
            if (String.IsNullOrEmpty(BadChars))
            {
                BadChars = ":/\\?*|" + ((char)34) + ((char)39) + ((char)9);
            }
            if (String.IsNullOrEmpty(ReplaceChar))
            {
                ReplaceChar = "_";
            }
            int intCounter;
            for (intCounter = 0; intCounter <= BadChars.Length - 1; intCounter++)
            {
                strFileName = strFileName.Replace(BadChars.Substring(intCounter, 1), ReplaceChar);
            }
            return strFileName;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// CleanName - removes characters from Module/Tab names that are being used for file names
        /// in Module/Tab Import/Export.  
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <returns>A cleaned string</returns>
        /// <history>
        /// 	[cnurse]	10/11/2007	moved from Import/Export Module user controls to avoid 
        ///                             duplication and for use in new Import and Export Tab
        ///                             user controls
        /// </history>
        /// -----------------------------------------------------------------------------
        public static string CleanName(string Name)
        {
            string strName = Name;
            string strBadChars = ". ~`!@#$%^&*()-_+={[}]|\\:;<,>?/" + ((char)34) + ((char)39);
            int intCounter;
            for (intCounter = 0; intCounter <= strBadChars.Length - 1; intCounter++)
            {
                strName = strName.Replace(strBadChars.Substring(intCounter, 1), "");
            }
            return strName;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///   CreateValidClass - removes characters from Module/Tab names which are invalid  
        ///   for use as an XHTML class attribute / CSS class selector value and optionally
        ///   prepends the letter 'A' if the first character is not alphabetic.  This differs 
        ///   from <see>CreateValidID</see> which replaces invalid characters with an underscore
        ///   and replaces the first letter with an 'A' if it is not alphabetic
        /// </summary>
        /// <param name = "inputValue">String to use to create the class value</param>
        /// <param name = "validateFirstChar">If set true, validate whether the first character
        ///   is alphabetic and, if not, prepend the letter 'A' to the returned value</param>
        /// <remarks>
        /// </remarks>
        /// <returns>A string suitable for use as a class value</returns>
        /// <history>
        ///   [jenni]	27/10/2010	Created
        /// </history>
        /// -----------------------------------------------------------------------------
        public static string CreateValidClass(string inputValue, bool validateFirstChar)
        {
            string returnValue = Null.NullString;

            //Regex is expensive so we will cache the results in a lookup table
            var validClassLookupDictionary = CBO.GetCachedObject<SharedDictionary<string, string>>(new CacheItemArgs("ValidClassLookup", 200, CacheItemPriority.NotRemovable),
                                                                                                   (CacheItemArgs cacheItemArgs) => new SharedDictionary<string, string>());

            bool idFound = Null.NullBoolean;
            using (ISharedCollectionLock readLock = validClassLookupDictionary.GetReadLock())
            {
                if (validClassLookupDictionary.ContainsKey(inputValue))
                {
                    //Return value
                    returnValue = validClassLookupDictionary[inputValue];
                    idFound = true;
                }
            }

            if (!idFound)
            {
                using (ISharedCollectionLock writeLock = validClassLookupDictionary.GetWriteLock())
                {
                    if (!validClassLookupDictionary.ContainsKey(inputValue))
                    {
                        //Create Valid Class
                        // letters ([a-zA-Z]), digits ([0-9]), hyphens ("-") and underscores ("_") are valid in class values
                        // Remove all characters that aren't in the list
                        var invalidCharacters = new Regex("[^A-Z0-9_-]", RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);
                        returnValue = invalidCharacters.Replace(inputValue, string.Empty);

                        // If we're asked to validate the first character...
                        if ((validateFirstChar))
                        {
                            // classes should begin with a letter ([A-Za-z])' 
                            // prepend a starting non-letter character with an A
                            var invalidInitialCharacters = new Regex("^[^A-Z]", RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);
                            if ((invalidCharacters.IsMatch(returnValue)))
                            {
                                returnValue = "A" + returnValue;
                            }
                        }

                        //put in Dictionary
                        validClassLookupDictionary[inputValue] = returnValue;
                    }
                }
            }

            //Return Value
            return returnValue;
        }

        /// <summary>
        ///   Creates the valid ID.
        /// </summary>
        /// <param name = "inputValue">The input value.</param>
        /// <returns>String with a valid ID</returns>
        public static string CreateValidID(string inputValue)
        {
            string returnValue = Null.NullString;

            //Regex is expensive so we will cache the results in a lookup table
            var validIDLookupDictionary = CBO.GetCachedObject<SharedDictionary<string, string>>(new CacheItemArgs("ValidIDLookup", 200, CacheItemPriority.NotRemovable),
                                                                                                (CacheItemArgs cacheItemArgs) => new SharedDictionary<string, string>());

            bool idFound = Null.NullBoolean;
            using (ISharedCollectionLock readLock = validIDLookupDictionary.GetReadLock())
            {
                if (validIDLookupDictionary.ContainsKey(inputValue))
                {
                    //Return value
                    returnValue = validIDLookupDictionary[inputValue];
                    idFound = true;
                }
            }

            if (!idFound)
            {
                using (ISharedCollectionLock writeLock = validIDLookupDictionary.GetWriteLock())
                {
                    if (!validIDLookupDictionary.ContainsKey(inputValue))
                    {
                        //Create Valid ID
                        // '... letters, digits ([0-9]), hyphens ("-"), underscores ("_"), colons (":"), and periods (".")' are valid identifiers
                        // We aren't allowing hyphens or periods, even though they're valid, since the previous version of this function didn't
                        // Replace all characters that aren't in the list with an underscore
                        var invalidCharacters = new Regex("[^A-Z0-9_:]", RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);
                        returnValue = invalidCharacters.Replace(inputValue, "_");

                        // identifiers '... must begin with a letter ([A-Za-z])' 
                        // replace a starting non-letter character with an A
                        var invalidInitialCharacters = new Regex("^[^A-Z]", RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);
                        returnValue = invalidInitialCharacters.Replace(returnValue, "A");

                        //put in Dictionary
                        validIDLookupDictionary[inputValue] = returnValue;
                    }
                }
            }
            return returnValue;
        }


        /// <summary>
        /// Get the path of page to show access denied message.
        /// </summary>
        /// <returns>url of access denied</returns>
        public static string AccessDeniedURL()
        {
            return AccessDeniedURL("");
        }

        /// <summary>
        /// Get the path of page to show access denied message.
        /// </summary>
        /// <param name="Message">The message.</param>
        /// <returns>url of access denied</returns>
        public static string AccessDeniedURL(string Message)
        {
            string strURL = "";
            PortalSettings _portalSettings = PortalController.GetCurrentPortalSettings();
            if (HttpContext.Current.Request.IsAuthenticated)
            {
                if (String.IsNullOrEmpty(Message))
                {
                    //redirect to access denied page
                    strURL = NavigateURL(_portalSettings.ActiveTab.TabID, "Access Denied");
                }
                else
                {
                    //redirect to access denied page with custom message
                    strURL = NavigateURL(_portalSettings.ActiveTab.TabID, "Access Denied", "message=" + HttpUtility.UrlEncode(Message));
                }
            }
            else
            {
                strURL = LoginURL(HttpUtility.UrlEncode(HttpContext.Current.Request.RawUrl), false);
            }
            return strURL;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Adds HTTP to URL if no other protocol specified
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="strURL">The url</param>
        /// <returns>The formatted url</returns>
        /// <history>
        ///		[cnurse]	12/16/2004	documented
        ///     [cnurse]    05/06/2005  added chack for mailto: protocol
        /// </history>
        /// -----------------------------------------------------------------------------
        public static string AddHTTP(string strURL)
        {
            if (!String.IsNullOrEmpty(strURL))
            {
                if (strURL.IndexOf("mailto:") == -1 && strURL.IndexOf("://") == -1 && strURL.IndexOf("~") == -1 && strURL.IndexOf("\\\\") == -1)
                {
                    if (HttpContext.Current != null && HttpContext.Current.Request.IsSecureConnection)
                    {
                        strURL = "https://" + strURL;
                    }
                    else
                    {
                        strURL = "http://" + strURL;
                    }
                }
            }
            return strURL;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Generates the Application root url (including the tab/page)
        /// </summary>
        /// <remarks>
        /// This overload assumes the current page
        /// </remarks>
        /// <returns>The formatted root url</returns>
        /// <history>
        ///		[cnurse]	12/16/2004	documented
        /// </history>
        /// -----------------------------------------------------------------------------
        public static string ApplicationURL()
        {
            PortalSettings _portalSettings = PortalController.GetCurrentPortalSettings();
            if (_portalSettings != null)
            {
                return (ApplicationURL(_portalSettings.ActiveTab.TabID));
            }
            else
            {
                return (ApplicationURL(-1));
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Generates the Application root url (including the tab/page)
        /// </summary>
        /// <remarks>
        /// This overload takes the tabid (page id) as a parameter
        /// </remarks>
        /// <param name="TabID">The id of the tab/page</param>
        /// <returns>The formatted root url</returns>
        /// <history>
        ///		[cnurse]	12/16/2004	documented
        /// </history>
        /// -----------------------------------------------------------------------------
        public static string ApplicationURL(int TabID)
        {
            string strURL = "~/" + glbDefaultPage;
            if (TabID != -1)
            {
                strURL += "?tabid=" + TabID;
            }
            return strURL;
        }

        /// <summary>
        /// Formats the help URL.
        /// </summary>
        /// <param name="HelpUrl">The help URL.</param>
        /// <param name="objPortalSettings">The portal settings.</param>
        /// <param name="Name">The name.</param>
        /// <returns>Formatted url.</returns>
        public static string FormatHelpUrl(string HelpUrl, PortalSettings objPortalSettings, string Name)
        {
            return FormatHelpUrl(HelpUrl, objPortalSettings, Name, "");
        }

        /// <summary>
        /// Formats the help URL.
        /// </summary>
        /// <param name="HelpUrl">The help URL.</param>
        /// <param name="objPortalSettings">The portal settings.</param>
        /// <param name="Name">The name.</param>
        /// <param name="Version">The version.</param>
        /// <returns>Formatted url.</returns>
        public static string FormatHelpUrl(string HelpUrl, PortalSettings objPortalSettings, string Name, string Version)
        {
            string strURL = HelpUrl;
            if (strURL.IndexOf("?") != -1)
            {
                strURL += "&helpculture=";
            }
            else
            {
                strURL += "?helpculture=";
            }
            if (!String.IsNullOrEmpty(Thread.CurrentThread.CurrentUICulture.ToString().ToLower()))
            {
                strURL += Thread.CurrentThread.CurrentUICulture.ToString().ToLower();
            }
            else
            {
                strURL += objPortalSettings.DefaultLanguage.ToLower();
            }
            if (!String.IsNullOrEmpty(Name))
            {
                strURL += "&helpmodule=" + HttpUtility.UrlEncode(Name);
            }
            if (!String.IsNullOrEmpty(Version))
            {
                strURL += "&helpversion=" + HttpUtility.UrlEncode(Version);
            }
            return AddHTTP(strURL);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Generates the correctly formatted friendly url.
        /// </summary>
        /// <remarks>
        /// Assumes Default.aspx, and that portalsettings are saved to Context
        /// </remarks>
        /// <param name="tab">The current tab</param>
        /// <param name="path">The path to format.</param>
        /// <returns>The formatted (friendly) url</returns>
        /// <history>
        ///		[cnurse]	12/16/2004	documented
        /// </history>
        /// -----------------------------------------------------------------------------
        public static string FriendlyUrl(TabInfo tab, string path)
        {
            return FriendlyUrl(tab, path, glbDefaultPage);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Generates the correctly formatted friendly url
        /// </summary>
        /// <remarks>
        /// This overload includes an optional page to include in the url.
        /// </remarks>
        /// <param name="tab">The current tab</param>
        /// <param name="path">The path to format.</param>
        /// <param name="pageName">The page to include in the url.</param>
        /// <returns>The formatted (friendly) url</returns>
        /// <history>
        ///		[cnurse]	12/16/2004	documented
        /// </history>
        /// -----------------------------------------------------------------------------
        public static string FriendlyUrl(TabInfo tab, string path, string pageName)
        {
            PortalSettings _portalSettings = PortalController.GetCurrentPortalSettings();
            return FriendlyUrl(tab, path, pageName, _portalSettings);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Generates the correctly formatted friendly url
        /// </summary>
        /// <remarks>
        /// This overload includes the portal settings for the site
        /// </remarks>
        /// <param name="tab">The current tab</param>
        /// <param name="path">The path to format.</param>
        /// <param name="settings">The portal Settings</param>
        /// <returns>The formatted (friendly) url</returns>
        /// <history>
        ///		[cnurse]	12/16/2004	documented
        /// </history>
        /// -----------------------------------------------------------------------------
        public static string FriendlyUrl(TabInfo tab, string path, PortalSettings settings)
        {
            return FriendlyUrl(tab, path, glbDefaultPage, settings);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Generates the correctly formatted friendly url
        /// </summary>
        /// <remarks>
        /// This overload includes an optional page to include in the url, and the portal 
        /// settings for the site
        /// </remarks>
        /// <param name="tab">The current tab</param>
        /// <param name="path">The path to format.</param>
        /// <param name="pageName">The page to include in the url.</param>
        /// <param name="settings">The portal Settings</param>
        /// <returns>The formatted (friendly) url</returns>
        /// <history>
        ///		[cnurse]	12/16/2004	documented
        /// </history>
        /// -----------------------------------------------------------------------------
        public static string FriendlyUrl(TabInfo tab, string path, string pageName, PortalSettings settings)
        {
            return FriendlyUrlProvider.Instance().FriendlyUrl(tab, path, pageName, settings);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Generates the correctly formatted friendly url
        /// </summary>
        /// <remarks>
        /// This overload includes an optional page to include in the url, and the portal 
        /// settings for the site
        /// </remarks>
        /// <param name="tab">The current tab</param>
        /// <param name="path">The path to format.</param>
        /// <param name="pageName">The page to include in the url.</param>
        /// <param name="portalAlias">The portal Alias for the site</param>
        /// <returns>The formatted (friendly) url</returns>
        /// <history>
        ///		[cnurse]	12/16/2004	documented
        /// </history>
        /// -----------------------------------------------------------------------------
        public static string FriendlyUrl(TabInfo tab, string path, string pageName, string portalAlias)
        {
            return FriendlyUrlProvider.Instance().FriendlyUrl(tab, path, pageName, portalAlias);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Returns the type of URl (T=other tab, F=file, U=URL, N=normal)
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="URL">The url</param>
        /// <returns>The url type</returns>
        /// <history>
        ///		[cnurse]	12/16/2004	documented
        /// </history>
        /// -----------------------------------------------------------------------------
        public static TabType GetURLType(string URL)
        {
            if (String.IsNullOrEmpty(URL))
            {
                return TabType.Normal;
            }
            if (URL.ToLower().StartsWith("mailto:") == false && URL.IndexOf("://") == -1 && URL.StartsWith("~") == false && URL.StartsWith("\\\\") == false && URL.StartsWith("/") == false)
            {
                if (Regex.IsMatch(URL, @"^\d+$"))
                {
                    return TabType.Tab;
                }
                if (URL.ToLower().StartsWith("userid="))
                {
                    return TabType.Member;
                }
                return TabType.File;
            }
            return TabType.Url;
        }

        /// <summary>
        /// Url's as internal links to Files, Tabs and Users should only be imported if
        /// those files, tabs and users exist. This function parses the url, and checks
        /// whether the internal links exist. 
        /// If the link does not exist, the function will return an empty string
        /// </summary>
        /// <param name="ModuleId">Integer</param>
        /// <param name="url">String</param>
        /// <returns>If an internal link does not exist, an empty string is returned, otherwise the passed in url is returned as is</returns>
        /// <history>
        ///     [erikvb]    06/11/2008     corrected file check and added tab and member check
        /// </history>
        public static string ImportUrl(int ModuleId, string url)
        {
            string strUrl = url;
            TabType urlType = GetURLType(url);
            int intId = -1;
            PortalSettings portalSettings = GetPortalSettings();
            switch (urlType)
            {
                case TabType.File:
                    if (Int32.TryParse(url.Replace("FileID=", ""), out intId))
                    {
                        var objFile = FileManager.Instance.GetFile(intId);
                        if (objFile == null)
                        {
                            //fileId does not exist in the portal
                            strUrl = "";
                        }
                    }
                    else
                    {
                        //failed to get fileId
                        strUrl = "";
                    }
                    break;
                case TabType.Member:
                    if (Int32.TryParse(url.Replace("UserID=", ""), out intId))
                    {
                        if (UserController.GetUserById(portalSettings.PortalId, intId) == null)
                        {
                            //UserId does not exist for this portal
                            strUrl = "";
                        }
                    }
                    else
                    {
                        //failed to get UserId
                        strUrl = "";
                    }
                    break;
                case TabType.Tab:
                    if (Int32.TryParse(url, out intId))
                    {
                        var objTabController = new TabController();
                        if (objTabController.GetTab(intId, portalSettings.PortalId, false) == null)
                        {
                            //the tab does not exist
                            strUrl = "";
                        }
                    }
                    else
                    {
                        //failed to get TabId
                        strUrl = "";
                    }
                    break;
            }
            return strUrl;
        }

        /// <summary>
        /// Gets Login URL.
        /// </summary>
        /// <param name="returnURL">The return URL.</param>
        /// <param name="override">if set to <c>true</c> [@override].</param>
        /// <returns>Formatted url.</returns>
        public static string LoginURL(string returnURL, bool @override)
        {
            string strURL = "";
            var portalSettings = PortalController.GetCurrentPortalSettings();
            if (!string.IsNullOrEmpty(returnURL))
            {
                returnURL = String.Format("returnurl={0}", returnURL);
            }
            if (portalSettings.LoginTabId != -1 && !@override)
            {
                if (ValidateLoginTabID(portalSettings.LoginTabId))
                {
                    strURL = string.IsNullOrEmpty(returnURL)
                                        ? NavigateURL(portalSettings.LoginTabId, "")
                                        : NavigateURL(portalSettings.LoginTabId, "", returnURL);
                }
                else
                {
                    string strMessage = String.Format("error={0}", Localization.GetString("NoLoginControl", Localization.GlobalResourceFile));
                    //No account module so use portal tab
                    strURL = string.IsNullOrEmpty(returnURL)
                                 ? NavigateURL(portalSettings.ActiveTab.TabID, "Login", strMessage)
                                 : NavigateURL(portalSettings.ActiveTab.TabID, "Login", returnURL, strMessage);
                }
            }
            else
            {
                //portal tab
                strURL = string.IsNullOrEmpty(returnURL)
                                ? NavigateURL(portalSettings.ActiveTab.TabID, "Login")
                                : NavigateURL(portalSettings.ActiveTab.TabID, "Login", returnURL);
            }
            return strURL;
        }

        /// <summary>
        /// Gets User profile URL.
        /// </summary>
        /// <param name="userId">The user id.</param>
        /// <returns>Formatted url.</returns>
        public static string UserProfileURL(int userId)
        {
            string strURL = "";
            PortalSettings portalSettings = PortalController.GetCurrentPortalSettings();

            strURL = NavigateURL(portalSettings.UserTabId, "", string.Format("userId={0}", userId));

            return strURL;
        }

        /// <summary>
        /// Gets the navigates URL.
        /// </summary>
        /// <returns>Formatted url.</returns>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public static string NavigateURL()
        {
            PortalSettings portalSettings = PortalController.GetCurrentPortalSettings();
            return NavigateURL(portalSettings.ActiveTab.TabID, Null.NullString);
        }

        /// <summary>
        /// Gets the navigates URL.
        /// </summary>
        /// <param name="tabID">The tab ID.</param>
        /// <returns>Formatted url.</returns>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public static string NavigateURL(int tabID)
        {
            return NavigateURL(tabID, Null.NullString);
        }

        /// <summary>
        /// Gets the navigates URL.
        /// </summary>
        /// <param name="tabID">The tab ID.</param>
        /// <param name="isSuperTab">if set to <c>true</c> [is super tab].</param>
        /// <returns>Formatted url.</returns>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public static string NavigateURL(int tabID, bool isSuperTab)
        {
            PortalSettings _portalSettings = PortalController.GetCurrentPortalSettings();
            string cultureCode = GetCultureCode(tabID, isSuperTab, _portalSettings);
            return NavigateURL(tabID, isSuperTab, _portalSettings, Null.NullString, cultureCode);
        }

        /// <summary>
        /// Gets the navigates URL.
        /// </summary>
        /// <param name="controlKey">The control key.</param>
        /// <returns>Formatted url.</returns>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public static string NavigateURL(string controlKey)
        {
            if (controlKey == "Access Denied")
            {
                return AccessDeniedURL();
            }
            else
            {
                PortalSettings _portalSettings = PortalController.GetCurrentPortalSettings();
                return NavigateURL(_portalSettings.ActiveTab.TabID, controlKey);
            }
        }

        /// <summary>
        /// Gets the navigates URL.
        /// </summary>
        /// <param name="controlKey">The control key.</param>
        /// <param name="additionalParameters">The additional parameters.</param>
        /// <returns>Formatted url.</returns>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public static string NavigateURL(string controlKey, params string[] additionalParameters)
        {
            PortalSettings _portalSettings = PortalController.GetCurrentPortalSettings();
            return NavigateURL(_portalSettings.ActiveTab.TabID, controlKey, additionalParameters);
        }

        /// <summary>
        /// Gets the navigates URL.
        /// </summary>
        /// <param name="tabID">The tab ID.</param>
        /// <param name="controlKey">The control key.</param>
        /// <returns>Formatted url.</returns>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public static string NavigateURL(int tabID, string controlKey)
        {
            PortalSettings _portalSettings = PortalController.GetCurrentPortalSettings();
            return NavigateURL(tabID, _portalSettings, controlKey, null);
        }

        /// <summary>
        /// Gets the navigates URL.
        /// </summary>
        /// <param name="tabID">The tab ID.</param>
        /// <param name="controlKey">The control key.</param>
        /// <param name="additionalParameters">The additional parameters.</param>
        /// <returns>Formatted url.</returns>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public static string NavigateURL(int tabID, string controlKey, params string[] additionalParameters)
        {
            PortalSettings _portalSettings = PortalController.GetCurrentPortalSettings();
            return NavigateURL(tabID, _portalSettings, controlKey, additionalParameters);
        }

        /// <summary>
        /// Gets the navigates URL.
        /// </summary>
        /// <param name="tabID">The tab ID.</param>
        /// <param name="settings">The settings.</param>
        /// <param name="controlKey">The control key.</param>
        /// <param name="additionalParameters">The additional parameters.</param>
        /// <returns>Formatted url.</returns>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public static string NavigateURL(int tabID, PortalSettings settings, string controlKey, params string[] additionalParameters)
        {
            bool isSuperTab = IsHostTab(tabID);

            return NavigateURL(tabID, isSuperTab, settings, controlKey, additionalParameters);
        }

        /// <summary>
        /// Gets the navigates URL.
        /// </summary>
        /// <param name="tabID">The tab ID.</param>
        /// <param name="isSuperTab">if set to <c>true</c> [is super tab].</param>
        /// <param name="settings">The settings.</param>
        /// <param name="controlKey">The control key.</param>
        /// <param name="additionalParameters">The additional parameters.</param>
        /// <returns>Formatted url.</returns>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public static string NavigateURL(int tabID, bool isSuperTab, PortalSettings settings, string controlKey, params string[] additionalParameters)
        {
            string cultureCode = GetCultureCode(tabID, isSuperTab, settings);
            return NavigateURL(tabID, isSuperTab, settings, controlKey, cultureCode, additionalParameters);
        }

        /// <summary>
        /// Gets the navigates URL.
        /// </summary>
        /// <param name="tabID">The tab ID.</param>
        /// <param name="isSuperTab">if set to <c>true</c> [is super tab].</param>
        /// <param name="settings">The settings.</param>
        /// <param name="controlKey">The control key.</param>
        /// <param name="language">The language.</param>
        /// <param name="additionalParameters">The additional parameters.</param>
        /// <returns>Formatted url.</returns>
        public static string NavigateURL(int tabID, bool isSuperTab, PortalSettings settings, string controlKey, string language, params string[] additionalParameters)
        {
            return NavigateURL(tabID, isSuperTab, settings, controlKey, language, glbDefaultPage, additionalParameters);
        }

        /// <summary>
        /// Gets the navigates URL.
        /// </summary>
        /// <param name="tabID">The tab ID.</param>
        /// <param name="isSuperTab">if set to <c>true</c> [is super tab].</param>
        /// <param name="settings">The settings.</param>
        /// <param name="controlKey">The control key.</param>
        /// <param name="language">The language.</param>
        /// <param name="additionalParameters">The additional parameters.</param>
        /// <returns>Formatted url.</returns>
        public static string NavigateURL(int tabID, bool isSuperTab, PortalSettings settings, string controlKey, string language, string pageName, params string[] additionalParameters)
        {
            string url = tabID == Null.NullInteger ? ApplicationURL() : ApplicationURL(tabID);
            if (!String.IsNullOrEmpty(controlKey))
            {
                url += "&ctl=" + controlKey;
            }
            if (additionalParameters != null)
            {
                url = additionalParameters.Where(parameter => !string.IsNullOrEmpty(parameter)).Aggregate(url, (current, parameter) => current + ("&" + parameter));
            }
            if (isSuperTab)
            {
                url += "&portalid=" + settings.PortalId;
            }

            var controller = new TabController();

            TabInfo tab = null;

            if (settings != null)
            {
                tab = controller.GetTab(tabID, isSuperTab ? Null.NullInteger : settings.PortalId, false);
            }

            //only add language to url if more than one locale is enabled
            if (settings != null && language != null && LocaleController.Instance.GetLocales(settings.PortalId).Count > 1)
            {
                if (settings.ContentLocalizationEnabled)
                {
                    if (language == "")
                    {
                        if (tab != null && !string.IsNullOrEmpty(tab.CultureCode))
                        {
                            url += "&language=" + tab.CultureCode;
                        }
                    }
                    else
                    {
                        url += "&language=" + language;
                    }
                }
                else if (settings.EnableUrlLanguage)
                {
                    //legacy pre 5.5 behavior
                    if (language == "")
                    {
                        url += "&language=" + Thread.CurrentThread.CurrentCulture.Name;
                    }
                    else
                    {
                        url += "&language=" + language;
                    }
                }
            }

            if (Host.UseFriendlyUrls || Config.GetFriendlyUrlProvider() == "advanced")
            {
                if (String.IsNullOrEmpty(pageName))
                {
                    pageName = glbDefaultPage;
                }

                url = (settings == null) ? FriendlyUrl(tab, url, pageName) : FriendlyUrl(tab, url, pageName, settings);
            }
            else
            {
                url = ResolveUrl(url);
            }

            return url;
        }

        /// <summary>
        /// UrlEncode query string
        /// </summary>
        /// <param name="QueryString">The query string.</param>
        /// <returns>Encoded content</returns>
        public static string QueryStringEncode(string QueryString)
        {
            QueryString = HttpUtility.UrlEncode(QueryString);
            return QueryString;
        }

        /// <summary>
        /// UrlDecode query string
        /// </summary>
        /// <param name="QueryString">The query string.</param>
        /// <returns>Decoded content</returns>
        public static string QueryStringDecode(string QueryString)
        {
            QueryString = HttpUtility.UrlDecode(QueryString);
            string fullPath;
            try
            {
                fullPath = HttpContext.Current.Request.MapPath(QueryString, HttpContext.Current.Request.ApplicationPath, false);
            }
            catch (HttpException exc)
            {
                Exceptions.ProcessHttpException(exc);
            }
            string strDoubleDecodeURL = HttpContext.Current.Server.UrlDecode(HttpContext.Current.Server.UrlDecode(QueryString));
            if (QueryString.IndexOf("..") != -1 || strDoubleDecodeURL.IndexOf("..") != -1)
            {
                Exceptions.ProcessHttpException();
            }
            return QueryString;
        }

        /// <summary>
        /// Gets Register URL.
        /// </summary>
        /// <param name="returnURL">The return URL.</param>
        /// <param name="originalURL">The original URL.</param>
        /// <returns>Formatted url.</returns>
        public static string RegisterURL(string returnURL, string originalURL)
        {
            string strURL;
            PortalSettings _portalSettings = PortalController.GetCurrentPortalSettings();
            string extraParams = String.Empty;
            if (!string.IsNullOrEmpty(returnURL))
            {
                extraParams = string.Concat("returnurl=", returnURL);
            }
            if (!string.IsNullOrEmpty(originalURL))
            {
                extraParams += string.Concat("&orignalurl=", originalURL);
            }
            if (_portalSettings.RegisterTabId != -1)
            {
                //user defined tab
                strURL = NavigateURL(_portalSettings.RegisterTabId, "", extraParams);
            }
            else
            {
                strURL = NavigateURL(_portalSettings.ActiveTab.TabID, "Register", extraParams);
            }
            return strURL;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Generates the correctly formatted url
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="url">The url to format.</param>
        /// <returns>The formatted (resolved) url</returns>
        /// <history>
        ///		[cnurse]	12/16/2004	documented
        /// </history>
        /// -----------------------------------------------------------------------------
        public static string ResolveUrl(string url)
        {
            // String is Empty, just return Url
            if (String.IsNullOrEmpty(url))
            {
                return url;
            }
            // String does not contain a ~, so just return Url
            if ((url.StartsWith("~") == false))
            {
                return url;
            }
            // There is just the ~ in the Url, return the appPath
            if ((url.Length == 1))
            {
                return ApplicationPath;
            }
            if ((url.ToCharArray()[1] == '/' || url.ToCharArray()[1] == '\\'))
            {
                // Url looks like ~/ or ~\
                if (!string.IsNullOrEmpty(ApplicationPath) && ApplicationPath.Length > 1)
                {
                    return ApplicationPath + "/" + url.Substring(2);
                }
                else
                {
                    return "/" + url.Substring(2);
                }
            }
            else
            {
                // Url look like ~something
                if (!string.IsNullOrEmpty(ApplicationPath) && ApplicationPath.Length > 1)
                {
                    return ApplicationPath + "/" + url.Substring(1);
                }
                else
                {
                    return ApplicationPath + url.Substring(1);
                }
            }
        }

        /// <summary>
        /// Encodes the reserved characters.
        /// </summary>
        /// <param name="QueryString">The query string.</param>
        /// <returns>Encoded content</returns>
        public static string EncodeReservedCharacters(string QueryString)
        {
            QueryString = QueryString.Replace("$", "%24");
            QueryString = QueryString.Replace("&", "%26");
            QueryString = QueryString.Replace("+", "%2B");
            QueryString = QueryString.Replace(",", "%2C");
            QueryString = QueryString.Replace("/", "%2F");
            QueryString = QueryString.Replace(":", "%3A");
            QueryString = QueryString.Replace(";", "%3B");
            QueryString = QueryString.Replace("=", "%3D");
            QueryString = QueryString.Replace("?", "%3F");
            QueryString = QueryString.Replace("@", "%40");
            return QueryString;
        }

        /// <summary>
        /// Dates to string.
        /// </summary>
        /// <param name="DateValue">The date value.</param>
        /// <returns>return value of input with SortableDateTimePattern.</returns>
        public static string DateToString(DateTime DateValue)
        {
            try
            {
                if (!Null.IsNull(DateValue))
                {
                    return DateValue.ToString("s");
                }
                else
                {
                    return Null.NullString;
                }
            }
            catch (Exception exc)
            {
                Logger.Error(exc);

                return Null.NullString;
            }
        }

        /// <summary>
        /// Gets the hash value.
        /// </summary>
        /// <param name="HashObject">The hash object.</param>
        /// <param name="DefaultValue">The default value.</param>
        /// <returns>HashOject's value or DefaultValue if HashObject is null.</returns>
        public static string GetHashValue(object HashObject, string DefaultValue)
        {
            if (HashObject != null)
            {
                if (!String.IsNullOrEmpty(HashObject.ToString()))
                {
                    return Convert.ToString(HashObject);
                }
                else
                {
                    return DefaultValue;
                }
            }
            else
            {
                return DefaultValue;
            }
        }

        /// <summary>
        /// Gets Link click url.
        /// </summary>
        /// <param name="Link">The link.</param>
        /// <param name="TabID">The tab ID.</param>
        /// <param name="ModuleID">The module ID.</param>
        /// <returns>Formatted url.</returns>
        public static string LinkClick(string Link, int TabID, int ModuleID)
        {
            return LinkClick(Link, TabID, ModuleID, true, "");
        }

        /// <summary>
        /// Gets Link click url.
        /// </summary>
        /// <param name="Link">The link.</param>
        /// <param name="TabID">The tab ID.</param>
        /// <param name="ModuleID">The module ID.</param>
        /// <param name="TrackClicks">if set to <c>true</c> [track clicks].</param>
        /// <returns>Formatted url.</returns>
        public static string LinkClick(string Link, int TabID, int ModuleID, bool TrackClicks)
        {
            return LinkClick(Link, TabID, ModuleID, TrackClicks, "");
        }

        /// <summary>
        /// Gets Link click url.
        /// </summary>
        /// <param name="Link">The link.</param>
        /// <param name="TabID">The tab ID.</param>
        /// <param name="ModuleID">The module ID.</param>
        /// <param name="TrackClicks">if set to <c>true</c> [track clicks].</param>
        /// <param name="ContentType">Type of the content.</param>
        /// <returns>Formatted url.</returns>
        public static string LinkClick(string Link, int TabID, int ModuleID, bool TrackClicks, string ContentType)
        {
            return LinkClick(Link, TabID, ModuleID, TrackClicks, !String.IsNullOrEmpty(ContentType));
        }

        /// <summary>
        /// Gets Link click url.
        /// </summary>
        /// <param name="Link">The link.</param>
        /// <param name="TabID">The tab ID.</param>
        /// <param name="ModuleID">The module ID.</param>
        /// <param name="TrackClicks">if set to <c>true</c> [track clicks].</param>
        /// <param name="ForceDownload">if set to <c>true</c> [force download].</param>
        /// <returns>Formatted url.</returns>
        public static string LinkClick(string Link, int TabID, int ModuleID, bool TrackClicks, bool ForceDownload)
        {
            PortalSettings _portalSettings = PortalController.GetCurrentPortalSettings();
            return LinkClick(Link, TabID, ModuleID, TrackClicks, ForceDownload, _portalSettings.PortalId, _portalSettings.EnableUrlLanguage, _portalSettings.GUID.ToString());
        }

        /// <summary>
        /// Gets Link click url.
        /// </summary>
        /// <param name="Link">The link.</param>
        /// <param name="TabID">The tab ID.</param>
        /// <param name="ModuleID">The module ID.</param>
        /// <param name="TrackClicks">if set to <c>true</c> [track clicks].</param>
        /// <param name="ForceDownload">if set to <c>true</c> [force download].</param>
        /// <param name="PortalId">The portal id.</param>
        /// <param name="EnableUrlLanguage">if set to <c>true</c> [enable URL language].</param>
        /// <param name="portalGuid">The portal GUID.</param>
        /// <returns>Formatted url.</returns>
        public static string LinkClick(string Link, int TabID, int ModuleID, bool TrackClicks, bool ForceDownload, int PortalId, bool EnableUrlLanguage, string portalGuid)
        {
            string strLink = "";
            TabType UrlType = GetURLType(Link);
            if (UrlType == TabType.Member)
            {
                strLink = UserProfileURL(Convert.ToInt32(UrlUtils.GetParameterValue(Link)));
            }
            else if (TrackClicks || ForceDownload || UrlType == TabType.File)
            {
                //format LinkClick wrapper
                if (Link.ToLowerInvariant().StartsWith("fileid="))
                {
                    strLink = ApplicationPath + "/LinkClick.aspx?fileticket=" + UrlUtils.EncryptParameter(UrlUtils.GetParameterValue(Link), portalGuid);
                    if (PortalId == Null.NullInteger) //To track Host files
                    {
                        strLink += "&hf=1";                        
                    }
                }
                if (String.IsNullOrEmpty(strLink))
                {
                    strLink = ApplicationPath + "/LinkClick.aspx?link=" + HttpUtility.UrlEncode(Link);
                }
                // tabid is required to identify the portal where the click originated
                if (TabID != Null.NullInteger)
                {
                    strLink += "&tabid=" + TabID;
                }
                //append portal id to query string to identity portal the click originated.
                if(PortalId != Null.NullInteger)
                {
                    strLink += "&portalid=" + PortalId;
                }
                // moduleid is used to identify the module where the url is stored
                if (ModuleID != -1)
                {
                    strLink += "&mid=" + ModuleID;
                }
                //only add language to url if more than one locale is enabled, and if admin did not turn it off
                if (LocaleController.Instance.GetLocales(PortalId).Count > 1 && EnableUrlLanguage)
                {
                    strLink += "&language=" + Thread.CurrentThread.CurrentCulture.Name;
                }
                //force a download dialog
                if (ForceDownload)
                {
                    strLink += "&forcedownload=true";
                }
            }
            else
            {
                switch (UrlType)
                {
                    case TabType.Tab:
                        strLink = NavigateURL(int.Parse(Link));
                        break;
                    default:
                        strLink = Link;
                        break;
                }
            }
            return strLink;
        }

        /// <summary>
        /// Gets the name of the role.
        /// </summary>
        /// <param name="RoleID">The role ID.</param>
        /// <returns>Role Name</returns>
        public static string GetRoleName(int RoleID)
        {
            switch (Convert.ToString(RoleID))
            {
                case glbRoleAllUsers:
                    return "All Users";
                case glbRoleUnauthUser:
                    return "Unauthenticated Users";
            }
            Hashtable htRoles = null;
            if (Host.PerformanceSetting != PerformanceSettings.NoCaching)
            {
                htRoles = (Hashtable)DataCache.GetCache("GetRoles");
            }
            if (htRoles == null)
            {
                var roles = TestableRoleController.Instance.GetRoles(Null.NullInteger, r => r.SecurityMode != SecurityMode.SocialGroup);
                htRoles = new Hashtable();
                int i;
                for (i = 0; i <= roles.Count - 1; i++)
                {
                    RoleInfo role = roles[i];
                    htRoles.Add(role.RoleID, role.RoleName);
                }
                if (Host.PerformanceSetting != PerformanceSettings.NoCaching)
                {
                    DataCache.SetCache("GetRoles", htRoles);
                }
            }
            return Convert.ToString(htRoles[RoleID]);
        }

        /// <summary>
        /// Gets the content.
        /// </summary>
        /// <param name="Content">The content.</param>
        /// <param name="ContentType">Type of the content.</param>
        /// <returns>specific node by content type of the whole document.</returns>
        public static XmlNode GetContent(string Content, string ContentType)
        {
            var xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(Content);
            if (String.IsNullOrEmpty(ContentType))
            {
                return xmlDoc.DocumentElement;
            }
            else
            {
                return xmlDoc.SelectSingleNode(ContentType);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// GenerateTabPath generates the TabPath used in Friendly URLS
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="ParentId">The Id of the Parent Tab</param>
        /// <param name="TabName">The Name of the current Tab</param>
        /// <returns>The TabPath</returns>
        /// <history>
        ///		[cnurse]	1/28/2005	documented
        ///                             modified to remove characters not allowed in urls
        /// </history>
        /// -----------------------------------------------------------------------------
        public static string GenerateTabPath(int ParentId, string TabName)
        {
            string strTabPath = "";
            var objTabs = new TabController();

            if (!Null.IsNull(ParentId))
            {
                string strTabName;
                var objTab = objTabs.GetTab(ParentId, Null.NullInteger, false);
                while (objTab != null)
                {
                    strTabName = HtmlUtils.StripNonWord(objTab.TabName, false);
                    strTabPath = "//" + strTabName + strTabPath;
                    if (Null.IsNull(objTab.ParentId))
                    {
                        objTab = null;
                    }
                    else
                    {
                        objTab = objTabs.GetTab(objTab.ParentId, objTab.PortalID, false);
                    }
                }
            }

            strTabPath = strTabPath + "//" + HtmlUtils.StripNonWord(TabName, false);
            return strTabPath;
        }

        /// <summary>
        /// Gets the help text.
        /// </summary>
        /// <param name="moduleControlId">The module control id.</param>
        /// <returns>help text.</returns>
        public static string GetHelpText(int moduleControlId)
        {
            string helpText = Null.NullString;
            ModuleControlInfo objModuleControl = ModuleControlController.GetModuleControl(moduleControlId);
            if (objModuleControl != null)
            {
                string FileName = Path.GetFileName(objModuleControl.ControlSrc);
                string LocalResourceFile = objModuleControl.ControlSrc.Replace(FileName, Localization.LocalResourceDirectory + "/" + FileName);
                if (!String.IsNullOrEmpty(Localization.GetString(ModuleActionType.HelpText, LocalResourceFile)))
                {
                    helpText = Localization.GetString(ModuleActionType.HelpText, LocalResourceFile);
                }
            }
            return helpText;
        }

        /// <summary>
        /// Gets the online help url.
        /// </summary>
        /// <param name="HelpUrl">The help URL.</param>
        /// <param name="moduleConfig">The module config.</param>
        /// <returns>url.</returns>
        public static string GetOnLineHelp(string HelpUrl, ModuleInfo moduleConfig)
        {
            if (string.IsNullOrEmpty(HelpUrl))
            {
                //bool isAdminModule = moduleConfig.DesktopModule.IsAdmin;
                //string ctlString = Convert.ToString(HttpContext.Current.Request.QueryString["ctl"]);
                //if (((Host.EnableModuleOnLineHelp && !isAdminModule) || (isAdminModule)))
                //{
                //    if ((isAdminModule) || (IsAdminControl() && ctlString == "Module") || (IsAdminControl() && ctlString == "Tab"))
                //    {
                //        HelpUrl = Host.HelpURL;
                //    }
                //}
                //else
                //{
                HelpUrl = Host.HelpURL;
                //}
            }
            return HelpUrl;
        }

        /// <summary>
        /// Check whether the tab contains "Account Login" module.
        /// </summary>
        /// <param name="tabId">The tab id.</param>
        /// <returns><c>true</c> if the tab contains "Account Login" module, otherwise, <c>false</c>.</returns>
        public static bool ValidateLoginTabID(int tabId)
        {
	        return ValidateModuleInTab(tabId, "Account Login");
        }

		/// <summary>
		/// Check whether the tab contains specific module.
		/// </summary>
		/// <param name="tabId">The tab id.</param>
		/// <param name="moduleName">The module need to check.</param>
		/// <returns><c>true</c> if the tab contains the module, otherwise, <c>false</c>.</returns>
		public static bool ValidateModuleInTab(int tabId, string moduleName)
		{
			bool hasModule = Null.NullBoolean;
            foreach (ModuleInfo objModule in new ModuleController().GetTabModules(tabId).Values)
            {
				if (objModule.ModuleDefinition.FriendlyName == moduleName)
                {
                    //We need to ensure that Anonymous Users or All Users have View permissions to the login page
                    TabInfo tab = new TabController().GetTab(tabId, objModule.PortalID, false);
                    if (TabPermissionController.CanViewPage(tab))
                    {
						hasModule = true;
                        break;
                    }
                }
            }
			return hasModule;
        }

        /// <summary>
        /// Check whether the Filename matches extensions.
        /// </summary>
        /// <param name="filename">The filename.</param>
        /// <param name="strExtensions">The valid extensions.</param>
        /// <returns><c>true</c> if the Filename matches extensions, otherwise, <c>false</c>.</returns>
        private static bool FilenameMatchesExtensions(string filename, string strExtensions)
        {
			bool result = string.IsNullOrEmpty(strExtensions);
            if (!result)
            {
                filename = filename.ToUpper();
                strExtensions = strExtensions.ToUpper();
                foreach (string extension in strExtensions.Split(','))
                {
                    string ext = extension.Trim();
                    if (!ext.StartsWith("."))
                    {
                        ext = "." + extension;
                    }
                    result = filename.EndsWith(extension);
                    if (result)
                    {
                        break;
                    }
                }
            }
            return result;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// DeserializeHashTableBase64 deserializes a Hashtable using Binary Formatting
        /// </summary>
        /// <remarks>
        /// While this method of serializing is no longer supported (due to Medium Trust
        /// issue, it is still required for upgrade purposes.
        /// </remarks>
        /// <param name="Source">The String Source to deserialize</param>
        /// <returns>The deserialized Hashtable</returns>
        /// <history>
        ///		[cnurse]	2/16/2005	moved to Globals
        /// </history>
        /// -----------------------------------------------------------------------------
        public static Hashtable DeserializeHashTableBase64(string Source)
        {
            Hashtable objHashTable;
            if (!String.IsNullOrEmpty(Source))
            {
                byte[] bits = Convert.FromBase64String(Source);
                var mem = new MemoryStream(bits);
                var bin = new BinaryFormatter();
                try
                {
                    objHashTable = (Hashtable)bin.Deserialize(mem);
                }
                catch (Exception exc)
                {
                    Logger.Error(exc);

                    objHashTable = new Hashtable();
                }
                mem.Close();
            }
            else
            {
                objHashTable = new Hashtable();
            }
            return objHashTable;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// DeserializeHashTableXml deserializes a Hashtable using Xml Serialization
        /// </summary>
        /// <remarks>
        /// This is the preferred method of serialization under Medium Trust
        /// </remarks>
        /// <param name="Source">The String Source to deserialize</param>
        /// <returns>The deserialized Hashtable</returns>
        /// <history>
        ///		[cnurse]	2/16/2005	created
        /// </history>
        /// -----------------------------------------------------------------------------
        public static Hashtable DeserializeHashTableXml(string Source)
        {
            return XmlUtils.DeSerializeHashtable(Source, "profile");
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// SerializeHashTableBase64 serializes a Hashtable using Binary Formatting
        /// </summary>
        /// <remarks>
        /// While this method of serializing is no longer supported (due to Medium Trust
        /// issue, it is still required for upgrade purposes.
        /// </remarks>
        /// <param name="Source">The Hashtable to serialize</param>
        /// <returns>The serialized String</returns>
        /// <history>
        ///		[cnurse]	2/16/2005	moved to Globals
        /// </history>
        /// -----------------------------------------------------------------------------
        public static string SerializeHashTableBase64(Hashtable Source)
        {
            string strString;
            if (Source.Count != 0)
            {
                var bin = new BinaryFormatter();
                var mem = new MemoryStream();
                try
                {
                    bin.Serialize(mem, Source);
                    strString = Convert.ToBase64String(mem.GetBuffer(), 0, Convert.ToInt32(mem.Length));
                }
                catch (Exception exc)
                {
                    Logger.Error(exc);

                    strString = "";
                }
                finally
                {
                    mem.Close();
                }
            }
            else
            {
                strString = "";
            }
            return strString;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// SerializeHashTableXml serializes a Hashtable using Xml Serialization
        /// </summary>
        /// <remarks>
        /// This is the preferred method of serialization under Medium Trust
        /// </remarks>
        /// <param name="Source">The Hashtable to serialize</param>
        /// <returns>The serialized String</returns>
        /// <history>
        ///		[cnurse]	2/16/2005	moved to Globals
        ///     [cnurse]    01/19/2007  extracted to XmlUtils
        /// </history>
        /// -----------------------------------------------------------------------------
        public static string SerializeHashTableXml(Hashtable Source)
        {
            return XmlUtils.SerializeDictionary(Source, "profile");
        }

        /// <summary>
        /// Check whether the specific tab is a host tab.
        /// </summary>
        /// <param name="tabId">tab id.</param>
        /// <returns>if true means the tab is a host tab, otherwise means not a host page.</returns>
        public static bool IsHostTab(int tabId)
        {
            bool isHostTab = false;
            TabCollection hostTabs = new TabController().GetTabsByPortal(Null.NullInteger);

            if (hostTabs != null)
            {
                isHostTab = hostTabs.Any(t => t.Value.TabID == tabId);
            }
            return isHostTab;
        }


        /// <summary>
        /// Return User Profile Picture Formatted Url. UserId, width and height can be passed to build a formatted Avatar Url.
        /// </summary>        
        /// <returns>Formatted url,  e.g. http://www.mysite.com/profilepic.ashx?userid={0}&amp;h={1}&amp;w={2} 
        /// </returns>
        /// <remarks>Usage: ascx - &lt;asp:Image ID="avatar" runat="server" CssClass="SkinObject" /&gt;
        /// code behind - avatar.ImageUrl = string.Format(Globals.UserProfilePicFormattedUrl(), userInfo.UserID, 32, 32)
        /// </remarks>
        public static string UserProfilePicFormattedUrl()
        {
            var avatarUrl = PortalController.GetCurrentPortalSettings().DefaultPortalAlias;
            if (string.IsNullOrEmpty(avatarUrl))
            {
                avatarUrl = HttpContext.Current.Request.Url.Host;
            }
            avatarUrl = string.Format("{0}://{1}{2}",
                                      UrlUtils.IsSecureConnectionOrSslOffload(HttpContext.Current.Request) ? "https" : "http",
                                      avatarUrl,
                                      !HttpContext.Current.Request.Url.IsDefaultPort && !avatarUrl.Contains(":") ? ":" + HttpContext.Current.Request.Url.Port : string.Empty);

            avatarUrl += "/profilepic.ashx?userId={0}&h={1}&w={2}";            

            return avatarUrl;
        }

        #region "Obsolete - retained for Binary Compatability"

        // TODO:  These constants are deprecated but cannot be removed until the next batch of breaking change
        // ****************************************************************************************
        // Constants are inlined in code and would require a rebuild of any module or skinobject
        // that may be using these constants.

        [Obsolete("Replaced in DotNetNuke 5.0 by SkinController.GetDefaultAdminSkin and SkinController.GetDefaultPortalSkin")]
        public static SkinDefaults DefaultSkin
        {
            get
            {
                return SkinDefaults.GetSkinDefaults(SkinDefaultType.SkinInfo);
            }
        }

        /// <summary>
        /// Gets the default container.
        /// </summary>
        /// <value>Default Container</value>
        public static SkinDefaults DefaultContainer
        {
            get
            {
                return SkinDefaults.GetSkinDefaults(SkinDefaultType.ContainerInfo);
            }
        }

        [Obsolete("Replaced in DotNetNuke 5.0 by Host.GetHostSettingDictionary")]
        public static Hashtable HostSettings
        {
            get
            {
                var h = new Hashtable();
                foreach (ConfigurationSetting kvp in HostController.Instance.GetSettings().Values)
                {
                    h.Add(kvp.Key, kvp.Value);
                }
                return h;
            }
        }

        [Obsolete("Replaced in DotNetNuke 5.0 by Host.PerformanceSetting")]
        public static PerformanceSettings PerformanceSetting
        {
            get
            {
                return Host.PerformanceSetting;
            }
        }

        [Obsolete("Deprecated in 5.1. Replaced by CachingProvider.Instance.IsWebFarm.")]
        public static bool WebFarmEnabled
        {
            get
            {
                return CachingProvider.Instance().IsWebFarm();
            }
        }

        #region "Html functions moved to HtmlUtils.vb"

        [Obsolete("This function has been replaced by DotNetNuke.Common.Utilities.HtmlUtils.FormatEmail")]
        public static string FormatEmail(string Email)
        {
            return HtmlUtils.FormatEmail(Email);
        }

        [Obsolete("This function has been replaced by DotNetNuke.Common.Utilities.HtmlUtils.FormatWebsite")]
        public static string FormatWebsite(object Website)
        {
            return HtmlUtils.FormatWebsite(Website);
        }

        #endregion

        #region "Xml functions moved to XmlUtils.vb"

        [Obsolete("This function has been replaced by DotNetNuke.Common.Utilities.XmlUtils.XMLEncode")]
        public static string XMLEncode(string HTML)
        {
            return XmlUtils.XMLEncode(HTML);
        }

        #endregion

        [Obsolete("This method has been deprecated.")]
        public static void AddFile(string strFileName, string strExtension, string FolderPath, string strContentType, int Length, int imageWidth, int imageHeight)
        {
            // Obtain PortalSettings from Current Context
            PortalSettings portalSettings = PortalController.GetCurrentPortalSettings();
            int portalId = IsHostTab(portalSettings.ActiveTab.TabID) ? Null.NullInteger : portalSettings.PortalId;
            var objFiles = new FileController();
            var objFolders = new FolderController();
			FolderInfo objFolder = objFolders.GetFolder(portalId, FolderPath, false);
            if ((objFolder != null))
            {
				var objFile = new FileInfo(portalId, strFileName, strExtension, Length, imageWidth, imageHeight, strContentType, FolderPath, objFolder.FolderID, objFolder.StorageLocation, true);
                objFiles.AddFile(objFile);
            }
        }

        [Obsolete("This function has been replaced by DotNetNuke.Common.Utilities.Config.GetConnectionString")]
        public static string GetDBConnectionString()
        {
            return Config.GetConnectionString();
        }

        [Obsolete("This method has been deprecated. ")]
        public static ArrayList GetFileList(DirectoryInfo CurrentDirectory, [Optional, DefaultParameterValue("")] // ERROR: Optional parameters aren't supported in C#
                                                                                string strExtensions, [Optional, DefaultParameterValue(true)] // ERROR: Optional parameters aren't supported in C#
                                                                                                          bool NoneSpecified)
        {
            var arrFileList = new ArrayList();
            string strExtension = "";

            if (NoneSpecified)
            {
                arrFileList.Add(new FileItem("", "<" + Localization.GetString("None_Specified") + ">"));
            }

            string File = null;
            string[] Files = Directory.GetFiles(CurrentDirectory.FullName);
            foreach (string File_loopVariable in Files)
            {
                File = File_loopVariable;
                if (File.IndexOf(".") > -1)
                {
                    strExtension = File.Substring(File.LastIndexOf(".") + 1);
                }
                string FileName = File.Substring(CurrentDirectory.FullName.Length);
                if (strExtensions.ToUpper().IndexOf(strExtension.ToUpper()) != -1 || string.IsNullOrEmpty(strExtensions))
                {
                    arrFileList.Add(new FileItem(FileName, FileName));
                }
            }

            return arrFileList;
        }

        [Obsolete("This method has been replaced by DesktopModuleController.GetDesktopModuleByModuleName() in DotNetNuke 5.0")]
        public static DesktopModuleInfo GetDesktopModuleByName(string name)
        {
            return DesktopModuleController.GetDesktopModuleByModuleName(name, Null.NullInteger);
        }

        [Obsolete("This method has been deprecated. Replaced by GetSubFolderPath(ByVal strFileNamePath As String, ByVal portaId as Integer).")]
        public static string GetSubFolderPath(string strFileNamePath)
        {
            // Obtain PortalSettings from Current Context
            PortalSettings _portalSettings = PortalController.GetCurrentPortalSettings();
            string ParentFolderName = null;
            if (IsHostTab(_portalSettings.ActiveTab.TabID))
            {
                ParentFolderName = HostMapPath.Replace("/", "\\");
            }
            else
            {
                ParentFolderName = _portalSettings.HomeDirectoryMapPath.Replace("/", "\\");
            }
            string strFolderpath = strFileNamePath.Substring(0, strFileNamePath.LastIndexOf("\\") + 1);

            return strFolderpath.Substring(ParentFolderName.Length).Replace("\\", "/");
        }

        [Obsolete("This method has been replaced in DotNetNuke 5.0 by the DatabaseVersion property.")]
        public static string GetDatabaseVersion()
        {
            string strDatabaseVersion = "";
            try
            {
                Version databaseVersion = DataProvider.Instance().GetVersion();
                strDatabaseVersion = databaseVersion.Major.ToString("00") + databaseVersion.Minor.ToString("00") + databaseVersion.Build.ToString("00");
            }
            catch (Exception ex)
            {
                Logger.Error(ex);

                strDatabaseVersion = "ERROR:" + ex.Message;
            }

            return strDatabaseVersion;
        }

        [Obsolete("This method has been replaced in DotNetNuke 5.0 by the DatabaseVersion property.")]
        public static string GetDatabaseVersion(string separator)
        {
            string strDatabaseVersion = "";
            try
            {
                Version databaseVersion = DataProvider.Instance().GetVersion();
                strDatabaseVersion = databaseVersion.Major.ToString("00") + separator + databaseVersion.Minor.ToString("00") + separator + databaseVersion.Build.ToString("00");
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                strDatabaseVersion = "ERROR:" + ex.Message;
            }

            return strDatabaseVersion;
        }

        [Obsolete("This method has been replaced in DotNetNuke 5.0 by TabController.GetPortalTabs().")]
        public static ArrayList GetPortalTabs(int intPortalId, bool blnNoneSpecified, bool blnHidden, bool blnDeleted, bool blnURL, bool bCheckAuthorised)
        {
            List<TabInfo> listTabs = TabController.GetPortalTabs(intPortalId, Null.NullInteger, blnNoneSpecified, Null.NullString, blnHidden, blnDeleted, blnURL, false, bCheckAuthorised);
            var arrTabs = new ArrayList();
            foreach (TabInfo objTab in listTabs)
            {
                TabInfo tabTemp = objTab.Clone();
                tabTemp.TabName = tabTemp.IndentedTabName;
                arrTabs.Add(tabTemp);
            }
            return arrTabs;
        }

        [Obsolete("This method has been replaced in DotNetNuke 5.0 by TabController.GetPortalTabs().")]
        public static ArrayList GetPortalTabs(int intPortalId, bool blnIncludeActiveTab, bool blnNoneSpecified, bool blnHidden, bool blnDeleted, bool blnURL, bool bCheckAuthorised)
        {
            // Obtain current PortalSettings from Current Context
            int excludeTabId = Null.NullInteger;
            PortalSettings _portalSettings = PortalController.GetCurrentPortalSettings();
            if (!blnIncludeActiveTab)
            {
                excludeTabId = _portalSettings.ActiveTab.TabID;
            }

            List<TabInfo> listTabs = TabController.GetPortalTabs(intPortalId, excludeTabId, blnNoneSpecified, Null.NullString, blnHidden, blnDeleted, blnURL, false, bCheckAuthorised);
            var arrTabs = new ArrayList();
            foreach (TabInfo objTab in listTabs)
            {
                TabInfo tabTemp = objTab.Clone();
                tabTemp.TabName = tabTemp.IndentedTabName;
                arrTabs.Add(tabTemp);
            }
            return arrTabs;
        }

        [Obsolete("This method has been replaced in DotNetNuke 5.0 by TabController.GetPortalTabs().")]
        public static ArrayList GetPortalTabs(ArrayList objDesktopTabs, bool blnNoneSpecified, bool blnHidden)
        {
            var arrPortalTabs = new ArrayList();
            TabInfo objTab = default(TabInfo);

            if (blnNoneSpecified)
            {
                objTab = new TabInfo();
                objTab.TabID = -1;
                objTab.TabName = "<" + Localization.GetString("None_Specified") + ">";
                objTab.TabOrder = 0;
                objTab.ParentId = -2;
                arrPortalTabs.Add(objTab);
            }

            foreach (TabInfo tab in objDesktopTabs)
            {
                if (!tab.IsSuperTab)
                {
                    if ((tab.IsVisible || blnHidden) && (tab.IsDeleted == false) && (tab.TabType == TabType.Normal))
                    {
                        TabInfo tabTemp = tab.Clone();
                        tabTemp.TabName = tabTemp.IndentedTabName;
                        arrPortalTabs.Add(tabTemp);
                    }
                }
            }

            return arrPortalTabs;
        }

        [Obsolete("This method has been replaced in DotNetNuke 5.0 by TabController.GetPortalTabs().")]
        public static ArrayList GetPortalTabs(ArrayList objDesktopTabs, bool blnNoneSpecified, bool blnHidden, bool blnDeleted, bool blnURL)
        {
            var arrPortalTabs = new ArrayList();
            TabInfo objTab = default(TabInfo);

            if (blnNoneSpecified)
            {
                objTab = new TabInfo();
                objTab.TabID = -1;
                objTab.TabName = "<" + Localization.GetString("None_Specified") + ">";
                objTab.TabOrder = 0;
                objTab.ParentId = -2;
                arrPortalTabs.Add(objTab);
            }

            foreach (TabInfo tab in objDesktopTabs)
            {
                if (!tab.IsSuperTab)
                {
                    if ((tab.IsVisible || blnHidden) && (tab.IsDeleted == false || blnDeleted) && (tab.TabType == TabType.Normal || blnURL))
                    {
                        TabInfo tabTemp = tab.Clone();
                        tabTemp.TabName = tabTemp.IndentedTabName;
                        arrPortalTabs.Add(tabTemp);
                    }
                }
            }

            return arrPortalTabs;
        }

        [Obsolete("This method has been replaced in DotNetNuke 5.0 by TabController.GetPortalTabs().")]
        public static ArrayList GetPortalTabs(ArrayList objDesktopTabs, int currentTab, bool blnNoneSpecified, bool blnHidden, bool blnDeleted, bool blnURL, bool bCheckAuthorised)
        {
            var arrPortalTabs = new ArrayList();
            TabInfo objTab = default(TabInfo);

            if (blnNoneSpecified)
            {
                objTab = new TabInfo();
                objTab.TabID = -1;
                objTab.TabName = "<" + Localization.GetString("None_Specified") + ">";
                objTab.TabOrder = 0;
                objTab.ParentId = -2;
                arrPortalTabs.Add(objTab);
            }

            foreach (TabInfo tab in objDesktopTabs)
            {
                if (((currentTab < 0) || (tab.TabID != currentTab)) && !tab.IsSuperTab)
                {
                    if ((tab.IsVisible || blnHidden) && (tab.IsDeleted == false || blnDeleted) && (tab.TabType == TabType.Normal || blnURL))
                    {
                        TabInfo tabTemp = tab.Clone();
                        tabTemp.TabName = tabTemp.IndentedTabName;
                        if (bCheckAuthorised)
                        {
                            //Check if User has Administrator rights to this tab
                            if (TabPermissionController.CanAdminPage(tabTemp))
                            {
                                arrPortalTabs.Add(tabTemp);
                            }
                        }
                        else
                        {
                            arrPortalTabs.Add(tabTemp);
                        }
                    }
                }
            }

            return arrPortalTabs;
        }

        [Obsolete("This method has been replaced in DotNetNuke 5.0 by the Status property. and the GetStatus method.")]
        public static UpgradeStatus GetUpgradeStatus()
        {
            return Status;
        }

        [Obsolete("This method has been replaced by IsAdminSkin() in DotNetNuke 5.0, as there is no longer the concept of an Admin Tab/Page")]
        public static bool IsAdminSkin(bool IsAdminTab)
        {
            string AdminKeys = "tab,module,importmodule,exportmodule,help";

            string ControlKey = "";
            if ((HttpContext.Current.Request.QueryString["ctl"] != null))
            {
                ControlKey = HttpContext.Current.Request.QueryString["ctl"].ToLower();
            }

            int ModuleID = -1;
            if ((HttpContext.Current.Request.QueryString["mid"] != null))
            {
                Int32.TryParse(HttpContext.Current.Request.QueryString["mid"], out ModuleID);
            }

            return IsAdminTab || (!string.IsNullOrEmpty(ControlKey) && ControlKey != "view" && ModuleID != -1) ||
                   (!string.IsNullOrEmpty(ControlKey) && AdminKeys.IndexOf(ControlKey) != -1 && ModuleID == -1);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///   Returns whether the tab being displayed is in preview mode
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// </remarks>
        /// <history>
        ///   [Jon Henning]	9/16/2004	Created
        /// </history>
        /// -----------------------------------------------------------------------------
        [Obsolete("Deprecated in DotNetNuke 5.0")]
        public static bool IsTabPreview()
        {
            return (PortalController.GetCurrentPortalSettings().UserMode == PortalSettings.Mode.View);
        }

        [Obsolete("This function has been obsoleted: Use Common.Globals.LinkClick() for proper handling of URLs")]
        public static string LinkClickURL(string Link)
        {
            PortalSettings _portalSettings = PortalController.GetCurrentPortalSettings();
            return LinkClick(Link, _portalSettings.ActiveTab.TabID, -1, false);
        }

        [Obsolete("Deprecated PreventSQLInjection Function to consolidate Security Filter functions in the PortalSecurity class")]
        public static string PreventSQLInjection(string strSQL)
        {
            return (new PortalSecurity()).InputFilter(strSQL, PortalSecurity.FilterFlag.NoSQL);
        }

        [Obsolete("Deprecated in DNN 5.3. Replaced by UserProfileURL")]
        public static string ProfileURL(int userID)
        {
            string strURL = "";
            PortalSettings _portalSettings = PortalController.GetCurrentPortalSettings();

            if (_portalSettings.UserTabId != -1)
            {
                strURL = NavigateURL(_portalSettings.UserTabId);
            }
            else
            {
                strURL = NavigateURL(_portalSettings.ActiveTab.TabID, "Profile", "UserID=" + userID);
            }

            return strURL;
        }

        [Obsolete("This method has been deprecated. Replaced by same method in FileSystemUtils class.")]
        public static string UploadFile(string RootPath, HttpPostedFile objHtmlInputFile, bool Unzip)
        {
            return FileSystemUtils.UploadFile(RootPath, objHtmlInputFile, Unzip);
        }

        #endregion

        private static readonly Stopwatch AppStopwatch = Stopwatch.StartNew();

        public static TimeSpan ElapsedSinceAppStart
        {
            get
            {
                return AppStopwatch.Elapsed;
            }
        }
    }
}