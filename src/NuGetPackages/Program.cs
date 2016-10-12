﻿#region Copyright
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2016
// by DotNetNuke Corporation
// All Rights Reserved
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
