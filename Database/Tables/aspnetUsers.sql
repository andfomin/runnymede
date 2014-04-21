CREATE TABLE [dbo].[aspnetUsers] (
    [Id]                   INT            IDENTITY (1, 1) NOT NULL,
    [UserName]             NVARCHAR (200) NOT NULL,
    [PasswordHash]         NVARCHAR (MAX) NULL,
    [SecurityStamp]        NVARCHAR (MAX) NULL,
    [Email]                NVARCHAR (200) NULL,
    [EmailConfirmed]       BIT            CONSTRAINT [DF_aspnetUsers_IsConfirmed] DEFAULT ((0)) NOT NULL,
    [PhoneNumber]          NVARCHAR (100) NULL,
    [PhoneNumberConfirmed] BIT            CONSTRAINT [DF_aspnetUsers_PhoneNumberConfirmed] DEFAULT ((0)) NOT NULL,
    [TwoFactorEnabled]     BIT            CONSTRAINT [DF_aspnetUsers_TwoFactorEnabled] DEFAULT ((0)) NOT NULL,
    [LockoutEndDateUtc]    DATETIME2 (2)  NULL,
    [LockoutEnabled]       BIT            CONSTRAINT [DF_aspnetUsers_LockoutEnabled] DEFAULT ((0)) NOT NULL,
    [AccessFailedCount]    INT            CONSTRAINT [DF_aspnetUsers_AccessFailedCount] DEFAULT ((0)) NOT NULL,
    CONSTRAINT [PK_aspnetUsers] PRIMARY KEY CLUSTERED ([Id] ASC)
);














GO
GRANT UPDATE
    ON OBJECT::[dbo].[aspnetUsers] TO [websiterole]
    AS [dbo];


GO
GRANT SELECT
    ON OBJECT::[dbo].[aspnetUsers] TO [websiterole]
    AS [dbo];


GO
GRANT INSERT
    ON OBJECT::[dbo].[aspnetUsers] TO [websiterole]
    AS [dbo];


GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_UserName]
    ON [dbo].[aspnetUsers]([UserName] ASC);

