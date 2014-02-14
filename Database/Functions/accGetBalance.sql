

CREATE FUNCTION [dbo].[accGetBalance]
(
	@UserId int
)
RETURNS decimal(18,2)
AS
BEGIN

declare @Balance decimal(18,2);

declare @AccountId int = dbo.accGetPersonalAccount(@UserId);

select @Balance = Balance
from dbo.accEntries
where Id = (select max(Id) from dbo.accEntries where AccountId = @AccountId)

return @Balance;

END
GO
GRANT EXECUTE
    ON OBJECT::[dbo].[accGetBalance] TO [websiterole]
    AS [dbo];

