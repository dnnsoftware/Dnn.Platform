// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Common
{
    using System;
    using System.Collections;
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
    using System.Threading.Tasks;
    using System.Web;
    using System.Web.Caching;
    using System.Web.UI;
    using System.Web.UI.HtmlControls;
    using System.Xml;

    using DotNetNuke.Abstractions;
    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Abstractions.Portals;
    using DotNetNuke.Collections.Internal;
    using DotNetNuke.Common.Extensions;
    using DotNetNuke.Common.Internal;
    using DotNetNuke.Common.Lists;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Data;
    using DotNetNuke.Entities.Host;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Modules.Actions;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Framework.JavaScriptLibraries;
    using DotNetNuke.Framework.Providers;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Internal.SourceGenerators;
    using DotNetNuke.Security;
    using DotNetNuke.Security.Permissions;
    using DotNetNuke.Security.Roles;
    using DotNetNuke.Services.Exceptions;
    using DotNetNuke.Services.FileSystem;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.Services.Personalization;
    using DotNetNuke.Services.Url.FriendlyUrl;
    using DotNetNuke.UI.Utilities;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.VisualBasic.CompilerServices;

    using DataCache = DotNetNuke.UI.Utilities.DataCache;

    /// <summary>The global instance of DotNetNuke. all basic functions and properties are defined in this instance.</summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "StyleCop.CSharp.NamingRules",
        "SA1303:Const field names should begin with upper-case letter",
        Justification = "Changing all these public constants to match the rule would be a massive breaking change just for a code style issue.")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "StyleCop.CSharp.NamingRules",
        "SA1310:Field names should not contain underscore",
        Justification = "Changing all these public fields names would be a massive breaking change only for a code style issue.")]
    [StandardModule]
    public sealed partial class Globals
    {
        /// <summary>Global role id for all users.</summary>
        /// <value>-1.</value>
        public const string glbRoleAllUsers = "-1";

        /// <summary>Global role id for super user.</summary>
        /// <value>-2.</value>
        public const string glbRoleSuperUser = "-2";

        /// <summary>Global role id for unauthenticated users.</summary>
        /// <value>-3.</value>
        public const string glbRoleUnauthUser = "-3";

        /// <summary>Global role id by default.</summary>
        /// <value>-4.</value>
        public const string glbRoleNothing = "-4";

        /// <summary>Global role name for all users.</summary>
        /// <value>All Users.</value>
        public const string glbRoleAllUsersName = "All Users";

        /// <summary>Global ro name for super user.</summary>
        /// <value>Superuser.</value>
        public const string glbRoleSuperUserName = "Superuser";

        /// <summary>Global role name for unauthenticated users.</summary>
        /// <value>Unauthenticated Users.</value>
        public const string glbRoleUnauthUserName = "Unauthenticated Users";

        /// <summary>Default page name.</summary>
        /// <value>Default.aspx.</value>
        public const string glbDefaultPage = "Default.aspx";

        /// <summary>Default host skin folder.</summary>
        /// <value>_default.</value>
        public const string glbHostSkinFolder = "_default";

        /// <summary>Default control panel.</summary>
        /// <value>Admin/ControlPanel/IconBar.ascx.</value>
        public const string glbDefaultControlPanel = "Admin/ControlPanel/IconBar.ascx";

        /// <summary>Default setting to determine if selected control panel is loaded to evaluate visibility.</summary>
        /// <value>false.</value>
        public const bool glbAllowControlPanelToDetermineVisibility = false;

        /// <summary>Default pane name.</summary>
        /// <value>ContentPane.</value>
        public const string glbDefaultPane = "ContentPane";

        /// <summary>Config files folder.</summary>
        /// <value>\Config\.</value>
        public const string glbConfigFolder = "\\Config\\";

        /// <summary>About page name.</summary>
        /// <value>about.htm.</value>
        public const string glbAboutPage = "about.htm";

        /// <summary>DotNetNuke config file.</summary>
        /// <value>DotNetNuke.config.</value>
        public const string glbDotNetNukeConfig = "DotNetNuke.config";

        /// <summary>Default portal id for super user.</summary>
        /// <value>-1.</value>
        public const int glbSuperUserAppName = -1;

        /// <summary>extension of protected files.</summary>
        /// <value>.resources.</value>
        public const string glbProtectedExtension = ".resources";

        /// <summary>Default container folder.</summary>
        /// <value>Portals/_default/Containers/.</value>
        public const string glbContainersPath = "Portals/_default/Containers/";

        /// <summary>Default skin folder.</summary>
        /// <value>Portals/_default/Skins/.</value>
        public const string glbSkinsPath = "Portals/_default/Skins/";

        /// <summary>
        /// Email address regex pattern that covers most scenarios.
        /// </summary>
        /// <value>A regex that covers most emails in a performant way.</value>
        public const string glbEmailRegEx =
            @"^[a-zA-Z0-9.!#$%&'*+/=?^_`{|}~-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";

        /// <summary>User Name regex pattern.</summary>
        /// <value></value>
        public const string glbUserNameRegEx = @"";

        /// <summary>User Name default minimum length.</summary>
        /// <value></value>
        public const int glbUserNameMinLength = 5;

        /// <summary>Format of a script tag.</summary>
        /// <value><![CDATA[<script type=\"text/javascript\" src=\"{0}\" ></script>]]></value>
        public const string glbScriptFormat = "<script type=\"text/javascript\" src=\"{0}\" ></script>";

        /// <summary>Validates for a valid email address.</summary>
        public static readonly Regex EmailValidatorRegex = new Regex(glbEmailRegEx, RegexOptions.Compiled);

        /// <summary>Finds non-alphanumeric characters.</summary>
        public static readonly Regex NonAlphanumericCharacters = new Regex("[^A-Za-z0-9]", RegexOptions.Compiled | RegexOptions.CultureInvariant);

        /// <summary>Finds any character that is not a letter, a number, an underscore or a dash.</summary>
        public static readonly Regex InvalidCharacters = new Regex("[^A-Za-z0-9_-]", RegexOptions.Compiled | RegexOptions.CultureInvariant);

        /// <summary>Validates if the first character is a letter.</summary>
        public static readonly Regex InvalidInitialCharacters = new Regex("^[^A-Za-z]", RegexOptions.Compiled | RegexOptions.CultureInvariant);

        /// <summary>Validates if the string contains only numbers.</summary>
        public static readonly Regex NumberMatchRegex = new Regex(@"^\d+$", RegexOptions.Compiled);

        /// <summary>Validates if a tag is an html 'base' tag.</summary>
        public static readonly Regex BaseTagRegex = new Regex("<base[^>]*>", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        /// <summary>Finds common file escaping characters.</summary>
        public static readonly Regex FileEscapingRegex = new Regex("[\\\\/]\\.\\.[\\\\/]", RegexOptions.Compiled);

        /// <summary>Checks if a string is a valid Windows file extension.</summary>
        public static readonly Regex FileExtensionRegex = new Regex(@"\..+;", RegexOptions.Compiled);

        /// <summary>Checks if a string is a valid Windows filename.</summary>
        public static readonly Regex FileValidNameRegex = new Regex(@"^(?!(?:PRN|AUX|CLOCK\$|NUL|CON|COM\d|LPT\d)(?:\..+)?$)[^\x00-\x1F\xA5\\?*:\"";|\/<>]+(?<![\s.])$", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

        /// <summary>Checks if a url part is a valid services framework url.</summary>
        public static readonly Regex ServicesFrameworkRegex = new Regex("/API/|DESKTOPMODULES/.+/API/", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

        /// <summary>Checks for invalid usernames.</summary>
        public static readonly string USERNAME_UNALLOWED_ASCII = "!\"#$%&'()*+,/:;<=>?[\\]^`{|}";

        private const string tabPathInvalidCharsEx = "[&\\? \\./'#:\\*]"; // this value should keep same with the value used in sp BuildTabLevelAndPath to remove invalid chars.

        private static readonly Regex TabPathInvalidCharsRx = new Regex(tabPathInvalidCharsEx, RegexOptions.Compiled);

        private static readonly Stopwatch AppStopwatch = Stopwatch.StartNew();

        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Globals));
        private static string applicationPath;
        private static string desktopModulePath;
        private static string imagePath;
        private static string hostMapPath;
        private static string hostPath;
        private static string installMapPath;
        private static string installPath;

        private static IServiceProvider dependencyProvider;
        private static IApplicationStatusInfo applicationStatusInfo;
        private static INavigationManager navigationManager;

        // global constants for the life of the application ( set in Application_Start )

        /// <inheritdoc cref="DotNetNuke.Abstractions.Application.PerformanceSettings"/>
        public enum PerformanceSettings
        {
            /// <inheritdoc cref="Abstractions.Application.PerformanceSettings.NoCaching"/>
            NoCaching = 0,

            /// <inheritdoc cref="Abstractions.Application.PerformanceSettings.LightCaching"/>
            LightCaching = 1,

            /// <inheritdoc cref="Abstractions.Application.PerformanceSettings.ModerateCaching"/>
            ModerateCaching = 3,

            /// <inheritdoc cref="Abstractions.Application.PerformanceSettings.HeavyCaching"/>
            HeavyCaching = 6,
        }

        /// <summary>Enumeration Of Registration Type for portal.</summary>
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
            /// <summary>Disabled Registration.</summary>
            NoRegistration = 0,

            /// <summary>Account need be approved by portal's administrator.</summary>
            PrivateRegistration = 1,

            /// <summary>Account will be available after post registration data successful.</summary>
            PublicRegistration = 2,

            /// <summary>Account will be available by verify code.</summary>
            VerifiedRegistration = 3,
        }

        /// <summary>Enumeration Of Application upgrade status.</summary>
        public enum UpgradeStatus
        {
            /// <summary>The application need update to a higher version.</summary>
            Upgrade = 0,

            /// <summary>The application need to install itself.</summary>
            Install = 1,

            /// <summary>The application is normal running.</summary>
            None = 2,

            /// <summary>The application occur error when running.</summary>
            Error = 3,

            /// <summary>The application status is unknown.</summary>
            /// <remarks>This status should never be returned. its is only used as a flag that Status hasn't been determined.</remarks>
            Unknown = 4,
        }

        /// <summary>Gets the application path.</summary>
        public static string ApplicationPath
        {
            get
            {
                if (applicationPath == null && (HttpContext.Current != null))
                {
                    if (HttpContext.Current.Request.ApplicationPath == "/")
                    {
                        applicationPath = string.IsNullOrEmpty(Config.GetSetting("InstallationSubfolder")) ? string.Empty : (Config.GetSetting("InstallationSubfolder") + "/").ToLowerInvariant();
                    }
                    else
                    {
                        applicationPath = HttpContext.Current.Request.ApplicationPath.ToLowerInvariant();
                    }
                }

                return applicationPath;
            }
        }

        /// <summary>Gets the application map path.</summary>
        /// <value>
        /// The application map path.
        /// </value>
        [Obsolete("Deprecated in DotNetNuke 9.7.1. Use Dependency Injection to resolve 'DotNetNuke.Abstractions.IApplicationStatusInfo' instead. Scheduled removal in v11.0.0.")]
        public static string ApplicationMapPath => applicationStatusInfo.ApplicationMapPath;

        /// <summary>Gets the desktop module path.</summary>
        /// <value>ApplicationPath + "/DesktopModules/".</value>
        public static string DesktopModulePath
        {
            get
            {
                if (desktopModulePath == null)
                {
                    desktopModulePath = ApplicationPath + "/DesktopModules/";
                }

                return desktopModulePath;
            }
        }

        /// <summary>Gets the image path.</summary>
        /// <value>ApplicationPath + "/Images/".</value>
        public static string ImagePath
        {
            get
            {
                if (imagePath == null)
                {
                    imagePath = ApplicationPath + "/Images/";
                }

                return imagePath;
            }
        }

        /// <summary>Gets the database version.</summary>
        [Obsolete("Deprecated in DotNetNuke 9.7.1. Use Dependency Injection to resolve 'DotNetNuke.Abstractions.IApplicationStatusInfo' instead. Scheduled removal in v11.0.0.")]
        public static Version DataBaseVersion { get => applicationStatusInfo.DatabaseVersion; }

        /// <summary>Gets the host map path.</summary>
        /// <value>ApplicationMapPath + "Portals\_default\".</value>
        public static string HostMapPath
        {
            get
            {
                if (hostMapPath == null)
                {
                    hostMapPath = Path.Combine(applicationStatusInfo.ApplicationMapPath, @"Portals\_default\");
                }

                return hostMapPath;
            }
        }

        /// <summary>Gets the host path.</summary>
        /// <value>ApplicationPath + "/Portals/_default/".</value>
        public static string HostPath
        {
            get
            {
                if (hostPath == null)
                {
                    hostPath = ApplicationPath + "/Portals/_default/";
                }

                return hostPath;
            }
        }

        /// <summary>Gets the install map path.</summary>
        /// <value>server map path of InstallPath.</value>
        public static string InstallMapPath
        {
            get
            {
                if (installMapPath == null)
                {
                    installMapPath = applicationStatusInfo.ApplicationMapPath + "\\Install\\";
                }

                return installMapPath;
            }
        }

        /// <summary>Gets the install path.</summary>
        /// <value>ApplicationPath + "/Install/".</value>
        public static string InstallPath
        {
            get
            {
                if (installPath == null)
                {
                    installPath = ApplicationPath + "/Install/";
                }

                return installPath;
            }
        }

        /// <summary>Gets the status of application.</summary>
        /// <seealso cref="GetStatus"/>
        [Obsolete("Deprecated in DotNetNuke 9.7.1. Use Dependency Injection to resolve 'DotNetNuke.Abstractions.IApplicationStatusInfo' instead. Scheduled removal in v11.0.0.")]
        public static UpgradeStatus Status { get => (UpgradeStatus)applicationStatusInfo.Status; }

        /// <summary>Gets image file types.</summary>
        /// <value>Values read from ImageTypes List. If there is not a List, default values will be jpg,jpeg,jpe,gif,bmp,png,svg,ico.</value>
        [Obsolete("Deprecated in DotNetNuke 9.8.1. Use ImageFileTypes instead. Scheduled removal in v11.0.0.")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "StyleCop.CSharp.NamingRules",
            "SA1300:Element should begin with upper-case letter",
            Justification = "Kept to prevent a breaking change, new overload created.")]
        public static string glbImageFileTypes => ImageFileTypes;

        /// <summary>Gets image file types.</summary>
        /// <value>Values read from ImageTypes List. If there is not a List, default values will be jpg,jpeg,jpe,gif,bmp,png,svg,ico.</value>
        public static string ImageFileTypes
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

        /// <summary>Gets a value indicating for how long the application has been running.</summary>
        public static TimeSpan ElapsedSinceAppStart
        {
            get
            {
                return AppStopwatch.Elapsed;
            }
        }

        /// <summary>Gets or sets the name of the IIS app.</summary>
        /// <value>
        /// request.ServerVariables["APPL_MD_PATH"].
        /// </value>
        public static string IISAppName { get; set; }

        /// <summary>Gets or sets the name of the server.</summary>
        /// <value>
        /// server name in config file or the server's marchine name.
        /// </value>
        public static string ServerName { get; set; }

        /// <summary>Gets or sets the operating system version.</summary>
        /// <value>
        /// The operating system version.
        /// </value>
        public static Version OperatingSystemVersion { get; set; }

        /// <summary>Gets or sets the NET framework version.</summary>
        /// <value>
        /// The NET framework version.
        /// </value>
        public static Version NETFrameworkVersion { get; set; }

        /// <summary>Gets the .Net framework version text.</summary>
        /// <value>The .Net framework version text.</value>
        public static string FormattedNetFrameworkVersion => FormatVersion(NETFrameworkVersion, "0", 3, ".");

        /// <summary>Gets or sets the database engine version.</summary>
        /// <value>
        /// The database engine version.
        /// </value>
        public static Version DatabaseEngineVersion { get; set; }

        /// <summary>Gets or sets the Dependency Service.</summary>
        /// <value>The Dependency Service.</value>
        internal static IServiceProvider DependencyProvider
        {
            get => dependencyProvider;
            set
            {
                dependencyProvider = value;
                if (dependencyProvider is INotifyPropertyChanged hasPropertyChanged)
                {
                    hasPropertyChanged.PropertyChanged += OnDependencyProviderChanged;
                }
                else
                {
                    OnDependencyProviderChanged(null, null);
                }
            }
        }

        /// <summary>Redirects the specified URL.</summary>
        /// <param name="url">The URL.</param>
        /// <param name="endResponse">if set to <see langword="true"/> [end response].</param>
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

        /// <summary>Checks if incremental sqlDataProvider files exist.</summary>
        /// <example>If a 09.08.01.01.sqlDataProvider file exists for a provided version 09.08.01 this method will return true.</example>
        /// <param name="version">The version.</param>
        /// <returns>A value indicating whether any incremental sql script file exists.</returns>
        [DnnDeprecated(9, 7, 1, "Use Dependency Injection to resolve 'DotNetNuke.Abstractions.IApplicationStatusInfo' instead")]
        public static partial bool IncrementalVersionExists(Version version) => applicationStatusInfo.IncrementalVersionExists(version);

        /// <summary>Builds the cross tab dataset.</summary>
        /// <param name="dataSetName">Name of the data set.</param>
        /// <param name="result">The result.</param>
        /// <param name="fixedColumns">The fixed columns.</param>
        /// <param name="variableColumns">The variable columns.</param>
        /// <param name="keyColumn">The key column.</param>
        /// <param name="fieldColumn">The field column.</param>
        /// <param name="fieldTypeColumn">The field type column.</param>
        /// <param name="stringValueColumn">The string value column.</param>
        /// <param name="numericValueColumn">The numeric value column.</param>
        /// <returns>the dataset instance.</returns>
        [DnnDeprecated(9, 8, 1, "No replacement")]
        public static partial DataSet BuildCrossTabDataSet(
            string dataSetName,
            IDataReader result,
            string fixedColumns,
            string variableColumns,
            string keyColumn,
            string fieldColumn,
            string fieldTypeColumn,
            string stringValueColumn,
            string numericValueColumn)
        {
            return BuildCrossTabDataSet(dataSetName, result, fixedColumns, variableColumns, keyColumn, fieldColumn, fieldTypeColumn, stringValueColumn, numericValueColumn, CultureInfo.CurrentCulture);
        }

        /// <summary>converts a data reader with serialized fields into a typed data set.</summary>
        /// <param name="dataSetName">Name of the dataset to be created.</param>
        /// <param name="result">Data reader that contains all field values serialized.</param>
        /// <param name="fixedColumns">List of fixed columns, delimited by commas. Columns must be contained in DataReader.</param>
        /// <param name="variableColumns">List of variable columns, delimited by commas. Columns must be contained in DataReader.</param>
        /// <param name="keyColumn">Name of the column, that contains the row ID. Column must be contained in DataReader.</param>
        /// <param name="fieldColumn">Name of the column, that contains the field name. Column must be contained in DataReader.</param>
        /// <param name="fieldTypeColumn">Name of the column, that contains the field type name. Column must be contained in DataReader.</param>
        /// <param name="stringValueColumn">Name of the column, that contains the field value, if stored as string. Column must be contained in DataReader.</param>
        /// <param name="numericValueColumn">Name of the column, that contains the field value, if stored as number. Column must be contained in DataReader.</param>
        /// <param name="culture">culture of the field values in data reader's string value column.</param>
        /// <returns>The generated DataSet.</returns>
        [DnnDeprecated(9, 8, 1, "No replacement")]
        public static partial DataSet BuildCrossTabDataSet(
            string dataSetName,
            IDataReader result,
            string fixedColumns,
            string variableColumns,
            string keyColumn,
            string fieldColumn,
            string fieldTypeColumn,
            string stringValueColumn,
            string numericValueColumn,
            CultureInfo culture)
        {
            string[] arrFixedColumns = null;
            string[] arrVariableColumns = null;
            string[] arrField;
            string fieldType;
            int intColumn;
            int intKeyColumn;

            // create dataset
            var crosstab = new DataSet(dataSetName);
            crosstab.Namespace = "NetFrameWork";

            // create table
            var tab = new DataTable(dataSetName);

            // split fixed columns
            arrFixedColumns = fixedColumns.Split(',');

            // add fixed columns to table
            for (intColumn = 0; intColumn < arrFixedColumns.Length; intColumn++)
            {
                arrField = arrFixedColumns[intColumn].Split('|');
                var col = new DataColumn(arrField[0], Type.GetType("System." + arrField[1]));
                tab.Columns.Add(col);
            }

            // split variable columns
            if (!string.IsNullOrEmpty(variableColumns))
            {
                arrVariableColumns = variableColumns.Split(',');

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
                if (Convert.ToInt32(result[keyColumn]) != intKeyColumn)
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
                    if (!string.IsNullOrEmpty(variableColumns))
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

                    intKeyColumn = Convert.ToInt32(result[keyColumn]);
                }

                // assign pivot column value
                if (!string.IsNullOrEmpty(fieldTypeColumn))
                {
                    fieldType = result[fieldTypeColumn].ToString();
                }
                else
                {
                    fieldType = "String";
                }

                switch (fieldType)
                {
                    case "Decimal":
                        row[Convert.ToInt32(result[fieldColumn])] = result[numericValueColumn];
                        break;
                    case "String":
                        if (ReferenceEquals(culture, CultureInfo.CurrentCulture))
                        {
                            row[result[fieldColumn].ToString()] = result[stringValueColumn];
                        }
                        else
                        {
                            switch (tab.Columns[result[fieldColumn].ToString()].DataType.ToString())
                            {
                                case "System.Decimal":
                                case "System.Currency":
                                    row[result[fieldColumn].ToString()] = decimal.Parse(result[stringValueColumn].ToString(), culture);
                                    break;
                                case "System.Int32":
                                    row[result[fieldColumn].ToString()] = int.Parse(result[stringValueColumn].ToString(), culture);
                                    break;
                                default:
                                    row[result[fieldColumn].ToString()] = result[stringValueColumn];
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

        /// <summary>Converts the datareader to dataset.</summary>
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

        /// <summary>Converts the datareader to datatable.</summary>
        /// <param name="reader">The reader.</param>
        /// <returns>the datatable instance.</returns>
        public static DataTable ConvertDataReaderToDataTable(IDataReader reader)
        {
            return ConvertDataReaderToDataTable(reader, true);
        }

        /// <summary>Converts the datareader to datatable.</summary>
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

        /// <summary>Gets the absolute server path.</summary>
        /// <param name="request">The request.</param>
        /// <returns>absolute server path.</returns>
        public static string GetAbsoluteServerPath(HttpRequest request)
        {
            string strServerPath;
            strServerPath = request.MapPath(request.ApplicationPath);
            if (!strServerPath.EndsWith("\\"))
            {
                strServerPath += "\\";
            }

            return strServerPath;
        }

        /// <summary>Gets the ApplicationName for the MemberRole API.</summary>
        /// <remarks>
        /// This overload is used to get the current ApplcationName.  The Application
        /// Name is in the form Prefix_Id, where Prefix is the object qualifier
        /// for this instance of DotNetNuke, and Id is the current PortalId for normal
        /// users or glbSuperUserAppName for SuperUsers.
        /// </remarks>
        /// <returns>A string representing the application name.</returns>
        public static string GetApplicationName()
        {
            string appName;
            if (HttpContext.Current.Items["ApplicationName"] == null || string.IsNullOrEmpty(HttpContext.Current.Items["ApplicationName"].ToString()))
            {
                var portalSettings = PortalController.Instance.GetCurrentSettings();
                if (portalSettings == null)
                {
                    appName = "/";
                }
                else
                {
                    appName = GetApplicationName(portalSettings.PortalId);
                }
            }
            else
            {
                appName = Convert.ToString(HttpContext.Current.Items["ApplicationName"]);
            }

            return appName;
        }

        /// <summary>Gets the ApplicationName for the MemberRole API.</summary>
        /// <remarks>
        /// This overload is used to build the Application Name from the Portal Id.
        /// </remarks>
        /// <param name="portalID">The id of the portal (site).</param>
        /// <returns>A string representing the application name.</returns>
        public static string GetApplicationName(int portalID)
        {
            string appName;

            // Get the Data Provider Configuration
            ProviderConfiguration providerConfiguration = ProviderConfiguration.GetProviderConfiguration("data");

            // Read the configuration specific information for the current Provider
            var objProvider = (Provider)providerConfiguration.Providers[providerConfiguration.DefaultProvider];

            // Get the Object Qualifier from the Provider Configuration
            string objectQualifier = objProvider.Attributes["objectQualifier"];
            if (!string.IsNullOrEmpty(objectQualifier) && objectQualifier.EndsWith("_") == false)
            {
                objectQualifier += "_";
            }

            appName = objectQualifier + Convert.ToString(portalID);
            return appName;
        }

        /// <summary>Finds the database version.</summary>
        /// <param name="major">The major.</param>
        /// <param name="minor">The minor.</param>
        /// <param name="build">The build.</param>
        /// <returns>return <see langword="true"/> if can find the specific version, otherwise will retur <see langword="false"/>.</returns>
        public static bool FindDatabaseVersion(int major, int minor, int build)
        {
            bool version = false;
            IDataReader dr = null;
            try
            {
                dr = DataProvider.Instance().FindDatabaseVersion(major, minor, build);
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

        /// <summary>Updates the database version.</summary>
        /// <param name="version">The version.</param>
        [DnnDeprecated(9, 7, 1, "Use Dependency Injection to resolve 'DotNetNuke.Abstractions.IApplicationStatusInfo' instead")]
        public static partial void UpdateDataBaseVersion(Version version) => applicationStatusInfo.UpdateDatabaseVersion(version);

        /// <summary>Updates the database version.</summary>
        /// <param name="version">The version.</param>
        /// <param name="increment">The increment (revision) number.</param>
        [DnnDeprecated(9, 7, 1, "Use Dependency Injection to resolve 'DotNetNuke.Abstractions.IApplicationStatusInfo' instead")]
        public static partial void UpdateDataBaseVersionIncrement(Version version, int increment) =>
            applicationStatusInfo.UpdateDatabaseVersionIncrement(version, increment);

        /// <summary>Gets the last applied iteration (revision).</summary>
        /// <param name="version">The version for which to check the last revision.</param>
        /// <returns>The last applied iteration (revision) for the requested version.</returns>
        [DnnDeprecated(9, 7, 1, "Use Dependency Injection to resolve 'DotNetNuke.Abstractions.IApplicationStatusInfo' instead")]
        public static partial int GetLastAppliedIteration(Version version) =>
            applicationStatusInfo.GetLastAppliedIteration(version);

        /// <summary>Adds the port.</summary>
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

        /// <summary>Gets the name of the domain.</summary>
        /// <param name="request">The request.</param>
        /// <returns>domain name.</returns>
        public static string GetDomainName(HttpRequest request)
        {
            return GetDomainName(new HttpRequestWrapper(request), false);
        }

        /// <summary>Gets the name of the domain.</summary>
        /// <param name="request">The request.</param>
        /// <returns>domain name.</returns>
        public static string GetDomainName(HttpRequestBase request)
        {
            return GetDomainName(request, false);
        }

        /// <summary>returns the domain name of the current request ( ie. www.domain.com or 207.132.12.123 or www.domain.com/directory if subhost ).</summary>
        /// <param name="request">The request.</param>
        /// <param name="parsePortNumber">if set to <see langword="true"/> [parse port number].</param>
        /// <returns>domain name.</returns>
        public static string GetDomainName(HttpRequest request, bool parsePortNumber)
        {
            return GetDomainName(new HttpRequestWrapper(request), parsePortNumber);
        }

        /// <summary>returns the domain name of the current request ( ie. www.domain.com or 207.132.12.123 or www.domain.com/directory if subhost ).</summary>
        /// <param name="request">The request.</param>
        /// <param name="parsePortNumber">if set to <see langword="true"/> [parse port number].</param>
        /// <returns>domain name.</returns>
        public static string GetDomainName(HttpRequestBase request, bool parsePortNumber)
        {
            return TestableGlobals.Instance.GetDomainName(request.Url, parsePortNumber);
        }

        /// <summary>Determin whether use port number by the value in config file.</summary>
        /// <returns>
        /// <see langword="true"/> if use port number, otherwise, return <see langword="false"/>.
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

        /// <summary>Gets the file list.</summary>
        /// <returns>file list.</returns>
        public static ArrayList GetFileList()
        {
            return GetFileList(-1, string.Empty, true, string.Empty, false);
        }

        /// <summary>Gets the file list.</summary>
        /// <param name="portalId">The portal id.</param>
        /// <returns>file list.</returns>
        public static ArrayList GetFileList(int portalId)
        {
            return GetFileList(portalId, string.Empty, true, string.Empty, false);
        }

        /// <summary>Gets the file list.</summary>
        /// <param name="portalId">The portal id.</param>
        /// <param name="strExtensions">The STR extensions.</param>
        /// <returns>file list.</returns>
        public static ArrayList GetFileList(int portalId, string strExtensions)
        {
            return GetFileList(portalId, strExtensions, true, string.Empty, false);
        }

        /// <summary>Gets the file list.</summary>
        /// <param name="portalId">The portal id.</param>
        /// <param name="strExtensions">The STR extensions.</param>
        /// <param name="noneSpecified">if set to <see langword="true"/> [none specified].</param>
        /// <returns>file list.</returns>
        public static ArrayList GetFileList(int portalId, string strExtensions, bool noneSpecified)
        {
            return GetFileList(portalId, strExtensions, noneSpecified, string.Empty, false);
        }

        /// <summary>Gets the file list.</summary>
        /// <param name="portalId">The portal id.</param>
        /// <param name="strExtensions">The STR extensions.</param>
        /// <param name="noneSpecified">if set to <see langword="true"/> [none specified].</param>
        /// <param name="folder">The folder.</param>
        /// <returns>file list.</returns>
        public static ArrayList GetFileList(int portalId, string strExtensions, bool noneSpecified, string folder)
        {
            return GetFileList(portalId, strExtensions, noneSpecified, folder, false);
        }

        /// <summary>Gets the file list.</summary>
        /// <param name="portalId">The portal id.</param>
        /// <param name="strExtensions">The STR extensions.</param>
        /// <param name="noneSpecified">if set to <see langword="true"/> [none specified].</param>
        /// <param name="folder">The folder.</param>
        /// <param name="includeHidden">if set to <see langword="true"/> [include hidden].</param>
        /// <returns>file list.</returns>
        public static ArrayList GetFileList(int portalId, string strExtensions, bool noneSpecified, string folder, bool includeHidden)
        {
            var arrFileList = new ArrayList();
            if (noneSpecified)
            {
                arrFileList.Add(new FileItem(string.Empty, "<" + Localization.GetString("None_Specified") + ">"));
            }

            var objFolder = FolderManager.Instance.GetFolder(portalId, folder);

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

        /// <summary>Gets the host portal settings.</summary>
        /// <returns>Host portal settings.</returns>
        [DnnDeprecated(10, 0, 2, "Use overload taking IHostSettings")]
        public static partial PortalSettings GetHostPortalSettings()
            => GetHostPortalSettings(GetCurrentServiceProvider().GetRequiredService<IHostSettings>());

        /// <summary>Gets the host portal settings.</summary>
        /// <param name="hostSettings">The host settings.</param>
        /// <returns>Host portal settings.</returns>
        public static PortalSettings GetHostPortalSettings(IHostSettings hostSettings)
        {
            int tabId = -1;
            int portalId = -1;
            IPortalAliasInfo objPortalAliasInfo = null;

            // if the portal alias exists
            if (hostSettings.HostPortalId > Null.NullInteger)
            {
                portalId = hostSettings.HostPortalId;

                // use the host portal
                objPortalAliasInfo = new PortalAliasInfo();
                objPortalAliasInfo.PortalId = portalId;
            }

            // load the PortalSettings into current context
            return new PortalSettings(tabId, objPortalAliasInfo as PortalAliasInfo);
        }

        /// <summary>Gets the portal domain name.</summary>
        /// <param name="strPortalAlias">The portal alias.</param>
        /// <param name="request">The request or <c>null</c>.</param>
        /// <param name="blnAddHTTP">if set to <see langword="true"/> calls <see cref="AddHTTP"/> on the result.</param>
        /// <returns>domain name.</returns>
        public static string GetPortalDomainName(string strPortalAlias, HttpRequest request, bool blnAddHTTP)
        {
            string strDomainName = string.Empty;
            string strURL = string.Empty;
            int intAlias;
            if (request != null)
            {
                strURL = GetDomainName(request);
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

        /// <summary>Gets the portal settings.</summary>
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

        /// <summary>Returns the folder path under the root for the portal.</summary>
        /// <param name="strFileNamePath">The folder the absolute path.</param>
        /// <param name="portalId">Portal Id.</param>
        /// <returns>A string containing the subfolder path.</returns>
        public static string GetSubFolderPath(string strFileNamePath, int portalId)
        {
            string parentFolderName;
            if (portalId == Null.NullInteger)
            {
                parentFolderName = HostMapPath.Replace("/", "\\");
            }
            else
            {
                PortalInfo objPortal = PortalController.Instance.GetPortal(portalId);
                parentFolderName = objPortal.HomeDirectoryMapPath.Replace("/", "\\");
            }

            string strFolderpath = strFileNamePath.Substring(0, strFileNamePath.LastIndexOf("\\") + 1);
            return strFolderpath.Substring(parentFolderName.Length).Replace("\\", "/");
        }

        /// <summary>The GetTotalRecords method gets the number of Records returned.</summary>
        /// <param name="dr">An <see cref="IDataReader"/> containing the Total number of records.</param>
        /// <returns>The total number of records in the data reader.</returns>
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

        /// <summary>Sets the status.</summary>
        /// <param name="status">The status.</param>
        [DnnDeprecated(9, 7, 1, "Use Dependency Injection to resolve 'DotNetNuke.Abstractions.IApplicationStatusInfo' instead")]
        public static partial void SetStatus(UpgradeStatus status) =>
            applicationStatusInfo.SetStatus((DotNetNuke.Abstractions.Application.UpgradeStatus)status);

        /// <summary>
        /// ImportFile - converts a file url (/Portals/0/somefile.gif) to the appropriate
        /// FileID=xx identification for use in importing portals, tabs and modules.
        /// </summary>
        /// <param name="portalId">The id of the portal (site) on which the file is imported.</param>
        /// <param name="url">The url to the file.</param>
        /// <returns>The file handler url for the file (FileID=xxxx).</returns>
        public static string ImportFile(int portalId, string url)
        {
            string strUrl = url;
            if (GetURLType(url) == TabType.File)
            {
                var fileName = Path.GetFileName(url);

                var folderPath = url.Substring(0, url.LastIndexOf(fileName));
                var folder = FolderManager.Instance.GetFolder(portalId, folderPath);

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

        /// <summary>Encode the post url.</summary>
        /// <param name="strPost">The post url.</param>
        /// <returns>encoded value.</returns>
        public static string HTTPPOSTEncode(string strPost)
        {
            strPost = strPost.Replace("\\", string.Empty);
            strPost = HttpUtility.UrlEncode(strPost);
            strPost = strPost.Replace("%2f", "/");
            return strPost;
        }

        /// <summary>Sets the ApplicationName for the MemberRole API.</summary>
        /// <remarks>
        /// This overload takes a the PortalId.
        /// </remarks>
        /// <param name="portalId">The Portal ID.</param>
        public static void SetApplicationName(int portalId)
        {
            HttpContext.Current.Items["ApplicationName"] = GetApplicationName(portalId);
        }

        /// <summary>Sets the ApplicationName for the MemberRole API.</summary>
        /// <remarks>
        /// This overload takes a the PortalId.
        /// </remarks>
        /// <param name="applicationName">The Application Name to set.</param>
        public static void SetApplicationName(string applicationName)
        {
            HttpContext.Current.Items["ApplicationName"] = applicationName;
        }

        /// <summary>Formats the address on a single line ( ie. Unit, Street, City, Region, Country, PostalCode ).</summary>
        /// <param name="unit">The unit.</param>
        /// <param name="street">The street.</param>
        /// <param name="city">The city.</param>
        /// <param name="region">The region.</param>
        /// <param name="country">The country.</param>
        /// <param name="postalCode">The postal code.</param>
        /// <returns>A string containing the whole address on a single line.</returns>
        public static string FormatAddress(object unit, object street, object city, object region, object country, object postalCode)
        {
            string strAddress = string.Empty;
            if (unit != null)
            {
                if (!string.IsNullOrEmpty(unit.ToString().Trim()))
                {
                    strAddress += ", " + unit;
                }
            }

            if (street != null)
            {
                if (!string.IsNullOrEmpty(street.ToString().Trim()))
                {
                    strAddress += ", " + street;
                }
            }

            if (city != null)
            {
                if (!string.IsNullOrEmpty(city.ToString().Trim()))
                {
                    strAddress += ", " + city;
                }
            }

            if (region != null)
            {
                if (!string.IsNullOrEmpty(region.ToString().Trim()))
                {
                    strAddress += ", " + region;
                }
            }

            if (country != null)
            {
                if (!string.IsNullOrEmpty(country.ToString().Trim()))
                {
                    strAddress += ", " + country;
                }
            }

            if (postalCode != null)
            {
                if (!string.IsNullOrEmpty(postalCode.ToString().Trim()))
                {
                    strAddress += ", " + postalCode;
                }
            }

            if (!string.IsNullOrEmpty(strAddress.Trim()))
            {
                strAddress = strAddress.Substring(2);
            }

            return strAddress;
        }

        /// <summary>Formats the system.version into the standard format nn.nn.nn.</summary>
        /// <param name="version">The version.</param>
        /// <returns>Formatted  version as string.</returns>
        public static string FormatVersion(Version version)
        {
            return FormatVersion(version, false);
        }

        /// <summary>Formats the version.</summary>
        /// <param name="version">The version.</param>
        /// <param name="includeBuild">if set to <see langword="true"/> [include build].</param>
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

        /// <summary>Formats  a version into the standard format nn.nn.nn.</summary>
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

        /// <summary>Cloaks the text, obfuscate sensitive data to prevent collection by robots and spiders and crawlers.</summary>
        /// <param name="personalInfo">The personal info.</param>
        /// <returns>obfuscated sensitive data by hustling ASCII characters.</returns>
        public static string CloakText(string personalInfo)
        {
            if (personalInfo == null)
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

            var characterCodes = personalInfo.Select(ch => ((int)ch).ToString(CultureInfo.InvariantCulture));
            return string.Format(Script, string.Join(",", characterCodes.ToArray()));
        }

        /// <summary>Gets the medium date by current culture.</summary>
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

        /// <summary>Gets the short date.</summary>
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

        /// <summary>Determines whether current request contains admin control information.</summary>
        /// <returns>
        ///   <see langword="true"/> if current request contains admin control information; otherwise, <see langword="false"/>.
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

        /// <summary>Determines whether current request use admin skin.</summary>
        /// <returns>
        ///   <see langword="true"/> if current request use admin skin; otherwise, <see langword="false"/>.
        /// </returns>
        public static bool IsAdminSkin()
        {
            bool isAdminSkin = Null.NullBoolean;
            if (HttpContext.Current != null)
            {
                string adminKeys = "tab,module,importmodule,exportmodule,help";
                string controlKey = string.Empty;
                if (HttpContext.Current.Request.QueryString["ctl"] != null)
                {
                    controlKey = HttpContext.Current.Request.QueryString["ctl"].ToLowerInvariant();
                }

                int moduleID = -1;
                if (HttpContext.Current.Request.QueryString["mid"] != null)
                {
                    if (!int.TryParse(HttpContext.Current.Request.QueryString["mid"], out moduleID))
                    {
                        moduleID = -1;
                    }
                }

                isAdminSkin = (!string.IsNullOrEmpty(controlKey) && controlKey != "view" && moduleID != -1) ||
                               (!string.IsNullOrEmpty(controlKey) && adminKeys.IndexOf(controlKey) != -1 && moduleID == -1);
            }

            return isAdminSkin;
        }

        /// <summary>Returns whether the current tab is in EditMode.</summary>
        /// <returns><see langword="true"/> if the tab is in Edit mode; otherwise <see langword="false"/>.</returns>
        public static bool IsEditMode()
        {
            return Personalization.GetUserMode() == PortalSettings.Mode.Edit && TabPermissionController.CanAddContentToPage();
        }

        /// <summary>Returns whether the current tab is in LayoutMode.</summary>
        /// <returns><see langword="true"/> if the current tab is in layout mode; otherwise <see langword="false"/>.</returns>
        public static bool IsLayoutMode()
        {
            return TabPermissionController.CanAddContentToPage() && Personalization.GetUserMode() == PortalSettings.Mode.Layout;
        }

        /// <summary>Creates the RSS.</summary>
        /// <param name="dr">The dr.</param>
        /// <param name="titleField">The title field.</param>
        /// <param name="urlField">The URL field.</param>
        /// <param name="createdDateField">The created date field.</param>
        /// <param name="syndicateField">The syndicate field.</param>
        /// <param name="domainName">Name of the domain.</param>
        /// <param name="fileName">Name of the file.</param>
        public static void CreateRSS(
            IDataReader dr,
            string titleField,
            string urlField,
            string createdDateField,
            string syndicateField,
            string domainName,
            string fileName)
        {
            // Obtain PortalSettings from Current Context
            var portalSettings = PortalController.Instance.GetCurrentSettings();
            var strRSS = new StringBuilder();
            var strRelativePath = domainName + fileName.Substring(fileName.IndexOf("\\Portals", StringComparison.InvariantCultureIgnoreCase)).Replace("\\", "/");
            strRelativePath = strRelativePath.Substring(0, strRelativePath.LastIndexOf("/", StringComparison.InvariantCulture));
            try
            {
                while (dr.Read())
                {
                    if (!int.TryParse((dr[syndicateField] ?? string.Empty).ToString(), out var field) || field <= 0)
                    {
                        continue;
                    }

                    strRSS.AppendLine(" <item>");
                    strRSS.AppendLine("  <title>" + dr[titleField] + "</title>");
                    var drUrl = (dr["URL"] ?? string.Empty).ToString();
                    if (drUrl.IndexOf("://", StringComparison.InvariantCulture) == -1)
                    {
                        strRSS.Append("  <link>");
                        if (NumberMatchRegex.IsMatch(drUrl))
                        {
                            strRSS.Append(domainName + "/" + glbDefaultPage + "?tabid=" + dr[urlField]);
                        }
                        else
                        {
                            strRSS.Append(strRelativePath + dr[urlField]);
                        }

                        strRSS.AppendLine("</link>");
                    }
                    else
                    {
                        strRSS.AppendLine("  <link>" + dr[urlField] + "</link>");
                    }

                    strRSS.AppendLine("  <description>" + portalSettings.PortalName + " " + GetMediumDate(dr[createdDateField].ToString()) + "</description>");
                    strRSS.AppendLine(" </item>");
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
                         portalSettings.PortalName + "</title>" + Environment.NewLine + "  <link>" + domainName + "</link>" + Environment.NewLine + "  <description>" +
                         portalSettings.PortalName + "</description>" + Environment.NewLine + "  <language>en-us</language>" + Environment.NewLine + "  <copyright>" +
                         (!string.IsNullOrEmpty(portalSettings.FooterText) ? portalSettings.FooterText.Replace("[year]", DateTime.Now.Year.ToString()) : string.Empty) +
                         "</copyright>" + Environment.NewLine + "  <webMaster>" + portalSettings.Email + "</webMaster>" + Environment.NewLine + strRSS + "   </channel>" + Environment.NewLine +
                         "</rss>");
                File.WriteAllText(fileName, strRSS.ToString());
            }
            else
            {
                if (File.Exists(fileName))
                {
                    File.Delete(fileName);
                }
            }
        }

        /// <summary>injects the upload directory into raw HTML for src and background tags.</summary>
        /// <param name="strHTML">raw HTML text.</param>
        /// <param name="strUploadDirectory">path of portal image directory.</param>
        /// <returns>HTML with paths for images and background corrected.</returns>
        public static string ManageUploadDirectory(string strHTML, string strUploadDirectory)
        {
            strHTML = ManageTokenUploadDirectory(strHTML, strUploadDirectory, "src");
            return ManageTokenUploadDirectory(strHTML, strUploadDirectory, "background");
        }

        /// <summary>Injects the upload directory into raw HTML for a single token.</summary>
        /// <param name="strHTML">The raw HTML text.</param>
        /// <param name="strUploadDirectory">The path of portal image directory.</param>
        /// <param name="strToken">The token to be replaced.</param>
        /// <returns>HTML with paths for images and background corrected.</returns>
        /// <remarks>
        /// Called by ManageUploadDirectory for each token.
        /// </remarks>
        public static string ManageTokenUploadDirectory(string strHTML, string strUploadDirectory, string strToken)
        {
            int tokenStartPosition;
            int endOfUrl;
            int urlStartPosition = 0;
            int tokenLength;
            string strURL;
            var sbBuff = new StringBuilder(string.Empty);
            if (!string.IsNullOrEmpty(strHTML))
            {
                tokenLength = strToken.Length + 2;
                string uploadDirectory = strUploadDirectory.ToLowerInvariant();

                tokenStartPosition = strHTML.IndexOf(strToken + "=\"", StringComparison.InvariantCultureIgnoreCase);
                while (tokenStartPosition != -1)
                {
                    sbBuff.Append(strHTML.Substring(urlStartPosition, tokenStartPosition - urlStartPosition + tokenLength)); // keep characters left of URL
                    urlStartPosition = tokenStartPosition + tokenLength;
                    endOfUrl = strHTML.IndexOf("\"", urlStartPosition);
                    if (endOfUrl >= 0)
                    {
                        strURL = strHTML.Substring(urlStartPosition, endOfUrl - urlStartPosition).ToLowerInvariant();
                    }
                    else
                    {
                        strURL = strHTML.Substring(urlStartPosition).ToLowerInvariant();
                    }

                    // add upload directory if we are linking internally and the upload directory is not already included
                    if (!strURL.Contains("://") && !strURL.StartsWith("/") && !strURL.StartsWith(uploadDirectory))
                    {
                        sbBuff.Append(strUploadDirectory);
                    }

                    // find position of next occurrance:
                    tokenStartPosition = strHTML.IndexOf(strToken + "=\"", urlStartPosition + strURL.Length + 2, StringComparison.InvariantCultureIgnoreCase);
                }

                if (urlStartPosition > -1)
                {
                    sbBuff.Append(strHTML.Substring(urlStartPosition));
                }
            }

            return sbBuff.ToString();
        }

        /// <summary>Finds the control recursive from child to parent.</summary>
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

        /// <summary>Searches control hierarchy from top down to find a control matching the passed in name.</summary>
        /// <param name="objParent">Root control to begin searching.</param>
        /// <param name="strControlName">Name of control to look for.</param>
        /// <returns>The found control or null.</returns>
        /// <remarks>
        /// This differs from FindControlRecursive in that it looks down the control hierarchy, whereas, the
        /// FindControlRecursive starts at the passed in control and walks the tree up.  Therefore, this function is
        /// more a expensive task.
        /// </remarks>
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

        /// <summary>Sets the form focus.</summary>
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

        /// <summary>Gets the external request.</summary>
        /// <param name="address">The address.</param>
        /// <returns>Web request.</returns>
        [DnnDeprecated(10, 0, 2, "Use overload taking IHostSettings")]
        public static partial HttpWebRequest GetExternalRequest(string address)
            => GetExternalRequest(GetCurrentServiceProvider().GetRequiredService<IHostSettings>(), address);

        /// <summary>Gets the external request.</summary>
        /// <param name="hostSettings">The host settings.</param>
        /// <param name="address">The address.</param>
        /// <returns>Web request.</returns>
        public static HttpWebRequest GetExternalRequest(IHostSettings hostSettings, string address)
        {
            // Create the request object
            var objRequest = (HttpWebRequest)WebRequest.Create(address);

            // Set a time-out to the request ... 10 seconds
            objRequest.Timeout = (int)hostSettings.WebRequestTimeout.TotalMilliseconds;

            // Attach a User Agent to the request
            objRequest.UserAgent = "DotNetNuke";

            // If there is Proxy info, apply it to the request
            if (!string.IsNullOrEmpty(hostSettings.ProxyServer))
            {
                // Create a new Proxy
                WebProxy proxy;

                // Create a new Network Credentials item
                NetworkCredential proxyCredentials;

                // Fill Proxy info from host settings
                proxy = new WebProxy(hostSettings.ProxyServer, hostSettings.ProxyPort);
                if (!string.IsNullOrEmpty(hostSettings.ProxyUsername))
                {
                    // Fill the credential info from host settings
                    proxyCredentials = new NetworkCredential(hostSettings.ProxyUsername, hostSettings.ProxyPassword);

                    // Apply credentials to proxy
                    proxy.Credentials = proxyCredentials;
                }

                // Apply Proxy to request
                objRequest.Proxy = proxy;
            }

            return objRequest;
        }

        /// <summary>Gets the external request.</summary>
        /// <param name="address">The address.</param>
        /// <param name="credentials">The credentials.</param>
        /// <returns>Web request.</returns>
        [DnnDeprecated(10, 0, 2, "Use overload taking IHostSettings")]
        public static partial HttpWebRequest GetExternalRequest(string address, NetworkCredential credentials)
            => GetExternalRequest(GetCurrentServiceProvider().GetRequiredService<IHostSettings>(), address, credentials);

        /// <summary>Gets the external request.</summary>
        /// <param name="hostSettings">The host settings.</param>
        /// <param name="address">The address.</param>
        /// <param name="credentials">The credentials.</param>
        /// <returns>Web request.</returns>
        public static HttpWebRequest GetExternalRequest(IHostSettings hostSettings, string address, NetworkCredential credentials)
        {
            // Create the request object
            var objRequest = (HttpWebRequest)WebRequest.Create(address);

            // Set a time-out to the request ... 10 seconds
            objRequest.Timeout = (int)hostSettings.WebRequestTimeout.TotalMilliseconds;

            // Attach a User Agent to the request
            objRequest.UserAgent = "DotNetNuke";

            // Attach supplied credentials
            if (credentials.UserName != null)
            {
                objRequest.Credentials = credentials;
            }

            // If there is Proxy info, apply it to the request
            if (!string.IsNullOrEmpty(hostSettings.ProxyServer))
            {
                // Create a new Proxy
                WebProxy proxy;

                // Create a new Network Credentials item
                NetworkCredential proxyCredentials;

                // Fill Proxy info from host settings
                proxy = new WebProxy(hostSettings.ProxyServer, hostSettings.ProxyPort);
                if (!string.IsNullOrEmpty(hostSettings.ProxyUsername))
                {
                    // Fill the credential info from host settings
                    proxyCredentials = new NetworkCredential(hostSettings.ProxyUsername, hostSettings.ProxyPassword);

                    // Apply credentials to proxy
                    proxy.Credentials = proxyCredentials;
                }

                objRequest.Proxy = proxy;
            }

            return objRequest;
        }

        /// <summary>Deletes the folder recursive, include the folder itself will be deleted.</summary>
        /// <param name="strRoot">The root.</param>
        public static void DeleteFolderRecursive(string strRoot)
            => FileSystemUtils.DeleteFolderRecursive(strRoot);

        /// <summary>Deletes the folder recursive, include the folder itself will be deleted.</summary>
        /// <param name="strRoot">The root.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A <see cref="Task"/> indicating completion.</returns>
        public static async Task DeleteFolderRecursiveAsync(string strRoot, CancellationToken cancellationToken = default)
            => await FileSystemUtils.DeleteFolderRecursiveAsync(strRoot, cancellationToken);

        /// <summary>Deletes the files recursive which match the filter, will not delete folders and will ignore folder which is hidden or system.</summary>
        /// <param name="strRoot">The root.</param>
        /// <param name="filter">The filter.</param>
        public static void DeleteFilesRecursive(string strRoot, string filter)
            => FileSystemUtils.DeleteFilesRecursive(strRoot, filter);

        /// <summary>Deletes the files recursive which match the filter, will not delete folders and will ignore folder which is hidden or system.</summary>
        /// <param name="strRoot">The root.</param>
        /// <param name="filter">The filter.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A <see cref="Task"/> indicating completion.</returns>
        public static async Task DeleteFilesRecursiveAsync(string strRoot, string filter, CancellationToken cancellationToken = default)
            => await FileSystemUtils.DeleteFilesRecursiveAsync(strRoot, filter, cancellationToken);

        /// <summary>Cleans the name of the file.</summary>
        /// <param name="fileName">Name of the file.</param>
        /// <returns>clean name.</returns>
        public static string CleanFileName(string fileName)
        {
            return CleanFileName(fileName, string.Empty, string.Empty);
        }

        /// <summary>Cleans the name of the file.</summary>
        /// <param name="fileName">Name of the file.</param>
        /// <param name="badChars">The bad chars.</param>
        /// <returns>clean name.</returns>
        public static string CleanFileName(string fileName, string badChars)
        {
            return CleanFileName(fileName, badChars, string.Empty);
        }

        /// <summary>Cleans the name of the file.</summary>
        /// <param name="fileName">Name of the file.</param>
        /// <param name="badChars">The bad chars.</param>
        /// <param name="replaceChar">The replace char.</param>
        /// <returns>clean name.</returns>
        public static string CleanFileName(string fileName, string badChars, string replaceChar)
        {
            string strFileName = fileName;
            if (string.IsNullOrEmpty(badChars))
            {
                badChars = ":/\\?*|" + ((char)34) + ((char)39) + ((char)9);
            }

            if (string.IsNullOrEmpty(replaceChar))
            {
                replaceChar = "_";
            }

            int intCounter;
            for (intCounter = 0; intCounter <= badChars.Length - 1; intCounter++)
            {
                strFileName = strFileName.Replace(badChars.Substring(intCounter, 1), replaceChar);
            }

            return strFileName;
        }

        /// <summary>
        /// CleanName - removes characters from Module/Tab names that are being used for file names
        /// in Module/Tab Import/Export.
        /// </summary>
        /// <param name="name">The name to clean.</param>
        /// <returns>A cleaned string.</returns>
        public static string CleanName(string name)
        {
            string strName = name;
            string strBadChars = ". ~`!@#$%^&*()-_+={[}]|\\:;<,>?/" + ((char)34) + ((char)39);
            int intCounter;
            for (intCounter = 0; intCounter <= strBadChars.Length - 1; intCounter++)
            {
                strName = strName.Replace(strBadChars.Substring(intCounter, 1), string.Empty);
            }

            return strName;
        }

        /// <summary>
        ///   CreateValidClass - removes characters from Module/Tab names which are invalid
        ///   for use as an XHTML class attribute / CSS class selector value and optionally
        ///   prepends the letter 'A' if the first character is not alphabetic.  This differs
        ///   from <see>CreateValidID</see> which replaces invalid characters with an underscore
        ///   and replaces the first letter with an 'A' if it is not alphabetic.
        /// </summary>
        /// <param name="inputValue">String to use to create the class value.</param>
        /// <param name="validateFirstChar">If set true, validate whether the first character
        ///   is alphabetic and, if not, prepend the letter 'A' to the returned value.</param>
        /// <returns>A string suitable for use as a class value.</returns>
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

        /// <summary>  Creates the valid ID.</summary>
        /// <param name="inputValue">The input value.</param>
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

        /// <summary>Get the URL to show the "access denied" message.</summary>
        /// <returns>URL to access denied view.</returns>
        public static string AccessDeniedURL()
        {
            return AccessDeniedURL(string.Empty);
        }

        /// <summary>Get the URL to show the "access denied" message.</summary>
        /// <param name="message">The message to display.</param>
        /// <returns>URL to access denied view.</returns>
        public static string AccessDeniedURL(string message)
        {
            string strURL = string.Empty;

            var currentTabId = TabController.CurrentPage.TabID;
            var currentUserId = UserController.Instance.GetCurrentUserInfo().UserID;
            if (HttpContext.Current.Request.IsAuthenticated)
            {
                if (string.IsNullOrEmpty(message))
                {
                    // redirect to access denied page
                    strURL = navigationManager.NavigateURL(currentTabId, "Access Denied");
                }
                else
                {
                    // redirect to access denied page with custom message
                    var messageGuid = DataProvider.Instance().AddRedirectMessage(
                        currentUserId, currentTabId, message).ToString("N");
                    strURL = navigationManager.NavigateURL(currentTabId, "Access Denied", "message=" + messageGuid);
                }
            }
            else
            {
                strURL = LoginURL(HttpUtility.UrlEncode(HttpContext.Current.Request.RawUrl), false);
            }

            return strURL;
        }

        /// <summary>Adds the current request's protocol (<c>"http://"</c> or <c>"https://"</c>) to the given URL, if it does not already have a protocol specified.</summary>
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

        /// <summary>Generates the Application root url (including the tab/page).</summary>
        /// <remarks>
        /// This overload assumes the current page.
        /// </remarks>
        /// <returns>The formatted root url.</returns>
        public static string ApplicationURL()
        {
            var currentPage = TabController.CurrentPage;
            if (currentPage != null && currentPage.HasAVisibleVersion)
            {
                return ApplicationURL(currentPage.TabID);
            }

            return ApplicationURL(-1);
        }

        /// <summary>Generates the Application root url (including the tab/page).</summary>
        /// <remarks>
        /// This overload takes the tabid (page id) as a parameter.
        /// </remarks>
        /// <param name="tabID">The id of the tab/page.</param>
        /// <returns>The formatted root url.</returns>
        public static string ApplicationURL(int tabID)
        {
            string strURL = "~/" + glbDefaultPage;
            if (tabID != -1)
            {
                strURL += "?tabid=" + tabID;
            }

            return strURL;
        }

        /// <summary>Formats the help URL, adding query-string parameters and a protocol (if missing).</summary>
        /// <param name="helpUrl">The help URL.</param>
        /// <param name="objPortalSettings">The portal settings.</param>
        /// <param name="name">The name of the module.</param>
        /// <returns>Formatted URL.</returns>
        public static string FormatHelpUrl(string helpUrl, PortalSettings objPortalSettings, string name)
        {
            return FormatHelpUrl(helpUrl, objPortalSettings, name, string.Empty);
        }

        /// <summary>Formats the help URL, adding query-string parameters and a protocol (if missing).</summary>
        /// <param name="helpUrl">The help URL.</param>
        /// <param name="objPortalSettings">The portal settings.</param>
        /// <param name="name">The name of the module.</param>
        /// <param name="version">The version of the module.</param>
        /// <returns>Formatted URL.</returns>
        public static string FormatHelpUrl(string helpUrl, PortalSettings objPortalSettings, string name, string version)
        {
            string strURL = helpUrl;
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

            if (!string.IsNullOrEmpty(name))
            {
                strURL += "&helpmodule=" + HttpUtility.UrlEncode(name);
            }

            if (!string.IsNullOrEmpty(version))
            {
                strURL += "&helpversion=" + HttpUtility.UrlEncode(version);
            }

            return AddHTTP(strURL);
        }

        /// <summary>Generates the correctly formatted friendly URL.</summary>
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

        /// <summary>Generates the correctly formatted friendly URL.</summary>
        /// <remarks>
        /// This overload includes an optional page to include in the url.
        /// </remarks>
        /// <param name="tab">The current tab.</param>
        /// <param name="path">The path to format.</param>
        /// <param name="pageName">The page to include in the url.</param>
        /// <returns>The formatted (friendly) URL.</returns>
        public static string FriendlyUrl(TabInfo tab, string path, string pageName)
        {
            var portalSettings = PortalController.Instance.GetCurrentSettings();
            return FriendlyUrl(tab, path, pageName, portalSettings);
        }

        /// <summary>Generates the correctly formatted friendly URL.</summary>
        /// <remarks>
        /// This overload includes the portal settings for the site.
        /// </remarks>
        /// <param name="tab">The current tab.</param>
        /// <param name="path">The path to format.</param>
        /// <param name="settings">The portal settings.</param>
        /// <returns>The formatted (friendly) URL.</returns>
        [DnnDeprecated(9, 4, 3, "Use the IPortalSettings overload")]
        public static partial string FriendlyUrl(TabInfo tab, string path, PortalSettings settings)
        {
            return FriendlyUrl(tab, path, (IPortalSettings)settings);
        }

        /// <summary>Generates the correctly formatted friendly URL.</summary>
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

        /// <summary>Generates the correctly formatted friendly URL.</summary>
        /// <remarks>
        /// This overload includes an optional page to include in the URL, and the portal
        /// settings for the site.
        /// </remarks>
        /// <param name="tab">The current tab.</param>
        /// <param name="path">The path to format.</param>
        /// <param name="pageName">The page to include in the URL.</param>
        /// <param name="settings">The portal settings.</param>
        /// <returns>The formatted (friendly) url.</returns>
        [DnnDeprecated(9, 4, 3, "Use the IPortalSettings overload")]
        public static partial string FriendlyUrl(TabInfo tab, string path, string pageName, PortalSettings settings)
        {
            return FriendlyUrl(tab, path, pageName, (IPortalSettings)settings);
        }

        /// <summary>Generates the correctly formatted friendly URL.</summary>
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

        /// <summary>Generates the correctly formatted friendly url.</summary>
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

        /// <summary>Returns the type of URl (T=other tab, F=file, U=URL, N=normal).</summary>
        /// <param name="url">The url.</param>
        /// <returns>The url type.</returns>
        public static TabType GetURLType(string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                return TabType.Normal;
            }

            if (url.ToLowerInvariant().StartsWith("mailto:") == false && url.IndexOf("://") == -1 && url.StartsWith("~") == false && url.StartsWith("\\\\") == false && url.StartsWith("/") == false)
            {
                if (NumberMatchRegex.IsMatch(url))
                {
                    return TabType.Tab;
                }

                if (url.ToLowerInvariant().StartsWith("userid="))
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
        /// <param name="moduleId">The id of the module (unused).</param>
        /// <param name="url">the url to import.</param>
        /// <returns>If an internal link does not exist, an empty string is returned, otherwise the passed in url is returned as is.</returns>
        [DnnDeprecated(9, 8, 1, "moduleId is not used in the method, use the overload that takes only the url")]
        public static partial string ImportUrl(int moduleId, string url)
        {
            return ImportUrl(url);
        }

        /// <summary>
        /// Url's as internal links to Files, Tabs and Users should only be imported if
        /// those files, tabs and users exist. This function parses the url, and checks
        /// whether the internal links exist.
        /// If the link does not exist, the function will return an empty string.
        /// </summary>
        /// <param name="url">The url to import.</param>
        /// <returns>If an internal link does not exist, an empty string is returned, otherwise the passed in url is returned as is.</returns>
        public static string ImportUrl(string url)
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

        /// <summary>Gets the login URL.</summary>
        /// <param name="returnUrl">The URL to redirect to after logging in.</param>
        /// <param name="overrideSetting">if set to <see langword="true"/>, show the login control on the current page, even if there is a login page defined for the site.</param>
        /// <returns>Formatted URL.</returns>
        public static string LoginURL(string returnUrl, bool overrideSetting)
        {
            return LoginURL(returnUrl, overrideSetting, PortalController.Instance.GetCurrentSettings());
        }

        /// <summary>Gets the login URL.</summary>
        /// <param name="returnUrl">The URL to redirect to after logging in.</param>
        /// <param name="overrideSetting">if set to <see langword="true"/>, show the login control on the current page, even if there is a login page defined for the site.</param>
        /// <param name="portalSettings">The Portal Settings.</param>
        /// <returns>Formatted URL.</returns>
        [DnnDeprecated(9, 8, 1, "Use the overload that takes IPortalSettings instead")]
        public static partial string LoginURL(string returnUrl, bool overrideSetting, PortalSettings portalSettings)
        {
            return LoginURL(returnUrl, overrideSetting, (IPortalSettings)portalSettings);
        }

        /// <summary>Gets the login URL.</summary>
        /// <param name="returnUrl">The URL to redirect to after logging in.</param>
        /// <param name="overrideSetting">if set to <see langword="true"/>, show the login control on the current page, even if there is a login page defined for the site.</param>
        /// <param name="portalSettings">The Portal Settings.</param>
        /// <returns>Formatted URL.</returns>
        public static string LoginURL(string returnUrl, bool overrideSetting, IPortalSettings portalSettings)
        {
            string loginUrl;
            var currentTabId = TabController.CurrentPage.TabID;

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
                                 ? navigationManager.NavigateURL(currentTabId, "Login", strMessage, popUpParameter)
                                 : navigationManager.NavigateURL(currentTabId, "Login", returnUrl, strMessage, popUpParameter);
                }
            }
            else
            {
                // portal tab
                loginUrl = string.IsNullOrEmpty(returnUrl)
                                ? navigationManager.NavigateURL(currentTabId, "Login", popUpParameter)
                                : navigationManager.NavigateURL(currentTabId, "Login", returnUrl, popUpParameter);
            }

            return loginUrl;
        }

        /// <summary>Gets User profile URL.</summary>
        /// <param name="userId">The user id.</param>
        /// <returns>Formatted url.</returns>
        public static string UserProfileURL(int userId)
        {
            string strURL = string.Empty;
            var portalSettings = PortalController.Instance.GetCurrentSettings();

            strURL = navigationManager.NavigateURL(portalSettings.UserTabId, string.Empty, string.Format("userId={0}", userId));

            return strURL;
        }

        /// <summary>Gets the URL to the current page.</summary>
        /// <returns>Formatted URL.</returns>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [DnnDeprecated(9, 4, 2, "Use INavigationManager via dependency injection")]
        public static partial string NavigateURL()
        {
            return navigationManager.NavigateURL();
        }

        /// <summary>Gets the URL to the given page.</summary>
        /// <param name="tabID">The tab ID.</param>
        /// <returns>Formatted URL.</returns>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [DnnDeprecated(9, 4, 2, "Use INavigationManager via dependency injection")]
        public static partial string NavigateURL(int tabID)
        {
            return navigationManager.NavigateURL(tabID);
        }

        /// <summary>Gets the URL to the given page.</summary>
        /// <param name="tabID">The tab ID.</param>
        /// <param name="isSuperTab">if set to <see langword="true"/> the page is a "super-tab," i.e. a host-level page.</param>
        /// <returns>Formatted URL.</returns>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [DnnDeprecated(9, 4, 2, "Use INavigationManager via dependency injection")]
        public static partial string NavigateURL(int tabID, bool isSuperTab)
        {
            return navigationManager.NavigateURL(tabID, isSuperTab);
        }

        /// <summary>Gets the URL to show the control associated with the given control key.</summary>
        /// <param name="controlKey">The control key, or <see cref="string.Empty"/> or <c>null</c>.</param>
        /// <returns>Formatted URL.</returns>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [DnnDeprecated(9, 4, 2, "Use INavigationManager via dependency injection")]
        public static partial string NavigateURL(string controlKey)
        {
            return navigationManager.NavigateURL(controlKey);
        }

        /// <summary>Gets the URL to show the control associated with the given control key.</summary>
        /// <param name="controlKey">The control key, or <see cref="string.Empty"/> or <c>null</c>.</param>
        /// <param name="additionalParameters">Any additional parameters, in <c>"key=value"</c> format.</param>
        /// <returns>Formatted URL.</returns>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [DnnDeprecated(9, 4, 2, "Use INavigationManager via dependency injection")]
        public static partial string NavigateURL(string controlKey, params string[] additionalParameters)
        {
            return navigationManager.NavigateURL(controlKey, additionalParameters);
        }

        /// <summary>Gets the URL to show the control associated with the given control key on the given page.</summary>
        /// <param name="tabID">The tab ID.</param>
        /// <param name="controlKey">The control key, or <see cref="string.Empty"/> or <c>null</c>.</param>
        /// <returns>Formatted URL.</returns>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [DnnDeprecated(9, 4, 2, "Use INavigationManager via dependency injection")]
        public static partial string NavigateURL(int tabID, string controlKey)
        {
            return navigationManager.NavigateURL(tabID, controlKey);
        }

        /// <summary>Gets the URL to show the given page.</summary>
        /// <param name="tabID">The tab ID.</param>
        /// <param name="controlKey">The control key, or <see cref="string.Empty"/> or <c>null</c>.</param>
        /// <param name="additionalParameters">Any additional parameters.</param>
        /// <returns>Formatted URL.</returns>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [DnnDeprecated(9, 4, 2, "Use INavigationManager via dependency injection")]
        public static partial string NavigateURL(int tabID, string controlKey, params string[] additionalParameters)
        {
            return navigationManager.NavigateURL(tabID, controlKey, additionalParameters);
        }

        /// <summary>Gets the URL to show the given page.</summary>
        /// <param name="tabID">The tab ID.</param>
        /// <param name="settings">The portal settings.</param>
        /// <param name="controlKey">The control key, or <see cref="string.Empty"/> or <c>null</c>.</param>
        /// <param name="additionalParameters">Any additional parameters.</param>
        /// <returns>Formatted URL.</returns>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [DnnDeprecated(9, 4, 2, "Use INavigationManager via dependency injection")]
        public static partial string NavigateURL(int tabID, PortalSettings settings, string controlKey, params string[] additionalParameters)
        {
            return navigationManager.NavigateURL(tabID, settings, controlKey, additionalParameters);
        }

        /// <summary>Gets the URL to show the given page.</summary>
        /// <param name="tabID">The tab ID.</param>
        /// <param name="isSuperTab">if set to <see langword="true"/> the page is a "super-tab," i.e. a host-level page.</param>
        /// <param name="settings">The portal settings.</param>
        /// <param name="controlKey">The control key, or <see cref="string.Empty"/> or <c>null</c>.</param>
        /// <param name="additionalParameters">Any additional parameters.</param>
        /// <returns>Formatted URL.</returns>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [DnnDeprecated(9, 4, 2, "Use INavigationManager via dependency injection")]
        public static partial string NavigateURL(int tabID, bool isSuperTab, PortalSettings settings, string controlKey, params string[] additionalParameters)
        {
            return navigationManager.NavigateURL(tabID, isSuperTab, settings, controlKey, additionalParameters);
        }

        /// <summary>Gets the URL to show the given page.</summary>
        /// <param name="tabID">The tab ID.</param>
        /// <param name="isSuperTab">if set to <see langword="true"/> the page is a "super-tab," i.e. a host-level page.</param>
        /// <param name="settings">The portal settings.</param>
        /// <param name="controlKey">The control key, or <see cref="string.Empty"/> or <c>null</c>.</param>
        /// <param name="language">The language code.</param>
        /// <param name="additionalParameters">Any additional parameters.</param>
        /// <returns>Formatted URL.</returns>
        [DnnDeprecated(9, 4, 2, "Use INavigationManager via dependency injection")]
        public static partial string NavigateURL(int tabID, bool isSuperTab, PortalSettings settings, string controlKey, string language, params string[] additionalParameters)
        {
            return navigationManager.NavigateURL(tabID, isSuperTab, settings, controlKey, language, additionalParameters);
        }

        /// <summary>Gets the URL to show the given page.</summary>
        /// <param name="tabID">The tab ID.</param>
        /// <param name="isSuperTab">if set to <see langword="true"/> the page is a "super-tab," i.e. a host-level page.</param>
        /// <param name="settings">The portal settings.</param>
        /// <param name="controlKey">The control key, or <see cref="string.Empty"/> or <c>null</c>.</param>
        /// <param name="language">The language code.</param>
        /// <param name="pageName">The page name to pass to <see cref="FriendlyUrl(DotNetNuke.Entities.Tabs.TabInfo,string,string)"/>.</param>
        /// <param name="additionalParameters">Any additional parameters.</param>
        /// <returns>Formatted url.</returns>
        [DnnDeprecated(9, 4, 2, "Use INavigationManager via dependency injection")]
        public static partial string NavigateURL(int tabID, bool isSuperTab, PortalSettings settings, string controlKey, string language, string pageName, params string[] additionalParameters)
        {
            return navigationManager.NavigateURL(tabID, isSuperTab, settings, controlKey, language, pageName, additionalParameters);
        }

        /// <summary>UrlEncode query string.</summary>
        /// <param name="queryString">The query string.</param>
        /// <returns>Encoded content.</returns>
        public static string QueryStringEncode(string queryString)
        {
            queryString = HttpUtility.UrlEncode(queryString);
            return queryString;
        }

        /// <summary>UrlDecode query string.</summary>
        /// <param name="queryString">The query string.</param>
        /// <returns>Decoded content.</returns>
        public static string QueryStringDecode(string queryString)
        {
            queryString = HttpUtility.UrlDecode(queryString);
            string fullPath;
            try
            {
                fullPath = HttpContext.Current.Request.MapPath(queryString, HttpContext.Current.Request.ApplicationPath, false);
            }
            catch (HttpException exc)
            {
                Exceptions.ProcessHttpException(exc);
            }

            string strDoubleDecodeURL = HttpContext.Current.Server.UrlDecode(HttpContext.Current.Server.UrlDecode(queryString));
            if (queryString.IndexOf("..") != -1 || strDoubleDecodeURL.IndexOf("..") != -1)
            {
                Exceptions.ProcessHttpException();
            }

            return queryString;
        }

        /// <summary>Gets Register URL.</summary>
        /// <param name="returnURL">The return URL.</param>
        /// <param name="originalURL">The original URL.</param>
        /// <returns>Formatted url.</returns>
        public static string RegisterURL(string returnURL, string originalURL)
        {
            string strURL;

            var portalSettings = PortalController.Instance.GetCurrentSettings();
            string extraParams = string.Empty;
            if (!string.IsNullOrEmpty(returnURL))
            {
                extraParams = string.Concat("returnurl=", returnURL);
            }

            if (!string.IsNullOrEmpty(originalURL))
            {
                extraParams += string.Concat("&orignalurl=", originalURL);
            }

            if (portalSettings.RegisterTabId != -1)
            {
                // user defined tab
                strURL = navigationManager.NavigateURL(portalSettings.RegisterTabId, string.Empty, extraParams);
            }
            else
            {
                strURL = navigationManager.NavigateURL(TabController.CurrentPage.TabID, "Register", extraParams);
            }

            return strURL;
        }

        /// <summary>Generates the correctly formatted url.</summary>
        /// <param name="url">The url to format.</param>
        /// <returns>The formatted (resolved) url.</returns>
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

        /// <summary>Encodes the reserved characters.</summary>
        /// <param name="queryString">The query string.</param>
        /// <returns>Encoded content.</returns>
        [DnnDeprecated(9, 8, 1, "Use System.Net.WebUtility.UrlEncode instead")]
        public static partial string EncodeReservedCharacters(string queryString)
        {
            queryString = queryString.Replace("$", "%24");
            queryString = queryString.Replace("&", "%26");
            queryString = queryString.Replace("+", "%2B");
            queryString = queryString.Replace(",", "%2C");
            queryString = queryString.Replace("/", "%2F");
            queryString = queryString.Replace(":", "%3A");
            queryString = queryString.Replace(";", "%3B");
            queryString = queryString.Replace("=", "%3D");
            queryString = queryString.Replace("?", "%3F");
            queryString = queryString.Replace("@", "%40");
            return queryString;
        }

        /// <summary>Dates to string.</summary>
        /// <param name="dateValue">The date value.</param>
        /// <returns>return value of input with SortableDateTimePattern.</returns>
        [DnnDeprecated(9, 8, 1, @"Use DateTime.ToString(""s"") instead")]
        public static partial string DateToString(DateTime dateValue)
        {
            try
            {
                if (!Null.IsNull(dateValue))
                {
                    return dateValue.ToString("s");
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

        /// <summary>Gets the hash value.</summary>
        /// <param name="hashObject">The hash object.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns>HashOject's value or DefaultValue if HashObject is null.</returns>
        [DnnDeprecated(9, 8, 1, "No replacement")]
        public static partial string GetHashValue(object hashObject, string defaultValue)
        {
            if (hashObject != null)
            {
                if (!string.IsNullOrEmpty(hashObject.ToString()))
                {
                    return Convert.ToString(hashObject);
                }
                else
                {
                    return defaultValue;
                }
            }
            else
            {
                return defaultValue;
            }
        }

        /// <summary>Gets Link click url.</summary>
        /// <param name="link">The link.</param>
        /// <param name="tabId">The tab ID.</param>
        /// <param name="moduleId">The module ID.</param>
        /// <returns>Formatted url.</returns>
        public static string LinkClick(string link, int tabId, int moduleId)
        {
            return LinkClick(link, tabId, moduleId, true, string.Empty);
        }

        /// <summary>Gets Link click url.</summary>
        /// <param name="link">The link.</param>
        /// <param name="tabId">The tab ID.</param>
        /// <param name="moduleId">The module ID.</param>
        /// <param name="trackClicks">if set to <see langword="true"/> [track clicks].</param>
        /// <returns>Formatted url.</returns>
        public static string LinkClick(string link, int tabId, int moduleId, bool trackClicks)
        {
            return LinkClick(link, tabId, moduleId, trackClicks, string.Empty);
        }

        /// <summary>Gets Link click url.</summary>
        /// <param name="link">The link.</param>
        /// <param name="tabId">The tab ID.</param>
        /// <param name="moduleId">The module ID.</param>
        /// <param name="trackClicks">if set to <see langword="true"/> [track clicks].</param>
        /// <param name="contentType">Type of the content.</param>
        /// <returns>Formatted url.</returns>
        public static string LinkClick(string link, int tabId, int moduleId, bool trackClicks, string contentType)
        {
            return LinkClick(link, tabId, moduleId, trackClicks, !string.IsNullOrEmpty(contentType));
        }

        /// <summary>Gets Link click url.</summary>
        /// <param name="link">The link.</param>
        /// <param name="tabId">The tab ID.</param>
        /// <param name="moduleId">The module ID.</param>
        /// <param name="trackClicks">if set to <see langword="true"/> [track clicks].</param>
        /// <param name="forceDownload">if set to <see langword="true"/> [force download].</param>
        /// <returns>Formatted url.</returns>
        public static string LinkClick(string link, int tabId, int moduleId, bool trackClicks, bool forceDownload)
        {
            var portalSettings = PortalController.Instance.GetCurrentSettings();
            return LinkClick(link, tabId, moduleId, trackClicks, forceDownload, portalSettings.PortalId, portalSettings.EnableUrlLanguage, portalSettings.GUID.ToString());
        }

        /// <summary>Gets Link click url.</summary>
        /// <param name="link">The link.</param>
        /// <param name="tabId">The tab ID.</param>
        /// <param name="moduleId">The module ID.</param>
        /// <param name="trackClicks">if set to <see langword="true"/> [track clicks].</param>
        /// <param name="forceDownload">if set to <see langword="true"/> [force download].</param>
        /// <param name="portalId">The portal id.</param>
        /// <param name="enableUrlLanguage">if set to <see langword="true"/> [enable URL language].</param>
        /// <param name="portalGuid">The portal GUID.</param>
        /// <returns>Formatted url.</returns>
        public static string LinkClick(
            string link,
            int tabId,
            int moduleId,
            bool trackClicks,
            bool forceDownload,
            int portalId,
            bool enableUrlLanguage,
            string portalGuid)
        {
            string strLink = string.Empty;
            TabType urlType = GetURLType(link);
            if (urlType == TabType.Member)
            {
                strLink = UserProfileURL(Convert.ToInt32(UrlUtils.GetParameterValue(link)));
            }
            else if (trackClicks || forceDownload || urlType == TabType.File)
            {
                // format LinkClick wrapper
                if (link.StartsWith("fileid=", StringComparison.InvariantCultureIgnoreCase))
                {
                    strLink = ApplicationPath + "/LinkClick.aspx?fileticket=" + UrlUtils.EncryptParameter(UrlUtils.GetParameterValue(link), portalGuid);
                    if (portalId == Null.NullInteger)
                    {
                        // To track Host files
                        strLink += "&hf=1";
                    }
                }

                if (string.IsNullOrEmpty(strLink))
                {
                    strLink = ApplicationPath + "/LinkClick.aspx?link=" + HttpUtility.UrlEncode(link);
                }

                // tabid is required to identify the portal where the click originated
                if (tabId != Null.NullInteger)
                {
                    strLink += "&tabid=" + tabId;
                }

                // append portal id to query string to identity portal the click originated.
                if (portalId != Null.NullInteger)
                {
                    strLink += "&portalid=" + portalId;
                }

                // moduleid is used to identify the module where the url is stored
                if (moduleId != -1)
                {
                    strLink += "&mid=" + moduleId;
                }

                // only add language to url if more than one locale is enabled, and if admin did not turn it off
                if (LocaleController.Instance.GetLocales(portalId).Count > 1 && enableUrlLanguage)
                {
                    strLink += "&language=" + Thread.CurrentThread.CurrentCulture.Name;
                }

                // force a download dialog
                if (forceDownload)
                {
                    strLink += "&forcedownload=true";
                }
            }
            else
            {
                switch (urlType)
                {
                    case TabType.Tab:
                        strLink = navigationManager.NavigateURL(int.Parse(link));
                        break;
                    default:
                        strLink = link;
                        break;
                }
            }

            return strLink;
        }

        /// <summary>Gets the name of the role.</summary>
        /// <param name="roleId">The role ID.</param>
        /// <returns>Role Name.</returns>
        [DnnDeprecated(10, 0, 2, "Use overload taking IHostSettings")]
        public static partial string GetRoleName(int roleId)
            => GetRoleName(GetCurrentServiceProvider().GetRequiredService<IHostSettings>(), roleId);

        /// <summary>Gets the name of the role.</summary>
        /// <param name="hostSettings">The host settings.</param>
        /// <param name="roleId">The role ID.</param>
        /// <returns>Role Name.</returns>
        public static string GetRoleName(IHostSettings hostSettings, int roleId)
        {
            switch (Convert.ToString(roleId))
            {
                case glbRoleAllUsers:
                    return glbRoleAllUsersName;
                case glbRoleUnauthUser:
                    return glbRoleUnauthUserName;
            }

            Hashtable htRoles = null;
            if (hostSettings.PerformanceSetting != DotNetNuke.Abstractions.Application.PerformanceSettings.NoCaching)
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

                if (hostSettings.PerformanceSetting != DotNetNuke.Abstractions.Application.PerformanceSettings.NoCaching)
                {
                    DataCache.SetCache("GetRoles", htRoles);
                }
            }

            return Convert.ToString(htRoles[roleId]);
        }

        /// <summary>Gets the content.</summary>
        /// <param name="content">The content.</param>
        /// <param name="contentType">Type of the content.</param>
        /// <returns>specific node by content type of the whole document.</returns>
        public static XmlNode GetContent(string content, string contentType)
        {
            var xmlDoc = new XmlDocument { XmlResolver = null };
            xmlDoc.LoadXml(content);
            if (string.IsNullOrEmpty(contentType))
            {
                return xmlDoc.DocumentElement;
            }
            else
            {
                return xmlDoc.SelectSingleNode(contentType);
            }
        }

        /// <summary>GenerateTabPath generates the TabPath used in Friendly URLS.</summary>
        /// <param name="parentId">The Id of the Parent Tab.</param>
        /// <param name="tabName">The Name of the current Tab.</param>
        /// <returns>The TabPath.</returns>
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

        /// <summary>Gets the help text.</summary>
        /// <param name="moduleControlId">The module control id.</param>
        /// <returns>help text.</returns>
        public static string GetHelpText(int moduleControlId)
        {
            string helpText = Null.NullString;
            ModuleControlInfo objModuleControl = ModuleControlController.GetModuleControl(moduleControlId);
            if (objModuleControl != null)
            {
                string fileName = Path.GetFileName(objModuleControl.ControlSrc);
                string localResourceFile = objModuleControl.ControlSrc.Replace(fileName, Localization.LocalResourceDirectory + "/" + fileName);
                if (!string.IsNullOrEmpty(Localization.GetString(ModuleActionType.HelpText, localResourceFile)))
                {
                    helpText = Localization.GetString(ModuleActionType.HelpText, localResourceFile);
                }
            }

            return helpText;
        }

        /// <summary>Gets the online help url or the host configured help url if no url provided.</summary>
        /// <param name="helpUrl">The help URL.</param>
        /// <param name="moduleConfig">The module config.</param>
        /// <returns>The help url.</returns>
        [DnnDeprecated(9, 8, 1, "ModuleInfo is unused, use the overload that does not take a ModuleInfo")]
        public static partial string GetOnLineHelp(string helpUrl, ModuleInfo moduleConfig)
        {
            return GetOnLineHelp(helpUrl);
        }

        /// <summary>Gets the online help url or the host configured help url if no url provided.</summary>
        /// <param name="helpUrl">The help URL.</param>
        /// <returns>The help url.</returns>
        [DnnDeprecated(10, 0, 2, "Use overload taking IHostSettings")]
        public static partial string GetOnLineHelp(string helpUrl)
            => GetOnLineHelp(GetCurrentServiceProvider().GetRequiredService<IHostSettings>(), helpUrl);

        /// <summary>Gets the online help url or the host configured help url if no url provided.</summary>
        /// <param name="hostSettings">The host settings.</param>
        /// <param name="helpUrl">The help URL.</param>
        /// <returns>The help url.</returns>
        public static string GetOnLineHelp(IHostSettings hostSettings, string helpUrl)
        {
            if (string.IsNullOrEmpty(helpUrl))
            {
                helpUrl = hostSettings.HelpUrl;
            }

            return helpUrl;
        }

        /// <summary>Check whether the tab contains "Account Login" module.</summary>
        /// <param name="tabId">The tab id.</param>
        /// <returns><see langword="true"/> if the tab contains "Account Login" module, otherwise, <see langword="false"/>.</returns>
        public static bool ValidateLoginTabID(int tabId)
        {
            return ValidateModuleInTab(tabId, "Account Login");
        }

        /// <summary>Check whether the tab contains specific module.</summary>
        /// <param name="tabId">The tab id.</param>
        /// <param name="moduleName">The module need to check.</param>
        /// <returns><see langword="true"/> if the tab contains the module, otherwise, <see langword="false"/>.</returns>
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

        /// <summary>DeserializeHashTableBase64 deserializes a Hashtable using Binary Formatting.</summary>
        /// <remarks>
        /// While this method of serializing is no longer supported (due to Medium Trust
        /// issue, it is still required for upgrade purposes.
        /// </remarks>
        /// <param name="source">The String Source to deserialize.</param>
        /// <returns>The deserialized Hashtable.</returns>
        [DnnDeprecated(9, 8, 1, "No replacement")]
        public static partial Hashtable DeserializeHashTableBase64(string source)
        {
            Hashtable objHashTable;
            if (!string.IsNullOrEmpty(source))
            {
                byte[] bits = Convert.FromBase64String(source);
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

        /// <summary>DeserializeHashTableXml deserializes a Hashtable using Xml Serialization.</summary>
        /// <remarks>
        /// This is the preferred method of serialization under Medium Trust.
        /// </remarks>
        /// <param name="source">The String Source to deserialize.</param>
        /// <returns>The deserialized Hashtable.</returns>
        [DnnDeprecated(9, 8, 1, "This API was not meant to be public and only deserializes xml with a root of 'profile'")]
        public static partial Hashtable DeserializeHashTableXml(string source)
        {
            return XmlUtils.DeSerializeHashtable(source, "profile");
        }

        /// <summary>SerializeHashTableBase64 serializes a Hashtable using Binary Formatting.</summary>
        /// <remarks>
        /// While this method of serializing is no longer supported (due to Medium Trust
        /// issue, it is still required for upgrade purposes.
        /// </remarks>
        /// <param name="source">The Hashtable to serialize.</param>
        /// <returns>The serialized String.</returns>
        [DnnDeprecated(9, 8, 1, "No replacement")]
        public static partial string SerializeHashTableBase64(Hashtable source)
        {
            string strString;
            if (source.Count != 0)
            {
                var bin = new BinaryFormatter();
                var mem = new MemoryStream();
                try
                {
                    bin.Serialize(mem, source);
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

        /// <summary>SerializeHashTableXml serializes a Hashtable using Xml Serialization.</summary>
        /// <remarks>
        /// This is the preferred method of serialization under Medium Trust.
        /// </remarks>
        /// <param name="source">The Hashtable to serialize.</param>
        /// <returns>The serialized String.</returns>
        [DnnDeprecated(9, 8, 1, "This method was never meant to be public and only works for 'profile' root namespace")]
        public static partial string SerializeHashTableXml(Hashtable source)
        {
            return XmlUtils.SerializeDictionary(source, "profile");
        }

        /// <summary>Check whether the specific page is a host page.</summary>
        /// <param name="tabId">The tab ID.</param>
        /// <returns>if <see langword="true"/> the tab is a host page; otherwise, it is not a host page.</returns>
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

        /// <summary>Return User Profile Picture Formatted Url. UserId, width and height can be passed to build a formatted Avatar Url.</summary>
        /// <returns>Formatted url,  e.g. http://www.mysite.com/DnnImageHandler.ashx?mode=profilepic&amp;userid={0}&amp;h={1}&amp;w={2}.
        /// </returns>
        /// <remarks>Usage: ascx - &lt;asp:Image ID="avatar" runat="server" CssClass="SkinObject" /&gt;
        /// code behind - avatar.ImageUrl = string.Format(Globals.UserProfilePicFormattedUrl(), userInfo.UserID, 32, 32).
        /// </remarks>
        [DnnDeprecated(7, 3, 0, "It causes issues in SSL-offloading scenarios - please use UserProfilePicRelativeUrl instead", RemovalVersion = 11)]
        public static partial string UserProfilePicFormattedUrl()
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

        /// <summary>Return User Profile Picture relative Url. UserId, width and height can be passed to build a formatted relative Avatar Url.</summary>
        /// <returns>Formatted url,  e.g. /DnnImageHandler.ashx?userid={0}&amp;h={1}&amp;w={2} considering child portal.
        /// </returns>
        /// <remarks>Usage: ascx - &lt;asp:Image ID="avatar" runat="server" CssClass="SkinObject" /&gt;
        /// code behind - avatar.ImageUrl = string.Format(Globals.UserProfilePicRelativeUrl(), userInfo.UserID, 32, 32).
        /// </remarks>
        [DnnDeprecated(8, 0, 0, "Please use UserController.Instance.GetUserProfilePictureUrl", RemovalVersion = 11)]
        public static partial string UserProfilePicRelativeUrl()
        {
            return UserProfilePicRelativeUrl(true);
        }

        /// <summary>Return User Profile Picture relative Url. UserId, width and height can be passed to build a formatted relative Avatar Url.</summary>
        /// <param name="includeCdv">Indicates if cdv (Cache Delayed Verification) has to be included in the returned URL.</param>
        /// <returns>Formatted url,  e.g. /DnnImageHandler.ashx?userid={0}&amp;h={1}&amp;w={2} considering child portal.
        /// </returns>
        /// <remarks>Usage: ascx - &lt;asp:Image ID="avatar" runat="server" CssClass="SkinObject" /&gt;
        /// code behind - avatar.ImageUrl = string.Format(Globals.UserProfilePicRelativeUrl(), userInfo.UserID, 32, 32).
        /// </remarks>
        [DnnDeprecated(8, 0, 0, "Please use UserController.Instance.GetUserProfilePictureUrl", RemovalVersion = 11)]
        public static partial string UserProfilePicRelativeUrl(bool includeCdv)
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

        /// <summary>Formats an email address as a cloacked html link.</summary>
        /// <param name="email">The formatted email address.</param>
        /// <returns>A cloacked html link.</returns>
        [DnnDeprecated(7, 0, 0, "This function has been replaced by DotNetNuke.Common.Utilities.HtmlUtils.FormatEmail", RemovalVersion = 11)]
        public static partial string FormatEmail(string email)
        {
            return HtmlUtils.FormatEmail(email);
        }

        /// <summary>Formats a domain name including link.</summary>
        /// <param name="website">The domain name to format.</param>
        /// <returns>The formatted domain name.</returns>
        [DnnDeprecated(7, 0, 0, "This function has been replaced by DotNetNuke.Common.Utilities.HtmlUtils.FormatWebsite", RemovalVersion = 11)]
        public static partial string FormatWebsite(object website)
        {
            return HtmlUtils.FormatWebsite(website);
        }

        /// <summary>Xml encodes an html string.</summary>
        /// <param name="html">The html to encode.</param>
        /// <returns>The encoded html for usage in xml.</returns>
        [DnnDeprecated(7, 0, 0, "This function has been replaced by DotNetNuke.Common.Utilities.XmlUtils.XMLEncode", RemovalVersion = 11)]
        public static partial string XMLEncode(string html)
        {
            return XmlUtils.XMLEncode(html);
        }

        /// <summary>Gets the database connection string.</summary>
        /// <returns>The database connection string.</returns>
        [DnnDeprecated(7, 0, 0, "This function has been replaced by DotNetNuke.Common.Utilities.Config.GetConnectionString", RemovalVersion = 11)]
        public static partial string GetDBConnectionString()
        {
            return Config.GetConnectionString();
        }

        /// <summary>Gets a file list.</summary>
        /// <param name="currentDirectory">The current directory.</param>
        /// <param name="strExtensions">The extensions to filter for.</param>
        /// <param name="noneSpecified">If true, adds 'None Specified' to the list.</param>
        /// <returns>A list of file names.</returns>
        [DnnDeprecated(7, 0, 0, "No replacement", RemovalVersion = 11)]
        public static partial ArrayList GetFileList(
            DirectoryInfo currentDirectory,
            [Optional, DefaultParameterValue("")] // ERROR: Optional parameters aren't supported in C#
            string strExtensions,
            [Optional, DefaultParameterValue(true)] // ERROR: Optional parameters aren't supported in C#
            bool noneSpecified)
        {
            var arrFileList = new ArrayList();
            string strExtension = string.Empty;

            if (noneSpecified)
            {
                arrFileList.Add(new FileItem(string.Empty, "<" + Localization.GetString("None_Specified") + ">"));
            }

            string file = null;
            string[] files = Directory.GetFiles(currentDirectory.FullName);
            foreach (string fileLoopVariable in files)
            {
                file = fileLoopVariable;
                if (file.IndexOf(".") > -1)
                {
                    strExtension = file.Substring(file.LastIndexOf(".") + 1);
                }

                string fileName = file.Substring(currentDirectory.FullName.Length);
                if (strExtensions.IndexOf(strExtension, StringComparison.InvariantCultureIgnoreCase) != -1 || string.IsNullOrEmpty(strExtensions))
                {
                    arrFileList.Add(new FileItem(fileName, fileName));
                }
            }

            return arrFileList;
        }

        /// <summary>Gets the subfolder path for a give filename path.</summary>
        /// <param name="strFileNamePath">The filename full path.</param>
        /// <returns>The subfolder name.</returns>
        [DnnDeprecated(7, 0, 0, "Replaced by GetSubFolderPath(string strFileNamePath, int portalId)", RemovalVersion = 11)]
        public static partial string GetSubFolderPath(string strFileNamePath)
        {
            // Obtain PortalSettings from Current Context
            var portalSettings = PortalController.Instance.GetCurrentPortalSettings();
            string parentFolderName = null;
            if (IsHostTab(portalSettings.ActiveTab.TabID))
            {
                parentFolderName = HostMapPath.Replace("/", "\\");
            }
            else
            {
                parentFolderName = portalSettings.HomeDirectoryMapPath.Replace("/", "\\");
            }

            string strFolderpath = strFileNamePath.Substring(0, strFileNamePath.LastIndexOf("\\") + 1);

            return strFolderpath.Substring(parentFolderName.Length).Replace("\\", "/");
        }

        /// <summary>Gets a LinkClick url for tracking purposes.</summary>
        /// <param name="link">The actual link the LinkClick handler should point to.</param>
        /// <returns>The formatted LinkClick url.</returns>
        [DnnDeprecated(7, 0, 0, "Use Common.Globals.LinkClick() for proper handling of URLs", RemovalVersion = 11)]
        public static partial string LinkClickURL(string link)
        {
            PortalSettings portalSettings = PortalController.Instance.GetCurrentPortalSettings();
            return LinkClick(link, portalSettings.ActiveTab.TabID, -1, false);
        }

        /// <summary>Helps prevent sql injection in certain scenarios.</summary>
        /// <remarks>IMPORTANT: It is highly recommended to use other forms of sql injection protection such as using SqlParameters or ORMs.</remarks>
        /// <param name="strSQL">The string to filter.</param>
        /// <returns>The filtered string.</returns>
        [DnnDeprecated(7, 0, 0, "Use Security Filter functions in the PortalSecurity class", RemovalVersion = 11)]
        public static partial string PreventSQLInjection(string strSQL)
        {
            return PortalSecurity.Instance.InputFilter(strSQL, PortalSecurity.FilterFlag.NoSQL);
        }

        /// <summary>Looks at various file artifacts to determine if DotNetNuke has already been installed.</summary>
        /// <returns>true if installed else false.</returns>
        /// <remarks>
        /// If DotNetNuke has been installed, then we should treat database connection errors as real errors.
        /// If DotNetNuke has not been installed, then we should expect to have database connection problems
        /// since the connection string may not have been configured yet, which can occur during the installation
        /// wizard.
        /// </remarks>
        [DnnDeprecated(9, 7, 1, "Use Dependency Injection to resolve 'DotNetNuke.Abstractions.IApplicationStatusInfo' instead")]
        internal static partial bool IsInstalled() => applicationStatusInfo.IsInstalled();

        /// <summary>Gets the culture code of the tab.</summary>
        /// <param name="tabId">The tab ID.</param>
        /// <param name="isSuperTab">if set to <see langword="true"/> [is super tab].</param>
        /// <param name="settings">The settings.</param>
        /// <returns>return the tab's culture code, if ths tab doesn't exist, it will return current culture name.</returns>
        internal static string GetCultureCode(int tabId, bool isSuperTab, IPortalSettings settings)
        {
            string cultureCode = Null.NullString;
            if (settings != null)
            {
                TabInfo linkTab = TabController.Instance.GetTab(tabId, isSuperTab ? Null.NullInteger : settings.PortalId, false);
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

        /// <summary>Resets the application started timer.</summary>
        internal static void ResetAppStartElapseTime()
        {
            AppStopwatch.Restart();
        }

        /// <summary>Gets an <see cref="IServiceScope"/> that should be disposed, trying to find the current request scope if possible.</summary>
        /// <returns>An <see cref="IServiceScope"/> instance that should be disposed (but which can be disposed without prematurely disposing the current request scope).</returns>
        internal static IServiceScope GetOrCreateServiceScope()
        {
            return new MaybeDisposableServiceScope(
                HttpContextSource.Current?.GetScope(),
                DependencyProvider.CreateScope);
        }

        /// <summary>Gets an <see cref="IServiceProvider"/> for the current request, or the global provider if not available.</summary>
        /// <returns>An <see cref="IServiceProvider"/> instance.</returns>
        internal static IServiceProvider GetCurrentServiceProvider()
        {
            return HttpContextSource.Current?.GetScope()?.ServiceProvider ?? DependencyProvider;
        }

        /// <summary>Check whether the Filename matches extensions.</summary>
        /// <param name="filename">The filename.</param>
        /// <param name="strExtensions">The valid extensions.</param>
        /// <returns><see langword="true"/> if the Filename matches extensions, otherwise, <see langword="false"/>.</returns>
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

        /// <summary>
        /// Resolves dependencies after the <see cref="Globals.DependencyProvider"/>
        /// has been initialized. This should only be called once to resolve all
        /// dependencies used by the <see cref="Globals"/> object.
        /// </summary>
        private static void OnDependencyProviderChanged(object sender, PropertyChangedEventArgs eventArguments)
        {
            applicationStatusInfo = DependencyProvider?.GetRequiredService<IApplicationStatusInfo>();
            navigationManager = DependencyProvider?.GetRequiredService<INavigationManager>();
        }

        private class MaybeDisposableServiceScope : IServiceScope
        {
            private readonly bool disposeServiceScope;
            private readonly IServiceScope serviceScope;

            public MaybeDisposableServiceScope(IServiceScope doNotDisposeServiceScope, Func<IServiceScope> createDisposableServiceScope)
            {
                this.disposeServiceScope = doNotDisposeServiceScope is null;
                this.serviceScope = doNotDisposeServiceScope ?? createDisposableServiceScope();
            }

            public IServiceProvider ServiceProvider => this.serviceScope.ServiceProvider;

            public void Dispose()
            {
                if (this.disposeServiceScope)
                {
                    this.serviceScope.Dispose();
                }
            }
        }
    }
}
