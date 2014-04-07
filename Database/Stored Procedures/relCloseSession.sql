

CREATE PROCEDURE [dbo].[relCloseSession]
	@EventId int
AS
BEGIN
/*
FirstUser - the user who offered and performed session. Who is payed.
SecondUser - the user who requested and consumed session. Who pays.
*/
SET NOCOUNT ON;

declare @ProcName sysname, @ExternalTran int, @XState int;
select  @ProcName = object_name(@@procid), @ExternalTran = @@trancount;

begin try
	if @ExternalTran > 0
		save transaction ProcedureSave;

	declare @Price decimal(18,2), @Id int;

	select @Id = Id, @Price = Price
	from dbo.relScheduleEvents 
	where Id = @EventId
		and [Type] = 'CFSN'
		and ClosingTime is null
		and [End] < sysutcdatetime();

	if @Id is null
		raiserror('%s,%d:: Session cannot be closed.', 16, 1, @ProcName, @EventId);

	if @ExternalTran = 0
		begin transaction;

		if (@Price is not null) begin

			exec dbo.accCloseSession @EventId;

		end
		else begin

			update dbo.relScheduleEvents			 
			set [Type] = 'PYSN', ClosingTime = sysutcdatetime()
			where Id = @EventId
				and [Type] = 'CFSN'
				and ClosingTime is null
				and [End] < sysutcdatetime();

			if @@rowcount = 0
				raiserror('%s,%d:: Failed to close session.', 16, 1, @ProcName, @EventId);

		end

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
    ON OBJECT::[dbo].[relCloseSession] TO [websiterole]
    AS [dbo];

