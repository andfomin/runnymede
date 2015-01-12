


CREATE FUNCTION [dbo].[accGetPersonalAccount]
(
	@UserId int
)
RETURNS int
AS
BEGIN

declare @AccountId int;

select @AccountId = Id 
from dbo.accAccounts
where UserId = @UserId and [Type] = 'ACPERS';

return @AccountId;

END