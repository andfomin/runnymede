CREATE TABLE [dbo].[lckEntries] (
    [Date]      SMALLDATETIME   CONSTRAINT [DF_lckEnties_EntryTime] DEFAULT (sysutcdatetime()) NOT NULL,
    [Email]     NVARCHAR (200)  NOT NULL,
    [Digits]    NVARCHAR (2)    NULL,
    [UserId]    INT             NULL,
    [IpAddress] NVARCHAR (100)  NULL,
    [UserAgent] NVARCHAR (1000) NULL,
    [ExtId]     NVARCHAR (100)  NULL,
    CONSTRAINT [PK_lckEntries] PRIMARY KEY CLUSTERED ([Date] ASC, [Email] ASC)
);




GO
GRANT SELECT
    ON OBJECT::[dbo].[lckEntries] TO [websiterole]
    AS [dbo];


GO
GRANT INSERT
    ON OBJECT::[dbo].[lckEntries] TO [websiterole]
    AS [dbo];

