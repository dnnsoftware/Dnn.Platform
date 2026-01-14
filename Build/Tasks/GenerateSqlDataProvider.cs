// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Build.Tasks
{
    using System;
    using System.IO;
    using System.Linq;

    using Cake.Frosting;

    /// <summary>A cake task to generate a SQL Data Provider script if it doesn't exist.</summary>
    [IsDependentOn(typeof(SetVersion))]
    public sealed class GenerateSqlDataProvider : FrostingTask<Context>
    {
        /// <inheritdoc/>
        public override void Run(Context context)
        {
            var fileName = context.GetTwoDigitsVersionNumber()[..8] + ".SqlDataProvider";
            var filePath = "./Dnn Platform/Website/Providers/DataProviders/SqlDataProvider/" + fileName;
            if (File.Exists(filePath))
            {
                context.SqlDataProviderExists = true;
                return;
            }

            context.SqlDataProviderExists = false;

            using (var file = new StreamWriter(filePath, true))
            {
                file.WriteLine("/************************************************************/");
                file.WriteLine("/*****              SqlDataProvider                     *****/");
                file.WriteLine("/*****                                                  *****/");
                file.WriteLine("/*****                                                  *****/");
                file.WriteLine("/***** Note: To manually execute this script you must   *****/");
                file.WriteLine("/*****       perform a search and replace operation     *****/");
                file.WriteLine("/*****       for {databaseOwner} and {objectQualifier}  *****/");
                file.WriteLine("/*****                                                  *****/");
                file.WriteLine("/************************************************************/");
            }
        }
    }
}
