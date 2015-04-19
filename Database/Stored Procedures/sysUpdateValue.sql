


CREATE PROCEDURE [dbo].[sysUpdateValue]
	@Type char(6),
	@Start smalldatetime,
	@Value nvarchar(max)
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

		update dbo.appValues 
		set [End] = @Start 
		where [Type] =  @Type 
			and [End] is null;
		
		insert appValues ([Type], Start, PreviousEnd, Value) 
		values (@Type, @Start, @Start, @Value);

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