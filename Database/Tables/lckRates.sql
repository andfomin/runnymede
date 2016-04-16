CREATE TABLE [dbo].[lckRates] (
    [Date]     DATE          NOT NULL,
    [Position] TINYINT       NOT NULL,
    [Active]   BIT           CONSTRAINT [DF_lckRates_Active] DEFAULT ((1)) NOT NULL,
    [Rate]     NVARCHAR (20) NOT NULL,
    [Currency] NVARCHAR (50) NULL,
    CONSTRAINT [PK_lckRates_1] PRIMARY KEY CLUSTERED ([Date] ASC, [Position] ASC)
);










GO
GRANT INSERT
    ON OBJECT::[dbo].[lckRates] TO [websiterole]
    AS [dbo];

