IF db_id('{DBName}') IS NOT NULL DROP DATABASE {DBName};
GO

CREATE DATABASE [{DBName}] ON PRIMARY
( NAME = N'{DBName}', FILENAME = N'{DBPath}\{DBName}.mdf')
 LOG ON 
( NAME = N'{DBName}_log', FILENAME = N'{DBPath}\{DBName}_log.ldf')
GO

EXEC dbo.sp_dbcmptlevel @dbname=N'{DBName}', @new_cmptlevel=100
GO
