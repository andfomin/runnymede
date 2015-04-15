


CREATE FUNCTION [dbo].[libGetUserResources]
(
	@UserId int,
	@Resources xml
)
RETURNS TABLE 	
AS
RETURN 

	select UR.ResourceId as Id, UR.IsPersonal, UR.LanguageLevelRating, UR.Comment
	from dbo.libUserResources UR
		inner join @Resources.nodes('/Resources/R') T(C) on UR.ResourceId = T.C.value('@Id[1]', 'int')
	where UR.UserId = @UserId;
GO
GRANT SELECT
    ON OBJECT::[dbo].[libGetUserResources] TO [websiterole]
    AS [dbo];

