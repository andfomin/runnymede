
CREATE FUNCTION [dbo].[libGetResourcesToDeleteFromIndex]
(
	@MaxCount int
)
RETURNS TABLE 	
AS
RETURN 

	select top(@MaxCount) R.Id
	from dbo.libResources R
	where R.ReindexSearch = 1
		and R.IsCommon = 0;
GO
GRANT SELECT
    ON OBJECT::[dbo].[libGetResourcesToDeleteFromIndex] TO [websiterole]
    AS [dbo];

