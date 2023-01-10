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

        private static Timer shutDownDelayTimer;
        private static bool handleShutdowns;
        private static bool shutdownInprogress;
        private static FileSystemWatcher binOrRootWatcher;
        private static string binFolder = string.Empty;

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
                        null,
                        null,
                        null);
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
            if (binOrRootWatcher == null)
            {
                lock (typeof(Initialize))
                {
                    if (binOrRootWatcher == null)
                    {
                        try
                        {
                            DotNetNukeShutdownOverload.handleShutdowns = handleShutdowns;
                            if (DotNetNukeShutdownOverload.handleShutdowns)
                            {
                                shutDownDelayTimer = new Timer(InitiateShutdown);
                            }

                            binFolder = Path.Combine(Globals.ApplicationMapPath, "bin").ToLowerInvariant();
                            binOrRootWatcher = new FileSystemWatcher
                            {
                                Filter = "*.*",
                                Path = handleShutdowns ? binFolder : Globals.ApplicationMapPath,
                                NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName,
                                IncludeSubdirectories = true,
                            };

                            binOrRootWatcher.Created += WatcherOnCreated;
                            binOrRootWatcher.Deleted += WatcherOnDeleted;
                            binOrRootWatcher.Renamed += WatcherOnRenamed;
                            binOrRootWatcher.Changed += WatcherOnChanged;
                            binOrRootWatcher.Error += WatcherOnError;

                            // begin watching;
                            binOrRootWatcher.EnableRaisingEvents = true;
                            Logger.Trace("Added watcher for: " + binOrRootWatcher.Path + "\\" + binOrRootWatcher.Filter);
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
            if (!handleShutdowns)
            {
                return;
            }

            try
            {
                HttpRuntime.UnloadAppDomain();
            }
            catch (Exception ex)
            {
                shutdownInprogress = false;
                Logger.Error(ex);
            }
        }

        private static void ShceduleShutdown()
        {
            // no need for locking; worst case is timer extended a bit more
            if (handleShutdowns && !shutdownInprogress)
            {
                shutdownInprogress = true;

                // delay for a very short period
                shutDownDelayTimer.Change(1500, Timeout.Infinite);
            }
        }

        private static void WatcherOnChanged(object sender, FileSystemEventArgs e)
        {
            if (Logger.IsInfoEnabled && !e.FullPath.EndsWith(".log.resources"))
            {
                Logger.Info($"Watcher Activity: {e.ChangeType}. Path: {e.FullPath}");
            }

            if (handleShutdowns && !shutdownInprogress && (e.FullPath ?? string.Empty).StartsWith(binFolder, StringComparison.InvariantCultureIgnoreCase))
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

            if (handleShutdowns && !shutdownInprogress && (e.FullPath ?? string.Empty).StartsWith(binFolder, StringComparison.InvariantCultureIgnoreCase))
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

            if (handleShutdowns && !shutdownInprogress && (e.FullPath ?? string.Empty).StartsWith(binFolder, StringComparison.InvariantCultureIgnoreCase))
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

            if (handleShutdowns && !shutdownInprogress && (e.FullPath ?? string.Empty).StartsWith(binFolder, StringComparison.InvariantCultureIgnoreCase))
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
