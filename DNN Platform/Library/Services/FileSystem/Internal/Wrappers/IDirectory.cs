// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Services.FileSystem.Internal;

using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

/// <summary>
/// A contract specifying the ability to take actions on directories.
/// A wrapper around functionality from <see cref="Directory"/>.
/// Also provides async overloads via <see cref="SchwabenCode.QuickIO.QuickIODirectory"/>.
/// </summary>
public interface IDirectory
{
    /// <inheritdoc cref="Directory.Delete(string, bool)" />
    void Delete(string path, bool recursive);

    /// <inheritdoc cref="SchwabenCode.QuickIO.QuickIODirectory.DeleteAsync(string, bool)" />
    Task DeleteAsync(string path, bool recursive);

    /// <inheritdoc cref="Directory.Exists(string)" />
    bool Exists(string path);

    /// <inheritdoc cref="SchwabenCode.QuickIO.QuickIODirectory.ExistsAsync(string)" />
    Task<bool> ExistsAsync(string path);

    /// <inheritdoc cref="Directory.GetDirectories(string)" />
    string[] GetDirectories(string path);

    /// <inheritdoc cref="SchwabenCode.QuickIO.QuickIODirectory.EnumerateDirectoryPathsAsync(string, string, SearchOption, SchwabenCode.QuickIO.QuickIOPathType, SchwabenCode.QuickIO.QuickIOEnumerateOptions)" />
    Task<IEnumerable<string>> GetDirectoriesAsync(string path);

    /// <inheritdoc cref="Directory.GetFiles(string)" />
    string[] GetFiles(string path);

    /// <inheritdoc cref="Directory.GetFiles(string, string, SearchOption)" />
    string[] GetFiles(string path, string searchPattern, SearchOption searchOption);

    /// <inheritdoc cref="SchwabenCode.QuickIO.QuickIODirectory.EnumerateFilePathsAsync(string, string, SearchOption, SchwabenCode.QuickIO.QuickIOPathType, SchwabenCode.QuickIO.QuickIOEnumerateOptions)" />
    Task<IEnumerable<string>> GetFilesAsync(string path);

    /// <inheritdoc cref="SchwabenCode.QuickIO.QuickIODirectory.EnumerateFilePathsAsync(string, string, SearchOption, SchwabenCode.QuickIO.QuickIOPathType, SchwabenCode.QuickIO.QuickIOEnumerateOptions)" />
    Task<IEnumerable<string>> GetFilesAsync(string path, string searchPattern, SearchOption searchOption);

    /// <inheritdoc cref="Directory.Move(string, string)" />
    void Move(string sourceDirName, string destDirName);

    /// <inheritdoc cref="SchwabenCode.QuickIO.QuickIODirectory.MoveAsync(string, string, bool)" />
    Task MoveAsync(string sourceDirName, string destDirName);

    /// <inheritdoc cref="Directory.CreateDirectory(string)" />
    void CreateDirectory(string path);

    /// <inheritdoc cref="SchwabenCode.QuickIO.QuickIODirectory.CreateAsync(string, bool)" />
    Task CreateDirectoryAsync(string path);

    /// <inheritdoc cref="SchwabenCode.QuickIO.QuickIODirectory.Create(string, bool)" />
    void CreateDirectory(string path, bool recursive);

    /// <inheritdoc cref="SchwabenCode.QuickIO.QuickIODirectory.CreateAsync(string, bool)" />
    Task CreateDirectoryAsync(string path, bool recursive);
}
