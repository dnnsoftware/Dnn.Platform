// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Utilities.Fakes;

using System;

using DotNetNuke.Abstractions.Application;

public class FakeApplicationStatusInfo(UpgradeStatus status = UpgradeStatus.None, string applicationMapPath = "", string databaseVersion = "10.0.0", bool isInstalled = true, bool incrementalVersionExists = false, int lastAppliedIteration = 0)
    : IApplicationStatusInfo
{
    /// <inheritdoc />
    public UpgradeStatus Status { get; set; } = status;

    /// <inheritdoc />
    public string ApplicationMapPath { get; set; } = applicationMapPath;

    /// <inheritdoc />
    public Version DatabaseVersion { get; set; } = new Version(databaseVersion);

    /// <inheritdoc />
    public bool IsInstalled() => isInstalled;

    /// <inheritdoc />
    public void SetStatus(UpgradeStatus status)
    {
        this.Status = status;
    }

    /// <inheritdoc />
    public void UpdateDatabaseVersion(Version version)
    {
        this.DatabaseVersion = version;
    }

    /// <inheritdoc />
    public void UpdateDatabaseVersionIncrement(Version version, int increment)
    {
        this.DatabaseVersion = version;
    }

    /// <inheritdoc />
    public bool IncrementalVersionExists(Version version) => incrementalVersionExists;

    /// <inheritdoc />
    public int GetLastAppliedIteration(Version version) => lastAppliedIteration;
}
