

CREATE PROCEDURE [dbo].[sesGetSessionsForLearner]
	@UserId int,
	@Start smalldatetime,
	@End smalldatetime
AS
BEGIN
SET NOCOUNT ON;

	declare @MinStart smalldatetime = dateadd(minute, dbo.appGetConstantAsInt('Sessions.BookingAdvance.Minutes'), sysutcdatetime());

	-- There is CX_LearnerUserId_End__Start_TeacherUserId on dbo.sesSessions

	-- Already booked sessions
	select Id, Start, [End], BookingTime, ConfirmationTime, CancellationTime, TeacherUserId, @UserId as LearnerUserId, Price, Rating
	from dbo.sesSessions
	where Start <= @End
		and [End] >= @Start
		and LearnerUserId = @UserId
	--union 
	---- Session offers
	--select nullif(max(iif(C.TeacherUserId is not null, Id, 0)), 0) as Id, 
	--	S.Start, S.[End], C.TeacherUserId, null as LearnerUserId,
	--	min(S.Price) as Price, null as Rating
	--from dbo.sesSessions S
	--	left join (
	--		select distinct TeacherUserId
	--		from dbo.sesSessions
	--		where LearnerUserId = @UserId
	--			and [End] < @MinStart
	--	) C on S.TeacherUserId = C.TeacherUserId
	--where S.Start <= @End 
	--	and S.[End] >= @Start						
	--	and S.LearnerUserId is null 
	--	and S.Start > @MinStart 
	--	and S.Price is not null
	--	and S.TeacherUserId is not null 
	--group by S.Start, S.[End], C.TeacherUserId
	;
	
END
GO
GRANT EXECUTE
    ON OBJECT::[dbo].[sesGetSessionsForLearner] TO [websiterole]
    AS [dbo];

