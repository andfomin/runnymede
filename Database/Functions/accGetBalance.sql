

CREATE FUNCTION [dbo].[accGetBalance]
(
	@UserId int
)
RETURNS decimal(18,2)
AS
BEGIN

declare @Balance decimal(18,2);

declare @AccountId int = dbo.accGetPersonalAccount(@UserId);

select top(1) @Balance = Balance
from dbo.accEntries
where AccountId = @AccountId
order by Id desc;

return @Balance;

END
GO
GRANT EXECUTE
    ON OBJECT::[dbo].[accGetBalance] TO [websiterole]
    AS [dbo];

