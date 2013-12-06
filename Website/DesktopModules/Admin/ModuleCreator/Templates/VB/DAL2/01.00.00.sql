/*
   SQL Database Install Script
   for [MODULE] Module
   Copyright (c) [YEAR]
   by [OWNER]
*/

CREATE TABLE [dbo].[OWNER]_[MODULE]s
(
    [MODULE]ID int IDENTITY(1,1) NOT NULL,
    ModuleID int NOT NULL,
    Title nvarchar(50) NOT NULL,
    Description nvarchar(max) NOT NULL,
    IsActive [bit] NOT NULL,
    CreatedOnDate [datetime] NOT NULL,
    CreatedByUserID [int] NOT NULL,
    CONSTRAINT [PK_[OWNER]_[MODULE]s]
    PRIMARY KEY CLUSTERED ( [[MODULE]ID] ASC )
)


GO

ALTER TABLE [dbo].[OWNER]_[MODULE]s 
    ADD CONSTRAINT
	FK_[OWNER]_[MODULE]s_Modules FOREIGN KEY
	(
	    ModuleID
	) REFERENCES dbo.Modules
	(
	    ModuleID
	) ON UPDATE NO ACTION 
	  ON DELETE CASCADE 
	  NOT FOR REPLICATION
GO
