CREATE TABLE [dbo].[lckRates] (
    [Date]     DATE           NOT NULL,
    [Position] TINYINT        NOT NULL,
    [Rate]     DECIMAL (9, 4) NOT NULL,
    CONSTRAINT [PK_lckRates_1] PRIMARY KEY CLUSTERED ([Date] ASC, [Position] ASC)
);



