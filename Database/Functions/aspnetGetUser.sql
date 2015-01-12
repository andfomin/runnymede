


CREATE FUNCTION [dbo].[aspnetGetUser]
(
	@Id int
)
RETURNS TABLE 	
AS
RETURN 

	select Id, UserName, Email, EmailConfirmed, PhoneNumber, PhoneNumberConfirmed
	from dbo.aspnetUsers
	where Id = @Id
	;
GO
GRANT SELECT
    ON OBJECT::[dbo].[aspnetGetUser] TO [websiterole]
    AS [dbo];

