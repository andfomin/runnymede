


CREATE FUNCTION [dbo].[libGetCategoryPaths]
(
	@Categories xml
)
RETURNS TABLE 	
AS
RETURN 

/*
declare @Categories xml = N'<Categories><C Id="0042d" /><C Id="0071b" /><C Id="0131_" /><C Id="0190_" /><C Id="0011_" /></Categories>';
*/

	--with MyCTE (Id, ParentId, [Level], NamePath, IdPath)
	--as
	--(
	--	select C.Id, C.ParentId, 0 as [Level], cast(C.Name as nvarchar(1000)) as NamePath, cast(Id as nvarchar(1000)) as IdPath
	--	from dbo.libCategories as C
	--		inner join @Categories.nodes('/Categories/C') T(C) on C.Id = T.C.value('@Id[1]', 'nchar(4)')
	--	union all
	--	select M.ID, C.ParentId, M.[Level] + 1, cast(C.Name + '||' + M.NamePath as nvarchar(1000)) as NamePath, cast((C.Id + ' ' + M.IdPath) as nvarchar(1000)) as IdPath
	--	from dbo.libCategories as C
	--		inner join MyCTE as M on C.Id = M.ParentId
	--)
	--select M.Id as CategoryId, M.NamePath, M.IdPath
	--from MyCTE M
	--	inner join (
	--		select Id, max([Level]) as [Level]
	--		from MyCTE
	--		group by Id
	--	) q on M.Id = q.Id and M.[Level] = q.[Level]
	--;

	select C.Id, C.NamePath, C.IdPath
	from dbo.libCategories as C
		inner join @Categories.nodes('/Categories/C') T(C) on C.Id = T.C.value('@Id[1]', 'nchar(4)');
GO
GRANT SELECT
    ON OBJECT::[dbo].[libGetCategoryPaths] TO [websiterole]
    AS [dbo];

