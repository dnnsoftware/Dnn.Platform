using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Data.OleDb;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Configuration;
using System.Web.Security;
using System.Windows.Forms;
using System.Xml;
using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.ComponentModel;
using DotNetNuke.Data;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Modules.Definitions;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Profile;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Entities.Users;
using DotNetNuke.Modules.HTMLEditorProvider;
using DotNetNuke.Modules.NavigationProvider;
using DotNetNuke.Security.Membership;
using DotNetNuke.Security.Permissions;
using DotNetNuke.Security.Profile;
using DotNetNuke.Security.Roles;
using DotNetNuke.Security.Roles.Internal;
using DotNetNuke.Services.Cache;
using DotNetNuke.Services.ClientCapability;
using DotNetNuke.Services.FileSystem;
using DotNetNuke.Services.Journal;
using DotNetNuke.Services.Log.EventLog;
using DotNetNuke.Services.ModuleCache;
using DotNetNuke.Services.OutputCache;
using DotNetNuke.Services.Scheduling;
using DotNetNuke.Services.Search;
using DotNetNuke.Services.Sitemap;
using DotNetNuke.Services.Social.Notifications;
using DotNetNuke.Services.Url.FriendlyUrl;
using DotNetNukeSetup.Entities;
using Newtonsoft.Json;
using FileInfo = System.IO.FileInfo;

namespace DotNetNukeSetup
{
    public partial class Setup : Form
    {
        private string _dnnPackageName;
        public Setup()
        {
            InitializeComponent();
        }
        protected SqlDataProvider SqlProvider;
        public int PortalId { get; private set; }
        private static bool _alreadyLoaded;
        private static bool _providersReady;
        private readonly int _portalId = int.Parse(ConfigurationManager.AppSettings["PortalId"]);
        public const string FileFilters = "swf,jpg,jpeg,jpe,gif,bmp,png,doc,docx,xls,xlsx,ppt,pptx,pdf,txt,xml,xsl,xsd,css,zip,template,htmtemplate,ico,avi,mpg,mpeg,mp3,wmv,mov,wav";
        private string _applicationDirectory;

        private void Setup_Load(object sender, EventArgs e)
        {
            var settings = ConfigurationManager.AppSettings;
            txtDatabaseServer.Text = settings["DBServer"] ?? string.Empty;
            txtDatabaseName.Text = settings["DBCatalog"] ?? string.Empty;
            txtSiteRoot.Text = settings["SiteRootPath"] ?? string.Empty;
            txtWebSite.Text = settings["SiteUrl"] ?? string.Empty;
            txtObjectQualifier.Text = settings["ObjectQualifier"] ?? string.Empty;
            chkSocial.Checked = bool.Parse(settings["IsSocial"]);
            _applicationDirectory = Directory.GetCurrentDirectory();

            var toTrim = "-/".ToCharArray();
            var args = Environment.GetCommandLineArgs()
                                  .Skip(1)
                                  .Select(a => a.TrimStart(toTrim).ToLower())
                                  .Distinct()
                                  .ToArray();

            if (args.Length > 0)
            {
                _applicationDirectory = args[0];
                RunBatchLoading(args);
                Close();
            }

        }

        private void RunBatchLoading(string[] parts2Run)
        {
            var controller = new ServiceController { MachineName = ".", ServiceName = "w3svc" };
            controller.Stop();
            var settings = ConfigurationManager.AppSettings;

            string[] modules = Directory.GetFiles(Path.Combine(_applicationDirectory, "Modules"));
            foreach (string module in modules)
            {
                var mod = new FileInfo(module);
                File.Copy(module, settings["SiteRootPath"] + "/Install/Module/" + mod.Name, true);
            }
            var runAll = parts2Run.Contains("all");
            if (runAll || parts2Run.Contains("smtp"))
                UpdateMailSettingsInWebConfigFile();

            controller.Start();
            var wc = new WebClient();
            wc.DownloadString(settings["SiteUrl"] + "/Install/Install.aspx");
            wc.DownloadString(settings["SiteUrl"] + "/default.aspx");


            if (runAll || parts2Run.Contains("users"))
                btnUsersRoles_Click(this, EventArgs.Empty);

            if (runAll || parts2Run.Contains("files"))
                btnAddFolderFiles_Click(this, EventArgs.Empty);

            if (runAll || parts2Run.Contains("modules"))
                btnCreateCoreModulePages_Click(this, EventArgs.Empty);

            if (parts2Run.Contains("social"))
                btnAddSocial_Click(this, EventArgs.Empty);
        }

        private void SetupDNNProviders()
        {
            UpdateAppSettingInExeConfigFile("SiteRootPath", txtSiteRoot.Text);
            var simulator = new DotNetNuke.Tests.Website.HttpSimulator.HttpSimulator();
            simulator.SimulateRequest();
            InstallComponents();
            HttpContextBase httpContextBase = new HttpContextWrapper(HttpContext.Current);
            HttpContextSource.RegisterInstance(httpContextBase);
            LoadDnnProviders("data;logging;caching;authentication;members;roles;profiles;permissions;folder");
            var appPath = ConfigurationManager.AppSettings["SiteRootPath"];
            if (!string.IsNullOrEmpty(appPath))
            {
                var mappath = typeof(Globals).GetField("_applicationMapPath",
                                                        BindingFlags.Static | BindingFlags.NonPublic);
                mappath.SetValue(null, appPath);
            }

            //fix membership
            var providerProp = typeof(Membership).GetField("s_Provider", BindingFlags.Static | BindingFlags.NonPublic);
            providerProp.SetValue(null, Membership.Providers["AspNetSqlMembershipProvider"]);

            SqlProvider = new SqlDataProvider();

            var objPortalAliasInfo = new PortalAliasInfo { PortalID = _portalId, HTTPAlias = txtWebSite.Text.Replace("http://", "").Replace("https://", "") };
            var ps = new PortalSettings(59, objPortalAliasInfo);
            HttpContext.Current.Items.Add("PortalSettings", ps);
            PortalId = _portalId;
        }

        private static void InstallComponents()
        {
            Globals.ServerName = String.IsNullOrEmpty(Config.GetSetting("ServerName"))
                                     ? Dns.GetHostName()
                                     : Config.GetSetting("ServerName");

            ComponentFactory.Container = new SimpleContainer();

            ComponentFactory.InstallComponents(new ProviderInstaller("data", typeof(DataProvider),
                                                                     typeof(SqlDataProvider)));
            ComponentFactory.InstallComponents(new ProviderInstaller("caching", typeof(CachingProvider),
                                                                     typeof(FBCachingProvider)));
            ComponentFactory.InstallComponents(new ProviderInstaller("logging", typeof(LoggingProvider),
                                                                     typeof(DBLoggingProvider)));
            ComponentFactory.InstallComponents(new ProviderInstaller("scheduling", typeof(SchedulingProvider),
                                                                     typeof(DNNScheduler)));
            ComponentFactory.InstallComponents(new ProviderInstaller("searchIndex", typeof(IndexingProvider),
                                                                     typeof(ModuleIndexer)));
            ComponentFactory.InstallComponents(new ProviderInstaller("searchDataStore", typeof(SearchDataStoreProvider),
                                                                     typeof(SearchDataStore)));
            ComponentFactory.InstallComponents(new ProviderInstaller("members", typeof(DotNetNuke.Security.Membership.MembershipProvider),
                                                                     typeof(AspNetMembershipProvider)));
            ComponentFactory.InstallComponents(new ProviderInstaller("roles", typeof(DotNetNuke.Security.Roles.RoleProvider),
                                                                     typeof(DNNRoleProvider)));
            ComponentFactory.InstallComponents(new ProviderInstaller("profiles", typeof(ProfileProvider),
                                                                     typeof(DNNProfileProvider)));
            ComponentFactory.InstallComponents(new ProviderInstaller("permissions", typeof(PermissionProvider),
                                                                     typeof(CorePermissionProvider)));
            ComponentFactory.InstallComponents(new ProviderInstaller("outputCaching", typeof(OutputCachingProvider)));
            ComponentFactory.InstallComponents(new ProviderInstaller("moduleCaching", typeof(ModuleCachingProvider)));
            ComponentFactory.InstallComponents(new ProviderInstaller("sitemap", typeof(SitemapProvider),
                                                                     typeof(CoreSitemapProvider)));

            ComponentFactory.InstallComponents(new ProviderInstaller("friendlyUrl", typeof(FriendlyUrlProvider)));
            ComponentFactory.InstallComponents(new ProviderInstaller("folder", typeof(FolderProvider)));
            RegisterIfNotAlreadyRegistered<FolderProvider, StandardFolderProvider>("StandardFolderProvider");
            RegisterIfNotAlreadyRegistered<FolderProvider, SecureFolderProvider>("SecureFolderProvider");
            RegisterIfNotAlreadyRegistered<FolderProvider, DatabaseFolderProvider>("DatabaseFolderProvider");
            RegisterIfNotAlreadyRegistered<PermissionProvider>();
            ComponentFactory.InstallComponents(new ProviderInstaller("htmlEditor", typeof(HtmlEditorProvider),
                                                                     ComponentLifeStyleType.Transient));
            ComponentFactory.InstallComponents(new ProviderInstaller("navigationControl", typeof(NavigationProvider),
                                                                     ComponentLifeStyleType.Transient));
            ComponentFactory.InstallComponents(new ProviderInstaller("clientcapability",
                                                                     typeof(ClientCapabilityProvider)));
        }

        private static void RegisterIfNotAlreadyRegistered<TConcrete>() where TConcrete : class, new()
        {
            RegisterIfNotAlreadyRegistered<TConcrete, TConcrete>("");
        }

        private static void RegisterIfNotAlreadyRegistered<TAbstract, TConcrete>(string name)
            where TAbstract : class
            where TConcrete : class, new()
        {
            var provider = ComponentFactory.GetComponent<TAbstract>();
            if (provider == null)
            {
                if (String.IsNullOrEmpty(name))
                {
                    ComponentFactory.RegisterComponentInstance<TAbstract>(new TConcrete());
                }
                else
                {
                    ComponentFactory.RegisterComponentInstance<TAbstract>(name, new TConcrete());
                }
            }
        }

        private static void LoadDnnProviders(string providerList)
        {
            if (_alreadyLoaded)
            {
                return;
            }
            _alreadyLoaded = true;
            if (providerList != null)
            {
                var providers = providerList.Split(';');
                foreach (var provider in providers)
                {
                    if (provider.Length > 0)
                    {
                        var config =
                            DotNetNuke.Framework.Providers.ProviderConfiguration.GetProviderConfiguration(provider);
                        if (config != null)
                        {
                            foreach (string providerName in config.Providers.Keys)
                            {
                                var providerValue =
                                    (DotNetNuke.Framework.Providers.Provider)config.Providers[providerName];
                                var type = providerValue.Type;
                                var assembly = providerValue.Name;

                                if (type.Contains(", ")) //get the straight typename, no assembly, for the cache key
                                {
                                    assembly = type.Substring(type.IndexOf(", ") + 1);
                                    type = type.Substring(0, type.IndexOf(", "));
                                }

                                var cacheKey = type;

                                DotNetNuke.Framework.Reflection.CreateType(providerValue.Type, cacheKey, true, false);
                            }
                        }
                    }
                }
            }
        }

        private void btnBigGreenButton_Click(object sender, EventArgs e)
        {
            btnSelectPackage_Click(sender, e);
            btnSetup_Click(sender, e);
            btnSmtp_Click(sender, e);
            btnUsersRoles_Click(sender, e);
            if (chkSocial.Checked)
            {
                btnAddSocial_Click(sender, e);
            }
            btnCreateCoreModulePages_Click(sender, e);
            btnAddFolderFiles_Click(sender, e);
            btnOpenIE_Click(sender, e);
        }

        private void btnSelectPackage_Click(object sender, EventArgs e)
        {
            DialogResult result = openPackage.ShowDialog();
            if (result == DialogResult.OK)
            {
                _dnnPackageName = openPackage.FileName;
                txtPackage.Text = _dnnPackageName;
            }
        }

        private void btnSiteRoot_Click(object sender, EventArgs e)
        {
            DialogResult result = folderSiteRoot.ShowDialog();
            if (result == DialogResult.OK)
            {
                txtSiteRoot.Text = folderSiteRoot.SelectedPath;
            }
        }

        private void btnSetup_Click(object sender, EventArgs e)
        {
            toolStripStatusLabel1.Text = "Unzipping package";

            ServiceController controller = new ServiceController();
            controller.MachineName = ".";
            controller.ServiceName = "w3svc";
            string status = controller.Status.ToString();
            controller.Stop();

            var setupName = _dnnPackageName.Substring(_dnnPackageName.LastIndexOf(@"\", StringComparison.Ordinal) + 1).Replace("zip", "").Replace(".", "").Replace("_", "");
            chkSocial.Checked = setupName.ToLower().Contains("social");
            txtDatabaseName.Text = setupName;
            if (txtSiteRoot.Text == "")
            {
                txtSiteRoot.Text = _dnnPackageName.Substring(0, _dnnPackageName.LastIndexOf(@"\", StringComparison.Ordinal));
            }
            txtWebSite.Text = string.Format("http://{0}.dnndev.me", setupName);
            var siteRoot = txtSiteRoot.Text + @"\" + setupName;
            UpdateAppSettingInExeConfigFile("SiteRootPath", siteRoot);
            txtSiteRoot.Text = siteRoot;
            Application.DoEvents();
            var siteDirectory = Directory.CreateDirectory(siteRoot);
            foreach (FileInfo file in siteDirectory.GetFiles())
            {
                file.Delete();
            }

            foreach (DirectoryInfo dir in siteDirectory.GetDirectories())
            {
                dir.Delete(true);
            }

            ZipFile.ExtractToDirectory(_dnnPackageName, siteRoot);
            //add mail settings to 07.00.06.config
            UpdateMailSettingsInWebConfigFile();
            string[] modules = Directory.GetFiles(Path.Combine(_applicationDirectory, "Modules"));
            foreach (string module in modules)
            {
                var mod = new FileInfo(module);
                File.Copy(module, siteRoot + "/Install/Module/" + mod.Name, true);
            }
            toolStripStatusLabel1.Text = "Creating database";
            Application.DoEvents();
            CreateDatabase(System.Reflection.Assembly.GetExecutingAssembly().Location.Replace("DotNetNukeSetup.exe", ""), setupName);
            toolStripStatusLabel1.Text = "Creating website";
            Application.DoEvents();
            controller.Start();
            IISManager.CreateIISWebSite(setupName, setupName + ".dnndev.me", siteRoot);
            //Update app.config and web.config connection string
            UpdateConnectionStringInWebConfigFile(setupName);
            var wc = new WebClient();
            wc.DownloadString("http://" + setupName + ".dnndev.me/Install/Install.aspx");
            wc.DownloadString("http://" + setupName + ".dnndev.me/default.aspx");
            toolStripStatusLabel1.Text = "Finished creating website";
            Application.DoEvents();
        }

        private void btnUsersRoles_Click(object sender, EventArgs e)
        {
            ProviderSetup();
            toolStripStatusLabel1.Text = "Adding users and roles";
            Application.DoEvents();
            AddUsersAndRoles(chkSocial.Checked);
            toolStripStatusLabel1.Text = "Setup Complete";
            _providersReady = true;
        }

        private void btnAddSocial_Click(object sender, EventArgs e)
        {
            ProviderSetup();
            CreateSocialGroup("iPhone 5 Fans", "A group specifically for passionate iPhone 5 users.", UploadFile(string.Format("{0}\\DataFiles\\iphone5-group.png", _applicationDirectory), "iphone5-group.png"));
            CreateSocialGroup("Lumia 920 Fans", "A group specifically for passionate Lumia 920 users.", UploadFile(string.Format("{0}\\DataFiles\\lumia920-group.jpg", _applicationDirectory), "lumia920-group.jpg"));
            CreateSocialGroup("Windows Phone 8X Fans", "A group specifically for passionate Windows Phone 8X users.", UploadFile(string.Format("{0}\\DataFiles\\Winphone8-group.png", _applicationDirectory), "Winphone8-group.png"));
            AddIdeas();
            toolStripStatusLabel1.Text = "Finished adding Ideas";
            Application.DoEvents();
            AddQuestions();
            toolStripStatusLabel1.Text = "Finished adding Questions";
            Application.DoEvents();
            AddEvents();
            toolStripStatusLabel1.Text = "Finished adding Events";
            Application.DoEvents();
            AddDiscussions();
            toolStripStatusLabel1.Text = "Finished adding Discussions";
            Application.DoEvents();
        }

        private void ProviderSetup()
        {
            if (!_providersReady)
            {
                UpdateConnectionStringInExeConfigFile(txtDatabaseName.Text);
                UpdateMachineKeyAndSqlDataProviderInExeConfigFile();
                CopyDllsToExeFolder(txtSiteRoot.Text);
                SetupDNNProviders();
                _providersReady = true;
            }
        }

        private void btnOpenIE_Click(object sender, EventArgs e)
        {
            Process.Start("IExplore.exe", txtWebSite.Text);
        }

        private int rootPages;
        private int pageLevels;
        private int pagesPerLevel;
        private int pageNameIndex = 0;
        private int modulesPerPage;
        private string[] pageNames;
        private void btnCreateCoreModulePages_Click(object sender, EventArgs e)
        {
            toolStripStatusLabel1.Text = "Start adding Pages and Modules";
            Application.DoEvents();
            ProviderSetup();

            var portalController = new PortalController();
            var portal = portalController.GetPortal(PortalId);
            var tabPermissionCollection = new TabPermissionCollection();
            var rc = new RoleController();
            var pageEditorRole = rc.GetRoleByName(PortalId, "Page Editors");
            AddPagePermission(tabPermissionCollection, "View", Convert.ToInt32(-1));
            AddPagePermission(tabPermissionCollection, "View", Convert.ToInt32(portal.AdministratorRoleId));
            AddPagePermission(tabPermissionCollection, "Edit", Convert.ToInt32(portal.AdministratorRoleId));
            AddPagePermission(tabPermissionCollection, "View", Convert.ToInt32(pageEditorRole.RoleID));
            AddPagePermission(tabPermissionCollection, "Edit", Convert.ToInt32(pageEditorRole.RoleID));

            var modulePermissionCollection = new ModulePermissionCollection();
            var moduleEditorRole = rc.GetRoleByName(PortalId, "Module Editors");
            AddModulePermission(modulePermissionCollection, "View", Convert.ToInt32(moduleEditorRole.RoleID));
            AddModulePermission(modulePermissionCollection, "Edit", Convert.ToInt32(moduleEditorRole.RoleID));

            var coreModulesPage = AddPage(PortalId, Null.NullInteger, "Core Modules", "", "", "", true, tabPermissionCollection, false);
            var desktopModules = DesktopModuleController.GetPortalDesktopModulesByPortalID(PortalId);
            var sql = new SqlDataProvider();
            foreach (var desktopModule in desktopModules)
            {
                var newPage = AddPage(PortalId, coreModulesPage.TabID, desktopModule.Value.FriendlyName, "", "", "", true, tabPermissionCollection, false);
                var moduleDefId = GetModuleDefinition(desktopModule.Value.DesktopModuleID, desktopModule.Value.FriendlyName);
                if (moduleDefId != Null.NullInteger)
                {
                    AddModuleToPage(newPage, moduleDefId, desktopModule.Value.FriendlyName, "", true, modulePermissionCollection);
                }
            }


            int.TryParse(cmbRootPages.SelectedItem.ToString(), out rootPages);
            int.TryParse(cmbPageLevels.SelectedItem.ToString(), out pageLevels);
            int.TryParse(cmdPagesPerLevel.SelectedItem.ToString(), out pagesPerLevel);
            int.TryParse(cmbModulesPerPage.SelectedItem.ToString(), out modulesPerPage);
            pageNames = File.ReadAllLines(string.Format("{0}\\DataFiles\\pageNames.txt", _applicationDirectory));
            Action<int, string> createPagesRecursive = null;
            var htmlDesktopModule = DesktopModuleController.GetDesktopModuleByModuleName("DNN_HTML", PortalId);
            var htmlModuleDefId = GetModuleDefinition(htmlDesktopModule.DesktopModuleID, "Text/HTML");
            createPagesRecursive = (level, destTabPath) =>
            {
                if (level == pageLevels)
                    return;

                for (var c = 0; c < pagesPerLevel; c++)
                {
                    string currentTabPath = string.Format("{0}/{1}", destTabPath, pageNames[pageNameIndex]);
                    if (currentTabPath.StartsWith("/"))
                    {
                        currentTabPath = currentTabPath.Remove(0, 1);
                    }
                    var parentPage = TabController.GetTabByTabPath(PortalId, "//" + destTabPath.Replace(" ", "").Replace("/", "//"));
                    var newPage = AddPage(PortalId, parentPage, pageNames[pageNameIndex], "", "", "", true, tabPermissionCollection, false);
                    for (int moduleIndex = 0; moduleIndex < modulesPerPage; moduleIndex++)
                    {
                        var htmlModuleId = AddModuleToPage(newPage, htmlModuleDefId, "HTML Module", "", true, modulePermissionCollection);
                        const string content = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Sed dignissim, velit id dapibus fermentum, nunc justo condimentum dolor, at fringilla arcu orci pharetra libero. Mauris lobortis rhoncus lectus, vitae gravida mauris pellentesque eget. Duis sagittis magna sed enim scelerisque placerat. Nulla facilisi. Suspendisse pellentesque sapien id sapien dignissim porta. Class aptent taciti sociosqu ad litora torquent per conubia nostra, per inceptos himenaeos. Cum sociis natoque penatibus et magnis dis parturient montes, nascetur ridiculus mus.";
                        var entryDate = DateTime.Today.ToString("yyyy-MM-dd HH:mm:ss");
                        var sqlString = string.Format("INSERT INTO {0}HtmlText (ModuleId, Content, Version, StateID, IsPublished, CreatedByUserID, CreatedOnDate, LastModifiedByUserID, LastModifiedOnDate) VALUES ({1}, '{2}', 1, 1, 1, -1, '{3}', -1, '{3}')", txtObjectQualifier.Text, htmlModuleId, content, entryDate);
                        sql.ExecuteSQL(sqlString);
                    }

                    pageNameIndex += 1;
                    if (pageNameIndex >= pageNames.Count())
                    {
                        pageNameIndex = 0;
                    }

                    createPagesRecursive.Invoke(level + 1, currentTabPath);
                }
            };

            for (var rootPage = 0; rootPage < rootPages; rootPage++)
            {
                var pageName = pageNames[pageNameIndex++];
                if (pageNameIndex >= pageNames.Count())
                {
                    pageNameIndex = 0;
                }
                AddPage(PortalId, -1, pageName, "", "", "", true, tabPermissionCollection, false);
                createPagesRecursive.Invoke(0, pageName);
            }
            toolStripStatusLabel1.Text = "Finished adding Pages and Modules";
            Application.DoEvents();
        }

        private int levels;
        private int foldersPerLevel;
        private int folderNameIndex = 0;
        private int filesPerFolder;
        private string[] folderNames;
        private string[] fileNames;
        private void btnAddFolderFiles_Click(object sender, EventArgs e)
        {
            ProviderSetup();
            int.TryParse(cmbLevels.SelectedItem.ToString(), out levels);
            int.TryParse(cmbFoldersPerLevel.SelectedItem.ToString(), out foldersPerLevel);
            int.TryParse(cmbFilesPerFolder.SelectedItem.ToString(), out filesPerFolder);
            fileNames = Directory.GetFiles(_applicationDirectory + @"\DataFiles");
            folderNames = File.ReadAllLines(string.Format("{0}\\DataFiles\\folderNames.txt", _applicationDirectory));
            var dir = new DirectoryInfo(txtSiteRoot.Text + @"\Portals\0");
            Action<int, string> createFoldersRecursive = null;
            createFoldersRecursive = (level, destFolder) =>
            {
                if (level == levels)
                    return;

                for (var c = 0; c < foldersPerLevel; c++)
                {
                    folderNameIndex += 1;
                    if (folderNameIndex >= folderNames.Count())
                    {
                        folderNameIndex = 0;
                    }

                    var currentPath = string.Format("{0}\\{1}", destFolder, folderNames[folderNameIndex]);
                    Directory.CreateDirectory(txtSiteRoot.Text + "\\Portals\\0" + currentPath);
                    var folderPath = currentPath.Replace(@"\", "/").Remove(0, 1);
                    FolderManager.Instance.AddFolder(PortalId, folderPath);
                    for (int fileIndex = 0; fileIndex < filesPerFolder; fileIndex++)
                    {
                        var fileToCopy = new FileInfo(fileNames[fileIndex]);
                        UploadFile(fileToCopy.FullName, fileToCopy.Name, folderPath);
                    }

                    Debug.Print(txtSiteRoot.Text + "\\Portals\\0" + currentPath);
                    createFoldersRecursive.Invoke(level + 1, currentPath);
                }
            };

            createFoldersRecursive.Invoke(0, "");
            toolStripStatusLabel1.Text = "Finished adding folders and files";
            Application.DoEvents();

        }

        private void AddFolder(DirectoryInfo dir, int level, string folderName, IFolderInfo folder)
        {
            if (level == levels)
            {
                return;
            }
            level += 1;
            for (int folderIndex = 0; folderIndex < foldersPerLevel; folderIndex++)
            {
                folderNameIndex += 1;
                if (folderNameIndex >= folderNames.Count())
                {
                    folderNameIndex = 0;
                }
                var newDir = dir.CreateSubdirectory(folderName);
                Debug.Print(newDir.FullName);
                //var newFolder = FolderManager.Instance.AddFolder(PortalId, string.Format("{0}{1}", folder.FolderPath, folderName));

                //for (int fileIndex = 0; fileIndex < filesPerFolder; fileIndex++)
                //{
                //    var fileToCopy = new FileInfo(fileNames[fileIndex]);
                //    UploadFile(fileToCopy.FullName, newDir.FullName.Replace(txtSiteRoot.Text + @"\Portals\0", "") + "\\", fileToCopy.Name, newFolder.FolderPath);
                //}
                AddFolder(newDir, level, folderNames[folderNameIndex], new FolderInfo());
            }
        }

        private static void CopyDllsToExeFolder(string siteRoot)
        {
            var binFiles = Directory.GetFiles(siteRoot + @"\bin");
            var targetPath = System.Reflection.Assembly.GetExecutingAssembly().Location.Replace("DNNSetup.exe", "");

            foreach (var binFile in binFiles)
            {
                if (!binFile.Contains("DotNetNuke.dll") && !binFile.Contains("DotNetNuke.pdb"))
                {
                    var destFile = Path.Combine(targetPath, Path.GetFileName(binFile));
                    File.Copy(binFile, destFile, true);
                }
            }

            var providerFiles = Directory.GetFiles(siteRoot + @"\bin\providers");
            var providerPath = targetPath + "Providers";
            Directory.CreateDirectory(providerPath);
            foreach (var provider in providerFiles)
            {
                var destFile = Path.Combine(providerPath, Path.GetFileName(provider));
                File.Copy(provider, destFile, true);
            }
        }

        private void AddUsersAndRoles(bool isSocial)
        {
            var rc = new RoleController();
            var adminRole = rc.GetRoleByName(PortalId, "Administrators");
            var communityManagerRole = new RoleInfo();
            var moderatorRole = new RoleInfo();
            if (isSocial)
            {
                communityManagerRole = rc.GetRoleByName(PortalId, "Community Manager");
                moderatorRole = rc.GetRoleByName(PortalId, "Moderators");
            }
            var user = new UserInfo()
                           {
                               FirstName = "Yasmine",
                               LastName = "Kelsey",
                               Username = "Yasmine",
                               PortalID = PortalId,
                               IsSuperUser = false,
                               Email = "yasmine@dnn.com",
                               DisplayName = "Yasmine",
                               Membership = { Password = "password" }
                           };
            user.Profile.InitialiseProfile(PortalId);
            user.Profile.City = "San Fran Cisco";
            user.Profile.Country = "United States";
            user.Profile.PostalCode = "555 555";
            user.Profile.Region = "California";
            user.Profile.Street = "211 – 9440 202nd Street San Fran Cisco";
            user.Profile.Photo = UploadFile(string.Format("{0}\\DataFiles\\Yasmine.png", _applicationDirectory), "Yasmine.png").ToString();
            UserController.CreateUser(ref user);
            rc.AddUserRole(PortalId, user.UserID, adminRole.RoleID, Null.NullDate);
            if (isSocial)
            {
                rc.AddUserRole(PortalId, user.UserID, communityManagerRole.RoleID, Null.NullDate);
            }
            var communityManagerUser2 = new UserInfo()
                                            {
                                                FirstName = "Sarah",
                                                LastName = "Jones",
                                                Username = "Sarah",
                                                PortalID = PortalId,
                                                IsSuperUser = false,
                                                Email = "Sarah@dnn.com",
                                                DisplayName = "Sarah",
                                                Membership = { Password = "password" }
                                            };
            communityManagerUser2.Profile.InitialiseProfile(PortalId);
            communityManagerUser2.Profile.Country = "United States";
            communityManagerUser2.Profile.Biography = "Sarah has been working with Anova Mobility for seven years and has been involved in the telecommunications and IT industries for over 12 years. Sarah is a passionate member of the Anova team who commenced her Anova career working as part of the support desk team. Sarah's enthusiasm and dedication to customer support, as well as her tendency to go the extra mile, soon saw her promoted to Community Manager.  Sarah says 'I love technology and the chance to share my knowledge with our customers. It is a great feeling to go home each evening knowing people can get on with using their devices rather than wasting time trying to fix problems.'  In her spare time Sarah likes traveling, cooking and scuba diving.";
            communityManagerUser2.Profile.Photo = UploadFile(string.Format("{0}\\DataFiles\\sarah.png", _applicationDirectory), "sarah.png").ToString();
            UserController.CreateUser(ref communityManagerUser2);
            if (isSocial)
            {
                rc.AddUserRole(PortalId, communityManagerUser2.UserID, communityManagerRole.RoleID, Null.NullDate);

            }
            //Page Editors
            var pageEditorRoleId = rc.AddRole(new RoleInfo()
            {
                AutoAssignment = false,
                RoleName = "Page Editors",
                Description = "People who have Full Control of a Page",
                IsPublic = false,
                Status = RoleStatus.Approved,
                IsSystemRole = false,
                PortalID = PortalId,
                SecurityMode = SecurityMode.SecurityRole,
                RoleGroupID = -1
            });
            var pageEditor1 = new UserInfo()
            {
                FirstName = "Joseph",
                LastName = "Beyer",
                Username = "Joseph",
                PortalID = PortalId,
                IsSuperUser = false,
                Email = "Joseph@dnn.com",
                DisplayName = "Joseph",
                Membership = { Password = "password" }
            };
            pageEditor1.Profile.InitialiseProfile(PortalId);
            pageEditor1.Profile.Photo = UploadFile(string.Format("{0}\\DataFiles\\joseph.png", _applicationDirectory), "joseph.png").ToString();
            UserController.CreateUser(ref pageEditor1);
            rc.AddUserRole(PortalId, pageEditor1.UserID, pageEditorRoleId, Null.NullDate);
            //Module Editors
            var moduleEditorsRoleId = rc.AddRole(new RoleInfo()
            {
                AutoAssignment = false,
                RoleName = "Module Editors",
                Description = "People who have Full Control of a Module",
                IsPublic = false,
                Status = RoleStatus.Approved,
                IsSystemRole = false,
                PortalID = PortalId,
                SecurityMode = SecurityMode.SecurityRole,
                RoleGroupID = -1
            });
            var moduleEditor1 = new UserInfo()
            {
                FirstName = "Juan",
                LastName = "Abadias",
                Username = "Juan",
                PortalID = PortalId,
                IsSuperUser = false,
                Email = "Juan@dnn.com",
                DisplayName = "Ignacious",
                Membership = { Password = "password" }
            };
            moduleEditor1.Profile.InitialiseProfile(PortalId);
            moduleEditor1.Profile.Photo = UploadFile(string.Format("{0}\\DataFiles\\juan.png", _applicationDirectory), "juan.png").ToString();
            UserController.CreateUser(ref moduleEditor1);
            rc.AddUserRole(PortalId, moduleEditor1.UserID, moduleEditorsRoleId, Null.NullDate);

            //Registered Users
            var regUser1 = new UserInfo()
                               {
                                   FirstName = "Alan",
                                   LastName = "Wright",
                                   Username = "Alan",
                                   PortalID = PortalId,
                                   IsSuperUser = false,
                                   Email = "Alan@dnn.com",
                                   DisplayName = "Alan",
                                   Membership = { Password = "password" }
                               };
            regUser1.Profile.InitialiseProfile(PortalId);
            regUser1.Profile.Photo = UploadFile(string.Format("{0}\\DataFiles\\alan.jpg", _applicationDirectory), "alan.jpg").ToString();
            regUser1.Profile.Country = "United States";
            UserController.CreateUser(ref regUser1);

            var regUser2 = new UserInfo()
                               {
                                   FirstName = "Bill",
                                   LastName = "Smith",
                                   Username = "Bill",
                                   PortalID = PortalId,
                                   IsSuperUser = false,
                                   Email = "Bill@dnn.com",
                                   DisplayName = "Bill",
                                   Membership = { Password = "password" }
                               };
            regUser2.Profile.InitialiseProfile(PortalId);
            regUser2.Profile.Photo = UploadFile(string.Format("{0}\\DataFiles\\bill.png", _applicationDirectory), "bill.png").ToString();
            regUser2.Profile.Country = "United States";
            UserController.CreateUser(ref regUser2);
            var regUser3 = new UserInfo()
                               {
                                   FirstName = "Olivia",
                                   LastName = "Sutherland",
                                   Username = "Olivia",
                                   PortalID = PortalId,
                                   IsSuperUser = false,
                                   Email = "Olivia@dnn.com",
                                   DisplayName = "Ollie",
                                   Membership = { Password = "password" }
                               };
            regUser3.Profile.InitialiseProfile(PortalId);
            regUser3.Profile.Photo = UploadFile(string.Format("{0}\\DataFiles\\olivia.png", _applicationDirectory), "olivia.png").ToString();
            regUser3.Profile.Country = "United States";
            UserController.CreateUser(ref regUser3);
            var regUser4 = new UserInfo()
                               {
                                   FirstName = "Spam",
                                   LastName = "McSpammer",
                                   Username = "Spam",
                                   PortalID = PortalId,
                                   IsSuperUser = false,
                                   Email = "Spam@dnn.com",
                                   DisplayName = "Spam",
                                   Membership = { Password = "password" }
                               };
            regUser4.Profile.InitialiseProfile(PortalId);
            regUser4.Profile.Country = "United States";
            UserController.CreateUser(ref regUser4);
            var regUser5 = new UserInfo()
                               {
                                   FirstName = "Zhang",
                                   LastName = "Chan",
                                   Username = "Zhang",
                                   PortalID = PortalId,
                                   IsSuperUser = false,
                                   Email = "Zhang@dnn.com",
                                   DisplayName = "Zhang",
                                   Membership = { Password = "password" }
                               };
            regUser5.Profile.InitialiseProfile(PortalId);
            regUser5.Profile.Photo = UploadFile(string.Format("{0}\\DataFiles\\zhang.png", _applicationDirectory), "zhang.png").ToString();
            regUser5.Profile.Country = "United States";
            UserController.CreateUser(ref regUser5);
            var regUser6 = new UserInfo()
                               {
                                   FirstName = "Ailsa",
                                   LastName = "Meng",
                                   Username = "Ailsa",
                                   PortalID = PortalId,
                                   IsSuperUser = false,
                                   Email = "Ailsa@dnn.com",
                                   DisplayName = "Ailsa",
                                   Membership = { Password = "password" }
                               };
            regUser6.Profile.InitialiseProfile(PortalId);
            regUser6.Profile.Photo = UploadFile(string.Format("{0}\\DataFiles\\ailsa.png", _applicationDirectory), "ailsa.png").ToString();
            regUser6.Profile.Country = "United States";
            UserController.CreateUser(ref regUser6);
            var regUser7 = new UserInfo()
                               {
                                   FirstName = "Camille",
                                   LastName = "Bertrand",
                                   Username = "Camille",
                                   PortalID = PortalId,
                                   IsSuperUser = false,
                                   Email = "Camille@dnn.com",
                                   DisplayName = "Camille",
                                   Membership = { Password = "password" }
                               };
            regUser7.Profile.InitialiseProfile(PortalId);
            regUser7.Profile.Photo = UploadFile(string.Format("{0}\\DataFiles\\camille.png", _applicationDirectory), "camille.png").ToString();
            regUser7.Profile.Country = "United States";
            UserController.CreateUser(ref regUser7);
            var regUser8 = new UserInfo()
                               {
                                   FirstName = "Lee",
                                   LastName = "Wong",
                                   Username = "Lee",
                                   PortalID = PortalId,
                                   IsSuperUser = false,
                                   Email = "Lee@dnn.com",
                                   DisplayName = "Lee",
                                   Membership = { Password = "password" }
                               };
            regUser8.Profile.InitialiseProfile(PortalId);
            regUser8.Profile.Photo = UploadFile(string.Format("{0}\\DataFiles\\lee.png", _applicationDirectory), "lee.png").ToString();
            regUser8.Profile.Country = "United States";
            UserController.CreateUser(ref regUser8);
            var regUser9 = new UserInfo()
                               {
                                   FirstName = "Seo-yeon",
                                   LastName = "Kim",
                                   Username = "Seo-yeon",
                                   PortalID = PortalId,
                                   IsSuperUser = false,
                                   Email = "Seo-yeon@dnn.com",
                                   DisplayName = "Seo-yeon",
                                   Membership = { Password = "password" }
                               };
            regUser9.Profile.InitialiseProfile(PortalId);
            regUser9.Profile.Photo = UploadFile(string.Format("{0}\\DataFiles\\seo-yeon.png", _applicationDirectory), "seo-yeon.png").ToString();
            regUser9.Profile.Country = "Korea";
            UserController.CreateUser(ref regUser9);
            var regUser10 = new UserInfo()
                                {
                                    FirstName = "Fatma",
                                    LastName = "Ali",
                                    Username = "Fatma",
                                    PortalID = PortalId,
                                    IsSuperUser = false,
                                    Email = "Fatma@dnn.com",
                                    DisplayName = "Fatma",
                                    Membership = { Password = "password" }
                                };
            regUser10.Profile.InitialiseProfile(PortalId);
            regUser10.Profile.Photo = UploadFile(string.Format("{0}\\DataFiles\\fatma.png", _applicationDirectory), "fatma.png").ToString();
            regUser10.Profile.Country = "Egypt";
            UserController.CreateUser(ref regUser10);
            var regUser11 = new UserInfo()
                                {
                                    FirstName = "Angel",
                                    LastName = "Dang",
                                    Username = "Angel",
                                    PortalID = PortalId,
                                    IsSuperUser = false,
                                    Email = "Angel@dnn.com",
                                    DisplayName = "Angel",
                                    Membership = { Password = "password" }
                                };
            regUser11.Profile.InitialiseProfile(PortalId);
            regUser11.Profile.Photo = UploadFile(string.Format("{0}\\DataFiles\\angel.png", _applicationDirectory), "angel.png").ToString();
            regUser11.Profile.Country = "India";
            UserController.CreateUser(ref regUser11);
            var regUser12 = new UserInfo()
                                {
                                    FirstName = "Jacques",
                                    LastName = "DuPont",
                                    Username = "Jacques",
                                    PortalID = PortalId,
                                    IsSuperUser = false,
                                    Email = "Jacques@dnn.com",
                                    DisplayName = "Jacques",
                                    Membership = { Password = "password" }
                                };
            regUser12.Profile.InitialiseProfile(PortalId);
            regUser12.Profile.Photo = UploadFile(string.Format("{0}\\DataFiles\\jacques.png", _applicationDirectory), "jacques.png").ToString();
            regUser12.Profile.Country = "France";
            UserController.CreateUser(ref regUser12);
            var usermoderatorUser1 = new UserInfo()
                                         {
                                             FirstName = "Lachlan",
                                             LastName = "McKenzie",
                                             Username = "Lachlan",
                                             PortalID = PortalId,
                                             IsSuperUser = false,
                                             Email = "Lachlan@dnn.com",
                                             DisplayName = "Lachlan",
                                             Membership = { Password = "password" }
                                         };
            usermoderatorUser1.Profile.InitialiseProfile(PortalId);
            usermoderatorUser1.Profile.Photo = UploadFile(string.Format("{0}\\DataFiles\\lachlan.png", _applicationDirectory), "lachlan.png").ToString();
            usermoderatorUser1.Profile.Country = "United States";
            UserController.CreateUser(ref usermoderatorUser1);
            if (isSocial)
            {
                rc.AddUserRole(PortalId, usermoderatorUser1.UserID, moderatorRole.RoleID, Null.NullDate);

            }
            var usermoderatorUser2 = new UserInfo()
                                         {
                                             FirstName = "Joey",
                                             LastName = "Blackburn",
                                             Username = "Joey",
                                             PortalID = PortalId,
                                             IsSuperUser = false,
                                             Email = "Joey@dnn.com",
                                             DisplayName = "Joey",
                                             Membership = { Password = "password" }
                                         };
            usermoderatorUser2.Profile.InitialiseProfile(PortalId);
            usermoderatorUser2.Profile.Photo = UploadFile(string.Format("{0}\\DataFiles\\joey.png", _applicationDirectory), "joey.png").ToString();
            usermoderatorUser2.Profile.Country = "United States";
            UserController.CreateUser(ref usermoderatorUser2);
            if (isSocial)
            {
                rc.AddUserRole(PortalId, usermoderatorUser2.UserID, moderatorRole.RoleID, Null.NullDate);
            }

            //Bulk add for Developer Data Volume case

            if (chkVolume.Checked)
            {
                var fileName = string.Format("{0}\\DataFiles\\UserNames.xlsx", _applicationDirectory);
                var connectionString = string.Format("Provider=Microsoft.ACE.OLEDB.12.0;Data Source={0}; Extended Properties=Excel 12.0;", fileName);
                var adapter = new OleDbDataAdapter("SELECT * FROM [Sheet1$]", connectionString);
                var ds = new DataSet();
                adapter.Fill(ds, "Sheet1");
                DataTable data = ds.Tables["Sheet1"];
                foreach (DataRow row in data.Rows)
                {
                    var regUser = new UserInfo()
                    {
                        FirstName = row["First"].ToString(),
                        LastName = row["Last"].ToString(),
                        Username = row["First"].ToString(),
                        PortalID = PortalId,
                        IsSuperUser = false,
                        Email = string.Format("{0}@dnn.com", row["First"].ToString()),
                        DisplayName = row["First"].ToString(),
                        Membership = { Password = "password" }
                    };
                    UserController.CreateUser(ref regUser);
                }


                var roleNamesFile = string.Format("{0}\\DataFiles\\RoleNames.xlsx", _applicationDirectory);
                var connectionString2 = string.Format("Provider=Microsoft.ACE.OLEDB.12.0;Data Source={0}; Extended Properties=Excel 12.0;", roleNamesFile);
                var adapter2 = new OleDbDataAdapter("SELECT * FROM [Sheet1$]", connectionString2);
                var ds2 = new DataSet();
                adapter2.Fill(ds2, "Sheet1");
                DataTable data2 = ds2.Tables["Sheet1"];
                foreach (DataRow row in data2.Rows)
                {
                    rc.AddRole(new RoleInfo()
                    {
                        AutoAssignment = false,
                        RoleName = row["RoleName"].ToString(),
                        Description = row["Description"].ToString(),
                        IsPublic = false,
                        Status = RoleStatus.Approved,
                        IsSystemRole = false,
                        PortalID = PortalId,
                        SecurityMode = SecurityMode.SecurityRole,
                        RoleGroupID = -1
                    });
                }
            }
        }

        private void CreateDatabase(string targetPath, string setupName)
        {
            var process = new Process { StartInfo = { UseShellExecute = false } };
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.FileName = "sqlcmd.exe";
            process.StartInfo.Arguments = string.Format("-E -S {0} -i \"{1}\" -v DatabaseName={2}", txtDatabaseServer.Text,
                                                        targetPath + @"Scripts\ResetDatabase.sql", setupName);
            process.StartInfo.WorkingDirectory = @"C:\";
            process.Start();
        }

        private void UpdateConnectionStringInExeConfigFile(string setupName)
        {
            var configuration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            var section = (ConnectionStringsSection)configuration.GetSection("connectionStrings");
            section.ConnectionStrings["SiteSqlServer"].ConnectionString =
                string.Format("Server={0};Database={1};Integrated Security=True", txtDatabaseServer.Text, setupName);
            configuration.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection(configuration.ConnectionStrings.SectionInformation.Name);
        }

        private void UpdateAppSettingInExeConfigFile(string setting, string value)
        {
            var configuration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            configuration.AppSettings.Settings[setting].Value = value;
            configuration.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection("appSettings");
        }


        private void UpdateConnectionStringInWebConfigFile(string setupName)
        {
            XmlDocument doc = new XmlDocument();
            string configFile = Path.Combine(txtSiteRoot.Text, "web.config");
            doc.Load(configFile);
            XmlNode nodeConnectionString = doc.SelectSingleNode("//connectionStrings");
            nodeConnectionString.RemoveAll();
            XmlNode nodeNewKey = doc.CreateElement("add");
            XmlAttribute attName = doc.CreateAttribute("name");
            XmlAttribute attConnectionString = doc.CreateAttribute("connectionString");
            XmlAttribute attProviderName = doc.CreateAttribute("providerName");
            attName.Value = "SiteSqlServer";
            attConnectionString.Value = string.Format("Server={0};Database={1};Integrated Security=True", txtDatabaseServer.Text, setupName);
            attProviderName.Value = "System.Data.SqlClient";
            nodeNewKey.Attributes.Append(attName);
            nodeNewKey.Attributes.Append(attConnectionString);
            nodeNewKey.Attributes.Append(attConnectionString);
            nodeNewKey.Attributes.Append(attProviderName);
            nodeConnectionString.AppendChild(nodeNewKey);
            doc.Save(Path.Combine(txtSiteRoot.Text, "web.config"));
        }

        private void UpdateMachineKeyAndSqlDataProviderInExeConfigFile()
        {
            XmlDocument doc = new XmlDocument();
            string configFile = Path.Combine(txtSiteRoot.Text, "web.config");
            doc.Load(configFile);
            XmlNode nodeMachineKey = doc.SelectSingleNode("//system.web/machineKey");
            var validationKey = nodeMachineKey.Attributes["validationKey"].Value;
            var decryptionKey = nodeMachineKey.Attributes["decryptionKey"].Value;

            XmlNode nodeSqlDataProvider = doc.SelectSingleNode("//configuration/dotnetnuke/data/providers/add");
            var objectQualifier = nodeSqlDataProvider.Attributes["objectQualifier"].Value;
            var databaseOwner = nodeSqlDataProvider.Attributes["databaseOwner"].Value;

            var configuration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            var a = configuration.FilePath;
            var section = (MachineKeySection)configuration.GetSection("system.web/machineKey");
            section.ValidationKey = validationKey;
            section.DecryptionKey = decryptionKey;
            configuration.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection(section.SectionInformation.Name);

        }

        private void UpdateMailSettingsInWebConfigFile()
        {
            XmlDocument webConfigDoc = new XmlDocument();
            string webConfigFile = Path.Combine(txtSiteRoot.Text, @"web.config");
            webConfigDoc.Load(webConfigFile);
            if (webConfigDoc.SelectSingleNode("//configuration/system.net/mailSettings") != null)
            {
                return;
            }
            XmlDocument configDoc = new XmlDocument();
            string configFile = Path.Combine(txtSiteRoot.Text, @"Install\Config\07.00.06.config");
            configDoc.Load(configFile);

            XmlDocument mailDoc = new XmlDocument();
            string mailConfigFile = Path.Combine(_applicationDirectory, @"Scripts\configurationMerges.txt");
            using (StreamReader sr = new StreamReader(mailConfigFile))
            {
                String mailMerge = sr.ReadToEnd().Replace("[location]", txtSiteRoot.Text);
                mailDoc.LoadXml(mailMerge);
            }
            XmlNode mailNode = mailDoc.SelectSingleNode("//nodes/node");

            XmlNode importNode = configDoc.ImportNode(mailNode, true);
            XmlNode configNodes = configDoc.SelectSingleNode("//nodes");
            configNodes.AppendChild(importNode);
            configDoc.Save(Path.Combine(txtSiteRoot.Text, @"Install\Config\07.00.06.config"));

            XmlDocument hostConfigDoc = new XmlDocument();
            string hostConfigFile = Path.Combine(txtSiteRoot.Text, @"Install\DotNetNuke.install.config.resources");
            hostConfigDoc.Load(hostConfigFile);
            XmlNode smtpNode = hostConfigDoc.SelectSingleNode("//dotnetnuke/settings/SMTPServer");
            smtpNode.InnerText = "local";
            hostConfigDoc.Save(hostConfigFile);
        }

        private void AddIdeas()
        {
            var fileName = string.Format("{0}\\DataFiles\\Ideas.xlsx", _applicationDirectory);
            var connectionString =
                string.Format("Provider=Microsoft.ACE.OLEDB.12.0;Data Source={0}; Extended Properties=Excel 12.0;", fileName);
            var adapter = new OleDbDataAdapter("SELECT * FROM [Sheet1$]", connectionString);
            var ds = new DataSet();
            adapter.Fill(ds, "Sheet1");
            DataTable data = ds.Tables["Sheet1"];
            int ideaId = -1;
            var sql = new SqlDataProvider();
            var dict = new Dictionary<string, string> { { "TabID", GetTabByName("Ideas").TabID.ToString() }, { "ModuleID", GetModuleByFriendlyName("Ideas").ModuleID.ToString() } };
            foreach (DataRow row in data.Rows)
            {
                using (
                    var userManager = new UserManager(txtWebSite.Text, row["Username"].ToString(), "password"))
                {
                    toolStripStatusLabel1.Text = "Logging in as:" + row["Username"];
                    Application.DoEvents();
                    var pastDate = DateTime.Today.AddDays(double.Parse(row["TimePastDays"].ToString()) * -1).ToString("yyyy-MM-dd HH:mm:ss");
                    if (userManager.Login())
                    {
                        if (row["Title or reply"].ToString() != "reply")
                        {
                            toolStripStatusLabel1.Text = "Adding idea:" + row["Title or reply"];
                            Application.DoEvents();
                            var idea = new IdeaReference
                            {
                                IdeaId = -1,
                                GroupId = -1,
                                Status = 15,
                                ContentTitle = row["Title or reply"].ToString(),
                                Content = row["Content"].ToString(),
                                Tags = row["Tags"].ToString().Split(','),
                                Authorized = true,
                                InitialVotes = int.Parse(row["Votes"].ToString()),
                                PortalID = PortalId,
                                SelectedCategoryId = -1,
                                SelectedTypeId = -1
                            };
                            var result = userManager.UploadUserContentAsJson("DesktopModules/DNNCorp/Ideas/API/Edit/Create", idea, dict);
                            var returnedObject = result.Content.ReadAsStringAsync().Result;
                            var post = JsonConvert.DeserializeObject<IdeaResponse>(returnedObject);
                            ideaId = post.Idea.ContentItemId;
                            var sqlString = string.Format("UPDATE {0}Ideas_Idea SET Authorized = 1 WHERE IdeaId={1}", txtObjectQualifier.Text, post.Idea.IdeaId);
                            sql.ExecuteSQL(sqlString);
                            sqlString = string.Format("UPDATE {0}ContentItems SET [CreatedOnDate]=CAST('{1}' AS DATETIME), [LastModifiedOnDate]=CAST('{1}' AS DATETIME) WHERE ContentItemId={2}", txtObjectQualifier.Text, pastDate, post.Idea.ContentItemId);
                            sql.ExecuteSQL(sqlString);
                        }
                        else
                        {
                            //Application.DoEvents();
                            //var reply = new CommentPost { ContentItemId = ideaId, Comment = "Test comment", JournalTypeId=18, PageSize = 5, Context = new int[] { } };
                            //var result = userManager.UploadUserContentAsJson("DesktopModules/DNNCorp/Ideas/API/Social/PostComment", reply, dict);
                            //var returnedObject = result.Content.ReadAsStringAsync().Result;
                            //var post = JsonConvert.DeserializeObject<CommentResponse>(returnedObject);
                            //var sqlString = string.Format("UPDATE {0}Journal_Comments SET [DateCreated]=CAST('{1}' AS DATETIME), [DateUpdated]=CAST('{1}' AS DATETIME) WHERE CommentId={2}", txtObjectQualifier.Text, pastDate, post.Comment.CommentId);
                            //sql.ExecuteSQL(sqlString);
                        }
                    }
                    else
                    {
                        MessageBox.Show(string.Format("User: {0} didnt login correctly", row["Username"]));
                    }
                }
            }
        }

        private void AddQuestions()
        {
            var fileName = string.Format("{0}\\DataFiles\\Questions.xlsx", _applicationDirectory);
            var connectionString =
                string.Format("Provider=Microsoft.ACE.OLEDB.12.0;Data Source={0}; Extended Properties=Excel 12.0;", fileName);
            var adapter = new OleDbDataAdapter("SELECT * FROM [Sheet1$]", connectionString);
            var ds = new DataSet();
            adapter.Fill(ds, "Sheet1");
            DataTable data = ds.Tables["Sheet1"];
            int questionId = -1;
            var sql = new SqlDataProvider();
            var dict = new Dictionary<string, string> { { "TabID", GetTabByName("Answers").TabID.ToString() }, { "ModuleID", GetModuleByFriendlyName("Answers").ModuleID.ToString() } };
            foreach (DataRow row in data.Rows)
            {
                using (
                    var userManager = new UserManager(txtWebSite.Text, row["Username"].ToString(), "password"))
                {
                    toolStripStatusLabel1.Text = "Logging in as:" + row["Username"];
                    Application.DoEvents();
                    var pastDate = DateTime.Today.AddDays(double.Parse(row["TimePastDays"].ToString()) * -1).ToString("yyyy-MM-dd HH:mm:ss");
                    if (userManager.Login())
                    {
                        if (row["Title or reply"].ToString() != "reply")
                        {
                            toolStripStatusLabel1.Text = "Adding question:" + row["Title or reply"];
                            Application.DoEvents();
                            var question = new QuestionReference
                                               {
                                                   Approved = true,
                                                   ContentTitle = row["Title or reply"].ToString(),
                                                   Content = row["Content"].ToString(),
                                                   Tags = row["Tags"].ToString().Split(',')
                                               };
                            var result = userManager.UploadUserContentAsJson("DesktopModules/DNNCorp/Answers/API/Edit/Ask", question, dict);
                            var returnedObject = result.Content.ReadAsStringAsync().Result;
                            var post = JsonConvert.DeserializeObject<PostInfo>(returnedObject);
                            questionId = post.PostId;
                            var sqlString = string.Format("UPDATE {0}Answers_Post SET [CreatedDate]=CAST('{1}' AS DATETIME), [LastModifiedDate]=CAST('{1}' AS DATETIME) WHERE PostId={2}", txtObjectQualifier.Text, pastDate, questionId);
                            sql.ExecuteSQL(sqlString);
                            sqlString = string.Format("UPDATE {0}ContentItems SET [CreatedOnDate]=CAST('{1}' AS DATETIME), [LastModifiedOnDate]=CAST('{1}' AS DATETIME) WHERE ContentItemId={2}", txtObjectQualifier.Text, pastDate, post.ContentItemId);
                            sql.ExecuteSQL(sqlString);
                        }
                        else
                        {
                            //Application.DoEvents();
                            //var answer = new AnswerReference { ParentId = questionId, ContentTitle = null, Content = row["Content"].ToString() };
                            //var result = userManager.UploadUserContentAsJson("DesktopModules/DNNCorp/Answers/API/Detail/PostAnswer", answer, dict);
                            //var returnedObject = result.Content.ReadAsStringAsync().Result;
                            //var post = JsonConvert.DeserializeObject<PostInfo>(returnedObject);
                            //var sqlString = string.Format("UPDATE {0}Answers_Post SET [CreatedDate]=CAST('{1}' AS DATETIME), [LastModifiedDate]=CAST('{1}' AS DATETIME) WHERE PostId={2}", txtObjectQualifier.Text, pastDate, post.PostId);
                            //sql.ExecuteSQL(sqlString);
                        }
                    }
                    else
                    {
                        MessageBox.Show(string.Format("User: {0} didnt login correctly", row["Username"]));
                    }
                }
            }
        }

        private void AddEvents()
        {
            var fileName = string.Format("{0}\\DataFiles\\Events.xlsx", _applicationDirectory);
            var connectionString =
                string.Format("Provider=Microsoft.ACE.OLEDB.12.0;Data Source={0}; Extended Properties=Excel 12.0;", fileName);
            var adapter = new OleDbDataAdapter("SELECT * FROM [Sheet1$]", connectionString);
            var ds = new DataSet();
            adapter.Fill(ds, "Sheet1");
            DataTable data = ds.Tables["Sheet1"];
            int eventId = -1;
            var dict = new Dictionary<string, string> { { "TabID", GetTabByName("Events").TabID.ToString() }, { "ModuleID", GetModuleByFriendlyName("Social Events").ModuleID.ToString() }, };
            foreach (DataRow row in data.Rows)
            {
                using (var userManager = new UserManager(txtWebSite.Text, row["Username"].ToString(), "password"))
                {
                    toolStripStatusLabel1.Text = "Logging in as:" + row["Username"];
                    Application.DoEvents();
                    if (userManager.Login())
                    {
                        toolStripStatusLabel1.Text = "Adding Event:" + row["Title"];
                        Application.DoEvents();
                        var socialEvent = new EventReference
                            {
                                Approved = true,
                                Tags = row["Tags"].ToString().Split(','),
                                ContentTitle = row["Title"].ToString(),
                                Content = row["Content"].ToString(),
                                StartTime = DateTime.Today.AddDays(double.Parse(row["TimePastDays"].ToString()) * -1).AddHours(18),
                                EndTime = DateTime.Today.AddDays(double.Parse(row["TimePastDays"].ToString()) * -1).AddHours(20),
                                Country = row["Country"].ToString(),
                                Region = row["Region"].ToString(),
                                City = row["City"].ToString(),
                                Street = row["Street"].ToString(),
                                PostalCode = row["PostalCode"].ToString(),
                                MaxAttendees = Null.NullInteger,
                                EnableRsvp = true,
                                ShowGuests = true
                            };

                        var result = userManager.UploadUserContentAsJson("DesktopModules/DNNCorp/SocialEvents/API/Edit/Create", socialEvent, dict);
                        var returnedObject = result.Content.ReadAsStringAsync().Result;
                        var post = JsonConvert.DeserializeObject<PostInfo>(returnedObject);
                        eventId = post.PostId;
                    }
                    else
                    {
                        MessageBox.Show(string.Format("User: {0} didnt login correctly", row["Username"]));
                    }
                }
            }
        }

        private void AddDiscussions()
        {
            var fileName = string.Format("{0}\\DataFiles\\Discussions.xlsx", _applicationDirectory);
            var connectionString =
                string.Format("Provider=Microsoft.ACE.OLEDB.12.0;Data Source={0}; Extended Properties=Excel 12.0;", fileName);
            var adapter = new OleDbDataAdapter("SELECT * FROM [Sheet1$]", connectionString);
            var ds = new DataSet();
            adapter.Fill(ds, "Sheet1");
            DataTable data = ds.Tables["Sheet1"];
            int discussionId = -1;
            var sql = new SqlDataProvider();
            var dict = new Dictionary<string, string> { { "TabID", GetTabByName("Discussions").TabID.ToString() }, { "ModuleID", GetModuleByFriendlyName("Discussions").ModuleID.ToString() }, };
            foreach (DataRow row in data.Rows)
            {
                using (var userManager = new UserManager(txtWebSite.Text, row["Username"].ToString(), "password"))
                {
                    toolStripStatusLabel1.Text = "Logging in as:" + row["Username"];
                    Application.DoEvents();
                    var pastDate = DateTime.Today.AddDays(double.Parse(row["TimePastDays"].ToString()) * -1).ToString("yyyy-MM-dd HH:mm:ss");
                    if (userManager.Login())
                    {
                        if (row["Title"].ToString() != "reply")
                        {
                            toolStripStatusLabel1.Text = "Adding Topic:" + row["Title"];
                            Application.DoEvents();
                            var discussionTopic = new DiscussionTopicReference
                            {
                                Approved = true,
                                Tags = row["Tags"].ToString().Split(','),
                                ContentTitle = row["Title"].ToString(),
                                Content = row["Content"].ToString(),
                                Closed = bool.Parse(row["Closed"].ToString()),
                                Pinned = bool.Parse(row["Pinned"].ToString())
                            };

                            var result = userManager.UploadUserContentAsJson("DesktopModules/DNNCorp/Discussions/API/Edit/Add", discussionTopic, dict);
                            var returnedObject = result.Content.ReadAsStringAsync().Result;
                            var post = JsonConvert.DeserializeObject<DiscussionTopicInfo>(returnedObject);
                            discussionId = post.ContentItemId;
                            var sqlString = string.Format("UPDATE {0}ContentItems SET [CreatedOnDate]=CAST('{1}' AS DATETIME), [LastModifiedOnDate]=CAST('{1}' AS DATETIME) WHERE ContentItemId={2}", txtObjectQualifier.Text, pastDate, post.ContentItemId);
                            sql.ExecuteSQL(sqlString);
                        }
                        else
                        {
                            Application.DoEvents();
                            //var reply = new CommentPost { ContentItemId = discussionId, Comment = row["Content"].ToString() };
                            //var result = userManager.UploadUserContentAsJson("DesktopModules/DNNCorp/Discussions/API/Social/PostComment", reply, dict);
                            //var returnedObject = result.Content.ReadAsStringAsync().Result;
                            //var post = JsonConvert.DeserializeObject<CommentResponse>(returnedObject);
                            //var sqlString = string.Format("UPDATE {0}Journal_Comments SET [DateCreated]=CAST('{1}' AS DATETIME), [DateUpdated]=CAST('{1}' AS DATETIME) WHERE CommentId={2}", txtObjectQualifier.Text, pastDate, post.Comment.CommentId);
                            //sql.ExecuteSQL(sqlString);
                        }
                    }
                    else
                    {
                        MessageBox.Show(string.Format("User: {0} didnt login correctly", row["Username"]));
                    }
                }
            }
        }

        private TabInfo GetTabByName(string pageName)
        {
            var tabController = new TabController();
            return tabController.GetTabByName(pageName, PortalId);
        }

        private ModuleInfo GetModuleByFriendlyName(string friendlyName)
        {
            var moduleController = new ModuleController();
            return moduleController.GetModuleByDefinition(PortalId, friendlyName);
        }

        private void CreateSocialGroup(string name, string description, int iconFile)
        {
            var adminUser = UserController.GetUserByName("Sarah");
            var roleController = new RoleController();
            var rc = new RoleController();
            var modRoles = new List<RoleInfo> { rc.GetRoleByName(PortalId, "Moderators") };
            var modUsers = new List<UserInfo> { adminUser };
            var roleInfo = new RoleInfo()
            {
                PortalID = PortalId,
                RoleName = name,
                Description = description,
                SecurityMode = SecurityMode.SocialGroup,
                Status = RoleStatus.Approved,
                IsPublic = true,
                IconFile = "FileID=" + iconFile
            };
            roleInfo.RoleGroupID = -1;
            roleInfo.RoleID = roleController.AddRole(roleInfo);
            roleInfo = roleController.GetRole(roleInfo.RoleID, PortalId);

            var groupUrl = Globals.NavigateURL(GetTabByName("Groups").TabID, "", new String[] { "groupid=" + roleInfo.RoleID.ToString() });
            if (groupUrl.StartsWith("http://") || groupUrl.StartsWith("https://"))
            {
                const int startIndex = 8; // length of https://
                groupUrl = groupUrl.Substring(groupUrl.IndexOf("/", startIndex, StringComparison.InvariantCultureIgnoreCase));
            }
            roleInfo.Settings.Add("URL", groupUrl);
            roleInfo.Settings.Add("GroupCreatorName", adminUser.DisplayName);
            roleInfo.Settings.Add("ReviewMembers", false.ToString());

            TestableRoleController.Instance.UpdateRoleSettings(roleInfo, true);
            roleController.AddUserRole(PortalId, adminUser.UserID, roleInfo.RoleID, RoleStatus.Approved, true, Null.NullDate, Null.NullDate);
            AddGroupNotification("GroupCreatedNotification", GetTabByName("Groups").TabID, GetModuleByFriendlyName("Social Groups").ModuleID, roleInfo, adminUser, modRoles, modUsers);
            CreateJournalEntry(roleInfo, adminUser);
        }

        private Notification AddGroupNotification(string notificationTypeName, int tabId, int moduleId, RoleInfo group, UserInfo initiatingUser, IList<RoleInfo> moderators, IList<UserInfo> recipients)
        {
            var notificationType = NotificationsController.Instance.GetNotificationType(notificationTypeName);
            var subject = string.Format("The {0} group has been created.", group.RoleName);
            var body = string.Format("The {0} group has been created.", group.RoleName);
            var notification = new Notification
            {
                NotificationTypeID = notificationType.NotificationTypeId,
                Subject = subject,
                Body = body,
                SenderUserID = initiatingUser.UserID,
                Context = String.Format("{0}:{1}:{2}", tabId, moduleId, group.RoleID)
            };
            NotificationsController.Instance.SendNotification(notification, initiatingUser.PortalID, moderators, recipients);
            return notification;
        }

        public static void CreateJournalEntry(RoleInfo roleInfo, UserInfo createdBy)
        {
            var journalController = JournalController.Instance;
            var journalItem = new JournalItem();

            string url = "";

            if (roleInfo.Settings.ContainsKey("URL"))
            {
                url = roleInfo.Settings["URL"];
            }

            journalItem.PortalId = roleInfo.PortalID;
            journalItem.ProfileId = createdBy.UserID;
            journalItem.UserId = createdBy.UserID;
            journalItem.Title = roleInfo.RoleName;
            journalItem.ItemData = new ItemData { Url = url };
            journalItem.SocialGroupId = roleInfo.RoleID;
            journalItem.Summary = roleInfo.Description;
            journalItem.Body = null;
            journalItem.JournalTypeId = journalController.GetJournalType("groupcreate").JournalTypeId;
            journalItem.ObjectKey = string.Format("groupcreate:{0}:{1}", roleInfo.RoleID.ToString(CultureInfo.InvariantCulture), createdBy.UserID.ToString(CultureInfo.InvariantCulture));

            if (journalController.GetJournalItemByKey(roleInfo.PortalID, journalItem.ObjectKey) != null)
                journalController.DeleteJournalItemByKey(roleInfo.PortalID, journalItem.ObjectKey);


            journalItem.SecuritySet = string.Empty;

            if (roleInfo.IsPublic)
                journalItem.SecuritySet += "E,";


            journalController.SaveJournalItem(journalItem, -1);
        }

        private int UploadFile(string originalPath, string fileName, string folderpath = "")
        {
            var folder = FolderManager.Instance.GetFolder(PortalId, folderpath);
            var fs = File.Open(originalPath, FileMode.Open);
            var fileId = FileManager.Instance.AddFile(folder, fileName, fs, true).FileId;
            fs.Close();
            return fileId;
        }

        private static TabInfo AddPage(int portalId, int parentId, string tabName, string description, string tabIconFile, string tabIconFileLarge, bool isVisible, TabPermissionCollection permissions, bool isAdmin)
        {
            var tabController = new TabController();

            TabInfo tab = tabController.GetTabByName(tabName, portalId, parentId);

            if (tab == null || tab.ParentId != parentId)
            {
                tab = new TabInfo
                {
                    TabID = Null.NullInteger,
                    PortalID = portalId,
                    TabName = tabName,
                    Title = "",
                    Description = description,
                    KeyWords = "",
                    IsVisible = isVisible,
                    DisableLink = false,
                    ParentId = parentId,
                    IconFile = tabIconFile,
                    IconFileLarge = tabIconFileLarge,
                    IsDeleted = false
                };
                tab.TabID = tabController.AddTab(tab, !isAdmin);

                if (((permissions != null)))
                {
                    foreach (TabPermissionInfo tabPermission in permissions)
                    {
                        tab.TabPermissions.Add(tabPermission, true);
                    }
                    TabPermissionController.SaveTabPermissions(tab);
                }
            }
            return tab;
        }

        private static void AddPagePermission(TabPermissionCollection permissions, string key, int roleId)
        {
            var permissionController = new PermissionController();
            var permission = (PermissionInfo)permissionController.GetPermissionByCodeAndKey("SYSTEM_TAB", key)[0];

            var tabPermission = new TabPermissionInfo { PermissionID = permission.PermissionID, RoleID = roleId, AllowAccess = true };

            permissions.Add(tabPermission);
        }

        public static int AddModuleToPage(TabInfo page, int moduleDefId, string moduleTitle, string moduleIconFile, bool inheritPermissions, ModulePermissionCollection permissions)
        {
            var moduleController = new ModuleController();
            ModuleInfo moduleInfo;
            int moduleId = Null.NullInteger;

            if ((page != null))
            {
                moduleInfo = new ModuleInfo
                {
                    ModuleID = Null.NullInteger,
                    PortalID = page.PortalID,
                    TabID = page.TabID,
                    ModuleOrder = -1,
                    ModuleTitle = moduleTitle,
                    PaneName = Globals.glbDefaultPane,
                    ModuleDefID = moduleDefId,
                    CacheTime = 0,
                    IconFile = moduleIconFile,
                    AllTabs = false,
                    Visibility = VisibilityState.None,
                    InheritViewPermissions = inheritPermissions
                };

                try
                {
                    moduleId = moduleController.AddModule(moduleInfo);
                }
                catch (Exception exc)
                {
                }
                if (((permissions != null)))
                {
                    foreach (ModulePermissionInfo modulePermission in permissions)
                    {
                        moduleInfo.ModulePermissions.Add(modulePermission, true);
                    }
                    ModulePermissionController.SaveModulePermissions(moduleInfo);
                }
            }
            return moduleId;
        }

        private static void AddModulePermission(ModulePermissionCollection permissions, string key, int roleId)
        {
            var permissionController = new PermissionController();
            var permission = (PermissionInfo)permissionController.GetPermissionByCodeAndKey("SYSTEM_MODULE_DEFINITION", key)[0];

            var modulePermission = new ModulePermissionInfo { PermissionID = permission.PermissionID, RoleID = roleId, AllowAccess = true };

            permissions.Add(modulePermission);
        }

        private static int GetModuleDefinition(int desktopModuleId, string moduleDefinitionName)
        {
            // get desktop module
            var desktopModule = DesktopModuleController.GetDesktopModule(desktopModuleId, Null.NullInteger);
            if (desktopModule == null)
            {
                return -1;
            }

            // get module definition
            ModuleDefinitionInfo objModuleDefinition = ModuleDefinitionController.GetModuleDefinitionByFriendlyName(moduleDefinitionName, desktopModule.DesktopModuleID);
            if (objModuleDefinition == null)
            {
                return -1;
            }


            return objModuleDefinition.ModuleDefID;
        }

        private void chkSocial_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void btnSmtp_Click(object sender, EventArgs e)
        {
            UpdateMailSettingsInWebConfigFile();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Process.Start(@"C:\Program Files (x86)\Microsoft Visual Studio 11.0\Common7\IDE\devenv.exe", @"C:\DotNetNuke\TFS\DotNetNuke\src\DotNetNuke_CS\Content\DNN_Platform_UnitTests.sln");
            MessageBox.Show("test");
        }
    }
}
