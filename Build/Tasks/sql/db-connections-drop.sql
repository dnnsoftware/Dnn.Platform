USE master

DECLARE @DatabaseName nvarchar(50)
SET @DatabaseName = N'{DBName}'

DECLARE @SQL varchar(max)
SET @SQL = ''

SELECT @SQL = @SQL + 'Kill ' + Convert(varchar, SPId) + ';'
FROM MASTER..SysProcesses
WHERE DBId = DB_ID(@DatabaseName) AND SPId <> @@SPId

EXEC(@SQL)
GO

