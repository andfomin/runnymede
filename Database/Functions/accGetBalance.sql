

CREATE FUNCTION [dbo].[accGetBalance]
(
	@UserId int
)
RETURNS decimal(18,2)
AS
BEGIN

declare @Balance decimal(18,2) = null;

declare @AccountId int = dbo.accGetPersonalAccount(@UserId);

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
    ON OBJECT::[dbo].[accGetBalance] TO [websiterole]
    AS [dbo];

