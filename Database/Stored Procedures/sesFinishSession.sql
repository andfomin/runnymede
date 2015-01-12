

CREATE PROCEDURE [dbo].[sesFinishSession]
	@SessionId int
AS
BEGIN
SET NOCOUNT ON;

declare @ProcName sysname, @ExternalTran int, @XState int;
select  @ProcName = object_name(@@procid), @ExternalTran = @@trancount;

begin try
	if @ExternalTran > 0
		save transaction ProcedureSave;

	declare @FinishDelay int, @Price decimal(9,2), @Now datetime2(2);

	-- Give the users opportunity to dispute the session before finish it.
	set @FinishDelay = dbo.appGetConstantAsInt('Sessions.FinishDelay.Minutes');

	set @Now = sysutcdatetime();

	select @Price = Price
	from dbo.sesSessions 
	where Id = @SessionId
		and FinishTime is null
		and DisputeTimeByHost is null
		and DisputeTimeByGuest is null
		and (datediff(minute, [End], @Now) >= @FinishDelay	or CancellationTime is not null);

	-- Price is not nullable.
	if @Price is null
		raiserror('%s,%d:: The session cannot be finished.', 16, 1, @ProcName, @SessionId);

	if (@Price > 0) begin

		exec dbo.accFinishSession @SessionId;

	end
	else begin

		if @ExternalTran = 0
			begin transaction;

			update dbo.sesSessions			 
			set FinishTime = @Now
			where Id = @SessionId
				and FinishTime is null
				and DisputeTimeByHost is null
				and DisputeTimeByGuest is null
				and (datediff(minute, [End], @Now) >= @FinishDelay	or CancellationTime is not null);

			if @@rowcount = 0
				raiserror('%s,%d:: Failed to finish session.', 16, 1, @ProcName, @SessionId);

		if @ExternalTran = 0
			commit;
	
	end

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
    ON OBJECT::[dbo].[sesFinishSession] TO [websiterole]
    AS [dbo];

