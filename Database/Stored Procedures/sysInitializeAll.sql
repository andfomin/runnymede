

CREATE PROCEDURE [dbo].[sysInitializeAll]
AS
BEGIN

SET NOCOUNT ON;

declare @ExternalTran int, @ProcName sysname, @XState int;
select @ExternalTran = @@trancount, @ProcName = object_name(@@procid);
--raiserror('%s: ', 16, 1, @ProcName);

begin try
	if @ExternalTran > 0
		save transaction ProcedureSave;

	if @ExternalTran = 0
		begin transaction;

		execute dbo.sysInitializeTypes;
		execute dbo.sysInitializeConstants;

		execute dbo.sysInitializeSpecialUsers;
		--execute dbo.[InitializeTestUsers];

		execute dbo.sysInitializeLibCategories;
		execute dbo.sysRefreshCategoryPaths;
		execute dbo.sysInitializeLibExponents;
		execute dbo.sysInitializeLibTitles;
		execute dbo.sysInitializeLibDescriptions;
		execute dbo.sysInitializeLibResources;

		--GRANT CONNECT TO websiteuser;

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