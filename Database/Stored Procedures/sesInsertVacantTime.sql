


CREATE PROCEDURE [dbo].[sesInsertVacantTime]
	@UserId int,
	@Start smalldatetime,
	@End smalldatetime
AS
BEGIN
/*
AF. 20140310
Periods of a user should be sequential and non-overlaping.
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
		[End] smalldatetime not null,
		[Type] char(6) not null
	);

	if @Start < dateadd(minute, -1, sysutcdatetime()) -- dbo.sesGetEarliestPossibleStartTime()
		raiserror('%s:: The start of the offered period must not be in the past.', 16, 1, @ProcName);

	-- [Start]<[End] implemented as a constraint on dbo.sesScheduleEvents 
	if  @Start >= @End
		raiserror('%s:: The start must precede the end.', 16, 1, @ProcName);

	-- Get all items adjucent to or overlapping with the new item.
	insert @t (Id, Start, [End], [Type])
		select Id, Start, [End], [Type]
		from dbo.sesScheduleEvents
		where UserId = @UserId
			and Start <= @End
			and [End] >= @Start;

	if exists (
		-- Notice the difference in time comparition. We use non-equality comparition here to allow for an adjacent event.
		select * 
		from @t 
		where Start < @End
			and [End] > @Start
			and [Type] not in ('SES_VT', 'SESSCS', 'SESSCO')
	)
		raiserror('%s:: There is an event overlapping with the offered period.', 16, 1, @ProcName);

	-- Find the new combined period.
	select @OldId = min(Id), @NewStart = min(Start), @NewEnd = max([End])
	from (
		select Id, Start, [End]
		from @t
		where [Type] = 'SES_VT'
		union all
		select null, @Start, @End
	) q;

	if @ExternalTran = 0
		begin transaction;

		merge dbo.sesScheduleEvents as target
		using (select @OldId) as source (Id)
		on (target.Id = source.Id)
		when matched then 
			update set Start = @NewStart, [End] = @NewEnd
		when not matched then	
			insert (UserId, Start, [End], [Type])
			values (@UserId, @NewStart, @NewEnd, 'SES_VT');

		delete dbo.sesScheduleEvents
		where Id in (
			select Id from @t where Id != @OldId
		);

		delete S
		from  dbo.sesScheduleEvents S
			inner join @t T on S.Id = T.Id
		where T.Id != @OldId;

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
    ON OBJECT::[dbo].[sesInsertVacantTime] TO [websiterole]
    AS [dbo];

