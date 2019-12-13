// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System;
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
            var files = Directory.EnumerateFiles(directory, "*.dll", SearchOption.AllDirectories);

            _catalog = new AggregateCatalog();

            foreach (var file in files)
            {
                try
                {
	                var asmCat = new AssemblyCatalog(file);

	                //Force MEF to load the plugin and figure out if there are any exports
	                // good assemblies will not throw the RTLE exception and can be added to the catalog
	                if (asmCat.Parts.ToList().Count > 0) _catalog.Catalogs.Add(asmCat);
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
