

CREATE FUNCTION [dbo].[accGetBalance]
(
	@UserId int
)
RETURNS decimal(18,2)
AS
BEGIN

declare @Balance decimal(18,2) = null;

declare @AccountId int = dbo.accGetUserCashAccount(@UserId);

-- The user may not have created an account yet.
if (@AccountId is not null) begin

	select @Balance = Balance
	from dbo.accEntries
	where Id = (
		select max(Id) from dbo.accEntries where AccountId = @AccountId
	)

end

return coalesce(@Balance, 0);

END
GO
GRANT EXECUTE
    ON OBJECT::[dbo].[accGetBalance] TO [websiterole]
    AS [dbo];

