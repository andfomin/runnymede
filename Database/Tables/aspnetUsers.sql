CREATE TABLE [dbo].[aspnetUsers] (
    [Id]            INT            IDENTITY (1, 1) NOT NULL,
    [UserName]      NVARCHAR (200) NOT NULL,
    [PasswordHash]  NVARCHAR (MAX) NULL,
    [SecurityStamp] NVARCHAR (MAX) NULL,
    [Email]         NVARCHAR (200) NULL,
    [IsConfirmed]   BIT            CONSTRAINT [DF_aspnetUsers_IsConfirmed] DEFAULT ((0)) NOT NULL,
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

