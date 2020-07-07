// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.Common.Internal
{
    using System;
    using System.IO;
    using System.Reflection;
    using System.Threading;
    using System.Web;

    using DotNetNuke.Common;
    using DotNetNuke.Instrumentation;

    internal static class DotNetNukeShutdownOverload
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(DotNetNukeShutdownOverload));

        private static Timer _shutDownDelayTimer;
        private static bool _handleShutdowns;
        private static bool _shutdownInprogress;
        private static FileSystemWatcher _binOrRootWatcher;
        private static string _binFolder = string.Empty;

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

                    // AddSiteFilesMonitoring(true);
                }
                else
                {
                    // ReSharper disable once PossibleNullReferenceException
                    var fcnVal = fileChangesMonitor.GetType()
                        .GetField("_FCNMode", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.IgnoreCase)
                        .GetValue(fileChangesMonitor);

                    Logger.Info("FCNMode = " + fcnVal + " (Modes: NotSet/Default=0, Disabled=1, Single=2)");

                    var dirMonCompletion = typeof(HttpRuntime).Assembly.GetType("System.Web.DirMonCompletion");
                    var dirMonCount = (int)dirMonCompletion.InvokeMember(
                        "_activeDirMonCompletions",
                        BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.GetField,
                        null, null, null);
                    Logger.Trace("DirMonCompletion count: " + dirMonCount);

                    // enable our monitor only when fcnMode="Disabled"
                    // AddSiteFilesMonitoring(fcnVal.ToString() == "1");
                }

                // just monitor the root folder but don't interfere
                AddSiteFilesMonitoring(false);
            }
            catch (Exception e)
            {
                Logger.Info(e);
            }
        }

        private static void AddSiteFilesMonitoring(bool handleShutdowns)
        {
            if (_binOrRootWatcher == null)
            {
                lock (typeof(Initialize))
                {
                    if (_binOrRootWatcher == null)
                    {
                        try
                        {
                            _handleShutdowns = handleShutdowns;
                            if (_handleShutdowns)
                            {
                                _shutDownDelayTimer = new Timer(InitiateShutdown);
                            }

                            _binFolder = Path.Combine(Globals.ApplicationMapPath, "bin").ToLowerInvariant();
                            _binOrRootWatcher = new FileSystemWatcher
                            {
                                Filter = "*.*",
                                Path = handleShutdowns ? _binFolder : Globals.ApplicationMapPath,
                                NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName,
                                IncludeSubdirectories = true,
                            };

                            _binOrRootWatcher.Created += WatcherOnCreated;
                            _binOrRootWatcher.Deleted += WatcherOnDeleted;
                            _binOrRootWatcher.Renamed += WatcherOnRenamed;
                            _binOrRootWatcher.Changed += WatcherOnChanged;
                            _binOrRootWatcher.Error += WatcherOnError;

                            // begin watching;
                            _binOrRootWatcher.EnableRaisingEvents = true;
                            Logger.Trace("Added watcher for: " + _binOrRootWatcher.Path + "\\" + _binOrRootWatcher.Filter);
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
            if (!_handleShutdowns)
            {
                return;
            }

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
            {
                Logger.Info($"Watcher Activity: {e.ChangeType}. Path: {e.FullPath}");
            }

            if (_handleShutdowns && !_shutdownInprogress && (e.FullPath ?? string.Empty).StartsWith(_binFolder, StringComparison.InvariantCultureIgnoreCase))
            {
                ShceduleShutdown();
            }
        }

        private static void WatcherOnCreated(object sender, FileSystemEventArgs e)
        {
            if (Logger.IsInfoEnabled && !e.FullPath.EndsWith(".log.resources"))
            {
                Logger.Info($"Watcher Activity: {e.ChangeType}. Path: {e.FullPath}");
            }

            if (_handleShutdowns && !_shutdownInprogress && (e.FullPath ?? string.Empty).StartsWith(_binFolder, StringComparison.InvariantCultureIgnoreCase))
            {
                ShceduleShutdown();
            }
        }

        private static void WatcherOnRenamed(object sender, RenamedEventArgs e)
        {
            if (Logger.IsInfoEnabled && !e.FullPath.EndsWith(".log.resources"))
            {
                Logger.Info($"Watcher Activity: {e.ChangeType}. New Path: {e.FullPath}. Old Path: {e.OldFullPath}");
            }

            if (_handleShutdowns && !_shutdownInprogress && (e.FullPath ?? string.Empty).StartsWith(_binFolder, StringComparison.InvariantCultureIgnoreCase))
            {
                ShceduleShutdown();
            }
        }

        private static void WatcherOnDeleted(object sender, FileSystemEventArgs e)
        {
            if (Logger.IsInfoEnabled && !e.FullPath.EndsWith(".log.resources"))
            {
                Logger.Info($"Watcher Activity: {e.ChangeType}. Path: {e.FullPath}");
            }

            if (_handleShutdowns && !_shutdownInprogress && (e.FullPath ?? string.Empty).StartsWith(_binFolder, StringComparison.InvariantCultureIgnoreCase))
            {
                ShceduleShutdown();
            }
        }

        private static void WatcherOnError(object sender, ErrorEventArgs e)
        {
            if (Logger.IsInfoEnabled)
            {
                Logger.Info("Watcher Activity: N/A. Error: " + e.GetException());
            }
        }
    }
}
