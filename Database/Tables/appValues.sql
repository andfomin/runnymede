CREATE TABLE [dbo].[appValues] (
    [Type]        CHAR (6)       NOT NULL,
    [Start]       SMALLDATETIME  NOT NULL,
    [End]         SMALLDATETIME  NULL,
    [PreviousEnd] SMALLDATETIME  NULL,
    [Value]       NVARCHAR (MAX) NULL,
    CONSTRAINT [PK_appValues] PRIMARY KEY CLUSTERED ([Type] ASC, [Start] ASC),
    CONSTRAINT [CK_appValues_Start_End] CHECK ([Start]<[End]),
    CONSTRAINT [CK_appValues_Start_PreviousEnd] CHECK ([Start]=[PreviousEnd] OR [PreviousEnd] IS NULL),
    CONSTRAINT [FK_appValues_appTypes] FOREIGN KEY ([Type]) REFERENCES [dbo].[appTypes] ([Id]),
    CONSTRAINT [FK_appValues_appValues] FOREIGN KEY ([Type], [PreviousEnd]) REFERENCES [dbo].[appValues] ([Type], [End]),
    CONSTRAINT [UC_appValues_Type_End] UNIQUE NONCLUSTERED ([Type] ASC, [End] ASC),
    CONSTRAINT [UC_appValues_Type_PreviousEnd] UNIQUE NONCLUSTERED ([Type] ASC, [PreviousEnd] ASC)
);

