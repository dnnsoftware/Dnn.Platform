// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Modules.Html.Components
{
    /// <summary>
    /// Provides helper methods for migrating HTML workflows and related database schema changes.
    /// </summary>
    public class MigrateHelper
    {
        /// <summary>
        /// Executes the migration of HTML workflows, including running the migration procedure,
        /// dropping obsolete tables and procedures, and adding required foreign keys.
        /// </summary>
        public static void MigrateHtmlWorkflows()
        {
            var db = Data.DataProvider.Instance();
            var databaseOwner = db.DatabaseOwner;
            var objectQualifier = db.ObjectQualifier;

            var localizationHelper = new LocalizationHelper();

            // "Published|veröffentlicht|Publicado|Publié|Pubblicato|Publiceren"
            var publishedLocalizations = string.Join("|", localizationHelper.StateLocalizations("DefaultWorkflowState3.StateName"));

            // "Draft|Entwurf|Borrador|Brouillon|Bozza|Concept"
            var draftLocalizations = string.Join("|", localizationHelper.StateLocalizations("DefaultWorkflowState1.StateName"));

            // 1. Execute the migration procedure
            db.ExecuteNonQuery("MigrateHtmlWorkflows", publishedLocalizations, draftLocalizations);

            // 2. Add FK_HtmlText_WorkflowStates if it does not exist
            db.ExecuteSQL($@"
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'FK_{objectQualifier}HtmlText_{objectQualifier}WorkflowStates') AND parent_object_id = OBJECT_ID(N'{databaseOwner}{objectQualifier}HtmlText'))
    ALTER TABLE {databaseOwner}{objectQualifier}HtmlText WITH NOCHECK ADD CONSTRAINT FK_{objectQualifier}HtmlText_{objectQualifier}WorkflowStates FOREIGN KEY (StateID) REFERENCES {databaseOwner}{objectQualifier}ContentWorkflowStates (StateID);
");

            // 3. Add FK_HtmlTextLog_WorkflowStates if it does not exist
            db.ExecuteSQL($@"
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'FK_{objectQualifier}HtmlTextLog_{objectQualifier}WorkflowStates') AND parent_object_id = OBJECT_ID(N'{databaseOwner}{objectQualifier}HtmlTextLog'))
    ALTER TABLE {databaseOwner}{objectQualifier}HtmlTextLog WITH NOCHECK ADD CONSTRAINT FK_{objectQualifier}HtmlTextLog_{objectQualifier}WorkflowStates FOREIGN KEY (StateID) REFERENCES {databaseOwner}{objectQualifier}ContentWorkflowStates (StateID);
");

            // 4. Enable HtmlText constraints after checking existing data
            db.ExecuteSQL($@"
ALTER TABLE {databaseOwner}{objectQualifier}HtmlText WITH CHECK CHECK CONSTRAINT FK_{objectQualifier}HtmlText_{objectQualifier}WorkflowStates;
");

            // 5. Enable HtmlTextLog constraints after checking existing data
            db.ExecuteSQL($@"
ALTER TABLE {databaseOwner}{objectQualifier}HtmlTextLog WITH CHECK CHECK CONSTRAINT FK_{objectQualifier}HtmlTextLog_{objectQualifier}WorkflowStates;
");

            // 6. Drop the migration procedure if it exists
            db.ExecuteSQL($@"
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'{databaseOwner}{objectQualifier}MigrateHtmlWorkflows') AND type in (N'P', N'PC'))
    DROP PROCEDURE {databaseOwner}{objectQualifier}MigrateHtmlWorkflows;
");

            // 7. Drop WorkflowStatePermission table if it exists
            db.ExecuteSQL($@"
IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = object_id(N'{databaseOwner}{objectQualifier}WorkflowStatePermission') AND OBJECTPROPERTY(id, N'IsTable') = 1)
    DROP TABLE {databaseOwner}{objectQualifier}WorkflowStatePermission;
");

            // 8. Drop WorkflowStates table if it exists
            db.ExecuteSQL($@"
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'{databaseOwner}{objectQualifier}WorkflowStates') AND type in (N'U'))
    DROP TABLE {databaseOwner}{objectQualifier}WorkflowStates;
");

            // 9. Drop Workflow table if it exists
            db.ExecuteSQL($@"
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'{databaseOwner}{objectQualifier}Workflow') AND type in (N'U'))
    DROP TABLE {databaseOwner}{objectQualifier}Workflow;
");
        }
    }
}
