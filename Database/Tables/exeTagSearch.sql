CREATE TABLE [dbo].[exeTagSearch] (
    [Tag]   NVARCHAR (50)  NOT NULL,
    [Value] NVARCHAR (400) NOT NULL,
    CONSTRAINT [PK_exeTagSearch] PRIMARY KEY CLUSTERED ([Tag] ASC, [Value] ASC)
);




GO
GRANT SELECT
    ON OBJECT::[dbo].[exeTagSearch] TO [websiterole]
    AS [dbo];

