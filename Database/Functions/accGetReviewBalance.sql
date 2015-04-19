


CREATE FUNCTION [dbo].[accGetReviewBalance]
(
	@UserId int
)
RETURNS decimal(18,2)
AS
BEGIN

declare @Balance decimal(18,2) = null;

declare @AccountId int = dbo.accGetReviewAccount(@UserId);

-- The user may not have an account created yet.
if (@AccountId is not null) begin

	select @Balance = Balance
	from dbo.accEntries
	where Id = (
		select max(Id) from dbo.accEntries where AccountId = @AccountId
	)

end

return @Balance;

END
GO
GRANT EXECUTE
    ON OBJECT::[dbo].[accGetReviewBalance] TO [websiterole]
    AS [dbo];

