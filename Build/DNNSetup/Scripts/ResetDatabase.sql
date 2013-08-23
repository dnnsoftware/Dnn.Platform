-- kills all the users in a particular database
-- dlhatheway/3M, 11-Jun-2000
declare @arg_dbname sysname
declare @a_spid smallint
declare @msg    varchar(255)
declare @a_dbid int

set @arg_dbname = '$(DatabaseName)'

select
        @a_dbid = sdb.dbid
from    master..sysdatabases sdb
where   sdb.name = @arg_dbname

declare db_users insensitive cursor for
select
        sp.spid
from    master..sysprocesses sp
where   sp.dbid = @a_dbid

open db_users

fetch next from db_users into @a_spid
while @@fetch_status = 0
        begin
        select @msg = 'kill '+convert(char(5),@a_spid)
        print @msg
        execute (@msg)
        fetch next from db_users into @a_spid
        end

close db_users
deallocate db_users
GO


IF EXISTS(SELECT 1 FROM [master].[sys].[databases] WHERE [name] =  N'$(DatabaseName)')  
BEGIN 
    drop database $(DatabaseName)
END;
go

IF EXISTS(SELECT 1 FROM [master].[sys].[databases] WHERE [name] = 'zzTempDBForDefaultPath')  
BEGIN 
    DROP DATABASE zzTempDBForDefaultPath  
END;

-- Create temp database. Because no options are given, the default data and --- log path locations are used
CREATE DATABASE zzTempDBForDefaultPath;

DECLARE @Default_Data_Path VARCHAR(512),  
        @Default_Log_Path VARCHAR(512);

--Get the default data path  
SELECT @Default_Data_Path =   
(   SELECT LEFT(physical_name,LEN(physical_name)-CHARINDEX('\',REVERSE(physical_name))+1)
    FROM sys.master_files mf  
    INNER JOIN sys.[databases] d  
    ON mf.[database_id] = d.[database_id]  
    WHERE d.[name] = 'zzTempDBForDefaultPath' AND type = 0);

--Get the default Log path  
SELECT @Default_Log_Path =   
(   SELECT LEFT(physical_name,LEN(physical_name)-CHARINDEX('\',REVERSE(physical_name))+1)  
    FROM sys.master_files mf  
    INNER JOIN sys.[databases] d  
    ON mf.[database_id] = d.[database_id]  
    WHERE d.[name] = 'zzTempDBForDefaultPath' AND type = 1);

--Clean up. 
IF EXISTS(SELECT 1 FROM [master].[sys].[databases] WHERE [name] = 'zzTempDBForDefaultPath')  
BEGIN 
    DROP DATABASE zzTempDBForDefaultPath  
END;

DECLARE @SQL nvarchar(max)

SET @SQL=
'CREATE DATABASE $(DatabaseName) ON  PRIMARY 
( NAME = N''$(DatabaseName)'', FILENAME = N''' + @Default_Data_Path + N'$(DatabaseName)' + '.mdf' + ''', SIZE = 79872KB , MAXSIZE = UNLIMITED, FILEGROWTH = 1024KB )
 LOG ON 
( NAME = N''$(DatabaseName)Log'', FILENAME = N''' + @Default_Log_Path + N'$(DatabaseName)' + '.ldf' + ''', SIZE = 84416KB , MAXSIZE = 2048GB , FILEGROWTH = 10%)
'

exec (@SQL)
GO

USE $(DatabaseName)
GO


CREATE USER [NT AUTHORITY\NETWORK SERVICE] FOR LOGIN [NT AUTHORITY\NETWORK SERVICE] WITH DEFAULT_SCHEMA=[db_owner]
GO

EXEC sp_addrolemember 'db_owner', 'NT AUTHORITY\NETWORK SERVICE'
GO


ALTER DATABASE $(DatabaseName) SET COMPATIBILITY_LEVEL = 100
GO

IF (1 = FULLTEXTSERVICEPROPERTY('IsFullTextInstalled'))
begin
EXEC $(DatabaseName).[dbo].[sp_fulltext_database] @action = 'enable'
end
GO

ALTER DATABASE $(DatabaseName) SET ANSI_NULL_DEFAULT OFF 
GO

ALTER DATABASE $(DatabaseName) SET ANSI_NULLS OFF 
GO

ALTER DATABASE $(DatabaseName) SET ANSI_PADDING OFF 
GO

ALTER DATABASE $(DatabaseName) SET ANSI_WARNINGS OFF 
GO

ALTER DATABASE $(DatabaseName) SET ARITHABORT OFF 
GO

ALTER DATABASE $(DatabaseName) SET AUTO_CLOSE OFF 
GO

ALTER DATABASE $(DatabaseName) SET AUTO_CREATE_STATISTICS ON 
GO

ALTER DATABASE $(DatabaseName) SET AUTO_SHRINK OFF 
GO

ALTER DATABASE $(DatabaseName) SET AUTO_UPDATE_STATISTICS ON 
GO

ALTER DATABASE $(DatabaseName) SET CURSOR_CLOSE_ON_COMMIT OFF 
GO

ALTER DATABASE $(DatabaseName) SET CURSOR_DEFAULT  GLOBAL 
GO

ALTER DATABASE $(DatabaseName) SET CONCAT_NULL_YIELDS_NULL OFF 
GO

ALTER DATABASE $(DatabaseName) SET NUMERIC_ROUNDABORT OFF 
GO

ALTER DATABASE $(DatabaseName) SET QUOTED_IDENTIFIER OFF 
GO

ALTER DATABASE $(DatabaseName) SET RECURSIVE_TRIGGERS OFF 
GO

ALTER DATABASE $(DatabaseName) SET  DISABLE_BROKER 
GO

ALTER DATABASE $(DatabaseName) SET AUTO_UPDATE_STATISTICS_ASYNC OFF 
GO

ALTER DATABASE $(DatabaseName) SET DATE_CORRELATION_OPTIMIZATION OFF 
GO

ALTER DATABASE $(DatabaseName) SET TRUSTWORTHY OFF 
GO

ALTER DATABASE $(DatabaseName) SET ALLOW_SNAPSHOT_ISOLATION OFF 
GO

ALTER DATABASE $(DatabaseName) SET PARAMETERIZATION SIMPLE 
GO

ALTER DATABASE $(DatabaseName) SET READ_COMMITTED_SNAPSHOT OFF 
GO

ALTER DATABASE $(DatabaseName) SET HONOR_BROKER_PRIORITY OFF 
GO

ALTER DATABASE $(DatabaseName) SET  READ_WRITE 
GO

ALTER DATABASE $(DatabaseName) SET RECOVERY FULL 
GO

ALTER DATABASE $(DatabaseName) SET  MULTI_USER 
GO

ALTER DATABASE $(DatabaseName) SET PAGE_VERIFY CHECKSUM  
GO

ALTER DATABASE $(DatabaseName) SET DB_CHAINING OFF 
GO

