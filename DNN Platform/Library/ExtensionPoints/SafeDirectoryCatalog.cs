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

            // ASP.NET will load all the DLL files under the bin folder; the first loop would be sufficient
            var loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies()
                                            .Where(asm => asm.GetName().CodeBase.StartsWith("file://"));

            var processedFiles = new List<string>();
            foreach (var assembly in loadedAssemblies)
            {
                var fileUri = new Uri(assembly.GetName().CodeBase);
                var fi = new FileInfo(fileUri.AbsolutePath);
                if (fi.DirectoryName != null &&
                    fi.DirectoryName.StartsWith(directory, StringComparison.InvariantCultureIgnoreCase))
                {
                    try
                    {
                        processedFiles.Add(fi.FullName.ToUpper());
                        var asmCatalog = new AssemblyCatalog(assembly);
                        //Force MEF to load the plugin and figure out if there are any exports
                        // good assemblies will not throw the RTLE exception and can be added to the catalog
                        if (asmCatalog.Parts.ToList().Count > 0) _catalog.Catalogs.Add(asmCatalog);
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
            }

            // process whatever left in subfolders
            var files = Directory.EnumerateFiles(directory, "*.dll", SearchOption.AllDirectories)
                                 .Select(f => f.ToUpper()).Except(processedFiles);
            foreach (var file in files)
            {
                try
                {
                    var asmCatalog = new AssemblyCatalog(file);
                    //Force MEF to load the plugin and figure out if there are any exports
                    // good assemblies will not throw the RTLE exception and can be added to the catalog
                    if (asmCatalog.Parts.ToList().Count > 0) _catalog.Catalogs.Add(asmCatalog);
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
        }

        public override IQueryable<ComposablePartDefinition> Parts
        {
            get
            {
                return _catalog.Parts;
            }
        }
    }
}