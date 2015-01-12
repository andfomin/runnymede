CREATE TABLE [dbo].[appFeeRates] (
    [Type]        CHAR (6)      NOT NULL,
    [Start]       SMALLDATETIME NOT NULL,
    [End]         SMALLDATETIME NULL,
    [PreviousEnd] SMALLDATETIME NULL,
    [FeeRates]    XML           NULL,
    CONSTRAINT [PK_appFees] PRIMARY KEY CLUSTERED ([Type] ASC, [Start] ASC),
    CONSTRAINT [CK_appFeeRates_Type] CHECK ([Type]='EXWRPH' OR [Type]='EXAREC' OR [Type]='TRSSFD' OR [Type]='TRIPFD'),
    CONSTRAINT [CK_appFees_Start_End] CHECK ([Start]<[End]),
    CONSTRAINT [CK_appFees_Start_PreviousEnd] CHECK ([Start]=[PreviousEnd] OR [PreviousEnd] IS NULL),
    CONSTRAINT [FK_appFeeRates_appTypes] FOREIGN KEY ([Type]) REFERENCES [dbo].[appTypes] ([Id]),
    CONSTRAINT [FK_appFees_appFees] FOREIGN KEY ([Type], [PreviousEnd]) REFERENCES [dbo].[appFeeRates] ([Type], [End]),
    CONSTRAINT [UC_appFees_Type_End] UNIQUE NONCLUSTERED ([Type] ASC, [End] ASC),
    CONSTRAINT [UC_appFees_Type_PreviousEnd] UNIQUE NONCLUSTERED ([Type] ASC, [PreviousEnd] ASC)
);





