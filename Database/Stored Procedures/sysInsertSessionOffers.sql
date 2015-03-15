


CREATE PROCEDURE [dbo].[sysInsertSessionOffers]
	@Start smalldatetime,
	@End smalldatetime,
	@ProposedTeacherUserId int = null,
	@Cost decimal(9,2) = null
AS
BEGIN
/*
AF. 20150225
This operation is not idempotent.
*/
SET NOCOUNT ON;

declare @ProcName sysname, @ExternalTran int, @XState int;
select  @ProcName = object_name(@@procid), @ExternalTran = @@trancount;

begin try
	if @ExternalTran > 0
		save transaction ProcedureSave;	

	declare @SessionLength int =  dbo.appGetConstantAsInt('Sessions.Length.Minutes'); -- 12 minutes
	declare @SessionSlot int =  dbo.appGetConstantAsInt('Sessions.SlotLength.Minutes'); -- 15 minutes

	declare @OldId int, @NewStart smalldatetime, @NewEnd smalldatetime, @SkypeName nvarchar(100), @IsTeacher bit, @SessionCost decimal(9,2);

	if @Start < dateadd(minute, -1, sysutcdatetime())
		raiserror('%s:: The start of the offered period must not be in the past.', 16, 1, @ProcName);

	if (datediff(minute, @Start, @End) < @SessionLength)
		raiserror('%s:: The interval length is too short.', 16, 1, @ProcName);

	if (
		datepart(minute, @Start) not in (0, @SessionSlot, @SessionSlot * 2, @SessionSlot * 3) or 
		datepart(minute, @End) not in (0, @SessionSlot, @SessionSlot * 2, @SessionSlot * 3)
	)
		raiserror('%s:: Sessions must be aligned to half-hour boundaries.', 16, 1, @ProcName);

	if @ExternalTran = 0
		begin transaction;

		-- Shred the interval into slots.
		with MyCte as
		(
			select @Start as Start
			union all
			select dateadd(minute, @SessionSlot, Start)
			from MyCte
			where dateadd(minute, @SessionSlot, Start) < @End
		) 
		insert dbo.sesSessions (Start, [End], ProposedTeacherUserId, Cost)
			select Start, dateadd(minute, @SessionLength, Start), @ProposedTeacherUserId, @Cost
			from MyCte

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