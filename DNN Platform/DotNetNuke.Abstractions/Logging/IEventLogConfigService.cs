// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Abstractions.Logging;

using System.Collections;
using System.Collections.Generic;

/// <summary>
/// The event log config service provides APIs for managing the
/// Event Log configuration and individual logs itself.
/// </summary>
public interface IEventLogConfigService
{
    /// <summary>Adds the log type via configuration file.</summary>
    /// <param name="configFile">The configuration file.</param>
    /// <param name="fallbackConfigFile">The fallback configuration file.</param>
    void AddLogType(string configFile, string fallbackConfigFile);

    /// <summary>Adds a <see cref="ILogTypeConfigInfo"/> to the database.</summary>
    /// <param name="logTypeConfig">The log type configuration.</param>
    void AddLogTypeConfigInfo(ILogTypeConfigInfo logTypeConfig);

    /// <summary>Adds a <see cref="ILogTypeInfo"/> to the database.</summary>
    /// <param name="logType">The log type.</param>
    void AddLogType(ILogTypeInfo logType);

    /// <summary>Deletes an existing <see cref="ILogTypeInfo"/> from the database.</summary>
    /// <param name="logType">The log type.</param>
    void DeleteLogType(ILogTypeInfo logType);

    /// <summary>Delete an existing <see cref="ILogTypeConfigInfo"/> from the database..</summary>
    /// <param name="logTypeConfig">The log type config.</param>
    void DeleteLogTypeConfigInfo(ILogTypeConfigInfo logTypeConfig);

    /// <summary>Gets all the <see cref="ILogTypeConfigInfo"/> from the database.</summary>
    /// <returns>The logs.</returns>
    IEnumerable<ILogTypeConfigInfo> GetLogTypeConfigInfo();

    /// <summary>Get the <see cref="ILogTypeConfigInfo"/> by id..</summary>
    /// <param name="id">The id.</param>
    /// <returns>The log type config info.</returns>
    ILogTypeConfigInfo GetLogTypeConfigInfoByID(string id);

    /// <summary>Gets the log type info dictionary.</summary>
    /// <returns>The dictionary.</returns>
    IDictionary<string, ILogTypeInfo> GetLogTypeInfoDictionary();

    /// <summary>UPdate the log type config info.</summary>
    /// <param name="logTypeConfig">The log type config.</param>
    void UpdateLogTypeConfigInfo(ILogTypeConfigInfo logTypeConfig);

    /// <summary>Update the log type.</summary>
    /// <param name="logType">The log type info.</param>
    void UpdateLogType(ILogTypeInfo logType);
}
