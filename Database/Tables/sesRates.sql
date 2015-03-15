CREATE TABLE [dbo].[sesRates] (
    [Weekday] INT            NOT NULL,
    [Hour]    INT            NOT NULL,
    [Rate]    DECIMAL (9, 2) NOT NULL,
    CONSTRAINT [PK_sesPrices] PRIMARY KEY CLUSTERED ([Weekday] ASC, [Hour] ASC)
);

