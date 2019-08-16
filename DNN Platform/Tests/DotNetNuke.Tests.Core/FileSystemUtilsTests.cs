#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2018
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
using System.Linq;
using System.Reflection;
using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.ComponentModel;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Tests.Utilities.Mocks;
using ICSharpCode.SharpZipLib.Zip;
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

        [Test]
        public void AddToZip_Should_Able_To_Add_Multiple_Files()
        {
            //Action
            DeleteZippedFiles();
            var zipFilePath = Path.Combine(Globals.ApplicationMapPath, $"Test{Guid.NewGuid().ToString().Substring(0, 8)}.zip");
            var files = Directory.GetFiles(Globals.ApplicationMapPath, "*.*", SearchOption.TopDirectoryOnly);
            using (var stream = File.Create(zipFilePath))
            {
                var zipStream = new ZipOutputStream(stream);
                zipStream.SetLevel(9);

                foreach (var file in files)
                {
                    var fileName = Path.GetFileName(file);
                    FileSystemUtils.AddToZip(ref zipStream, file, fileName, string.Empty);
                }

                zipStream.Finish();
                zipStream.Close();
            }

            //Assert
            var destPath = Path.Combine(Globals.ApplicationMapPath, Path.GetFileNameWithoutExtension(zipFilePath));
            if (!Directory.Exists(destPath))
            {
                Directory.CreateDirectory(destPath);
            }
            try
            {
                using (var stream = File.OpenRead(zipFilePath))
                {
                    var zipStream = new ZipInputStream(stream);
                    FileSystemUtils.UnzipResources(zipStream, destPath);
                    zipStream.Close();
                }

                var unZippedFiles = Directory.GetFiles(destPath, "*.*", SearchOption.TopDirectoryOnly);
                Assert.AreEqual(files.Length, unZippedFiles.Length);
            }
            finally
            {
                DeleteZippedFiles();
                DeleteUnzippedFolder(destPath);
            }
        }

        [Test]
        public void DeleteFile_Should_Delete_File()
        {
            //Action
            var testPath = Globals.ApplicationMapPath + $"/Test{Guid.NewGuid().ToString().Substring(0, 8)}.txt";
            using (StreamWriter sw = File.CreateText(testPath))
            {
                sw.WriteLine("48");
            }
            
            FileSystemUtils.DeleteFile(testPath);

            //Assert
            bool res = File.Exists(testPath.Replace("/", "\\"));
            Assert.IsFalse(res);
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase(" ")]
        [TestCase("/")]
        [TestCase("Test/Test ")]
        public void FixPath_Should_Change_Slashes_And_Trim(string input)
        {
            //Action
            var result = FileSystemUtils.FixPath(input);

            //Assert
            if (string.IsNullOrEmpty(input))
            {
                Assert.IsTrue(input == result);
            }
            else if(string.IsNullOrWhiteSpace(input))
            {
                Assert.IsTrue(result == string.Empty);
            }
            else
            {
                Assert.IsFalse(result.Contains(" "));
                Assert.IsFalse(result.Contains("/"));
            }

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

        private void DeleteZippedFiles()
        {
            var excludedFiles = Directory.GetFiles(Globals.ApplicationMapPath, "Test*", SearchOption.TopDirectoryOnly);
            foreach (var f in excludedFiles)
            {
                try
                {
                    File.Delete(f);
                }
                catch (Exception)
                {
                    //ignore
                }
            }
        }
        private void DeleteUnzippedFolder(string zippedFolder)
        {
            try
            {
                Directory.Delete(zippedFolder, true);
            }
            catch (Exception)
            {
                //ignore
            }
        }
    }
}