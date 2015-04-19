



CREATE FUNCTION [dbo].[accGetReviewAccount]
(
	@UserId int
)
RETURNS int
AS
BEGIN

declare @AccountId int;

select @AccountId = Id 
from dbo.accAccounts
where UserId = @UserId and [Type] = 'ACREVW';

return @AccountId;

END