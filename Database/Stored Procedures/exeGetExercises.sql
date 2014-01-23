

CREATE PROCEDURE [dbo].exeGetExercises
	@UserId int, 
	@RowOffset int, 
	@RowLimit int
AS
BEGIN

set nocount on;

declare @RowCount int;

declare @t table (
	Id int,
	CreateTime datetime2(0),
	TypeId nchar(4),
	Title nvarchar(100),
	[Length] int
);

insert into @t (Id, CreateTime, TypeId, Title, [Length])
	select Id, CreateTime, TypeId, Title, [Length]
	from dbo.exeExercises
	where UserId = @UserId;

select @RowCount = count(*) from @t;

delete @t 
	where Id not in 
		(
			select Id
			from @t 
			order by CreateTime desc
			offset @RowOffset rows
			fetch next @RowLimit rows only
		);

set nocount off;

-- The order of the recordsets does matter.

select @RowCount;

select Id, CreateTime, TypeId, Title, [Length]
from @t
order by CreateTime desc;

select R.ExerciseId, R.Id, R.Reward, R.RequestTime, R.StartTime, R.FinishTime, R.CancelTime, R.ReviewerName 
from dbo.exeReviews R 
	inner join @t T on R.ExerciseId = T.Id;

END
GO
GRANT EXECUTE
    ON OBJECT::[dbo].[exeGetExercises] TO [websiterole]
    AS [dbo];

