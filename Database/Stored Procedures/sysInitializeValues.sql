CREATE PROCEDURE [dbo].[sysInitializeValues] 
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

-- Added 20150417
insert dbo.appValues ([Type], Start, Value) values ('PLDSCT', '1900-01-01 00:00:00', 
'<PricelistDiscounts>
	<Percent amountFrom="0" amountTo="19.99">0</Percent>
	<Percent amountFrom="20" amountTo="29.99">5</Percent>
	<Percent amountFrom="30" amountTo="9999.99">10</Percent>
</PricelistDiscounts>');
insert dbo.appValues ([Type], Start, Value) values ('SVRIW_', '1900-01-01 00:00:00', 
'<Price>12.00</Price>');
insert dbo.appValues ([Type], Start, Value) values ('SVRIS_', '1900-01-01 00:00:00', 
'<Price>11.00</Price>');
insert dbo.appValues ([Type], Start, Value) values ('SVRIW1', '1900-01-01 00:00:00', 
'<Price>8.00</Price>');
insert dbo.appValues ([Type], Start, Value) values ('SVRIW2', '1900-01-01 00:00:00', 
'<Price>11.00</Price>');

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