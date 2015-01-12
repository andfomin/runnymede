

CREATE PROCEDURE [dbo].[libResetIndexedResources]
	@Resources xml
AS
BEGIN
SET NOCOUNT ON;

declare @ProcName sysname, @ExternalTran int, @XState int;
select  @ProcName = object_name(@@procid), @ExternalTran = @@trancount;
--raiserror('%s,%d::', 16, 1, @ProcName, @);

begin try
	if @ExternalTran > 0
		save transaction ProcedureSave;

	if @ExternalTran = 0
		begin transaction;

		update R set 
			IndexedLanguageLevel = T.C.value('@Level[1]', 'tinyint'),
			IndexedRating = T.C.value('@Rating[1]', 'tinyint'),
			ReindexSearch = 0
		from dbo.libResources R
			inner join @Resources.nodes('/Resources/R') T(C) on R.Id = T.C.value('@Id[1]', 'int');

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
GO
GRANT EXECUTE
    ON OBJECT::[dbo].[libResetIndexedResources] TO [websiterole]
    AS [dbo];

