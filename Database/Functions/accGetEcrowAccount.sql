


CREATE FUNCTION [dbo].[accGetEcrowAccount]
(
	@UserId int
)
RETURNS int
AS
BEGIN

declare @AccountId int;

select @AccountId = Id 
from dbo.accAccounts
where UserId = @UserId 
	and [Type] = 'ACESCR';

return @AccountId;

END