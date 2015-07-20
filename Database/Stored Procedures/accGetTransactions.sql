

CREATE PROCEDURE [dbo].[accGetTransactions]
	@UserId int,
	@RowOffset int,
	@RowLimit int
AS
BEGIN
SET NOCOUNT ON;

	declare @AccountId int = dbo.accGetUserCashAccount(@UserId);

	declare @TotalCount int;

	select @TotalCount = count(*) 
	from dbo.accEntries 
	where AccountId = @AccountId;

	select T.ObservedTime, TT.Name as [Description], E.Debit, E.Credit, E.Balance
	from dbo.accEntries E
		inner join dbo.accTransactions T on E.TransactionId = T.Id
		inner join dbo.appTypes TT on T.[Type] = TT.Id
	where E.AccountId = @AccountId
	order by T.Id desc
	offset @RowOffset rows
		fetch next @RowLimit rows only;

	-- Do not return @TotalCount as a column in the main query, because the main query may return no rows for a non-existing account or for a big @RowOffset.
	select @TotalCount as TotalCount;

END
GO
GRANT EXECUTE
    ON OBJECT::[dbo].[accGetTransactions] TO [websiterole]
    AS [dbo];

