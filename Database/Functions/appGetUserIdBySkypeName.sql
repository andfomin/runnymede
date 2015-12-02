



create FUNCTION [dbo].[appGetUserIdBySkypeName]
(
	@SkypeName nvarchar(100)
)
RETURNS int
AS
BEGIN

	declare @UserId int;

	select @UserId = max(Id)
	from dbo.appUsers 
	where SkypeName = @SkypeName
	having count(*) = 1 -- Avoid ambiguity if more than one user have the same Skype name.
	;

	return @UserId;

END
GO
GRANT EXECUTE
    ON OBJECT::[dbo].[appGetUserIdBySkypeName] TO [websiterole]
    AS [dbo];

