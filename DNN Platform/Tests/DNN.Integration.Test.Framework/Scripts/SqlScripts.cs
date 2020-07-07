// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DNN.Integration.Test.Framework.Scripts
{
    public static class SqlScripts
    {
        public const string SingleUserCreation = @"
            DECLARE @UserName nvarchar(100)
            DECLARE @Password nvarchar(1000)
            DECLARE @Email nvarchar(100)
            DECLARE @PasswordSalt nvarchar(1000)
            DECLARE @PasswordFormat INT
            DECLARE @ApplicationId nvarchar(1000)
            DECLARE @PortalID INT
            DECLARE @RoleID INT
            DECLARE @AspUserID UNIQUEIDENTIFIER
            DECLARE @CurrentDate AS DATETIME
            DECLARE @FirstName nvarchar(100)
            DECLARE @LastName nvarchar(100)
            DECLARE @DisplayName nvarchar(100)
            DECLARE @UserID INT
            DECLARE @StartingUserID INT
            DECLARE @UsersToCreate INT
            DECLARE @UserSuffix nvarchar(25)
            DECLARE @UserToCopy nvarchar(100)
            DECLARE @PhotoPropertyDefinitionID INT
            DECLARE @FirstNamePropertyDefinitionID INT
            DECLARE @LastNamePropertyDefinitionID INT
            DECLARE @AvatarFileID INT
            DECLARE @SuperUser BIT
            DECLARE @CreateFriends BIT
            DECLARE @CreateFollowers BIT
            DECLARE @IsAdministrator BIT
            DECLARE @IsModerator BIT
            DECLARE @RoleName nvarchar(100)
            DECLARE @FriendsRelationShipId INT
            DECLARE @FollowersRelationShipId INT

            SET @PortalID = '$[portal_id]'
            SET @UserToCopy = '$[users_to_copy]'
            SET @FirstName = '$[first_name]'
            SET @LastName = '$[last_name]'
            SET @SuperUser = '$[isSuperUser]'
            SET @RoleName = '$[role_name]'
            SET @CreateFriends = '$[CreateFriends]'
            SET @CreateFollowers = '$[CreateFollowers]'

            SELECT  @Password = [Password], @PasswordSalt = PasswordSalt, @PasswordFormat = PasswordFormat, @ApplicationId = dbo.aspnet_Membership.ApplicationId FROM dbo.aspnet_Membership
            INNER JOIN dbo.aspnet_Users ON dbo.aspnet_Membership.UserId = dbo.aspnet_Users.UserId
            WHERE UserName = @UserToCopy

            SELECT  @PhotoPropertyDefinitionID = PropertyDefinitionID FROM dbo.{objectQualifier}ProfilePropertyDefinition WHERE PortalID = @PortalID AND PropertyName = 'Photo'
            SELECT  @FirstNamePropertyDefinitionID = PropertyDefinitionID FROM dbo.{objectQualifier}ProfilePropertyDefinition WHERE PortalID = @PortalID AND PropertyName = 'FirstName'
            SELECT  @LastNamePropertyDefinitionID = PropertyDefinitionID FROM dbo.{objectQualifier}ProfilePropertyDefinition WHERE PortalID = @PortalID AND PropertyName = 'LastName'

            SELECT  @FriendsRelationShipId = RelationshipID FROM dbo.{objectQualifier}Relationships WHERE PortalID = @PortalID AND [Name] = 'Friends'
            SELECT  @FollowersRelationShipId = RelationshipID FROM dbo.{objectQualifier}Relationships WHERE PortalID = @PortalID AND [Name] = 'Followers'

            IF @ApplicationId is NULL
	            BEGIN
		            RAISERROR ('User does not exist', 16, 1);
		            return
	            END

            SELECT  @RoleID = [RoleID] FROM dbo.{objectQualifier}Roles WHERE PortalID = @PortalID AND RoleName = 'Registered Users'

            SELECT @StartingUserID = max(userid) from dbo.{objectQualifier}Users --Find the last user id
            SET @StartingUserID = @StartingUserID + 1 --Start from the next

            BEGIN
	            SET @AspUserID = NEWID()
	            SET @UserName = @FirstName + '.' + @LastName
	            SET @Email = LOWER(@UserName + '@test.com')
	            SET @DisplayName = @FirstName + ' ' + @LastName
	            SET @CurrentDate = GETDATE()

	            IF EXISTS(SELECT * from dbo.{objectQualifier}Users where Username = @UserName)
	            BEGIN
		            return
	            END

	            INSERT INTO dbo.aspnet_Users
			            ( ApplicationId ,
			                UserId ,
			                UserName ,
			                LoweredUserName ,
			                MobileAlias ,
			                IsAnonymous ,
			                LastActivityDate
			            )
		            VALUES
			            ( @ApplicationId , -- ApplicationId - uniqueidentifier
			                @AspUserID , -- UserId - uniqueidentifier
			                @UserName , -- UserName - nvarchar(256)
			                @UserName , -- LoweredUserName - nvarchar(256)
			                NULL , -- MobileAlias - nvarchar(16)
			                0 , -- IsAnonymous - bit
			                @CurrentDate  -- LastActivityDate - datetime
			            )

	            INSERT INTO dbo.aspnet_Membership
			            ( ApplicationId ,
			                UserId ,
			                Password ,
			                PasswordFormat ,
			                PasswordSalt ,
			                MobilePIN ,
			                Email ,
			                LoweredEmail ,
			                PasswordQuestion ,
			                PasswordAnswer ,
			                IsApproved ,
			                IsLockedOut ,
			                CreateDate ,
			                LastLoginDate ,
			                LastPasswordChangedDate ,
			                LastLockoutDate ,
			                FailedPasswordAttemptCount ,
			                FailedPasswordAttemptWindowStart ,
			                FailedPasswordAnswerAttemptCount ,
			                FailedPasswordAnswerAttemptWindowStart ,
			                Comment
			            )
		            VALUES
			            ( @ApplicationId , -- ApplicationId - uniqueidentifier
			                @AspUserID , -- UserId - uniqueidentifier
			                @Password , -- Password - nvarchar(128)
			                @PasswordFormat , -- PasswordFormat - int
			                @PasswordSalt , -- PasswordSalt - nvarchar(128)
			                NULL , -- MobilePIN - nvarchar(16)
			                @Email , -- Email - nvarchar(256)
			                @Email , -- LoweredEmail - nvarchar(256)
			                NULL , -- PasswordQuestion - nvarchar(256)
			                NULL , -- PasswordAnswer - nvarchar(128)
			                1 , -- IsApproved - bit
			                0 , -- IsLockedOut - bit
			                @CurrentDate , -- CreateDate - datetime
			                @CurrentDate , -- LastLoginDate - datetime
			                @CurrentDate , -- LastPasswordChangedDate - datetime
			                '1754-01-01 00:00:00.000' , -- LastLockoutDate - datetime
			                0 , -- FailedPasswordAttemptCount - int
			                '1754-01-01 00:00:00.000' , -- FailedPasswordAttemptWindowStart - datetime
			                0 , -- FailedPasswordAnswerAttemptCount - int
			                '1754-01-01 00:00:00.000' , -- FailedPasswordAnswerAttemptWindowStart - datetime
			                NULL  -- Comment - ntext
			            )

	            INSERT INTO dbo.{objectQualifier}Users
			            ( Username ,
			                FirstName ,
			                LastName ,
			                IsSuperUser ,
			                AffiliateId ,
			                Email ,
			                DisplayName ,
			                UpdatePassword ,
			                LastIPAddress ,
			                IsDeleted ,
			                CreatedByUserID ,
			                CreatedOnDate ,
			                LastModifiedByUserID ,
			                LastModifiedOnDate
			            )
		            VALUES
			            ( @UserName , -- Username - nvarchar(100)
			                @FirstName , -- FirstName - nvarchar(50)
			                @LastName , -- LastName - nvarchar(50)
			                @SuperUser , -- IsSuperUser - bit
			                NULL , -- AffiliateId - int
			                @Email , -- Email - nvarchar(256)
			                @DisplayName , -- DisplayName - nvarchar(128)
			                0 , -- UpdatePassword - bit
			                N'' , -- LastIPAddress - nvarchar(50)
			                0 , -- IsDeleted - bit
			                0 , -- CreatedByUserID - int
			                @CurrentDate , -- CreatedOnDate - datetime
			                0 , -- LastModifiedByUserID - int
			                @CurrentDate  -- LastModifiedOnDate - datetime
			            )

	            SET @UserID = SCOPE_IDENTITY()

	            INSERT INTO dbo.{objectQualifier}UserRoles
			            ( UserID ,
			                RoleID ,
			                ExpiryDate ,
			                IsTrialUsed ,
			                EffectiveDate ,
			                CreatedByUserID ,
			                CreatedOnDate ,
			                LastModifiedByUserID ,
			                LastModifiedOnDate
			            )
		            VALUES
			            ( @UserID , -- UserID - int
			                @RoleID , -- RoleID - int
			                NULL , -- ExpiryDate - datetime
			                1 , -- IsTrialUsed - bit
			                NULL , -- EffectiveDate - datetime
			                0 , -- CreatedByUserID - int
			                @CurrentDate , -- CreatedOnDate - datetime
			                0 , -- LastModifiedByUserID - int
			                @CurrentDate  -- LastModifiedOnDate - datetime
			            )

	            INSERT INTO dbo.{objectQualifier}UserPortals
			            ( UserId ,
			                PortalId ,
			                CreatedDate ,
			                Authorised ,
			                IsDeleted ,
			                RefreshRoles
			            )
		            VALUES
			            ( @UserID , -- UserId - int
			                @PortalID , -- PortalId - int
			                @CurrentDate , -- CreatedDate - datetime
			                1 , -- Authorised - bit
			                0 , -- IsDeleted - bit
			                0  -- RefreshRoles - bit
			            )

	            --Create FirstName UserProfile Property
	            INSERT INTO dbo.{objectQualifier}UserProfile
			            ( UserID ,
			                PropertyDefinitionID ,
			                PropertyValue ,
			                PropertyText ,
			                Visibility ,
			                LastUpdatedDate ,
			                ExtendedVisibility
			            )
		            VALUES
			            ( @UserID , -- UserID - int
			                @FirstNamePropertyDefinitionID , -- PropertyDefinitionID - int
			                @FirstName , -- PropertyValue - nvarchar(3750)
			                NULL , -- PropertyText - nvarchar(max)
			                0 , -- Visibility - int
			                @CurrentDate , -- LastUpdatedDate - datetime
			                ''  -- ExtendedVisibility - varchar(400)
			            )

	            --Create LastName UserProfile Property
	            INSERT INTO dbo.{objectQualifier}UserProfile
			            ( UserID ,
			                PropertyDefinitionID ,
			                PropertyValue ,
			                PropertyText ,
			                Visibility ,
			                LastUpdatedDate ,
			                ExtendedVisibility
			            )
		            VALUES
			            ( @UserID , -- UserID - int
			                @LastNamePropertyDefinitionID , -- PropertyDefinitionID - int
			                @LastName , -- PropertyValue - nvarchar(3750)
			                NULL , -- PropertyText - nvarchar(max)
			                0 , -- Visibility - int
			                @CurrentDate , -- LastUpdatedDate - datetime
			                ''  -- ExtendedVisibility - varchar(400)
			            )

	            ;WITH Avatars
	                AS (
		            SELECT ROW_NUMBER() OVER(ORDER BY FileId)-1 AS RowNumber, fi.FileID
		            FROM dbo.{objectQualifier}Files fi INNER JOIN dbo.{objectQualifier}Folders fo ON fi.FolderID = fo.FolderID
		            WHERE fo.FolderPath like 'Users%'
		                AND fi.Extension in ('png', 'gif', 'jpg', 'jpeg')
		                AND fi.PortalId = @PortalID
		            )
	            SELECT @AvatarFileID = FileID
	            FROM Avatars
	            WHERE RowNumber = (@UserID % (SELECT COUNT(*) FROM Avatars))

	            IF @AvatarFileID IS NULL
	            BEGIN
		            ;WITH Avatars2
		                AS (
			            SELECT ROW_NUMBER() OVER(ORDER BY FileId)-1 AS RowNumber, FileID
			            FROM dbo.{objectQualifier}Files
			            WHERE Extension in ('png', 'gif', 'jpg', 'jpeg')
			                AND PortalId = @PortalID
			            )
		            SELECT @AvatarFileID = FileID
		            FROM Avatars2
		            WHERE RowNumber = (@UserID % (SELECT COUNT(*) FROM Avatars2))
	            END

	            --Create Photo UserProfile Property
	            INSERT INTO dbo.{objectQualifier}UserProfile
			            ( UserID ,
			                PropertyDefinitionID ,
			                PropertyValue ,
			                PropertyText ,
			                Visibility ,
			                LastUpdatedDate ,
			                ExtendedVisibility
			            )
		            VALUES
			            ( @UserID , -- UserID - int
			                @PhotoPropertyDefinitionID , -- PropertyDefinitionID - int
			                @AvatarFileID , -- PropertyValue - nvarchar(3750)
			                NULL , -- PropertyText - nvarchar(max)
			                0 , -- Visibility - int
			                @CurrentDate , -- LastUpdatedDate - datetime
			                ''  -- ExtendedVisibility - varchar(400)
			            )

                --Create Friends
                IF @CreateFriends > 0
                BEGIN
	                INSERT INTO dbo.{objectQualifier}UserRelationships
			                    ([UserID]
			                    ,[RelatedUserID]
			                    ,[RelationshipID]
			                    ,[Status]
			                    ,[CreatedByUserID]
			                    ,[CreatedOnDate]
			                    ,[LastModifiedByUserID]
			                    ,[LastModifiedOnDate])
		                SELECT DISTINCT TOP 10 @UserID, UserId, 1, 2, @UserID, @CurrentDate, @UserID, @CurrentDate
		                FROM dbo.{objectQualifier}Users
		                WHERE UserId <> @UserID
		                    AND IsSuperUser = 0
		                    AND (UserID NOT IN (SELECT RelatedUserID FROM dbo.{objectQualifier}UserRelationships WHERE UserID = @userID AND RelationshipID = @FriendsRelationshipId))

	                INSERT INTO dbo.{objectQualifier}UserRelationships
			                    ([UserID]
			                    ,[RelatedUserID]
			                    ,[RelationshipID]
			                    ,[Status]
			                    ,[CreatedByUserID]
			                    ,[CreatedOnDate]
			                    ,[LastModifiedByUserID]
			                    ,[LastModifiedOnDate])
		                SELECT DISTINCT TOP 15 @UserID, UserId, 1, 1, @UserID, @CurrentDate, @UserID, @CurrentDate
		                FROM dbo.{objectQualifier}Users
		                WHERE UserId <> @UserID
		                    AND IsSuperUser = 0
		                    AND (UserID NOT IN (SELECT RelatedUserID FROM dbo.{objectQualifier}UserRelationships WHERE UserID = @userID AND RelationshipID = @FriendsRelationshipId))
                END

	            --Create Followers
                IF @CreateFollowers > 0
                BEGIN
	                --Create Followers
	                INSERT INTO dbo.{objectQualifier}UserRelationships
			                    ([UserID]
			                    ,[RelatedUserID]
			                    ,[RelationshipID]
			                    ,[Status]
			                    ,[CreatedByUserID]
			                    ,[CreatedOnDate]
			                    ,[LastModifiedByUserID]
			                    ,[LastModifiedOnDate])
		                SELECT DISTINCT TOP 5 @UserID, UserId, 2, 2, @UserID, @CurrentDate, @UserID, @CurrentDate
		                FROM dbo.{objectQualifier}Users
		                WHERE UserId <> @UserID
		                    AND IsSuperUser = 0
		                    AND (UserID NOT IN (SELECT RelatedUserID FROM dbo.{objectQualifier}UserRelationships WHERE UserID = @userID AND RelationshipID = @FollowersRelationshipId))

	                --Create Followings
	                INSERT INTO dbo.{objectQualifier}UserRelationships
			                    ([UserID]
			                    ,[RelatedUserID]
			                    ,[RelationshipID]
			                    ,[Status]
			                    ,[CreatedByUserID]
			                    ,[CreatedOnDate]
			                    ,[LastModifiedByUserID]
			                    ,[LastModifiedOnDate])
		                SELECT DISTINCT TOP 6 UserId, @UserID, 2, 2, @UserID, @CurrentDate, @UserID, @CurrentDate
		                FROM dbo.{objectQualifier}Users
		                WHERE UserId <> @UserID
		                    AND IsSuperUser = 0
		                    AND (UserID NOT IN (SELECT RelatedUserID FROM dbo.{objectQualifier}UserRelationships WHERE [RelatedUserID] = @userID AND RelationshipID = @FollowersRelationshipId))
                END

	            IF DATALENGTH(@RoleName) > 0
	            BEGIN
		            DECLARE @ExistingRoleId INT
		            SELECT @ExistingRoleId = RoleId from dbo.{objectQualifier}Roles where RoleName = @RoleName

		            IF NOT EXISTS(SELECT UserRoleID from dbo.{objectQualifier}UserRoles where UserId = @UserID and RoleID = @ExistingRoleId)
		            BEGIN
			                EXEC dbo.{objectQualifier}AddUserRole
					                @PortalID = 0,
					                @UserID = @UserID,
					                @RoleId = @ExistingRoleId,
					                @Status = 1,
					                @IsOwner = 1,
					                @CreatedByUserID = 1
		            END
	            END
            END";

        public const string SingleRoleCreation = @"
            DECLARE @PortalID INT
            DECLARE @RoleName NVARCHAR(50)
            DECLARE @Description NVARCHAR(1000)
            DECLARE @CurrentDate DATETIME

            SET @PortalID = '$[portal_id]'
            SET @RoleName = '$[role_name]'
            SET @Description = '$[role_description]'
            SET @CurrentDate = GETDATE()

        IF EXISTS(SELECT * from [dbo].[{objectQualifier}Roles] where [RoleName] = @RoleName)
	        BEGIN
		        RETURN
	        END

            INSERT INTO [dbo].[{objectQualifier}Roles] 
            (
	        [PortalID],
	        [RoleName],
	        [Description],
	        [ServiceFee],
	        [BillingFrequency],
	        [TrialPeriod],
	        [TrialFrequency],
	        [BillingPeriod],
	        [TrialFee],
	        [IsPublic],
	        [AutoAssignment],
	        [RoleGroupID],
	        [RSVPCode],
	        [IconFile],
	        [CreatedByUserID],
	        [CreatedOnDate],
	        [LastModifiedByUserID],
	        [LastModifiedOnDate],
	        [Status],
	        [SecurityMode],
	        [IsSystemRole]
            )
            VALUES
            (
	        @PortalID,
	        @RoleName,
	        @Description,
	        0.00,
	        'N',
	        -1,
	        'N',
	        -1,
	        0.00,
	        0,
	        0,
	        NULL,
	        '',
	        '',
	        -1,
	        @CurrentDate,
	        -1,
	        @CurrentDate,
	        1,
	        0,
	        0
            )";

        public const string AssignRoleToUser = @"
            DECLARE @CurrentDate DATETIME
            DECLARE @UserId INT
            DECLARE @RoleId INT

            SET @UserId = '$[user_id]'
            SET @RoleId= '$[role_id]'
            SET @CurrentDate = GETDATE()

            DELETE FROM [dbo].[{objectQualifier}UserRoles]
            WHERE [UserID] = @UserId AND [RoleID] = @RoleId

            INSERT INTO [dbo].[{objectQualifier}UserRoles]
            (
	            [UserID],
	            [RoleID],
	            [ExpiryDate],
	            [IsTrialUsed],
	            [EffectiveDate],
	            [CreatedByUserID],
	            [CreatedOnDate],
	            [LastModifiedByUserID],
	            [LastModifiedOnDate],
	            [Status],
	            [IsOwner]
            )
            VALUES
            (
	            @UserId,
	            @RoleId,
	            NULL,
	            1,
	            NULL,
	            -1,
	            @CurrentDate,
	            -1,
	            @CurrentDate,
	            1,
	            0
            )";

        public const string UserDelete = @"
            DECLARE @userId INT
            DECLARE @username NVARCHAR(100)
            DECLARE @userGuid UNIQUEIDENTIFIER

            SET @userId = '$[user_id]'
            SELECT @username = [Username] FROM [dbo].[{objectQualifier}Users] WHERE [UserID] = @userId
            SELECT @userGuid = [UserId] FROM [dbo].[aspnet_Users] WHERE UserName = @username

            DELETE FROM [dbo].[{objectQualifier}Journal_Comments] WHERE [UserId] = @userId
            DELETE FROM [dbo].[{objectQualifier}Blogs_Entry] WHERE [BlogId] IN 
                (SELECT [BlogId] FROM [dbo].[{objectQualifier}Blogs_Blog] WHERE [UserId] = @userId)
            DELETE FROM [dbo].[{objectQualifier}Blogs_Blog] WHERE [UserId] = @userId

            DELETE FROM [dbo].[aspnet_Membership] WHERE [UserId] = @userGuid
            DELETE FROM [dbo].[aspnet_Users] WHERE [UserId] = @userGuid
            DELETE FROM [dbo].[{objectQualifier}UserRelationships] WHERE [UserID] = @userId OR [RelatedUserID] =  @userId
            DELETE FROM [dbo].[{objectQualifier}Users] WHERE [UserID] = @userId";

        public const string AddModulePermission = @"
            DECLARE @PortalId INT
            DECLARE @RoleId INT
            DECLARE @ModuleId INT
            DECLARE @PermissionCode NVARCHAR(50)
            DECLARE @PermissionKey NVARCHAR(50)

            DECLARE @ModuledDefId INT
            DECLARE @CurrentDate DATETIME
            DECLARE @PermissionId INT

            SET @PortalId = '$[portal_id]'
            SET @RoleId = '$[role_id]'
            SET @ModuleId = '$[module_id]'
            SET @PermissionCode = '$[permission_code]'
            SET @PermissionKey = '$[permission_key]'

            SET @CurrentDate = GETDATE()
            SELECT @ModuledDefId = ModuleDefId from [dbo].[{objectQualifier}Modules] WHERE [ModuleID] = @ModuleId

            SELECT @PermissionId = [PermissionID] FROM [dbo].[{objectQualifier}Permission]
            WHERE ModuleDefID = @ModuledDefId AND [PermissionCode] = @PermissionCode AND [PermissionKey] = @PermissionKey

            DELETE FROM [dbo].[{objectQualifier}ModulePermission]
            WHERE [ModuleID] = @ModuleId AND [PermissionID] = @PermissionId AND [RoleID] = @RoleId AND [UserID] IS NULL

            INSERT INTO [dbo].[{objectQualifier}ModulePermission]
            (
	            [ModuleID],
	            [PermissionID],
	            [AllowAccess],
	            [RoleID],
	            [UserID],
	            [CreatedByUserID],
	            [CreatedOnDate],
	            [LastModifiedByUserID],
	            [LastModifiedOnDate],
	            [PortalID]
            )
            VALUES
            (
	            @ModuleId,
	            @PermissionId,
	            1,
	            @RoleId,
	            NULL,
	            -1,
	            @CurrentDate,
	            -1,
	            @CurrentDate,
	            @PortalId
            )

            SELECT SCOPE_IDENTITY() AS ModulePermissionId";

        public const string PortalGetTimeZone = @"
            DECLARE @PortalID INT
            SET @PortalID = '$[portal_id]'

            SELECT SettingValue FROM [dbo].[{objectQualifier}PortalSettings]
            WHERE PortalID = @PortalID AND SettingName = 'TimeZone'";

        public const string Modulenformation = @"
            DECLARE @PortalID INT = 0
                DECLARE @friendlyName NVARCHAR(100) = 'Scheduler'
                         
                SELECT  dtm.DesktopModuleID, dtm.FriendlyName AS DtmFriendlyName,
                        mdfn.ModuleDefID, mdfn.FriendlyName AS ModuleFriendlyName, mdfn.DefinitionName,
                        mdl.ModuleID, mdl.PortalID,
                        tab.TabID, tab.TabName, tab.TabPath,
		                tm.TabModuleID
                FROM    dbo.{objectQualifier}DesktopModules dtm
			                INNER JOIN dbo.{objectQualifier}ModuleDefinitions mdfn on dtm.DesktopModuleID = mdfn.DesktopModuleID
			                INNER JOIN dbo.{objectQualifier}Modules mdl on mdfn.ModuleDefID = mdl.ModuleDefID
			                INNER JOIN dbo.{objectQualifier}TabModules tm on mdl.ModuleID = tm.ModuleID
			                INNER JOIN dbo.{objectQualifier}Tabs tab on tm.TabID = tab.TabID
                WHERE   (mdl.PortalID IS NULL OR mdl.PortalID = @PortalID) -- null for HOST modules
                AND   (dtm.FriendlyName = @friendlyName OR mdfn.FriendlyName = @friendlyName)
                ORDER BY dtm.FriendlyName, mdfn.FriendlyName, dtm.DesktopModuleID, mdfn.ModuleDefID, mdl.ModuleID;";

        public const string TableExist = @"
            IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.[{objectQualifier}$[table_name]]') AND type in (N'U'))
                SELECT 1
            ELSE
                SELECT 0
            ;";

        public static string UserGetPreferredTimeZone = @"
            DECLARE @UserId INT
            DECLARE @PortalID INT
            DECLARE @PropertyDefinitionId INT

            SET @UserId = '$[user_id]'
            SET @PortalID = '$[portal_id]'

            SELECT TOP 1 @PropertyDefinitionId = PropertyDefinitionID
            FROM [dbo].[{objectQualifier}ProfilePropertyDefinition]
            WHERE PortalID = @PortalID AND PropertyName = 'PreferredTimeZone' AND PropertyCategory = 'Preferences'

            SELECT PropertyValue FROM [dbo].[{objectQualifier}UserProfile]
            WHERE PropertyDefinitionID = @PropertyDefinitionId AND [UserId] = @UserId";
    }
}
