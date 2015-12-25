﻿// DotNetNuke® - http://www.dnnsoftware.com
// Copyright (c) 2002-2015, DNN Corp.
// All Rights Reserved

using System.IO;
using System.Reflection;

namespace DotNetNuke.Tests.Integration.Framework.Helpers
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
