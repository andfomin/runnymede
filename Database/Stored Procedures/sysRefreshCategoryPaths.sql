

CREATE PROCEDURE [dbo].[sysRefreshCategoryPaths]
AS
BEGIN
SET NOCOUNT ON;
/*
20141101 AF. Rewrites NamePath and IdPath for every row in the dbo.libCategories table. Call this SP manually after any manipulations with categories.
*/
declare @ProcName sysname, @ExternalTran int, @XState int;
select  @ProcName = object_name(@@procid), @ExternalTran = @@trancount;

begin try
	if @ExternalTran > 0
		save transaction ProcedureSave;

	if @ExternalTran = 0
		begin transaction;

		with MyCTE (Id, ParentId, [Level], NamePath, IdPath)
		as
		(
			select C.Id, C.ParentId, 0 as [Level], 
				cast(C.Name as nvarchar(500)) as NamePath, 
				cast(Id as nvarchar(50)) as IdPath
			from dbo.libCategories as C
			union all
			select M.ID, C.ParentId, M.[Level] + 1, 
				cast(C.Name + '||' + M.NamePath as nvarchar(500)) as NamePath, 
				cast((C.Id + ' ' + M.IdPath) as nvarchar(50)) as IdPath
			from dbo.libCategories as C
				inner join MyCTE as M on C.Id = M.ParentId
		)
		update C set NamePath = M.NamePath, IdPath = M.IdPath
		--select M.Id as CategoryId, M.NamePath, M.IdPath, C.*
		from dbo.libCategories as C
			inner join MyCTE M on C.Id = M.Id
			inner join (
				select Id, max([Level]) as [Level]
				from MyCTE
				group by Id
			) q on M.Id = q.Id and M.[Level] = q.[Level]

	if @ExternalTran = 0
		commit;
end try
begin catch
	set @XState = xact_state();
	if @XState = 1 and @ExternalTran > 0
		rollback transaction ProcedureSave;
	if @XState = 1 and @ExternalTran = 0
		rollback;
	if @XState = -1
		rollback;
	throw;
end catch

END