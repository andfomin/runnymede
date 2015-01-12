
CREATE PROCEDURE [dbo].[exeGetReviews]
	@UserId int,
	@RowOffset int,
	@RowLimit int
AS
BEGIN
SET NOCOUNT ON;

declare @TotalCount int;

select @TotalCount = count(*) from dbo.exeReviews where UserId = @UserId;

select Id, ExerciseId, ExerciseType, ExerciseLength, Price, StartTime, FinishTime, AuthorUserId, AuthorName
from dbo.exeReviews
where UserId = @UserId
order by StartTime desc
offset @RowOffset rows
	fetch next @RowLimit rows only;

-- Do not return @TotalCount as a column in the main query, because the main query may return no rows for a big @RowOffset.
select @TotalCount as TotalCount;

END
GO
GRANT EXECUTE
    ON OBJECT::[dbo].[exeGetReviews] TO [websiterole]
    AS [dbo];

