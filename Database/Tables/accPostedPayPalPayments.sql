CREATE TABLE [dbo].[accPostedPayPalPayments] (
    [ExtId] NVARCHAR (100) NOT NULL,
    CONSTRAINT [PK_PostedPayments] PRIMARY KEY CLUSTERED ([ExtId] ASC)
);




GO
GRANT SELECT
    ON OBJECT::[dbo].[accPostedPayPalPayments] TO [websiterole]
    AS [dbo];

