
CREATE PROCEDURE [dbo].[accGetEntries]
	@UserId int,
	@RowOffset int,
	@RowLimit int
AS
BEGIN
SET NOCOUNT ON;

declare @AccountId int = dbo.accGetPersonalAccount(@UserId);

declare @TotalCount int;
select @TotalCount = count(*) from dbo.accEntries where AccountId = @AccountId;

select T.ObservedTime, TT.[Description], E.Debit, E.Credit, E.Balance
from dbo.accEntries E
	inner join dbo.accTransactions T on E.TransactionId = T.Id
	inner join dbo.accTransactionTypes TT on T.TransactionTypeId = TT.Id
where E.AccountId = @AccountId
order by T.Id desc
offset @RowOffset rows
	fetch next @RowLimit rows only;

-- Do not return @TotalCount as a column in the main query, because the main query may return no rows for big @RowOffset.
select @TotalCount as TotalCount;

END
GO
GRANT EXECUTE
    ON OBJECT::[dbo].[accGetEntries] TO [websiterole]
    AS [dbo];

