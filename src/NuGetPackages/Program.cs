﻿#region Copyright
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

﻿using System;
using System.Diagnostics;

namespace NuGetPackages
{
    class Program
    {
        static void Main()
        {
            Console.Clear();
            Console.WriteLine(@"

============================================================================

This application exists for the main purpose of referencing Nuget and other
external packages and copying them to a local folder so that we don't need
to change referencing the various packages in every dependent project; we
need only reference these from the copied location and which saves a lot
of build/compilation time besides TeamCity total build time.

============================================================================

All referenced libraries are copied to a relative folder (..\bin\) above this
project's folder. Forany new added reference to the solution we need to add it
first here (and don't forget to limit the allowed versions in packages.config
file) then reference the DLL from that project next. This requires to set the
NuGetPackages project as the first one in the solution's build order.

============================================================================

");

            if (Debugger.IsAttached)
            {
                Console.WriteLine();
                Console.WriteLine(@"Press any key to continue ... ");
                Console.ReadKey();
            }
        }
    }
}
