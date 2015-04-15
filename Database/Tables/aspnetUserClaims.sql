CREATE TABLE [dbo].[aspnetUserClaims] (
    [Id]         INT            IDENTITY (1, 1) NOT NULL,
    [ClaimType]  NVARCHAR (MAX) NULL,
    [ClaimValue] NVARCHAR (MAX) NULL,
    [UserId]     INT            NOT NULL,
    CONSTRAINT [PK_AspNetUserClaims] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_AspNetUserClaims_AspNetUsers_User_Id] FOREIGN KEY ([UserId]) REFERENCES [dbo].[aspnetUsers] ([Id]) ON DELETE CASCADE
);
















GO
CREATE NONCLUSTERED INDEX [IX_User_Id]
    ON [dbo].[aspnetUserClaims]([UserId] ASC);




GO



GO
GRANT SELECT
    ON OBJECT::[dbo].[aspnetUserClaims] TO [websiterole]
    AS [dbo];


GO



GO


