// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Common
{
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

    using DotNetNuke.Abstractions;
    using DotNetNuke.Abstractions.Portals;
    using DotNetNuke.Application;
    using DotNetNuke.Collections.Internal;
    using DotNetNuke.Common.Internal;
    using DotNetNuke.Common.Lists;
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
    using DotNetNuke.Framework.JavaScriptLibraries;
    using DotNetNuke.Framework.Providers;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Security;
    using DotNetNuke.Security.Permissions;
    using DotNetNuke.Security.Roles;
    using DotNetNuke.Services.Cache;
    using DotNetNuke.Services.Exceptions;
    using DotNetNuke.Services.FileSystem;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.Services.Upgrade;
    using DotNetNuke.Services.Url.FriendlyUrl;
    using DotNetNuke.UI.Skins;
    using DotNetNuke.UI.Utilities;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.VisualBasic.CompilerServices;

    using DataCache = DotNetNuke.UI.Utilities.DataCache;
    using FileInfo = DotNetNuke.Services.FileSystem.FileInfo;

    /// <summary>
    /// The global instance of DotNetNuke. all basic functions and properties are defined in this instance.
    /// </summary>
    [StandardModule]
    public sealed class Globals
    {
        /// <summary>
        /// Global role id for all users.
        /// </summary>
        /// <value>-1.</value>
        public const string glbRoleAllUsers = "-1";

        /// <summary>
        /// Global role id for super user.
        /// </summary>
        /// <value>-2.</value>
        public const string glbRoleSuperUser = "-2";

        /// <summary>
        /// Global role id for unauthenticated users.
        /// </summary>
        /// <value>-3.</value>
        public const string glbRoleUnauthUser = "-3";

        /// <summary>
        /// Global role id by default.
        /// </summary>
        /// <value>-4.</value>
        public const string glbRoleNothing = "-4";

        /// <summary>
        /// Global role name for all users.
        /// </summary>
        /// <value>All Users.</value>
        public const string glbRoleAllUsersName = "All Users";

        /// <summary>
        /// Global ro name for super user.
        /// </summary>
        /// <value>Superuser.</value>
        public const string glbRoleSuperUserName = "Superuser";

        /// <summary>
        /// Global role name for unauthenticated users.
        /// </summary>
        /// <value>Unauthenticated Users.</value>
        public const string glbRoleUnauthUserName = "Unauthenticated Users";

        /// <summary>
        /// Default page name.
        /// </summary>
        /// <value>Default.aspx.</value>
        public const string glbDefaultPage = "Default.aspx";

        /// <summary>
        /// Default host skin folder.
        /// </summary>
        /// <value>_default.</value>
        public const string glbHostSkinFolder = "_default";

        /// <summary>
        /// Default control panel.
        /// </summary>
        /// <value>Admin/ControlPanel/IconBar.ascx.</value>
        public const string glbDefaultControlPanel = "Admin/ControlPanel/IconBar.ascx";

        /// <summary>
        /// Default setting to determine if selected control panel is loaded to evaluate visibility.
        /// </summary>
        /// <value>false.</value>
        public const bool glbAllowControlPanelToDetermineVisibility = false;

        /// <summary>
        /// Default pane name.
        /// </summary>
        /// <value>ContentPane.</value>
        public const string glbDefaultPane = "ContentPane";

        /// <summary>
        /// Config files folder.
        /// </summary>
        /// <value>\Config\.</value>
        public const string glbConfigFolder = "\\Config\\";

        /// <summary>
        /// About page name.
        /// </summary>
        /// <value>about.htm.</value>
        public const string glbAboutPage = "about.htm";

        /// <summary>
        /// DotNetNuke config file.
        /// </summary>
        /// <value>DotNetNuke.config.</value>
        public const string glbDotNetNukeConfig = "DotNetNuke.config";

        /// <summary>
        /// Default portal id for super user.
        /// </summary>
        /// <value>-1.</value>
        public const int glbSuperUserAppName = -1;

        /// <summary>
        /// extension of protected files.
        /// </summary>
        /// <value>.resources.</value>
        public const string glbProtectedExtension = ".resources";

        /// <summary>
        /// Default container folder.
        /// </summary>
        /// <value>Portals/_default/Containers/.</value>
        public const string glbContainersPath = "Portals/_default/Containers/";

        /// <summary>
        /// Default skin folder.
        /// </summary>
        /// <value>Portals/_default/Skins/.</value>
        public const string glbSkinsPath = "Portals/_default/Skins/";

        /// <summary>
        /// Email address regex pattern.
        /// </summary>
        /// <value><![CDATA[^[a-zA-Z0-9_%+#&'*/=^`{|}~-](?:\.?[a-zA-Z0-9_%+#&'*/=^`{|}~-])*@(?:[a-zA-Z0-9_](?:(?:\.?|-*)[a-zA-Z0-9_])*\.[a-zA-Z]{2,9}|\[(?:2[0-4]\d|25[0-5]|[01]?\d\d?)\.(?:2[0-4]\d|25[0-5]|[01]?\d\d?)\.(?:2[0-4]\d|25[0-5]|[01]?\d\d?)\.(?:2[0-4]\d|25[0-5]|[01]?\d\d?)])$]]></value>
        public const string glbEmailRegEx =
            @"^\s*[a-zA-Z0-9_%+#&'*/=^`{|}~-](?:\.?[a-zA-Z0-9_%+#&'*/=^`{|}~-])*@(?:[a-zA-Z0-9_](?:(?:\.?|-*)[a-zA-Z0-9_])*\.[a-zA-Z]{2,9}|\[(?:2[0-4]\d|25[0-5]|[01]?\d\d?)\.(?:2[0-4]\d|25[0-5]|[01]?\d\d?)\.(?:2[0-4]\d|25[0-5]|[01]?\d\d?)\.(?:2[0-4]\d|25[0-5]|[01]?\d\d?)])\s*$";

        /// <summary>
        /// User Name regex pattern.
        /// </summary>
        /// <value></value>
        public const string glbUserNameRegEx = @"";

        /// <summary>
        /// format of a script tag.
        /// </summary>
        /// <value><![CDATA[<script type=\"text/javascript\" src=\"{0}\" ></script>]]></value>
        public const string glbScriptFormat = "<script type=\"text/javascript\" src=\"{0}\" ></script>";

        private const string _tabPathInvalidCharsEx = "[&\\? \\./'#:\\*]"; // this value should keep same with the value used in sp BuildTabLevelAndPath to remove invalid chars.

        public static readonly Regex EmailValidatorRegex = new Regex(glbEmailRegEx, RegexOptions.Compiled);
        public static readonly Regex NonAlphanumericCharacters = new Regex("[^A-Za-z0-9]", RegexOptions.Compiled | RegexOptions.CultureInvariant);
        public static readonly Regex InvalidCharacters = new Regex("[^A-Za-z0-9_-]", RegexOptions.Compiled | RegexOptions.CultureInvariant);
        public static readonly Regex InvalidInitialCharacters = new Regex("^[^A-Za-z]", RegexOptions.Compiled | RegexOptions.CultureInvariant);
        public static readonly Regex NumberMatchRegex = new Regex(@"^\d+$", RegexOptions.Compiled);
        public static readonly Regex BaseTagRegex = new Regex("<base[^>]*>", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        public static readonly Regex FileEscapingRegex = new Regex("[\\\\/]\\.\\.[\\\\/]", RegexOptions.Compiled);
        public static readonly Regex FileExtensionRegex = new Regex(@"\..+;", RegexOptions.Compiled);
        public static readonly Regex FileValidNameRegex = new Regex(@"^(?!(?:PRN|AUX|CLOCK\$|NUL|CON|COM\d|LPT\d)(?:\..+)?$)[^\x00-\x1F\xA5\\?*:\"";|\/<>]+(?<![\s.])$", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
        public static readonly Regex ServicesFrameworkRegex = new Regex("/API/|DESKTOPMODULES/.+/API/", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
        public static readonly string USERNAME_UNALLOWED_ASCII = "!\"#$%&'()*+,/:;<=>?[\\]^`{|}";

        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Globals));
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
        private static readonly Regex TabPathInvalidCharsRx = new Regex(_tabPathInvalidCharsEx, RegexOptions.Compiled);

        private static readonly Stopwatch AppStopwatch = Stopwatch.StartNew();

        // global constants for the life of the application ( set in Application_Start )


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
        ///     <item>HostSettingsCacheTimeOut: 20</item>
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
            HeavyCaching = 6,
        }

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
            VerifiedRegistration = 3,
        }

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
            Unknown,
        }

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
                        _applicationPath = string.IsNullOrEmpty(Config.GetSetting("InstallationSubfolder")) ? string.Empty : (Config.GetSetting("InstallationSubfolder") + "/").ToLowerInvariant();
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
        /// Gets the application map path.
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

        /// <summary>
        /// Gets the desktop module path.
        /// </summary>
        /// <value>ApplicationPath + "/DesktopModules/".</value>
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
        /// <value>ApplicationPath + "/Images/".</value>
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
        /// Gets the host map path.
        /// </summary>
        /// <value>ApplicationMapPath + "Portals\_default\".</value>
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
        /// Gets the host path.
        /// </summary>
        /// <value>ApplicationPath + "/Portals/_default/".</value>
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
        /// Gets the install map path.
        /// </summary>
        /// <value>server map path of InstallPath.</value>
        public static string InstallMapPath
        {
            get
            {
                if (_installMapPath == null)
                {
                    _installMapPath = ApplicationMapPath + "\\Install\\";
                }

                return _installMapPath;
            }
        }

        /// <summary>
        /// Gets the install path.
        /// </summary>
        /// <value>ApplicationPath + "/Install/".</value>
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

                    // first call GetProviderPath - this insures that the Database is Initialised correctly
                    // and also generates the appropriate error message if it cannot be initialised correctly
                    string strMessage = DataProvider.Instance().GetProviderPath();

                    // get current database version from DB
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
                            // Errors connecting to the database after an initial installation should be treated as errors.
                            tempStatus = UpgradeStatus.Error;
                        }
                        else
                        {
                            // An error that occurs before the database has been installed should be treated as a new install
                            tempStatus = UpgradeStatus.Install;
                        }
                    }
                    else if (DataBaseVersion == null)
                    {
                        // No Db Version so Install
                        tempStatus = UpgradeStatus.Install;
                    }
                    else
                    {
                        var version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
                        if (version.Major > DataBaseVersion.Major)
                        {
                            // Upgrade Required (Major Version Upgrade)
                            tempStatus = UpgradeStatus.Upgrade;
                        }
                        else if (version.Major == DataBaseVersion.Major && version.Minor > DataBaseVersion.Minor)
                        {
                            // Upgrade Required (Minor Version Upgrade)
                            tempStatus = UpgradeStatus.Upgrade;
                        }
                        else if (version.Major == DataBaseVersion.Major && version.Minor == DataBaseVersion.Minor &&
                                 version.Build > DataBaseVersion.Build)
                        {
                            // Upgrade Required (Build Version Upgrade)
                            tempStatus = UpgradeStatus.Upgrade;
                        }
                        else if (version.Major == DataBaseVersion.Major && version.Minor == DataBaseVersion.Minor &&
                                 version.Build == DataBaseVersion.Build && IncrementalVersionExists(version))
                        {
                            // Upgrade Required (Build Version Upgrade)
                            tempStatus = UpgradeStatus.Upgrade;
                        }
                    }

                    _status = tempStatus;

                    Logger.Trace(string.Format("result of getting providerpath: {0}", strMessage));
                    Logger.Trace("Application status is " + _status);
                }

                return _status;
            }
        }

        /// <summary>
        /// Gets image file types.
        /// </summary>
        /// <value>Values read from ImageTypes List. If there is not List, default values will be jpg,jpeg,jpe,gif,bmp,png,svg,ico.</value>
        public static string glbImageFileTypes
        {
            get
            {
                var listController = new ListController();
                var listEntries = listController.GetListEntryInfoItems("ImageTypes");
                if (listEntries == null || listEntries.Count() == 0)
                {
                    return "jpg,jpeg,jpe,gif,bmp,png,svg,ico";
                }

                return string.Join(",", listEntries.Select(l => l.Value));
            }
        }

        public static TimeSpan ElapsedSinceAppStart
        {
            get
            {
                return AppStopwatch.Elapsed;
            }
        }

        /// <summary>
        /// Gets or sets the name of the IIS app.
        /// </summary>
        /// <value>
        /// request.ServerVariables["APPL_MD_PATH"].
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
        /// Gets or sets the Dependency Service.
        /// </summary>
        /// <value>
        /// The Dependency Service.
        /// </value>
        internal static IServiceProvider DependencyProvider { get; set; }

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
                // we are ignoreing this error simply because there is no graceful way to redirect the user, wihtout the threadabort exception.
                // RobC
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
        }

        public static bool IncrementalVersionExists(Version version)
        {
            Provider currentdataprovider = Config.GetDefaultProvider("data");
            string providerpath = currentdataprovider.Attributes["providerPath"];

            // If the provider path does not exist, then there can't be any log files
            if (!string.IsNullOrEmpty(providerpath))
            {
                providerpath = HttpRuntime.AppDomainAppPath + providerpath.Replace("~", string.Empty);
                if (Directory.Exists(providerpath))
                {
                    var incrementalcount = Directory.GetFiles(providerpath, Upgrade.GetStringVersion(version) + ".*." + Upgrade.DefaultProvider).Length;

                    if (incrementalcount > Globals.GetLastAppliedIteration(version))
                    {
                        return true;
                    }
                }
            }

            return false;
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
        /// <returns>the dataset instance.</returns>
        public static DataSet BuildCrossTabDataSet(string DataSetName, IDataReader result, string FixedColumns, string VariableColumns, string KeyColumn, string FieldColumn, string FieldTypeColumn,
                                                   string StringValueColumn, string NumericValueColumn)
        {
            return BuildCrossTabDataSet(DataSetName, result, FixedColumns, VariableColumns, KeyColumn, FieldColumn, FieldTypeColumn, StringValueColumn, NumericValueColumn, CultureInfo.CurrentCulture);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// converts a data reader with serialized fields into a typed data set.
        /// </summary>
        /// <param name="DataSetName">Name of the dataset to be created.</param>
        /// <param name="result">Data reader that contains all field values serialized.</param>
        /// <param name="FixedColumns">List of fixed columns, delimited by commas. Columns must be contained in DataReader.</param>
        /// <param name="VariableColumns">List of variable columns, delimited by commas. Columns must be contained in DataReader.</param>
        /// <param name="KeyColumn">Name of the column, that contains the row ID. Column must be contained in DataReader.</param>
        /// <param name="FieldColumn">Name of the column, that contains the field name. Column must be contained in DataReader.</param>
        /// <param name="FieldTypeColumn">Name of the column, that contains the field type name. Column must be contained in DataReader.</param>
        /// <param name="StringValueColumn">Name of the column, that contains the field value, if stored as string. Column must be contained in DataReader.</param>
        /// <param name="NumericValueColumn">Name of the column, that contains the field value, if stored as number. Column must be contained in DataReader.</param>
        /// <param name="Culture">culture of the field values in data reader's string value column.</param>
        /// <returns>The generated DataSet.</returns>
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
            if (!string.IsNullOrEmpty(VariableColumns))
            {
                arrVariableColumns = VariableColumns.Split(',');

                // add varible columns to table
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
                // loop using KeyColumn as control break
                if (Convert.ToInt32(result[KeyColumn]) != intKeyColumn)
                {
                    // add row
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

                    // initialize variable column values
                    if (!string.IsNullOrEmpty(VariableColumns))
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
                                    row[arrField[0]] = string.Empty;
                                    break;
                            }
                        }
                    }

                    intKeyColumn = Convert.ToInt32(result[KeyColumn]);
                }

                // assign pivot column value
                if (!string.IsNullOrEmpty(FieldTypeColumn))
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
                                    row[result[FieldColumn].ToString()] = int.Parse(result[StringValueColumn].ToString(), Culture);
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
        /// <returns>the dataset instance.</returns>
        public static DataSet ConvertDataReaderToDataSet(IDataReader reader)
        {
            // add datatable to dataset
            var objDataSet = new DataSet();

            try
            {
                do
                {
                    objDataSet.Tables.Add(ConvertDataReaderToDataTable(reader, false));
                }
                while (reader.NextResult());
            }
            finally
            {
                reader.Close();
            }

            return objDataSet;
        }

        /// <summary>
        /// Converts the datareader to datatable.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <returns>the datatable instance.</returns>
        public static DataTable ConvertDataReaderToDataTable(IDataReader reader)
        {
            return ConvertDataReaderToDataTable(reader, true);
        }

        /// <summary>
        /// Converts the datareader to datatable.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="closeReader">Whether close reader.</param>
        /// <returns>the datatable instance.</returns>
        public static DataTable ConvertDataReaderToDataTable(IDataReader reader, bool closeReader)
        {
            try
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

                objDataTable.EndLoadData();
                return objDataTable;
            }
            finally
            {
                if (closeReader)
                {
                    reader.Close();
                }
            }
        }

        /// <summary>
        /// Gets the absolute server path.
        /// </summary>
        /// <param name="Request">The request.</param>
        /// <returns>absolute server path.</returns>
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
        /// <returns></returns>
        /// -----------------------------------------------------------------------------
        public static string GetApplicationName()
        {
            string appName;
            if (HttpContext.Current.Items["ApplicationName"] == null || string.IsNullOrEmpty(HttpContext.Current.Items["ApplicationName"].ToString()))
            {
                PortalSettings _PortalSettings = PortalController.Instance.GetCurrentPortalSettings();
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
        /// This overload is used to build the Application Name from the Portal Id.
        /// </remarks>
        /// <returns></returns>
        /// -----------------------------------------------------------------------------
        public static string GetApplicationName(int PortalID)
        {
            string appName;

            // Get the Data Provider Configuration
            ProviderConfiguration _providerConfiguration = ProviderConfiguration.GetProviderConfiguration("data");

            // Read the configuration specific information for the current Provider
            var objProvider = (Provider)_providerConfiguration.Providers[_providerConfiguration.DefaultProvider];

            // Get the Object Qualifier frm the Provider Configuration
            string _objectQualifier = objProvider.Attributes["objectQualifier"];
            if (!string.IsNullOrEmpty(_objectQualifier) && _objectQualifier.EndsWith("_") == false)
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
            // update the version
            DataProvider.Instance().UpdateDatabaseVersion(version.Major, version.Minor, version.Build, DotNetNukeContext.Current.Application.Name);
            _dataBaseVersion = version;
        }

        /// <summary>
        /// Updates the database version.
        /// </summary>
        /// <param name="version">The version.</param>
        /// <param name="increment">The increment.</param>
        public static void UpdateDataBaseVersionIncrement(Version version, int increment)
        {
            // update the version and increment
            DataProvider.Instance().UpdateDatabaseVersionIncrement(version.Major, version.Minor, version.Build, increment, DotNetNukeContext.Current.Application.Name);
            _dataBaseVersion = version;
        }

        public static int GetLastAppliedIteration(Version version)
        {
            try
            {
                return DataProvider.Instance().GetLastAppliedIteration(version.Major, version.Minor, version.Build);
            }
            catch (Exception)
            {
                return 0;
            }
        }

        /// <summary>
        /// Adds the port.
        /// </summary>
        /// <param name="httpAlias">The HTTP alias.</param>
        /// <param name="originalUrl">The original URL.</param>
        /// <returns>url with port if the post number is not 80.</returns>
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
        /// <returns>domain name.</returns>
        public static string GetDomainName(HttpRequest request)
        {
            return GetDomainName(new HttpRequestWrapper(request), false);
        }

        /// <summary>
        /// Gets the name of the domain.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns>domain name.</returns>
        public static string GetDomainName(HttpRequestBase request)
        {
            return GetDomainName(request, false);
        }

        /// <summary>
        /// returns the domain name of the current request ( ie. www.domain.com or 207.132.12.123 or www.domain.com/directory if subhost ).
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="parsePortNumber">if set to <c>true</c> [parse port number].</param>
        /// <returns>domain name.</returns>
        public static string GetDomainName(HttpRequest request, bool parsePortNumber)
        {
            return GetDomainName(new HttpRequestWrapper(request), parsePortNumber);
        }

        /// <summary>
        /// returns the domain name of the current request ( ie. www.domain.com or 207.132.12.123 or www.domain.com/directory if subhost ).
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="parsePortNumber">if set to <c>true</c> [parse port number].</param>
        /// <returns>domain name.</returns>
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
            if (Config.GetSetting("UsePortNumber") != null)
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
            return GetFileList(-1, string.Empty, true, string.Empty, false);
        }

        /// <summary>
        /// Gets the file list.
        /// </summary>
        /// <param name="portalId">The portal id.</param>
        /// <returns>file list.</returns>
        public static ArrayList GetFileList(int portalId)
        {
            return GetFileList(portalId, string.Empty, true, string.Empty, false);
        }

        /// <summary>
        /// Gets the file list.
        /// </summary>
        /// <param name="portalId">The portal id.</param>
        /// <param name="strExtensions">The STR extensions.</param>
        /// <returns>file list.</returns>
        public static ArrayList GetFileList(int portalId, string strExtensions)
        {
            return GetFileList(portalId, strExtensions, true, string.Empty, false);
        }

        /// <summary>
        /// Gets the file list.
        /// </summary>
        /// <param name="portalId">The portal id.</param>
        /// <param name="strExtensions">The STR extensions.</param>
        /// <param name="noneSpecified">if set to <c>true</c> [none specified].</param>
        /// <returns>file list.</returns>
        public static ArrayList GetFileList(int portalId, string strExtensions, bool noneSpecified)
        {
            return GetFileList(portalId, strExtensions, noneSpecified, string.Empty, false);
        }

        /// <summary>
        /// Gets the file list.
        /// </summary>
        /// <param name="PortalId">The portal id.</param>
        /// <param name="strExtensions">The STR extensions.</param>
        /// <param name="NoneSpecified">if set to <c>true</c> [none specified].</param>
        /// <param name="Folder">The folder.</param>
        /// <returns>file list.</returns>
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
        /// <returns>file list.</returns>
        public static ArrayList GetFileList(int PortalId, string strExtensions, bool NoneSpecified, string Folder, bool includeHidden)
        {
            var arrFileList = new ArrayList();
            if (NoneSpecified)
            {
                arrFileList.Add(new FileItem(string.Empty, "<" + Localization.GetString("None_Specified") + ">"));
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
                                // File is stored in DB - Just add to arraylist
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
        /// <returns>Host portal settings.</returns>
        public static PortalSettings GetHostPortalSettings()
        {
            int TabId = -1;
            int PortalId = -1;
            PortalAliasInfo objPortalAliasInfo = null;

            // if the portal alias exists
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
        /// <param name="strPortalAlias">The portal alias.</param>
        /// <param name="Request">The request or <c>null</c>.</param>
        /// <param name="blnAddHTTP">if set to <c>true</c> calls <see cref="AddHTTP"/> on the result.</param>
        /// <returns>domain name.</returns>
        public static string GetPortalDomainName(string strPortalAlias, HttpRequest Request, bool blnAddHTTP)
        {
            string strDomainName = string.Empty;
            string strURL = string.Empty;
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

            if (string.IsNullOrEmpty(strDomainName))
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
        /// <returns>Portal settings.</returns>
        public static PortalSettings GetPortalSettings()
        {
            PortalSettings portalSettings = null;

            // Try getting the settings from the Context
            if (HttpContext.Current != null)
            {
                portalSettings = (PortalSettings)HttpContext.Current.Items["PortalSettings"];
            }

            // If nothing then try getting the Host Settings
            if (portalSettings == null)
            {
                portalSettings = GetHostPortalSettings();
            }

            return portalSettings;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Returns the folder path under the root for the portal.
        /// </summary>
        /// <param name="strFileNamePath">The folder the absolute path.</param>
        /// <param name="portalId">Portal Id.</param>
        /// <remarks>
        /// </remarks>
        /// <returns></returns>
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
                PortalInfo objPortal = PortalController.Instance.GetPortal(portalId);
                ParentFolderName = objPortal.HomeDirectoryMapPath.Replace("/", "\\");
            }

            string strFolderpath = strFileNamePath.Substring(0, strFileNamePath.LastIndexOf("\\") + 1);
            return strFolderpath.Substring(ParentFolderName.Length).Replace("\\", "/");
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// The GetTotalRecords method gets the number of Records returned.
        /// </summary>
        /// <param name="dr">An <see cref="IDataReader"/> containing the Total no of records.</param>
        /// <returns>An Integer.</returns>
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
        /// FileID=xx identification for use in importing portals, tabs and modules.
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <returns>An UpgradeStatus enum Upgrade/Install/None.</returns>
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
        /// Encode the post url.
        /// </summary>
        /// <param name="strPost">The post url.</param>
        /// <returns>encoded value.</returns>
        public static string HTTPPOSTEncode(string strPost)
        {
            strPost = strPost.Replace("\\", string.Empty);
            strPost = HttpUtility.UrlEncode(strPost);
            strPost = strPost.Replace("%2f", "/");
            return strPost;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Sets the ApplicationName for the MemberRole API.
        /// </summary>
        /// <remarks>
        /// This overload takes a the PortalId.
        /// </remarks>
        ///     <param name="PortalID">The Portal Id.</param>
        /// -----------------------------------------------------------------------------
        public static void SetApplicationName(int PortalID)
        {
            HttpContext.Current.Items["ApplicationName"] = GetApplicationName(PortalID);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Sets the ApplicationName for the MemberRole API.
        /// </summary>
        /// <remarks>
        /// This overload takes a the PortalId.
        /// </remarks>
        ///     <param name="ApplicationName">The Application Name to set.</param>
        /// -----------------------------------------------------------------------------
        public static void SetApplicationName(string ApplicationName)
        {
            HttpContext.Current.Items["ApplicationName"] = ApplicationName;
        }

        /// <summary>
        /// Formats the address on a single line ( ie. Unit, Street, City, Region, Country, PostalCode ).
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
            string strAddress = string.Empty;
            if (Unit != null)
            {
                if (!string.IsNullOrEmpty(Unit.ToString().Trim()))
                {
                    strAddress += ", " + Unit;
                }
            }

            if (Street != null)
            {
                if (!string.IsNullOrEmpty(Street.ToString().Trim()))
                {
                    strAddress += ", " + Street;
                }
            }

            if (City != null)
            {
                if (!string.IsNullOrEmpty(City.ToString().Trim()))
                {
                    strAddress += ", " + City;
                }
            }

            if (Region != null)
            {
                if (!string.IsNullOrEmpty(Region.ToString().Trim()))
                {
                    strAddress += ", " + Region;
                }
            }

            if (Country != null)
            {
                if (!string.IsNullOrEmpty(Country.ToString().Trim()))
                {
                    strAddress += ", " + Country;
                }
            }

            if (PostalCode != null)
            {
                if (!string.IsNullOrEmpty(PostalCode.ToString().Trim()))
                {
                    strAddress += ", " + PostalCode;
                }
            }

            if (!string.IsNullOrEmpty(strAddress.Trim()))
            {
                strAddress = strAddress.Substring(2);
            }

            return strAddress;
        }

        /// <summary>
        /// Formats the system.version into the standard format nn.nn.nn.
        /// </summary>
        /// <param name="version">The version.</param>
        /// <returns>Formatted  version as string.</returns>
        public static string FormatVersion(Version version)
        {
            return FormatVersion(version, false);
        }

        /// <summary>
        /// Formats the version.
        /// </summary>
        /// <param name="version">The version.</param>
        /// <param name="includeBuild">if set to <c>true</c> [include build].</param>
        /// <returns>Formatted version as string.</returns>
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
        /// Formats  a version into the standard format nn.nn.nn.
        /// </summary>
        /// <param name="version">The version to be formatted.</param>
        /// <param name="fieldFormat">The field format.</param>
        /// <param name="fieldCount">The field count.</param>
        /// <param name="delimiterCharacter">The delimiter character.</param>
        /// <returns>Formatted version as a string.</returns>
        public static string FormatVersion(Version version, string fieldFormat, int fieldCount, string delimiterCharacter)
        {
            string strVersion = string.Empty;
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
        /// Cloaks the text, obfuscate sensitive data to prevent collection by robots and spiders and crawlers.
        /// </summary>
        /// <param name="PersonalInfo">The personal info.</param>
        /// <returns>obfuscated sensitive data by hustling ASCII characters.</returns>
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
            if (!string.IsNullOrEmpty(strDate))
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
            if (!string.IsNullOrEmpty(strDate))
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
                string ControlKey = string.Empty;
                if (HttpContext.Current.Request.QueryString["ctl"] != null)
                {
                    ControlKey = HttpContext.Current.Request.QueryString["ctl"].ToLowerInvariant();
                }

                int ModuleID = -1;
                if (HttpContext.Current.Request.QueryString["mid"] != null)
                {
                    int.TryParse(HttpContext.Current.Request.QueryString["mid"], out ModuleID);
                }

                _IsAdminSkin = (!string.IsNullOrEmpty(ControlKey) && ControlKey != "view" && ModuleID != -1) ||
                               (!string.IsNullOrEmpty(ControlKey) && AdminKeys.IndexOf(ControlKey) != -1 && ModuleID == -1);
            }

            return _IsAdminSkin;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Returns whether the current tab is in EditMode.
        /// </summary>
        /// <returns><c>true</c> if the tab is in Edit mode; otherwise <c>false</c>.</returns>
        /// <remarks>
        /// </remarks>
        /// -----------------------------------------------------------------------------
        public static bool IsEditMode()
        {
            var portalSettings = PortalController.Instance.GetCurrentPortalSettings();
            if (portalSettings == null)
            {
                return false;
            }

            return portalSettings.UserMode == PortalSettings.Mode.Edit && TabPermissionController.CanAddContentToPage();
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Returns whether the current tab is in LayoutMode.
        /// </summary>
        /// <returns><c>true</c> if the current tab is in layout mode; otherwise <c>false</c>.</returns>
        /// <remarks>
        /// </remarks>
        /// -----------------------------------------------------------------------------
        public static bool IsLayoutMode()
        {
            return TabPermissionController.CanAddContentToPage() && PortalController.Instance.GetCurrentPortalSettings().UserMode == PortalSettings.Mode.Layout;
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
            var _portalSettings = PortalController.Instance.GetCurrentPortalSettings();
            var strRSS = new StringBuilder();
            var strRelativePath = DomainName + FileName.Substring(FileName.IndexOf("\\Portals", StringComparison.InvariantCultureIgnoreCase)).Replace("\\", "/");
            strRelativePath = strRelativePath.Substring(0, strRelativePath.LastIndexOf("/", StringComparison.InvariantCulture));
            try
            {
                while (dr.Read())
                {
                    int field;
                    int.TryParse((dr[SyndicateField] ?? string.Empty).ToString(), out field);
                    if (field > 0)
                    {
                        strRSS.AppendLine(" <item>");
                        strRSS.AppendLine("  <title>" + dr[TitleField] + "</title>");
                        var drUrl = (dr["URL"] ?? string.Empty).ToString();
                        if (drUrl.IndexOf("://", StringComparison.InvariantCulture) == -1)
                        {
                            strRSS.Append("  <link>");
                            if (NumberMatchRegex.IsMatch(drUrl))
                            {
                                strRSS.Append(DomainName + "/" + glbDefaultPage + "?tabid=" + dr[URLField]);
                            }
                            else
                            {
                                strRSS.Append(strRelativePath + dr[URLField]);
                            }

                            strRSS.AppendLine("</link>");
                        }
                        else
                        {
                            strRSS.AppendLine("  <link>" + dr[URLField] + "</link>");
                        }

                        strRSS.AppendLine("  <description>" + _portalSettings.PortalName + " " + GetMediumDate(dr[CreatedDateField].ToString()) + "</description>");
                        strRSS.AppendLine(" </item>");
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

            if (strRSS.Length == 0)
            {
                strRSS.Append("<?xml version=\"1.0\" encoding=\"iso-8859-1\"?>" + Environment.NewLine + "<rss version=\"0.91\">" + Environment.NewLine + "  <channel>" + Environment.NewLine + "  <title>" +
                         _portalSettings.PortalName + "</title>" + Environment.NewLine + "  <link>" + DomainName + "</link>" + Environment.NewLine + "  <description>" +
                         _portalSettings.PortalName + "</description>" + Environment.NewLine + "  <language>en-us</language>" + Environment.NewLine + "  <copyright>" +
                         (!string.IsNullOrEmpty(_portalSettings.FooterText) ? _portalSettings.FooterText.Replace("[year]", DateTime.Now.Year.ToString()) : string.Empty) +
                         "</copyright>" + Environment.NewLine + "  <webMaster>" + _portalSettings.Email + "</webMaster>" + Environment.NewLine + strRSS + "   </channel>" + Environment.NewLine +
                         "</rss>");
                File.WriteAllText(FileName, strRSS.ToString());
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
        /// injects the upload directory into raw HTML for src and background tags.
        /// </summary>
        /// <param name="strHTML">raw HTML text.</param>
        /// <param name="strUploadDirectory">path of portal image directory.</param>
        /// <returns>HTML with paths for images and background corrected.</returns>
        /// <remarks>
        /// </remarks>
        /// -----------------------------------------------------------------------------
        public static string ManageUploadDirectory(string strHTML, string strUploadDirectory)
        {
            strHTML = ManageTokenUploadDirectory(strHTML, strUploadDirectory, "src");
            return ManageTokenUploadDirectory(strHTML, strUploadDirectory, "background");
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// injects the upload directory into raw HTML for a single token.
        /// </summary>
        /// <param name="strHTML">raw HTML text.</param>
        /// <param name="strUploadDirectory">path of portal image directory.</param>
        /// <param name="strToken">token to be replaced.</param>
        /// <returns>HTML with paths for images and background corrected.</returns>
        /// <remarks>
        /// called by ManageUploadDirectory for each token.
        /// </remarks>
        /// -----------------------------------------------------------------------------
        public static string ManageTokenUploadDirectory(string strHTML, string strUploadDirectory, string strToken)
        {
            int P;
            int R;
            int S = 0;
            int tLen;
            string strURL;
            var sbBuff = new StringBuilder(string.Empty);
            if (!string.IsNullOrEmpty(strHTML))
            {
                tLen = strToken.Length + 2;
                string _UploadDirectory = strUploadDirectory.ToLowerInvariant();

                // find position of first occurrance:
                P = strHTML.IndexOf(strToken + "=\"", StringComparison.InvariantCultureIgnoreCase);
                while (P != -1)
                {
                    sbBuff.Append(strHTML.Substring(S, P - S + tLen)); // keep charactes left of URL
                    S = P + tLen; // save startpos of URL
                    R = strHTML.IndexOf("\"", S); // end of URL
                    if (R >= 0)
                    {
                        strURL = strHTML.Substring(S, R - S).ToLowerInvariant();
                    }
                    else
                    {
                        strURL = strHTML.Substring(S).ToLowerInvariant();
                    }

                    // add uploaddirectory if we are linking internally and the uploaddirectory is not already included
                    if (!strURL.Contains("://") && !strURL.StartsWith("/") && !strURL.StartsWith(_UploadDirectory))
                    {
                        sbBuff.Append(strUploadDirectory);
                    }

                    // find position of next occurrance:
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
        /// Searches control hierarchy from top down to find a control matching the passed in name.
        /// </summary>
        /// <param name="objParent">Root control to begin searching.</param>
        /// <param name="strControlName">Name of control to look for.</param>
        /// <returns></returns>
        /// <remarks>
        /// This differs from FindControlRecursive in that it looks down the control hierarchy, whereas, the
        /// FindControlRecursive starts at the passed in control and walks the tree up.  Therefore, this function is
        /// more a expensive task.
        /// </remarks>
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
                    // JH dnn.js mod
                    if (ClientAPI.ClientAPIDisabled() == false)
                    {
                        JavaScript.RegisterClientReference(control.Page, ClientAPI.ClientNamespaceReferences.dnn);
                        DNNClientAPI.SetInitialFocus(control.Page, control);
                    }
                    else
                    {
                        // Create JavaScript
                        var sb = new StringBuilder();
                        sb.Append("<script type=\"text/javascript\">");
                        sb.Append("<!--");
                        sb.Append(Environment.NewLine);
                        sb.Append("function SetInitialFocus() {");
                        sb.Append(Environment.NewLine);
                        sb.Append(" document.");

                        // Find the Form
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
        /// <returns>Web request.</returns>
        public static HttpWebRequest GetExternalRequest(string Address)
        {
            // Obtain PortalSettings from Current Context
            PortalSettings _portalSettings = PortalController.Instance.GetCurrentPortalSettings();

            // Create the request object
            var objRequest = (HttpWebRequest)WebRequest.Create(Address);

            // Set a time out to the request ... 10 seconds
            objRequest.Timeout = Host.WebRequestTimeout;

            // Attach a User Agent to the request
            objRequest.UserAgent = "DotNetNuke";

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

                    // Apply credentials to proxy
                    Proxy.Credentials = ProxyCredentials;
                }

                // Apply Proxy to request
                objRequest.Proxy = Proxy;
            }

            return objRequest;
        }

        /// <summary>
        /// Gets the external request.
        /// </summary>
        /// <param name="Address">The address.</param>
        /// <param name="Credentials">The credentials.</param>
        /// <returns>Web request.</returns>
        public static HttpWebRequest GetExternalRequest(string Address, NetworkCredential Credentials)
        {
            // Create the request object
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

                    // Apply credentials to proxy
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
            FileSystemUtils.DeleteFolderRecursive(strRoot);
        }

        /// <summary>
        /// Deletes the files recursive which match the filter, will not delete folders and will ignore folder which is hidden or system.
        /// </summary>
        /// <param name="strRoot">The root.</param>
        /// <param name="filter">The filter.</param>
        public static void DeleteFilesRecursive(string strRoot, string filter)
        {
            FileSystemUtils.DeleteFilesRecursive(strRoot, filter);
        }

        /// <summary>
        /// Cleans the name of the file.
        /// </summary>
        /// <param name="FileName">Name of the file.</param>
        /// <returns>clean name.</returns>
        public static string CleanFileName(string FileName)
        {
            return CleanFileName(FileName, string.Empty, string.Empty);
        }

        /// <summary>
        /// Cleans the name of the file.
        /// </summary>
        /// <param name="FileName">Name of the file.</param>
        /// <param name="BadChars">The bad chars.</param>
        /// <returns>clean name.</returns>
        public static string CleanFileName(string FileName, string BadChars)
        {
            return CleanFileName(FileName, BadChars, string.Empty);
        }

        /// <summary>
        /// Cleans the name of the file.
        /// </summary>
        /// <param name="FileName">Name of the file.</param>
        /// <param name="BadChars">The bad chars.</param>
        /// <param name="ReplaceChar">The replace char.</param>
        /// <returns>clean name.</returns>
        public static string CleanFileName(string FileName, string BadChars, string ReplaceChar)
        {
            string strFileName = FileName;
            if (string.IsNullOrEmpty(BadChars))
            {
                BadChars = ":/\\?*|" + ((char)34) + ((char)39) + ((char)9);
            }

            if (string.IsNullOrEmpty(ReplaceChar))
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
        /// <returns>A cleaned string.</returns>
        /// -----------------------------------------------------------------------------
        public static string CleanName(string Name)
        {
            string strName = Name;
            string strBadChars = ". ~`!@#$%^&*()-_+={[}]|\\:;<,>?/" + ((char)34) + ((char)39);
            int intCounter;
            for (intCounter = 0; intCounter <= strBadChars.Length - 1; intCounter++)
            {
                strName = strName.Replace(strBadChars.Substring(intCounter, 1), string.Empty);
            }

            return strName;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///   CreateValidClass - removes characters from Module/Tab names which are invalid
        ///   for use as an XHTML class attribute / CSS class selector value and optionally
        ///   prepends the letter 'A' if the first character is not alphabetic.  This differs
        ///   from <see>CreateValidID</see> which replaces invalid characters with an underscore
        ///   and replaces the first letter with an 'A' if it is not alphabetic.
        /// </summary>
        /// <param name = "inputValue">String to use to create the class value.</param>
        /// <param name = "validateFirstChar">If set true, validate whether the first character
        ///   is alphabetic and, if not, prepend the letter 'A' to the returned value.</param>
        /// <remarks>
        /// </remarks>
        /// <returns>A string suitable for use as a class value.</returns>
        /// -----------------------------------------------------------------------------
        public static string CreateValidClass(string inputValue, bool validateFirstChar)
        {
            string returnValue = Null.NullString;

            // Regex is expensive so we will cache the results in a lookup table
            var validClassLookupDictionary = CBO.GetCachedObject<SharedDictionary<string, string>>(
                new CacheItemArgs("ValidClassLookup", 200, CacheItemPriority.NotRemovable),
                (CacheItemArgs cacheItemArgs) => new SharedDictionary<string, string>());

            bool idFound = Null.NullBoolean;
            using (ISharedCollectionLock readLock = validClassLookupDictionary.GetReadLock())
            {
                if (validClassLookupDictionary.ContainsKey(inputValue))
                {
                    // Return value
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
                        // Create Valid Class
                        // letters ([a-zA-Z]), digits ([0-9]), hyphens ("-") and underscores ("_") are valid in class values
                        // Remove all characters that aren't in the list
                        returnValue = InvalidCharacters.Replace(inputValue, string.Empty);

                        // If we're asked to validate the first character...
                        if (validateFirstChar)
                        {
                            // classes should begin with a letter ([A-Za-z])'
                            // prepend a starting non-letter character with an A
                            if (InvalidCharacters.IsMatch(returnValue))
                            {
                                returnValue = "A" + returnValue;
                            }
                        }

                        // put in Dictionary
                        validClassLookupDictionary[inputValue] = returnValue;
                    }
                }
            }

            // Return Value
            return returnValue;
        }

        /// <summary>
        ///   Creates the valid ID.
        /// </summary>
        /// <param name = "inputValue">The input value.</param>
        /// <returns>String with a valid ID.</returns>
        public static string CreateValidID(string inputValue)
        {
            string returnValue = Null.NullString;

            // Regex is expensive so we will cache the results in a lookup table
            var validIDLookupDictionary = CBO.GetCachedObject<SharedDictionary<string, string>>(
                new CacheItemArgs("ValidIDLookup", 200, CacheItemPriority.NotRemovable),
                (CacheItemArgs cacheItemArgs) => new SharedDictionary<string, string>());

            bool idFound = Null.NullBoolean;
            using (ISharedCollectionLock readLock = validIDLookupDictionary.GetReadLock())
            {
                if (validIDLookupDictionary.ContainsKey(inputValue))
                {
                    // Return value
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
                        // Create Valid ID
                        // '... letters, digits ([0-9]), hyphens ("-"), underscores ("_"), colons (":"), and periods (".")' are valid identifiers
                        // We aren't allowing hyphens or periods, even though they're valid, since the previous version of this function didn't
                        // Replace all characters that aren't in the list with an underscore
                        returnValue = InvalidCharacters.Replace(inputValue, "_");

                        // identifiers '... must begin with a letter ([A-Za-z])'
                        // replace a starting non-letter character with an A
                        returnValue = InvalidInitialCharacters.Replace(returnValue, "A");

                        // put in Dictionary
                        validIDLookupDictionary[inputValue] = returnValue;
                    }
                }
            }

            return returnValue;
        }

        /// <summary>
        /// Get the URL to show the "access denied" message.
        /// </summary>
        /// <returns>URL to access denied view.</returns>
        public static string AccessDeniedURL()
        {
            return AccessDeniedURL(string.Empty);
        }

        /// <summary>
        /// Get the URL to show the "access denied" message.
        /// </summary>
        /// <param name="Message">The message to display.</param>
        /// <returns>URL to access denied view.</returns>
        public static string AccessDeniedURL(string Message)
        {
            var navigationManager = DependencyProvider.GetRequiredService<INavigationManager>();
            string strURL = string.Empty;
            PortalSettings _portalSettings = PortalController.Instance.GetCurrentPortalSettings();
            if (HttpContext.Current.Request.IsAuthenticated)
            {
                if (string.IsNullOrEmpty(Message))
                {
                    // redirect to access denied page
                    strURL = navigationManager.NavigateURL(_portalSettings.ActiveTab.TabID, "Access Denied");
                }
                else
                {
                    // redirect to access denied page with custom message
                    var messageGuid = DataProvider.Instance().AddRedirectMessage(
                        _portalSettings.UserId, _portalSettings.ActiveTab.TabID, Message).ToString("N");
                    strURL = navigationManager.NavigateURL(_portalSettings.ActiveTab.TabID, "Access Denied", "message=" + messageGuid);
                }
            }
            else
            {
                strURL = LoginURL(HttpUtility.UrlEncode(HttpContext.Current.Request.RawUrl), false);
            }

            return strURL;
        }

        /// <summary>
        /// Adds the current request's protocol (<c>"http://"</c> or <c>"https://"</c>) to the given URL, if it does not already have a protocol specified.
        /// </summary>
        /// <param name="strURL">The URL.</param>
        /// <returns>The formatted URL.</returns>
        public static string AddHTTP(string strURL)
        {
            if (!string.IsNullOrEmpty(strURL))
            {
                if (strURL.IndexOf("mailto:") == -1 && strURL.IndexOf("://") == -1 && strURL.IndexOf("~") == -1 && strURL.IndexOf("\\\\") == -1)
                {
                    strURL = ((HttpContext.Current != null && UrlUtils.IsSecureConnectionOrSslOffload(HttpContext.Current.Request)) ? "https://" : "http://") + strURL;
                }
            }

            return strURL;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Generates the Application root url (including the tab/page).
        /// </summary>
        /// <remarks>
        /// This overload assumes the current page.
        /// </remarks>
        /// <returns>The formatted root url.</returns>
        /// -----------------------------------------------------------------------------
        public static string ApplicationURL()
        {
            PortalSettings _portalSettings = PortalController.Instance.GetCurrentPortalSettings();
            if (_portalSettings != null && _portalSettings.ActiveTab.HasAVisibleVersion)
            {
                return ApplicationURL(_portalSettings.ActiveTab.TabID);
            }

            return ApplicationURL(-1);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Generates the Application root url (including the tab/page).
        /// </summary>
        /// <remarks>
        /// This overload takes the tabid (page id) as a parameter.
        /// </remarks>
        /// <param name="TabID">The id of the tab/page.</param>
        /// <returns>The formatted root url.</returns>
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
        /// Formats the help URL, adding query-string parameters and a protocol (if missing).
        /// </summary>
        /// <param name="HelpUrl">The help URL.</param>
        /// <param name="objPortalSettings">The portal settings.</param>
        /// <param name="Name">The name of the module.</param>
        /// <returns>Formatted URL.</returns>
        public static string FormatHelpUrl(string HelpUrl, PortalSettings objPortalSettings, string Name)
        {
            return FormatHelpUrl(HelpUrl, objPortalSettings, Name, string.Empty);
        }

        /// <summary>
        /// Formats the help URL, adding query-string parameters and a protocol (if missing).
        /// </summary>
        /// <param name="HelpUrl">The help URL.</param>
        /// <param name="objPortalSettings">The portal settings.</param>
        /// <param name="Name">The name of the module.</param>
        /// <param name="Version">The version of the module.</param>
        /// <returns>Formatted URL.</returns>
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

            if (!string.IsNullOrEmpty(Thread.CurrentThread.CurrentUICulture.ToString().ToLowerInvariant()))
            {
                strURL += Thread.CurrentThread.CurrentUICulture.ToString().ToLowerInvariant();
            }
            else
            {
                strURL += objPortalSettings.DefaultLanguage.ToLowerInvariant();
            }

            if (!string.IsNullOrEmpty(Name))
            {
                strURL += "&helpmodule=" + HttpUtility.UrlEncode(Name);
            }

            if (!string.IsNullOrEmpty(Version))
            {
                strURL += "&helpversion=" + HttpUtility.UrlEncode(Version);
            }

            return AddHTTP(strURL);
        }

        /// <summary>
        /// Generates the correctly formatted friendly URL.
        /// </summary>
        /// <remarks>
        /// Assumes Default.aspx, and that portalsettings are saved to Context.
        /// </remarks>
        /// <param name="tab">The current tab.</param>
        /// <param name="path">The path to format.</param>
        /// <returns>The formatted (friendly) URL.</returns>
        public static string FriendlyUrl(TabInfo tab, string path)
        {
            return FriendlyUrl(tab, path, glbDefaultPage);
        }

        /// <summary>
        /// Generates the correctly formatted friendly URL.
        /// </summary>
        /// <remarks>
        /// This overload includes an optional page to include in the url.
        /// </remarks>
        /// <param name="tab">The current tab.</param>
        /// <param name="path">The path to format.</param>
        /// <param name="pageName">The page to include in the url.</param>
        /// <returns>The formatted (friendly) URL.</returns>
        public static string FriendlyUrl(TabInfo tab, string path, string pageName)
        {
            PortalSettings _portalSettings = PortalController.Instance.GetCurrentPortalSettings();
            return FriendlyUrl(tab, path, pageName, _portalSettings);
        }

        /// <summary>
        /// Generates the correctly formatted friendly URL.
        /// </summary>
        /// <remarks>
        /// This overload includes the portal settings for the site.
        /// </remarks>
        /// <param name="tab">The current tab.</param>
        /// <param name="path">The path to format.</param>
        /// <param name="settings">The portal settings.</param>
        /// <returns>The formatted (friendly) URL.</returns>
        [Obsolete("Deprecated in Platform 9.4.3. Scheduled for removal in v11.0.0. Use the IPortalSettings overload")]
        public static string FriendlyUrl(TabInfo tab, string path, PortalSettings settings)
        {
            return FriendlyUrl(tab, path, (IPortalSettings)settings);
        }

        /// <summary>
        /// Generates the correctly formatted friendly URL.
        /// </summary>
        /// <remarks>
        /// This overload includes the portal settings for the site.
        /// </remarks>
        /// <param name="tab">The current tab.</param>
        /// <param name="path">The path to format.</param>
        /// <param name="settings">The portal settings.</param>
        /// <returns>The formatted (friendly) URL.</returns>
        public static string FriendlyUrl(TabInfo tab, string path, IPortalSettings settings)
        {
            return FriendlyUrl(tab, path, glbDefaultPage, settings);
        }

        /// <summary>
        /// Generates the correctly formatted friendly URL.
        /// </summary>
        /// <remarks>
        /// This overload includes an optional page to include in the URL, and the portal
        /// settings for the site.
        /// </remarks>
        /// <param name="tab">The current tab.</param>
        /// <param name="path">The path to format.</param>
        /// <param name="pageName">The page to include in the URL.</param>
        /// <param name="settings">The portal settings.</param>
        /// <returns>The formatted (friendly) url.</returns>
        [Obsolete("Deprecated in Platform 9.4.3. Scheduled for removal in v11.0.0. Use the IPortalSettings overload")]
        public static string FriendlyUrl(TabInfo tab, string path, string pageName, PortalSettings settings)
        {
            return FriendlyUrl(tab, path, pageName, (IPortalSettings)settings);
        }

        /// <summary>
        /// Generates the correctly formatted friendly URL.
        /// </summary>
        /// <remarks>
        /// This overload includes an optional page to include in the URL, and the portal
        /// settings for the site.
        /// </remarks>
        /// <param name="tab">The current tab.</param>
        /// <param name="path">The path to format.</param>
        /// <param name="pageName">The page to include in the URL.</param>
        /// <param name="settings">The portal settings.</param>
        /// <returns>The formatted (friendly) url.</returns>
        public static string FriendlyUrl(TabInfo tab, string path, string pageName, IPortalSettings settings)
        {
            return FriendlyUrlProvider.Instance().FriendlyUrl(tab, path, pageName, settings);
        }

        /// <summary>
        /// Generates the correctly formatted friendly url.
        /// </summary>
        /// <remarks>
        /// This overload includes an optional page to include in the url, and the portal
        /// alias for the site.
        /// </remarks>
        /// <param name="tab">The current tab.</param>
        /// <param name="path">The path to format.</param>
        /// <param name="pageName">The page to include in the URL.</param>
        /// <param name="portalAlias">The portal alias for the site.</param>
        /// <returns>The formatted (friendly) URL.</returns>
        public static string FriendlyUrl(TabInfo tab, string path, string pageName, string portalAlias)
        {
            return FriendlyUrlProvider.Instance().FriendlyUrl(tab, path, pageName, portalAlias);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Returns the type of URl (T=other tab, F=file, U=URL, N=normal).
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="URL">The url.</param>
        /// <returns>The url type.</returns>
        /// -----------------------------------------------------------------------------
        public static TabType GetURLType(string URL)
        {
            if (string.IsNullOrEmpty(URL))
            {
                return TabType.Normal;
            }

            if (URL.ToLowerInvariant().StartsWith("mailto:") == false && URL.IndexOf("://") == -1 && URL.StartsWith("~") == false && URL.StartsWith("\\\\") == false && URL.StartsWith("/") == false)
            {
                if (NumberMatchRegex.IsMatch(URL))
                {
                    return TabType.Tab;
                }

                if (URL.ToLowerInvariant().StartsWith("userid="))
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
        /// If the link does not exist, the function will return an empty string.
        /// </summary>
        /// <param name="ModuleId">Integer.</param>
        /// <param name="url">String.</param>
        /// <returns>If an internal link does not exist, an empty string is returned, otherwise the passed in url is returned as is.</returns>
        public static string ImportUrl(int ModuleId, string url)
        {
            string strUrl = url;
            TabType urlType = GetURLType(url);
            int intId = -1;
            PortalSettings portalSettings = GetPortalSettings();
            switch (urlType)
            {
                case TabType.File:
                    if (int.TryParse(url.Replace("FileID=", string.Empty), out intId))
                    {
                        var objFile = FileManager.Instance.GetFile(intId);
                        if (objFile == null)
                        {
                            // fileId does not exist in the portal
                            strUrl = string.Empty;
                        }
                    }
                    else
                    {
                        // failed to get fileId
                        strUrl = string.Empty;
                    }

                    break;
                case TabType.Member:
                    if (int.TryParse(url.Replace("UserID=", string.Empty), out intId))
                    {
                        if (UserController.GetUserById(portalSettings.PortalId, intId) == null)
                        {
                            // UserId does not exist for this portal
                            strUrl = string.Empty;
                        }
                    }
                    else
                    {
                        // failed to get UserId
                        strUrl = string.Empty;
                    }

                    break;
                case TabType.Tab:
                    if (int.TryParse(url, out intId))
                    {
                        if (TabController.Instance.GetTab(intId, portalSettings.PortalId, false) == null)
                        {
                            // the tab does not exist
                            strUrl = string.Empty;
                        }
                    }
                    else
                    {
                        // failed to get TabId
                        strUrl = string.Empty;
                    }

                    break;
            }

            return strUrl;
        }

        /// <summary>
        /// Gets the login URL.
        /// </summary>
        /// <param name="returnUrl">The URL to redirect to after logging in.</param>
        /// <param name="overrideSetting">if set to <c>true</c>, show the login control on the current page, even if there is a login page defined for the site.</param>
        /// <returns>Formatted URL.</returns>
        public static string LoginURL(string returnUrl, bool overrideSetting)
        {
            return LoginURL(returnUrl, overrideSetting, PortalController.Instance.GetCurrentPortalSettings());
        }

        /// <summary>
        /// Gets the login URL.
        /// </summary>
        /// <param name="returnUrl">The URL to redirect to after logging in.</param>
        /// <param name="overrideSetting">if set to <c>true</c>, show the login control on the current page, even if there is a login page defined for the site.</param>
        /// <param name="portalSettings">The Portal Settings.</param>
        /// <returns>Formatted URL.</returns>
        public static string LoginURL(string returnUrl, bool overrideSetting, PortalSettings portalSettings)
        {
            var navigationManager = DependencyProvider.GetRequiredService<INavigationManager>();
            string loginUrl;
            if (!string.IsNullOrEmpty(returnUrl))
            {
                returnUrl = string.Format("returnurl={0}", returnUrl);
            }

            var popUpParameter = string.Empty;
            if (HttpUtility.UrlDecode(returnUrl).IndexOf("popUp=true", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                popUpParameter = "popUp=true";
            }

            if (portalSettings.LoginTabId != -1 && !overrideSetting)
            {
                if (ValidateLoginTabID(portalSettings.LoginTabId))
                {
                    loginUrl = string.IsNullOrEmpty(returnUrl)
                                        ? navigationManager.NavigateURL(portalSettings.LoginTabId, string.Empty, popUpParameter)
                                        : navigationManager.NavigateURL(portalSettings.LoginTabId, string.Empty, returnUrl, popUpParameter);
                }
                else
                {
                    string strMessage = string.Format("error={0}", Localization.GetString("NoLoginControl", Localization.GlobalResourceFile));

                    // No account module so use portal tab
                    loginUrl = string.IsNullOrEmpty(returnUrl)
                                 ? navigationManager.NavigateURL(portalSettings.ActiveTab.TabID, "Login", strMessage, popUpParameter)
                                 : navigationManager.NavigateURL(portalSettings.ActiveTab.TabID, "Login", returnUrl, strMessage, popUpParameter);
                }
            }
            else
            {
                // portal tab
                loginUrl = string.IsNullOrEmpty(returnUrl)
                                ? navigationManager.NavigateURL(portalSettings.ActiveTab.TabID, "Login", popUpParameter)
                                : navigationManager.NavigateURL(portalSettings.ActiveTab.TabID, "Login", returnUrl, popUpParameter);
            }

            return loginUrl;
        }

        /// <summary>
        /// Gets User profile URL.
        /// </summary>
        /// <param name="userId">The user id.</param>
        /// <returns>Formatted url.</returns>
        public static string UserProfileURL(int userId)
        {
            string strURL = string.Empty;
            PortalSettings portalSettings = PortalController.Instance.GetCurrentPortalSettings();

            strURL = DependencyProvider.GetRequiredService<INavigationManager>().NavigateURL(portalSettings.UserTabId, string.Empty, string.Format("userId={0}", userId));

            return strURL;
        }

        /// <summary>
        /// Gets the URL to the current page.
        /// </summary>
        /// <returns>Formatted URL.</returns>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Obsolete("Deprecated in Platform 9.4.2. Scheduled removal in v11.0.0.")]
        public static string NavigateURL()
        {
            return DependencyProvider.GetRequiredService<INavigationManager>().NavigateURL();
        }

        /// <summary>
        /// Gets the URL to the given page.
        /// </summary>
        /// <param name="tabID">The tab ID.</param>
        /// <returns>Formatted URL.</returns>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Obsolete("Deprecated in Platform 9.4.2. Scheduled removal in v11.0.0.")]
        public static string NavigateURL(int tabID)
        {
            return DependencyProvider.GetRequiredService<INavigationManager>().NavigateURL(tabID);
        }

        /// <summary>
        /// Gets the URL to the given page.
        /// </summary>
        /// <param name="tabID">The tab ID.</param>
        /// <param name="isSuperTab">if set to <c>true</c> the page is a "super-tab," i.e. a host-level page.</param>
        /// <returns>Formatted URL.</returns>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Obsolete("Deprecated in Platform 9.4.2. Scheduled removal in v11.0.0.")]
        public static string NavigateURL(int tabID, bool isSuperTab)
        {
            return DependencyProvider.GetRequiredService<INavigationManager>().NavigateURL(tabID, isSuperTab);
        }

        /// <summary>
        /// Gets the URL to show the control associated with the given control key.
        /// </summary>
        /// <param name="controlKey">The control key, or <see cref="string.Empty"/> or <c>null</c>.</param>
        /// <returns>Formatted URL.</returns>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Obsolete("Deprecated in Platform 9.4.2. Scheduled removal in v11.0.0.")]
        public static string NavigateURL(string controlKey)
        {
            return DependencyProvider.GetRequiredService<INavigationManager>().NavigateURL(controlKey);
        }

        /// <summary>
        /// Gets the URL to show the control associated with the given control key.
        /// </summary>
        /// <param name="controlKey">The control key, or <see cref="string.Empty"/> or <c>null</c>.</param>
        /// <param name="additionalParameters">Any additional parameters, in <c>"key=value"</c> format.</param>
        /// <returns>Formatted URL.</returns>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Obsolete("Deprecated in Platform 9.4.2. Scheduled removal in v11.0.0.")]
        public static string NavigateURL(string controlKey, params string[] additionalParameters)
        {
            return DependencyProvider.GetRequiredService<INavigationManager>().NavigateURL(controlKey, additionalParameters);
        }

        /// <summary>
        /// Gets the URL to show the control associated with the given control key on the given page.
        /// </summary>
        /// <param name="tabID">The tab ID.</param>
        /// <param name="controlKey">The control key, or <see cref="string.Empty"/> or <c>null</c>.</param>
        /// <returns>Formatted URL.</returns>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Obsolete("Deprecated in Platform 9.4.2. Scheduled removal in v11.0.0.")]
        public static string NavigateURL(int tabID, string controlKey)
        {
            return DependencyProvider.GetRequiredService<INavigationManager>().NavigateURL(tabID, controlKey);
        }

        /// <summary>
        /// Gets the URL to show the given page.
        /// </summary>
        /// <param name="tabID">The tab ID.</param>
        /// <param name="controlKey">The control key, or <see cref="string.Empty"/> or <c>null</c>.</param>
        /// <param name="additionalParameters">Any additional parameters.</param>
        /// <returns>Formatted URL.</returns>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Obsolete("Deprecated in Platform 9.4.2. Scheduled removal in v11.0.0.")]
        public static string NavigateURL(int tabID, string controlKey, params string[] additionalParameters)
        {
            return DependencyProvider.GetRequiredService<INavigationManager>().NavigateURL(tabID, controlKey, additionalParameters);
        }

        /// <summary>
        /// Gets the URL to show the given page.
        /// </summary>
        /// <param name="tabID">The tab ID.</param>
        /// <param name="settings">The portal settings.</param>
        /// <param name="controlKey">The control key, or <see cref="string.Empty"/> or <c>null</c>.</param>
        /// <param name="additionalParameters">Any additional parameters.</param>
        /// <returns>Formatted URL.</returns>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Obsolete("Deprecated in Platform 9.4.2. Scheduled removal in v11.0.0.")]
        public static string NavigateURL(int tabID, PortalSettings settings, string controlKey, params string[] additionalParameters)
        {
            return DependencyProvider.GetRequiredService<INavigationManager>().NavigateURL(tabID, settings, controlKey, additionalParameters);
        }

        /// <summary>
        /// Gets the URL to show the given page.
        /// </summary>
        /// <param name="tabID">The tab ID.</param>
        /// <param name="isSuperTab">if set to <c>true</c> the page is a "super-tab," i.e. a host-level page.</param>
        /// <param name="settings">The portal settings.</param>
        /// <param name="controlKey">The control key, or <see cref="string.Empty"/> or <c>null</c>.</param>
        /// <param name="additionalParameters">Any additional parameters.</param>
        /// <returns>Formatted URL.</returns>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Obsolete("Deprecated in Platform 9.4.2. Scheduled removal in v11.0.0.")]
        public static string NavigateURL(int tabID, bool isSuperTab, PortalSettings settings, string controlKey, params string[] additionalParameters)
        {
            return DependencyProvider.GetRequiredService<INavigationManager>().NavigateURL(tabID, isSuperTab, settings, controlKey, additionalParameters);
        }

        /// <summary>
        /// Gets the URL to show the given page.
        /// </summary>
        /// <param name="tabID">The tab ID.</param>
        /// <param name="isSuperTab">if set to <c>true</c> the page is a "super-tab," i.e. a host-level page.</param>
        /// <param name="settings">The portal settings.</param>
        /// <param name="controlKey">The control key, or <see cref="string.Empty"/> or <c>null</c>.</param>
        /// <param name="language">The language code.</param>
        /// <param name="additionalParameters">Any additional parameters.</param>
        /// <returns>Formatted URL.</returns>
        [Obsolete("Deprecated in Platform 9.4.2. Scheduled removal in v11.0.0.")]
        public static string NavigateURL(int tabID, bool isSuperTab, PortalSettings settings, string controlKey, string language, params string[] additionalParameters)
        {
            return DependencyProvider.GetRequiredService<INavigationManager>().NavigateURL(tabID, isSuperTab, settings, controlKey, language, additionalParameters);
        }

        /// <summary>
        /// Gets the URL to show the given page.
        /// </summary>
        /// <param name="tabID">The tab ID.</param>
        /// <param name="isSuperTab">if set to <c>true</c> the page is a "super-tab," i.e. a host-level page.</param>
        /// <param name="settings">The portal settings.</param>
        /// <param name="controlKey">The control key, or <see cref="string.Empty"/> or <c>null</c>.</param>
        /// <param name="language">The language code.</param>
        /// <param name="pageName">The page name to pass to <see cref="FriendlyUrl(DotNetNuke.Entities.Tabs.TabInfo,string,string)"/>.</param>
        /// <param name="additionalParameters">Any additional parameters.</param>
        /// <returns>Formatted url.</returns>
        [Obsolete("Deprecated in Platform 9.4.2. Scheduled removal in v11.0.0.")]
        public static string NavigateURL(int tabID, bool isSuperTab, PortalSettings settings, string controlKey, string language, string pageName, params string[] additionalParameters)
        {
            return DependencyProvider.GetRequiredService<INavigationManager>().NavigateURL(tabID, isSuperTab, settings, controlKey, language, pageName, additionalParameters);
        }

        /// <summary>
        /// UrlEncode query string.
        /// </summary>
        /// <param name="QueryString">The query string.</param>
        /// <returns>Encoded content.</returns>
        public static string QueryStringEncode(string QueryString)
        {
            QueryString = HttpUtility.UrlEncode(QueryString);
            return QueryString;
        }

        /// <summary>
        /// UrlDecode query string.
        /// </summary>
        /// <param name="QueryString">The query string.</param>
        /// <returns>Decoded content.</returns>
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
            var navigationManager = DependencyProvider.GetRequiredService<INavigationManager>();
            string strURL;
            PortalSettings _portalSettings = PortalController.Instance.GetCurrentPortalSettings();
            string extraParams = string.Empty;
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
                // user defined tab
                strURL = navigationManager.NavigateURL(_portalSettings.RegisterTabId, string.Empty, extraParams);
            }
            else
            {
                strURL = navigationManager.NavigateURL(_portalSettings.ActiveTab.TabID, "Register", extraParams);
            }

            return strURL;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Generates the correctly formatted url.
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="url">The url to format.</param>
        /// <returns>The formatted (resolved) url.</returns>
        /// -----------------------------------------------------------------------------
        public static string ResolveUrl(string url)
        {
            // String is Empty, just return Url
            if (string.IsNullOrEmpty(url))
            {
                return url;
            }

            // String does not contain a ~, so just return Url
            if (url.StartsWith("~") == false)
            {
                return url;
            }

            // There is just the ~ in the Url, return the appPath
            if (url.Length == 1)
            {
                return ApplicationPath;
            }

            if (url.ToCharArray()[1] == '/' || url.ToCharArray()[1] == '\\')
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
        /// <returns>Encoded content.</returns>
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
                if (!string.IsNullOrEmpty(HashObject.ToString()))
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
            return LinkClick(Link, TabID, ModuleID, true, string.Empty);
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
            return LinkClick(Link, TabID, ModuleID, TrackClicks, string.Empty);
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
            return LinkClick(Link, TabID, ModuleID, TrackClicks, !string.IsNullOrEmpty(ContentType));
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
            PortalSettings _portalSettings = PortalController.Instance.GetCurrentPortalSettings();
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
            string strLink = string.Empty;
            TabType UrlType = GetURLType(Link);
            if (UrlType == TabType.Member)
            {
                strLink = UserProfileURL(Convert.ToInt32(UrlUtils.GetParameterValue(Link)));
            }
            else if (TrackClicks || ForceDownload || UrlType == TabType.File)
            {
                // format LinkClick wrapper
                if (Link.StartsWith("fileid=", StringComparison.InvariantCultureIgnoreCase))
                {
                    strLink = ApplicationPath + "/LinkClick.aspx?fileticket=" + UrlUtils.EncryptParameter(UrlUtils.GetParameterValue(Link), portalGuid);
                    if (PortalId == Null.NullInteger) // To track Host files
                    {
                        strLink += "&hf=1";
                    }
                }

                if (string.IsNullOrEmpty(strLink))
                {
                    strLink = ApplicationPath + "/LinkClick.aspx?link=" + HttpUtility.UrlEncode(Link);
                }

                // tabid is required to identify the portal where the click originated
                if (TabID != Null.NullInteger)
                {
                    strLink += "&tabid=" + TabID;
                }

                // append portal id to query string to identity portal the click originated.
                if (PortalId != Null.NullInteger)
                {
                    strLink += "&portalid=" + PortalId;
                }

                // moduleid is used to identify the module where the url is stored
                if (ModuleID != -1)
                {
                    strLink += "&mid=" + ModuleID;
                }

                // only add language to url if more than one locale is enabled, and if admin did not turn it off
                if (LocaleController.Instance.GetLocales(PortalId).Count > 1 && EnableUrlLanguage)
                {
                    strLink += "&language=" + Thread.CurrentThread.CurrentCulture.Name;
                }

                // force a download dialog
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
                        strLink = DependencyProvider.GetRequiredService<INavigationManager>().NavigateURL(int.Parse(Link));
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
        /// <returns>Role Name.</returns>
        public static string GetRoleName(int RoleID)
        {
            switch (Convert.ToString(RoleID))
            {
                case glbRoleAllUsers:
                    return glbRoleAllUsersName;
                case glbRoleUnauthUser:
                    return glbRoleUnauthUserName;
            }

            Hashtable htRoles = null;
            if (Host.PerformanceSetting != PerformanceSettings.NoCaching)
            {
                htRoles = (Hashtable)DataCache.GetCache("GetRoles");
            }

            if (htRoles == null)
            {
                var roles = RoleController.Instance.GetRoles(Null.NullInteger, r => r.SecurityMode != SecurityMode.SocialGroup);
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
            var xmlDoc = new XmlDocument { XmlResolver = null };
            xmlDoc.LoadXml(Content);
            if (string.IsNullOrEmpty(ContentType))
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
        /// GenerateTabPath generates the TabPath used in Friendly URLS.
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="parentId">The Id of the Parent Tab.</param>
        /// <param name="tabName">The Name of the current Tab.</param>
        /// <returns>The TabPath.</returns>
        /// -----------------------------------------------------------------------------
        public static string GenerateTabPath(int parentId, string tabName)
        {
            var strTabPath = Null.NullString;

            if (!Null.IsNull(parentId))
            {
                var objTab = TabController.Instance.GetTab(parentId, Null.NullInteger, false);
                while (objTab != null)
                {
                    var strTabName = TabPathInvalidCharsRx.Replace(objTab.TabName, string.Empty);
                    strTabPath = "//" + strTabName + strTabPath;
                    objTab = Null.IsNull(objTab.ParentId)
                        ? null
                        : TabController.Instance.GetTab(objTab.ParentId, objTab.PortalID, false);
                }
            }

            strTabPath = strTabPath + "//" + TabPathInvalidCharsRx.Replace(tabName, string.Empty);
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
                if (!string.IsNullOrEmpty(Localization.GetString(ModuleActionType.HelpText, LocalResourceFile)))
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
                // bool isAdminModule = moduleConfig.DesktopModule.IsAdmin;
                // string ctlString = Convert.ToString(HttpContext.Current.Request.QueryString["ctl"]);
                // if (((Host.EnableModuleOnLineHelp && !isAdminModule) || (isAdminModule)))
                // {
                //    if ((isAdminModule) || (IsAdminControl() && ctlString == "Module") || (IsAdminControl() && ctlString == "Tab"))
                //    {
                //        HelpUrl = Host.HelpURL;
                //    }
                // }
                // else
                // {
                HelpUrl = Host.HelpURL;

                // }
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
            foreach (ModuleInfo objModule in ModuleController.Instance.GetTabModules(tabId).Values)
            {
                if (objModule.ModuleDefinition.FriendlyName == moduleName)
                {
                    // We need to ensure that Anonymous Users or All Users have View permissions to the login page
                    TabInfo tab = TabController.Instance.GetTab(tabId, objModule.PortalID, false);
                    if (TabPermissionController.CanViewPage(tab))
                    {
                        hasModule = true;
                        break;
                    }
                }
            }

            return hasModule;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// DeserializeHashTableBase64 deserializes a Hashtable using Binary Formatting.
        /// </summary>
        /// <remarks>
        /// While this method of serializing is no longer supported (due to Medium Trust
        /// issue, it is still required for upgrade purposes.
        /// </remarks>
        /// <param name="Source">The String Source to deserialize.</param>
        /// <returns>The deserialized Hashtable.</returns>
        /// -----------------------------------------------------------------------------
        public static Hashtable DeserializeHashTableBase64(string Source)
        {
            Hashtable objHashTable;
            if (!string.IsNullOrEmpty(Source))
            {
                byte[] bits = Convert.FromBase64String(Source);
                using (var mem = new MemoryStream(bits))
                {
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
            }
            else
            {
                objHashTable = new Hashtable();
            }

            return objHashTable;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// DeserializeHashTableXml deserializes a Hashtable using Xml Serialization.
        /// </summary>
        /// <remarks>
        /// This is the preferred method of serialization under Medium Trust.
        /// </remarks>
        /// <param name="Source">The String Source to deserialize.</param>
        /// <returns>The deserialized Hashtable.</returns>
        /// -----------------------------------------------------------------------------
        public static Hashtable DeserializeHashTableXml(string Source)
        {
            return XmlUtils.DeSerializeHashtable(Source, "profile");
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// SerializeHashTableBase64 serializes a Hashtable using Binary Formatting.
        /// </summary>
        /// <remarks>
        /// While this method of serializing is no longer supported (due to Medium Trust
        /// issue, it is still required for upgrade purposes.
        /// </remarks>
        /// <param name="Source">The Hashtable to serialize.</param>
        /// <returns>The serialized String.</returns>
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

                    strString = string.Empty;
                }
                finally
                {
                    mem.Close();
                }
            }
            else
            {
                strString = string.Empty;
            }

            return strString;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// SerializeHashTableXml serializes a Hashtable using Xml Serialization.
        /// </summary>
        /// <remarks>
        /// This is the preferred method of serialization under Medium Trust.
        /// </remarks>
        /// <param name="Source">The Hashtable to serialize.</param>
        /// <returns>The serialized String.</returns>
        /// -----------------------------------------------------------------------------
        public static string SerializeHashTableXml(Hashtable Source)
        {
            return XmlUtils.SerializeDictionary(Source, "profile");
        }

        /// <summary>
        /// Check whether the specific page is a host page.
        /// </summary>
        /// <param name="tabId">The tab ID.</param>
        /// <returns>if <c>true</c> the tab is a host page; otherwise, it is not a host page.</returns>
        public static bool IsHostTab(int tabId)
        {
            bool isHostTab = false;
            TabCollection hostTabs = TabController.Instance.GetTabsByPortal(Null.NullInteger);

            if (hostTabs != null)
            {
                isHostTab = hostTabs.Any(t => t.Value.TabID == tabId);
            }

            return isHostTab;
        }

        /// <summary>
        /// Return User Profile Picture Formatted Url. UserId, width and height can be passed to build a formatted Avatar Url.
        /// </summary>
        /// <returns>Formatted url,  e.g. http://www.mysite.com/DnnImageHandler.ashx?mode=profilepic&amp;userid={0}&amp;h={1}&amp;w={2}.
        /// </returns>
        /// <remarks>Usage: ascx - &lt;asp:Image ID="avatar" runat="server" CssClass="SkinObject" /&gt;
        /// code behind - avatar.ImageUrl = string.Format(Globals.UserProfilePicFormattedUrl(), userInfo.UserID, 32, 32).
        /// </remarks>
        [Obsolete("Obsoleted in DNN 7.3.0 as it causes issues in SSL-offloading scenarios - please use UserProfilePicRelativeUrl instead.. Scheduled removal in v11.0.0.")]
        public static string UserProfilePicFormattedUrl()
        {
            var avatarUrl = PortalController.Instance.GetCurrentPortalSettings().DefaultPortalAlias;
            if (string.IsNullOrEmpty(avatarUrl))
            {
                avatarUrl = HttpContext.Current.Request.Url.Host;
            }

            avatarUrl = string.Format(
                "{0}://{1}{2}",
                UrlUtils.IsSecureConnectionOrSslOffload(HttpContext.Current.Request) ? "https" : "http",
                avatarUrl,
                !HttpContext.Current.Request.Url.IsDefaultPort && !avatarUrl.Contains(":") ? ":" + HttpContext.Current.Request.Url.Port : string.Empty);

            avatarUrl += "/DnnImageHandler.ashx?mode=profilepic&userId={0}&h={1}&w={2}";

            return avatarUrl;
        }

        /// <summary>
        /// Return User Profile Picture relative Url. UserId, width and height can be passed to build a formatted relative Avatar Url.
        /// </summary>
        /// <returns>Formatted url,  e.g. /DnnImageHandler.ashx?userid={0}&amp;h={1}&amp;w={2} considering child portal.
        /// </returns>
        /// <remarks>Usage: ascx - &lt;asp:Image ID="avatar" runat="server" CssClass="SkinObject" /&gt;
        /// code behind - avatar.ImageUrl = string.Format(Globals.UserProfilePicRelativeUrl(), userInfo.UserID, 32, 32).
        /// </remarks>
        [Obsolete("Deprecated in Platform 8.0.0. Please use UserController.Instance.GetUserProfilePictureUrl. Scheduled removal in v11.0.0.")]
        public static string UserProfilePicRelativeUrl()
        {
            return UserProfilePicRelativeUrl(true);
        }

        /// <summary>
        /// Return User Profile Picture relative Url. UserId, width and height can be passed to build a formatted relative Avatar Url.
        /// </summary>
        /// <param name="includeCdv">Indicates if cdv (Cache Delayed Verification) has to be included in the returned URL.</param>
        /// <returns>Formatted url,  e.g. /DnnImageHandler.ashx?userid={0}&amp;h={1}&amp;w={2} considering child portal.
        /// </returns>
        /// <remarks>Usage: ascx - &lt;asp:Image ID="avatar" runat="server" CssClass="SkinObject" /&gt;
        /// code behind - avatar.ImageUrl = string.Format(Globals.UserProfilePicRelativeUrl(), userInfo.UserID, 32, 32).
        /// </remarks>
        [Obsolete("Deprecated in Platform 8.0.0. Please use UserController.Instance.GetUserProfilePictureUrl. Scheduled removal in v11.0.0.")]
        public static string UserProfilePicRelativeUrl(bool includeCdv)
        {
            const string query = "/DnnImageHandler.ashx?mode=profilepic&userId={0}&h={1}&w={2}";
            var currentAlias = GetPortalSettings().PortalAlias.HTTPAlias;
            var index = currentAlias.IndexOf('/');
            var childPortalAlias = index > 0 ? "/" + currentAlias.Substring(index + 1) : string.Empty;

            var cdv = string.Empty;
            if (includeCdv)
            {
                cdv = "&cdv=" + DateTime.Now.Ticks;
            }

            if (childPortalAlias.StartsWith(ApplicationPath))
            {
                return childPortalAlias + query + cdv;
            }

            return ApplicationPath + childPortalAlias + query + cdv;
        }

        // ****************************************************************************************
        // Constants are inlined in code and would require a rebuild of any module or skinobject
        // that may be using these constants.
        [Obsolete("This function has been replaced by DotNetNuke.Common.Utilities.HtmlUtils.FormatEmail. Scheduled removal in v11.0.0.")]
        public static string FormatEmail(string Email)
        {
            return HtmlUtils.FormatEmail(Email);
        }

        [Obsolete("This function has been replaced by DotNetNuke.Common.Utilities.HtmlUtils.FormatWebsite. Scheduled removal in v11.0.0.")]
        public static string FormatWebsite(object Website)
        {
            return HtmlUtils.FormatWebsite(Website);
        }

        [Obsolete("This function has been replaced by DotNetNuke.Common.Utilities.XmlUtils.XMLEncode. Scheduled removal in v11.0.0.")]
        public static string XMLEncode(string HTML)
        {
            return XmlUtils.XMLEncode(HTML);
        }

        [Obsolete("This function has been replaced by DotNetNuke.Common.Utilities.Config.GetConnectionString. Scheduled removal in v11.0.0.")]
        public static string GetDBConnectionString()
        {
            return Config.GetConnectionString();
        }

        [Obsolete("This method has been deprecated. . Scheduled removal in v11.0.0.")]
        public static ArrayList GetFileList(DirectoryInfo CurrentDirectory, [Optional, DefaultParameterValue("")] // ERROR: Optional parameters aren't supported in C#
                                                                                string strExtensions, [Optional, DefaultParameterValue(true)] // ERROR: Optional parameters aren't supported in C#
                                                                                bool NoneSpecified)
        {
            var arrFileList = new ArrayList();
            string strExtension = string.Empty;

            if (NoneSpecified)
            {
                arrFileList.Add(new FileItem(string.Empty, "<" + Localization.GetString("None_Specified") + ">"));
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
                if (strExtensions.IndexOf(strExtension, StringComparison.InvariantCultureIgnoreCase) != -1 || string.IsNullOrEmpty(strExtensions))
                {
                    arrFileList.Add(new FileItem(FileName, FileName));
                }
            }

            return arrFileList;
        }

        [Obsolete("This method has been deprecated. Replaced by GetSubFolderPath(ByVal strFileNamePath As String, ByVal portaId as Integer).. Scheduled removal in v11.0.0.")]
        public static string GetSubFolderPath(string strFileNamePath)
        {
            // Obtain PortalSettings from Current Context
            PortalSettings _portalSettings = PortalController.Instance.GetCurrentPortalSettings();
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

        [Obsolete("This function has been obsoleted: Use Common.Globals.LinkClick() for proper handling of URLs. Scheduled removal in v11.0.0.")]
        public static string LinkClickURL(string Link)
        {
            PortalSettings _portalSettings = PortalController.Instance.GetCurrentPortalSettings();
            return LinkClick(Link, _portalSettings.ActiveTab.TabID, -1, false);
        }

        [Obsolete("Deprecated PreventSQLInjection Function to consolidate Security Filter functions in the PortalSecurity class. Scheduled removal in v11.0.0.")]
        public static string PreventSQLInjection(string strSQL)
        {
            return PortalSecurity.Instance.InputFilter(strSQL, PortalSecurity.FilterFlag.NoSQL);
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

            // This calculation ensures that you have a more than one item that indicates you have already installed DNN.
            // While it is possible that you might not have an installation date or that you have deleted log files
            // it is unlikely that you have removed every trace of an installation and yet still have a working install
            bool isInstalled = (!IsInstallationURL()) && ((installationdatefactor + dataproviderfactor + htmlmodulefactor + portaldirectoryfactor + localexecutionfactor) >= c_PassingScore);

            // we need to tighten this check. We now are enforcing the existence of the InstallVersion value in web.config. If
            // this value exists, then DNN was previously installed, and we should never try to re-install it
            return isInstalled || HasInstallVersion();
        }

        /// <summary>
        /// Gets the culture code of the tab.
        /// </summary>
        /// <param name="TabID">The tab ID.</param>
        /// <param name="IsSuperTab">if set to <c>true</c> [is super tab].</param>
        /// <param name="settings">The settings.</param>
        /// <returns>return the tab's culture code, if ths tab doesn't exist, it will return current culture name.</returns>
        internal static string GetCultureCode(int TabID, bool IsSuperTab, IPortalSettings settings)
        {
            string cultureCode = Null.NullString;
            if (settings != null)
            {
                TabInfo linkTab = TabController.Instance.GetTab(TabID, IsSuperTab ? Null.NullInteger : settings.PortalId, false);
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

        internal static void ResetAppStartElapseTime()
        {
            AppStopwatch.Restart();
        }

        private static string GetCurrentDomainDirectory()
        {
            var dir = AppDomain.CurrentDomain.BaseDirectory.Replace("/", "\\");
            if (dir.Length > 3 && dir.EndsWith("\\"))
            {
                dir = dir.Substring(0, dir.Length - 1);
            }

            return dir;
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

            // If the provider path does not exist, then there can't be any log files
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
            string requestURL = HttpContext.Current.Request.RawUrl.ToLowerInvariant().Replace("\\", "/");
            return requestURL.Contains("/install.aspx") || requestURL.Contains("/installwizard.aspx");
        }

        private static void DeleteFile(string filePath)
        {
            try
            {
                FileSystemUtils.DeleteFile(filePath);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
        }

        private static void DeleteFolder(string strRoot)
        {
            try
            {
                Directory.Delete(strRoot);
            }
            catch (IOException)
            {
                // Force Deletion. Directory should be empty
                try
                {
                    Thread.Sleep(50);
                    Directory.Delete(strRoot, true);
                }
                catch (Exception ex)
                {
                    Logger.Error(ex);
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
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
    }
}
