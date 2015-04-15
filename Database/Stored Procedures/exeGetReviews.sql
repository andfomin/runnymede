
CREATE PROCEDURE [dbo].[exeGetReviews]
	@UserId int,
	@RowOffset int,
	@RowLimit int
AS
BEGIN
SET NOCOUNT ON;

	declare @TotalCount int;

	-- There is index IX_UserId_FinishTime
	select @TotalCount = count(*) 
	from dbo.exeReviews 
	where UserId = @UserId;

	select R.Id, R.StartTime, R.FinishTime, E.[Type] as ExerciseType, E.[Length] as ExerciseLength
	from dbo.exeReviews R
		inner join dbo.exeExercises E on R.ExerciseId = E.Id
	where R.UserId = @UserId
	order by R.StartTime desc
	offset @RowOffset rows
		fetch next @RowLimit rows only;

	-- Do not return @TotalCount as a column in the main query, because the main query may return no rows for a big @RowOffset.
	select @TotalCount as TotalCount;

END
GO
GRANT EXECUTE
    ON OBJECT::[dbo].[exeGetReviews] TO [websiterole]
    AS [dbo];

