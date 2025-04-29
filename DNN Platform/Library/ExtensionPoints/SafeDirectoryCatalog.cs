// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.ExtensionPoints;

using System;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.IO;
using System.Linq;
using System.Reflection;

public class SafeDirectoryCatalog : ComposablePartCatalog
{
    private readonly AggregateCatalog catalog;

    /// <summary>Initializes a new instance of the <see cref="SafeDirectoryCatalog"/> class.</summary>
    /// <param name="directory"></param>
    public SafeDirectoryCatalog(string directory)
    {
        var files = Directory.EnumerateFiles(directory, "*.dll", SearchOption.AllDirectories);

        this.catalog = new AggregateCatalog();

        foreach (var file in files)
        {
            try
            {
                var asmCat = new AssemblyCatalog(file);

                // Force MEF to load the plugin and figure out if there are any exports
                // good assemblies will not throw the RTLE exception and can be added to the catalog
                if (asmCat.Parts.ToList().Count > 0)
                {
                    this.catalog.Catalogs.Add(asmCat);
                }
            }
            catch (ReflectionTypeLoadException)
            {
            }
            catch (BadImageFormatException)
            {
            }
            catch (FileLoadException)
            {
                // ignore when the assembly load failed.
            }
        }
    }

    /// <inheritdoc/>
    public override IQueryable<ComposablePartDefinition> Parts
    {
        get
        {
            return this.catalog.Parts;
        }
    }
}
