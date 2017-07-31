#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2017
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
using System.Reflection;
using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.ComponentModel;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Tests.Utilities.Mocks;
using NUnit.Framework;

namespace DotNetNuke.Tests.Core
{
    /// <summary>
    ///   FileSystemUtilsTests
    /// </summary>
    [TestFixture]
    public class FileSystemUtilsTests
    {
        [SetUp]
        public void SetUp()
        {
            var field = typeof(Globals).GetField("_applicationMapPath", BindingFlags.Static | BindingFlags.NonPublic);
            field.SetValue(null, null);

            var rootPath = Path.Combine(Globals.ApplicationMapPath, "FileSystemUtilsTest");
            PrepareRootPath(rootPath);

            field.SetValue(null, rootPath);
        }


        [TestCase("/")]
        [TestCase("//")]
        [TestCase("///")]
        [TestCase("\\")]
        [TestCase("\\\\")]
        [TestCase("\\\\\\")]
        [TestCase("/Test/../")]
        [TestCase("/Test/mmm/../../")]
        [TestCase("\\Test\\..\\")]
        [TestCase("\\Test\\mmm\\..\\..\\")]
        [TestCase("\\Test\\")]
        [TestCase("..\\")]
        public void DeleteFiles_Should_Not_Able_To_Delete_Root_Folder(string path)
        {
            //Action
            FileSystemUtils.DeleteFiles(new string[] {path});

            var files = Directory.GetFiles(Globals.ApplicationMapPath, "*.*", SearchOption.AllDirectories);
            Assert.Greater(files.Length, 0);
        }

        private void PrepareRootPath(string rootPath)
        {
            if (!Directory.Exists(rootPath))
            {
                Directory.CreateDirectory(rootPath);
            }

            foreach (var file in Directory.GetFiles(Globals.ApplicationMapPath, "*.*", SearchOption.TopDirectoryOnly))
            {
                File.Copy(file, Path.Combine(rootPath, Path.GetFileName(file)), true);
            }
        }
    }
}