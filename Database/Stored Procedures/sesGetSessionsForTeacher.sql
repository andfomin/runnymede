


CREATE PROCEDURE [dbo].[sesGetSessionsForTeacher]
	@UserId int,
	@Start smalldatetime,
	@End smalldatetime
AS
BEGIN
SET NOCOUNT ON;

	declare @Now smalldatetime = sysutcdatetime();

	select Id, Start, [End], TeacherUserId, LearnerUserId, ConfirmationTime, CancellationTime
	from (
		select Id, Start, [End], TeacherUserId, LearnerUserId, ConfirmationTime, CancellationTime,
			row_number() over (partition by Start order by 
				case 
					when TeacherUserId = @UserId then 1
					--when TeacherUserId is null and ProposedTeacherUserId = @UserId then 2
					--when TeacherUserId is null and ProposedTeacherUserId is null then 3
				end	
			) as RowNumber
		from dbo.sesSessions
		where Start <= @End
			and [End] >= @Start
			and (
				TeacherUserId = @UserId
				or (					
					TeacherUserId is null
					and Start > @Now
					--and nullif(ProposedTeacherUserId, @UserId) is null
				)
			)
		) q
		where q.RowNumber = 1
	;
	
END
GO
GRANT EXECUTE
    ON OBJECT::[dbo].[sesGetSessionsForTeacher] TO [websiterole]
    AS [dbo];

