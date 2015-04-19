CREATE TABLE [dbo].[exeCards] (
    [Id]    INT            IDENTITY (1, 1) NOT NULL,
    [Type]  CHAR (6)       NOT NULL,
    [Title] NVARCHAR (100) NULL,
    CONSTRAINT [PK_exeCards] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_exeCards_appTypes] FOREIGN KEY ([Type]) REFERENCES [dbo].[appTypes] ([Id])
);


GO
GRANT SELECT
    ON OBJECT::[dbo].[exeCards] TO [websiterole]
    AS [dbo];

