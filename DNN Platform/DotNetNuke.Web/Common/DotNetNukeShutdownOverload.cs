using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Web;
using System.Web.Hosting;
using DotNetNuke.Common;
using DotNetNuke.Instrumentation;

namespace DotNetNuke.Web.Common.Internal
{
    internal static class DotNetNukeShutdownOverload
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(DotNetNukeShutdownOverload));

        private static Timer _shutDownDelayTimer;
        private static bool _handleShutdowns;
        private static bool _shutdownInprogress;
        private static FileSystemWatcher _configWatcher;
        private static FileSystemWatcher _binFolderWatcher;
        private static string _binFolder = "";
        private static string _configFileName = "web.config".ToLower();

        [DllImport("webengine4.dll", CharSet = CharSet.Unicode)]
        private static extern int GrowFileNotificationBuffer(string appId, bool fWatchSubtree);

        internal static void InitializeFcnSettings()
        {
            // any error/message logged below should be informational only
            try
            {
                // ReSharper disable once PossibleNullReferenceException
                var fileChangesMonitor = typeof(HttpRuntime)
                    .GetProperty("FileChangesMonitor", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static)
                    .GetValue(null, null);

                if (fileChangesMonitor == null)
                {
                    Logger.Info("fileChangesMonitor is null");
                    AddSiteFilesMonitors(true);
                }
                else
                {
                    // ReSharper disable once PossibleNullReferenceException
                    var fcnVal = fileChangesMonitor.GetType()
                        .GetField("_FCNMode", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.IgnoreCase)
                        .GetValue(fileChangesMonitor);

                    Logger.Info("FCNMode = " + fcnVal + " (Modes: NotSet/Default=0, Disabled=1, Single=2)");

                    var dirMonCompletion = typeof(HttpRuntime).Assembly.GetType("System.Web.DirMonCompletion");
                    var dirMonCount = (int)dirMonCompletion.InvokeMember("_activeDirMonCompletions",
                        BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.GetField,
                        null, null, null);
                    Logger.Trace("DirMonCompletion count: " + dirMonCount);

                    if (fcnVal.ToString() == "1" /*Disabled*/)
                    {
                        AddSiteFilesMonitors(true);
                    }
                    else
                    {
                        try
                        {
                            AddSiteFilesMonitors(false);
                            var result = GrowFileNotificationBuffer(HostingEnvironment.ApplicationID, true);
                            Logger.Trace("Calling GrowFileNotificationBuffer() returned: " + result);
                        }
                        catch (Exception e)
                        {
                            Logger.Trace("Calling GrowFileNotificationBuffer() threw this error: " + e);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Info(e);
            }
        }

        private static void AddSiteFilesMonitors(bool handleShutdowns)
        {
            if (_configWatcher == null)
            {
                lock (typeof(Initialize))
                {
                    if (_configWatcher == null)
                    {
                        try
                        {
                            _handleShutdowns = handleShutdowns;
                            if (_handleShutdowns)
                                _shutDownDelayTimer = new Timer(InitiateShutdown);

                            _binFolder = Path.Combine(Globals.ApplicationMapPath, "bin").ToLower();
                            _binFolderWatcher = new FileSystemWatcher
                            {
                                Filter = "*.*",
                                Path = _binFolder,
                                NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName,
                                IncludeSubdirectories = true,
                            };

                            _configWatcher = new FileSystemWatcher
                            {
                                Filter = _configFileName,
                                Path = Globals.ApplicationMapPath,
                                NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName,
                                IncludeSubdirectories = false,
                            };

                            _binFolderWatcher.Created += WatcherOnCreated;
                            _binFolderWatcher.Deleted += WatcherOnDeleted;
                            _binFolderWatcher.Renamed += WatcherOnRenamed;
                            _binFolderWatcher.Changed += WatcherOnChanged;
                            _binFolderWatcher.Error += WatcherOnError;

                            // no need for all events; just the changed
                            //_configWatcher.Created += WatcherOnCreated;
                            //_configWatcher.Deleted += WatcherOnDeleted;
                            //_configWatcher.Renamed += WatcherOnRenamed;
                            _configWatcher.Changed += WatcherOnChanged;
                            //_configWatcher.Error += WatcherOnError;

                            // begin watching;
                            _configWatcher.EnableRaisingEvents = true;
                            _binFolderWatcher.EnableRaisingEvents = true;
                            Logger.Trace("Added watcher for: " + _binFolderWatcher.Path + "\\" + _binFolderWatcher.Filter);
                            Logger.Trace("Added watcher for: " + _configWatcher.Path + "\\" + _configWatcher.Filter);
                        }
                        catch (Exception ex)
                        {
                            Logger.Trace("Error adding our own file monitoring object. " + ex);
                        }
                    }
                }
            }
        }

        private static void InitiateShutdown(object state)
        {
            if (!_handleShutdowns) return;
            try
            {
                HttpRuntime.UnloadAppDomain();
            }
            catch (Exception ex)
            {
                _shutdownInprogress = false;
                Logger.Error(ex);
            }
        }

        private static void ShceduleShutdown()
        {
            // no need for locking; worst case is timer extended a bit more
            if (_handleShutdowns && !_shutdownInprogress)
            {
                _shutdownInprogress = true;
                // delay for a very short period
                _shutDownDelayTimer.Change(1500, Timeout.Infinite);
            }
        }

        private static void WatcherOnChanged(object sender, FileSystemEventArgs e)
        {
            if (Logger.IsInfoEnabled && !e.FullPath.EndsWith(".log.resources"))
                Logger.Info($"Watcher Activity: {e.ChangeType}. Path: {e.FullPath}");

            if (_handleShutdowns && !_shutdownInprogress && (
                    (e.FullPath ?? "").ToLower().StartsWith(_binFolder) ||
                    (e.FullPath ?? "").ToLower().EndsWith(_configFileName)))
            {
                ShceduleShutdown();
            }
        }

        private static void WatcherOnCreated(object sender, FileSystemEventArgs e)
        {
            if (Logger.IsInfoEnabled && !e.FullPath.EndsWith(".log.resources"))
                Logger.Info($"Watcher Activity: {e.ChangeType}. Path: {e.FullPath}");

            if (_handleShutdowns && !_shutdownInprogress && (e.FullPath ?? "").ToLower().StartsWith(_binFolder))
                ShceduleShutdown();
        }

        private static void WatcherOnRenamed(object sender, RenamedEventArgs e)
        {
            if (Logger.IsInfoEnabled && !e.FullPath.EndsWith(".log.resources"))
                Logger.Info($"Watcher Activity: {e.ChangeType}. New Path: {e.FullPath}. Old Path: {e.OldFullPath}");

            if (_handleShutdowns && !_shutdownInprogress && (e.FullPath ?? "").ToLower().StartsWith(_binFolder))
                ShceduleShutdown();
        }

        private static void WatcherOnDeleted(object sender, FileSystemEventArgs e)
        {
            if (Logger.IsInfoEnabled && !e.FullPath.EndsWith(".log.resources"))
                Logger.Info($"Watcher Activity: {e.ChangeType}. Path: {e.FullPath}");

            if (_handleShutdowns && !_shutdownInprogress && (e.FullPath ?? "").ToLower().StartsWith(_binFolder))
                ShceduleShutdown();
        }

        private static void WatcherOnError(object sender, ErrorEventArgs e)
        {
            if (Logger.IsInfoEnabled)
                Logger.Info("Watcher Activity: N/A. Error: " + e.GetException());
        }
    }
}