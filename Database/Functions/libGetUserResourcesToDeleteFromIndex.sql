



CREATE FUNCTION [dbo].[libGetUserResourcesToDeleteFromIndex]
(
)
RETURNS TABLE 	
AS
RETURN 

	select UserId, ResourceId as Id
	from dbo.libUserResources
	where IsPersonal = 0
		and ReindexSearch = 1;
GO
GRANT SELECT
    ON OBJECT::[dbo].[libGetUserResourcesToDeleteFromIndex] TO [websiterole]
    AS [dbo];

