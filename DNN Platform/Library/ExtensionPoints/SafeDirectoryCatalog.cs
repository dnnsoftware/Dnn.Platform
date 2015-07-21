#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2014
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
using System.Collections.Generic;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.IO;
using System.Linq;
using System.Reflection;

namespace DotNetNuke.ExtensionPoints
{
    public class SafeDirectoryCatalog : ComposablePartCatalog
    {
        private readonly AggregateCatalog _catalog;

        public SafeDirectoryCatalog(string directory)
        {
            _catalog = new AggregateCatalog();
            var matchingAssemblyFiles = GetMatchingAssemblies(directory);

            foreach (var assembly in matchingAssemblyFiles.Select(f => f.Assembly))
            {
                TryAddCatalog(_catalog, assembly);
            }

            // process whatever left in folders - but the strange question is: why ASP.NET didn't load these? Are they loaded from the GAC?
            var otherAssemblyFiles = Directory.EnumerateFiles(directory, "*.dll", SearchOption.AllDirectories)
                                              .Except(matchingAssemblyFiles.Select(f => f.FullName), StringComparer.OrdinalIgnoreCase)
                                              .ToList();

            foreach (var assemblyPath in otherAssemblyFiles)
            {
                TryAddCatalog(_catalog, assemblyPath);
            }

            //UNDONE: We can add another enhancement as follows:
            //      - Enumerate the collected parts, select unique assemblies and flag them
            //      - Add all the assemblies list into a persstent storage
            //      - Use this list during the next startup time to minimize the time needed to go through all of these asseblies
            //      - Look for changed/new assemblies on the disk and repeat the process for whatever not in the persisted list
        }

        private static IList<MatchingAssemblyInfo> GetMatchingAssemblies(string directory)
        {
            // ASP.NET will load all the DLL files under the bin folder
            var matchingAssemblyFiles =
                from assembly in AppDomain.CurrentDomain.GetAssemblies()
                let codeBase = assembly.GetName().CodeBase
                where codeBase.StartsWith("file://")
                let fileUri = new Uri(codeBase)
                let fileInfo = new FileInfo(fileUri.AbsolutePath)
                where fileInfo.DirectoryName != null &&
                      fileInfo.DirectoryName.StartsWith(directory, StringComparison.OrdinalIgnoreCase)
                select new MatchingAssemblyInfo
                {
                    FullName = fileInfo.FullName,
                    Assembly = assembly,
                };

            return matchingAssemblyFiles.ToList();
        }

        private static void TryAddCatalog(AggregateCatalog catalog, Assembly assembly)
        {
            try
            {
                var asmCatalog = new AssemblyCatalog(assembly);
                //Force MEF to load the plugin and figure out if there are any exports
                // good assemblies will not throw the RTLE exception and can be added to the catalog
                if (asmCatalog.Parts.Any()) catalog.Catalogs.Add(asmCatalog);
            }
            catch (ReflectionTypeLoadException)
            {
            }
            catch (BadImageFormatException)
            {
            }
            catch (FileLoadException) //ignore when the assembly load failed.
            {
            }
        }

        private static void TryAddCatalog(AggregateCatalog catalog, string assemblyPath)
        {
            try
            {
                var asmCatalog = new AssemblyCatalog(assemblyPath);
                //Force MEF to load the plugin and figure out if there are any exports
                // good assemblies will not throw the RTLE exception and can be added to the catalog
                if (asmCatalog.Parts.Any()) catalog.Catalogs.Add(asmCatalog);
            }
            catch (ReflectionTypeLoadException)
            {
            }
            catch (BadImageFormatException)
            {
            }
            catch (FileLoadException) //ignore when the assembly load failed.
            {
            }
        }

        public override IQueryable<ComposablePartDefinition> Parts
        {
            get
            {
                return _catalog.Parts;
            }
        }

        class MatchingAssemblyInfo
        {
            public string FullName;
            public Assembly Assembly;
        }
    }
}