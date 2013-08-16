DotNetNuke

For more details please see the DotNetNuke Installation Guide (downloadable from dotnetnuke.com) and http://www.dotnetnuke.com/Resources/Blogs/EntryId/3418/Announcing-DotNetNuke-7.aspx for an explanantion of minimum requirements.

Minimum Requirements
-------------
Windows 2008
SQL Server 2008
ASP.NET 4.0 (or higher) to be installed.
IIS 7.0 (Integrated Pipeline Only)


Clean Installation
------------------
- ASP.NET Framework 4.0 must be installed

- unzip package into C:\DotNetNuke (note: the install package is the most commonly used, the source package is intended for those who wish to make core changes or debug through the source)

- the website user account must have Read, Write, and Change Control of the root website directory and subdirectories ( this allows the application to create files/folders and update it¡¯s config files), This account is different depending on the version of the OS/webserver, please see the list below for the correct account
 - If using Windows 2008 R2 (IIS 7.5) or Windows 7 or Windows Server 2012 8 (IIS7.5, IIS8.0) this is the IIS AppPool\DefaultAppPool User Account.

- create Virtual Directory in IIS called DotNetNuke which points to the directory where the DotNetNuke.webproj file exists )

- make sure you have default.aspx specified as a Default Document in IIS

- The install version needs no other changes, but if you use the source version rename release.config -> web.config

- Note: SQL Server 2008, 2012 / SQL Express 2008, 2012 all supported.
  - manually create SQL Server database named "DotNetNuke" ( using Enterprise Manager or your tool of choice )
  - make sure you grant sufficient database permissions (db_owner is commonly used)
  - set the connection string in web.config in TWO places ( ie. <add key="SiteSqlServer" value="Server=(local);Database=DotNetNuke;uid=;pwd=;" /> )

- browse to localhost/DotNetNuke_Community (or whatever you have used for your url) in your web browser

- the application will automatically execute the necessary database scripts and provide feedback in the browser

Application Upgrades
--------------------
- make sure you always backup your files/database before upgrading to a new version

- BACKUP your web.config file in the root of your site

- unzip the code over top of your existing application ( using the Overwrite and Use Folder Names options )

- browse to localhost/DotNetNuke_Community (or whatever you have used for your url) in your web browser

- the application will automatically execute the necessary database scripts and provide feedback in the browser


Deprecated functions
--------------------
The following items have been deprecated and are no longer supported and will not be further developed.

- The BroadcastPollingCachingProvider (June 23, 2009) was deprecated due to known performance issues. The recommended replacements are the FileBasedCachingProvider or WebRequestCachingProvider (PE only)
- The XMLLoggingProvider (June 23, 2009)  was deprecated due to known performance problems. The recommended replacement is the DBLoggingProvider (which has been the default since 3.3)

Note: SQL 2000/MSDE 2000 support was dropped with the release of 5.2 (November 2009)

Marketshare and Adsense modules were deprecated and removed from new installs with the 5.3.0 release (Mar 17th 2010)