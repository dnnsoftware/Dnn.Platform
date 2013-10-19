#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2013
// by DotNetNuke Corporation
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation 
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and 
// to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions 
// of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
// TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.
#endregion
using System;
using System.IO;

using DotNetNuke.ComponentModel;

namespace DotNetNuke.Services.FileSystem.Internal
{
    public class FileWrapper : ComponentBase<IFile, FileWrapper>, IFile
    {
        public Stream Create(string path)
        {
            return File.Create(path);
        }

        public void Delete(string path)
        {
            File.Delete(path);
        }

        public bool Exists(string path)
        {
            return File.Exists(path);
        }

        public FileAttributes GetAttributes(string path)
        {
            return File.GetAttributes(path);
        }

        public DateTime GetLastWriteTime(string path)
        {
            return File.GetLastWriteTime(path);
        }

        public void Move(string sourceFileName, string destFileName)
        {
            File.Move(sourceFileName, destFileName);
        }

        public void Copy(string sourceFileName, string destinationFileName, bool overwrite)
        {
            File.Copy(sourceFileName, destinationFileName, overwrite);
        }

        public Stream OpenRead(string path)
        {
            return File.OpenRead(path);
        }

        public byte[] ReadAllBytes(string path)
        {
            return File.ReadAllBytes(path);
        }

        public void SetAttributes(string path, FileAttributes fileAttributes)
        {
            File.SetAttributes(path, fileAttributes);
        }
    }
}
