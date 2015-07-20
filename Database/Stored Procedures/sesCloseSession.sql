

CREATE PROCEDURE [dbo].[sesCloseSession]
	@SessionId int
AS
BEGIN
SET NOCOUNT ON;

declare @ProcName sysname, @ExternalTran int, @XState int;
select  @ProcName = object_name(@@procid), @ExternalTran = @@trancount;

begin try
	if @ExternalTran > 0
		save transaction ProcedureSave;

	declare @UserId int, @Price decimal(9,2), @Attribute nvarchar(100), @Now datetime2(2); 

	select @UserId = LearnerUserId, @Price = Price
	from dbo.sesSessions 
	where Id = @SessionId
		and ClosingTime is null
		and (
			-- Give the learner an opportunity to dispute the session before it is closed programmatically.
			datediff(minute, [End], sysutcdatetime()) >= dbo.appGetConstantAsInt('Sessions.ClosingDelay.Minutes') 
			or Rating is not null				
		);

	-- We assume that the Price of the session must be non-zero.
	if coalesce(@Price, 0) <= 0
		raiserror('%s,%d:: The session cannot be finished.', 16, 1, @ProcName, @SessionId);

	set @Attribute = cast(@SessionId as nvarchar(100));

	if @ExternalTran = 0
		begin transaction;

		exec dbo.accPostRevenue @UserId = @UserId, @Amount = @Price, @TransactionType = 'TRSSCL', @Attribute = @Attribute, @Now = @Now output;

		update dbo.sesSessions			 
			set ClosingTime = @Now
		where Id = @SessionId
			and ClosingTime is null;					

		if @@rowcount = 0
			raiserror('%s,%d:: Failed to finish session.', 16, 1, @ProcName, @SessionId);

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
    ON OBJECT::[dbo].[sesCloseSession] TO [websiterole]
    AS [dbo];

