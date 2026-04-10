// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Build.Tasks
{
    using Cake.Common.IO;
    using Cake.FileHelpers;
    using Cake.Frosting;

    /// <summary>A cake task to generate a SQL Data Provider script if it doesn't exist.</summary>
    [IsDependentOn(typeof(SetVersion))]
    public sealed class GenerateSqlDataProvider : FrostingTask<Context>
    {
        /// <inheritdoc />
        public override void Run(Context context)
        {
            var fileName = context.File($"{context.GetTwoDigitsVersionNumber()[..8]}.SqlDataProvider");
            var filePath = context.Directory("./Dnn Platform/Website/Providers/DataProviders/SqlDataProvider/") + fileName;
            if (context.FileExists(filePath))
            {
                context.SqlDataProviderExists = true;
                return;
            }

            context.SqlDataProviderExists = false;

            const string DefaultSqlFileContents =
                """
                /************************************************************/
                /*****              SqlDataProvider                     *****/
                /*****                                                  *****/
                /*****                                                  *****/
                /***** Note: To manually execute this script you must   *****/
                /*****       perform a search and replace operation     *****/
                /*****       for {databaseOwner} and {objectQualifier}  *****/
                /*****                                                  *****/
                /************************************************************/
                """;
            context.FileWriteText(filePath, DefaultSqlFileContents);
        }
    }
}
