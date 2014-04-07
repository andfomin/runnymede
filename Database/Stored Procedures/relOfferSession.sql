

CREATE PROCEDURE [dbo].[relOfferSession]
	@UserId int,
	@Start smalldatetime,
	@End smalldatetime
AS
BEGIN
/*
AF. 20140310
Periods of a user should be continuous and non-overlaping.
First, we search for any overlaping periods. 
Second, we merge overlaping periods if any with the new one to find the combined time stretch.
Third, we update the earleast overlaping period if present inplace or insert the new merged period if no overlapping period found.
Then we delete the latest overlapping period if it exists.

This operation is idempotent.
*/
SET NOCOUNT ON;

declare @ProcName sysname, @ExternalTran int, @XState int;
select  @ProcName = object_name(@@procid), @ExternalTran = @@trancount;

begin try
	if @ExternalTran > 0
		save transaction ProcedureSave;	

	declare @OldId int, @NewStart smalldatetime, @NewEnd smalldatetime;

	declare @t table (
		Id int,
		Start smalldatetime not null,
		[End] smalldatetime not null
	);

	-- [Start]<[End] implemented as a constraint on dbo.relSchedulePeriods 
	-- The UI operates with 15 minute steps, i.e. 00:00, 00:15, 00:30, 00:45,
	if  @Start < dateadd(minute, -17, sysutcdatetime())
		raiserror('%s:: The start of the offered period is in the distant past.', 16, 1, @ProcName);

	if  @End < sysutcdatetime()
		raiserror('%s:: The end of the offered period is in the past.', 16, 1, @ProcName);

	insert @t (Id, Start, [End])
		select Id, Start, [End]
		from dbo.relScheduleEvents
		where UserId = @UserId
			and Start <= @End
			and [End] >= @Start
			and [Type] = 'OFFR';

	select @OldId = min(Id), @NewStart = min(Start), @NewEnd = max([End])
	from (
		select Id, Start, [End]
		from @t
		union all
		select null, @Start, @End
	) q;

	if @ExternalTran = 0
		begin transaction;

		merge dbo.relScheduleEvents as target
		using (select @OldId) as source (Id)
		on (target.Id = source.Id)
		when matched then 
			update set Start = @NewStart, [End] = @NewEnd, CreationTime = sysutcdatetime()
		when not matched then	
			insert (UserId, Start, [End], [Type], CreationTime)
			values (@UserId, @NewStart, @NewEnd, 'OFFR', sysutcdatetime());

		delete dbo.relScheduleEvents
		where Id in (
			select Id from @t where Id != @OldId
		);

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
    ON OBJECT::[dbo].[relOfferSession] TO [websiterole]
    AS [dbo];

