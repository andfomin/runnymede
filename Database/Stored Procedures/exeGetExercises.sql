

CREATE PROCEDURE [dbo].[exeGetExercises]
	@UserId int,
	@Type char(6),
	@RowOffset int, 
	@RowLimit int
AS
BEGIN

set nocount on;

declare @RowCount int;

declare @t table (
	Id int,
	CreateTime datetime2(0),
	[Type] char(6),
	Title nvarchar(100),
	[Length] int
);

insert into @t (Id, CreateTime, [Type], Title, [Length])
	select Id, CreateTime, [Type], Title, [Length]
	from dbo.exeExercises
	where UserId = @UserId
		and [Type] = @Type;

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

select Id, CreateTime, [Type], Title, [Length]
from @t
order by CreateTime desc;

select R.ExerciseId, R.Id, R.Price, R.RequestTime, R.StartTime, R.FinishTime
from dbo.exeReviews R 
	inner join @t T on R.ExerciseId = T.Id;

select @RowCount;

END
GO
GRANT EXECUTE
    ON OBJECT::[dbo].[exeGetExercises] TO [websiterole]
    AS [dbo];

