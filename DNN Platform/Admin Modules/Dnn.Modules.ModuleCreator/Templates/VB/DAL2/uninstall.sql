/*
   SQL Database Uninstall Script
   for _MODULE_ Module
   Copyright (c) _YEAR_
   by _OWNER_
*/

ALTER TABLE [dbo]._OWNER___MODULE_s DROP CONSTRAINT [FK__OWNER___MODULE_s_Modules]
GO

DROP TABLE [dbo]._OWNER___MODULE_s
GO
