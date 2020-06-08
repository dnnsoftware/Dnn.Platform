// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
using System.IO;
using System.Reflection;

namespace DNN.Integration.Test.Framework.Helpers
{
    public static class FileHelper
    {
        public static string GetAbsoluteDir(string relativePathIn)
        {
            if (!relativePathIn.StartsWith("\\")) relativePathIn = "\\" + relativePathIn;

            var rootDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            var pathOut = rootDirectory + relativePathIn;

            return pathOut;
        }
    }
}
