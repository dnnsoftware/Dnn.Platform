USE [{DBName}]
GO
IF NOT EXISTS (SELECT * FROM sys.database_principals p INNER JOIN sys.server_principals sp ON sp.sid=p.sid WHERE sp.[name]='{DBLogin}' AND sp.[type]='S')
BEGIN
 CREATE USER [DNN Connection] FOR LOGIN [{DBLogin}];
 EXEC sp_addrolemember N'db_owner', N'DNN Connection';
END
GO
