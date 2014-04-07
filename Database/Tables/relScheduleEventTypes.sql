CREATE TABLE [dbo].[relScheduleEventTypes] (
    [Id]          NCHAR (4)      NOT NULL,
    [Name]        NVARCHAR (100) NULL,
    [Description] NVARCHAR (100) NULL,
    CONSTRAINT [PK_relScheduleEventTypes] PRIMARY KEY CLUSTERED ([Id] ASC)
);






GO
GRANT SELECT
    ON OBJECT::[dbo].[relScheduleEventTypes] TO [websiterole]
    AS [dbo];

