CREATE TABLE [dbo].[aspnetUserLogins] (
    [UserId]        INT            NOT NULL,
    [LoginProvider] NVARCHAR (128) NOT NULL,
    [ProviderKey]   NVARCHAR (128) NOT NULL,
    CONSTRAINT [PK_AspNetUserLogins] PRIMARY KEY CLUSTERED ([UserId] ASC, [LoginProvider] ASC, [ProviderKey] ASC),
    CONSTRAINT [FK_AspNetUserLogins_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [dbo].[aspnetUsers] ([Id]) ON DELETE CASCADE
);












GO
CREATE NONCLUSTERED INDEX [IX_UserId]
    ON [dbo].[aspnetUserLogins]([UserId] ASC);


GO
GRANT UPDATE
    ON OBJECT::[dbo].[aspnetUserLogins] TO [websiterole]
    AS [dbo];


GO
GRANT SELECT
    ON OBJECT::[dbo].[aspnetUserLogins] TO [websiterole]
    AS [dbo];


GO
GRANT INSERT
    ON OBJECT::[dbo].[aspnetUserLogins] TO [websiterole]
    AS [dbo];


GO
GRANT DELETE
    ON OBJECT::[dbo].[aspnetUserLogins] TO [websiterole]
    AS [dbo];

