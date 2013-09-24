/*
   SQL Database Uninstall Script
   for [MODULE] Module
   Copyright (c) [YEAR]
   by [OWNER]
*/

ALTER TABLE [dbo].[OWNER]_[MODULE]s DROP CONSTRAINT [FK_[OWNER]_[MODULE]s_Modules]
GO

DROP TABLE [dbo].[OWNER]_[MODULE]s
GO
