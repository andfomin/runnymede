CREATE TABLE [dbo].[aspnetRoles] (
    [Id]   NVARCHAR (128) NOT NULL,
    [Name] NVARCHAR (128) NOT NULL,
    CONSTRAINT [PK_AspNetRoles] PRIMARY KEY CLUSTERED ([Id] ASC)
);




GO
GRANT SELECT
    ON OBJECT::[dbo].[aspnetRoles] TO [websiterole]
    AS [dbo];

