CREATE TABLE [dbo].[aspnetUserLogins] (
    [LoginProvider] NVARCHAR (128) NOT NULL,
    [ProviderKey]   NVARCHAR (128) NOT NULL,
    [UserId]        INT            NOT NULL,
    CONSTRAINT [PK_aspnetUserLogins] PRIMARY KEY CLUSTERED ([LoginProvider] ASC, [ProviderKey] ASC, [UserId] ASC),
    CONSTRAINT [FK_aspnetUserLogins_aspnetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [dbo].[aspnetUsers] ([Id]) ON DELETE CASCADE
);














GO
CREATE NONCLUSTERED INDEX [IX_UserId]
    ON [dbo].[aspnetUserLogins]([UserId] ASC);


GO



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

