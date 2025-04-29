// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Services.FileSystem.Internal;

using System;
using System.IO;
using System.Threading.Tasks;

/// <summary>
/// A contract specifying the ability to take actions on files.
/// A wrapper around functionality from <see cref="File"/>.
/// Also provides async overloads via <see cref="SchwabenCode.QuickIO.QuickIOFile"/>.
/// </summary>
public interface IFile
{
    /// <inheritdoc cref="File.Create(string)" />
    Stream Create(string path);

    /// <inheritdoc cref="File.Delete(string)" />
    void Delete(string path);

    /// <inheritdoc cref="SchwabenCode.QuickIO.QuickIOFile.DeleteAsync(string)" />
    Task DeleteAsync(string path);

    /// <inheritdoc cref="File.Exists(string)" />
    bool Exists(string path);

    /// <inheritdoc cref="SchwabenCode.QuickIO.QuickIOFile.ExistsAsync(string)" />
    Task<bool> ExistsAsync(string path);

    /// <inheritdoc cref="File.GetAttributes(string)" />
    FileAttributes GetAttributes(string path);

    /// <inheritdoc cref="SchwabenCode.QuickIO.QuickIOFile.GetAttributesAsync(string)" />
    Task<FileAttributes> GetAttributesAsync(string path);

    /// <inheritdoc cref="File.GetLastWriteTime(string)" />
    DateTime GetLastWriteTime(string path);

    /// <inheritdoc cref="SchwabenCode.QuickIO.QuickIOFile.GetLastWriteTimeAsync(string)" />
    Task<DateTime> GetLastWriteTimeAsync(string path);

    /// <inheritdoc cref="File.Move(string, string)" />
    void Move(string sourceFileName, string destFileName);

    /// <inheritdoc cref="SchwabenCode.QuickIO.QuickIOFile.MoveAsync(string, string)" />
    Task MoveAsync(string sourceFileName, string destFileName);

    /// <inheritdoc cref="File.Copy(string, string, bool)" />
    void Copy(string sourceFileName, string destinationFileName, bool overwrite);

    /// <inheritdoc cref="SchwabenCode.QuickIO.QuickIOFile.CopyAsync(string, string, bool)" />
    Task CopyAsync(string sourceFileName, string destinationFileName, bool overwrite);

    /// <inheritdoc cref="File.OpenRead(string)" />
    Stream OpenRead(string path);

    /// <inheritdoc cref="File.ReadAllBytes(string)" />
    byte[] ReadAllBytes(string path);

    /// <summary>Reads the contents of the file into a byte collection.</summary>
    /// <param name="path">Full path to the file (regular or UNC).</param>
    /// <returns>A byte collection containing the contents.</returns>
    Task<byte[]> ReadAllBytesAsync(string path);

    /// <inheritdoc cref="File.SetAttributes(string, FileAttributes)" />
    void SetAttributes(string path, FileAttributes fileAttributes);

    /// <inheritdoc cref="SchwabenCode.QuickIO.QuickIOFile.SetAttributesAsync(string, FileAttributes)" />
    Task SetAttributesAsync(string path, FileAttributes fileAttributes);
}
