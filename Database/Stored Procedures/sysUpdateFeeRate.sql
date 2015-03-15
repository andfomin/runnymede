


CREATE PROCEDURE [dbo].[sysUpdateFeeRate]
	@Type char(6),
	@Start smalldatetime,
	@FeeRates xml
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

		update dbo.appFeeRates set [End] = @Start where [Type] =  @Type and [End] is null;
		
		insert appFeeRates ([Type], Start, PreviousEnd, FeeRates) values (@Type, @Start, @Start, @FeeRates);

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