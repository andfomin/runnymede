


CREATE FUNCTION [dbo].[accGetUserCashAccount]
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
		and [Type] = 'ACUCSH';

	return @AccountId;

END