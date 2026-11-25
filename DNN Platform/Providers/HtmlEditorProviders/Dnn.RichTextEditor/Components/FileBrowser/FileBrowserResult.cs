// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace Dnn.RichTextEditorProvider.Components.FileBrowser
{
    /// <summary>The outcome of a file browser operation.</summary>
    /// <typeparam name="T">The type of the value produced on success.</typeparam>
    public sealed class FileBrowserResult<T>
    {
        private FileBrowserResult(T value, FileBrowserError error)
        {
            this.Value = value;
            this.Error = error;
        }

        /// <summary>Gets the value produced by a successful operation.</summary>
        public T Value { get; }

        /// <summary>Gets the reason the operation failed, or <see cref="FileBrowserError.None"/>.</summary>
        public FileBrowserError Error { get; }

        /// <summary>Gets a value indicating whether the operation succeeded.</summary>
        public bool Succeeded => this.Error == FileBrowserError.None;

        /// <summary>Creates a successful result.</summary>
        /// <param name="value">The value produced.</param>
        /// <returns>The result.</returns>
        public static FileBrowserResult<T> Success(T value) => new FileBrowserResult<T>(value, FileBrowserError.None);

        /// <summary>Creates a failed result.</summary>
        /// <param name="error">The reason for the failure.</param>
        /// <returns>The result.</returns>
        public static FileBrowserResult<T> Failure(FileBrowserError error) => new FileBrowserResult<T>(default, error);
    }
}
