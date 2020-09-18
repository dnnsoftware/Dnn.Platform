/*
   SQL Database Install Script
   for _MODULE_ Module
   Copyright (c) _YEAR_
   by _OWNER_
*/

CREATE TABLE [dbo]._OWNER___MODULE_s
(
    _MODULE_ID int IDENTITY(1,1) NOT NULL,
    ModuleID int NOT NULL,
    Title nvarchar(50) NOT NULL,
    Description nvarchar(max) NOT NULL,
    IsActive [bit] NOT NULL,
    CreatedOnDate [datetime] NOT NULL,
    CreatedByUserID [int] NOT NULL,
    CONSTRAINT [PK__OWNER___MODULE_s]
    PRIMARY KEY CLUSTERED ( [_MODULE_ID] ASC )
)


GO

ALTER TABLE [dbo]._OWNER___MODULE_s 
    ADD CONSTRAINT
	FK__OWNER___MODULE_s_Modules FOREIGN KEY
	(
	    ModuleID
	) REFERENCES dbo.Modules
	(
	    ModuleID
	) ON UPDATE NO ACTION 
	  ON DELETE CASCADE 
	  NOT FOR REPLICATION
GO
