CREATE ROLE [websiterole]
    AUTHORIZATION [dbo];


GO
EXECUTE sp_addrolemember @rolename = N'websiterole', @membername = N'websiteuser';

